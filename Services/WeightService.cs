using System.Net.Cache;
using Dapper;
using Models;
using Npgsql;
using Tantalus.Entities;
using Tantalus.Models;

namespace Tantalus.Services;

public interface IWeightService {
    Task<AllWeightsResponse> GetWeightMeasurements(Guid userId, WeightStatRequest parameters);
    Task<WeightResponse?> GetWeightMeasurement(DateTime date, Guid userId);
    Task<int> UpdateWeightMeasurement(Guid userId, WeightUpdateRequest request);
    Task<int> ImportWeightMeasurements(IList<WeightMeasurement> measurements, bool overwrite);
    Task<int> DeleteWeightMeasurement(Guid userId, DateTimeOffset measuredOn);
    Task<IEnumerable<ContiguousWeightResponse>> FindDuplicates(Guid userId, WeightStatRequest request);
}

public class WeightService : IWeightService {
    
    private readonly string _connectionString;
    private NpgsqlConnection DbConnection => new(_connectionString);

    public WeightService(IConfiguration configuration) {
        _connectionString = configuration.GetConnectionString("Database") ?? throw new InvalidOperationException();
    }

    public async Task<AllWeightsResponse> GetWeightMeasurements(Guid userId, WeightStatRequest parameters) {

        var sortProperty = parameters.Sort == SortAttributes.MeasuredOn ? "measured_on" : parameters.Sort.ToString();
        
        var query = $@"
            SELECT weight, fat, measured_on, note
            FROM weight_measurements
            WHERE
                user_id = @userId AND
                measured_on >= @start AND
                measured_on < @end
            ORDER BY {sortProperty} {parameters.Direction}
            FETCH FIRST @pageSize ROWS ONLY OFFSET @offset";
        // brackets around parameters.Direction required to avoid Rider issues

        const string countQuery = @"
            SELECT count(*)
            FROM weight_measurements
            WHERE
                user_id = @userId AND
                measured_on >= @start AND
                measured_on < @end";

        // the end date is inclusive
        var queryParameters = new {
            userId,
            start = parameters.Start, 
            end = parameters.End.AddDays(1),
            pageSize = parameters.PageSize,
            offset = parameters.PageIndex * parameters.PageSize
        };
        await using var connection = DbConnection;
        return new AllWeightsResponse {
            Measurements = await connection.QueryAsync<WeightResponse>(query, queryParameters),
            Count = await connection.ExecuteScalarAsync<int>(countQuery, queryParameters)
        };
    }
    
    public async Task<WeightResponse?> GetWeightMeasurement(DateTime date, Guid userId) {
        const string query = "SELECT * FROM weight_measurements WHERE measured_on = @date AND user_id = @userId";
        await using var connection = DbConnection;
        return await connection.QueryFirstOrDefaultAsync<WeightResponse>(query, new { date, userId });
    }

    public async Task<int> UpdateWeightMeasurement(Guid userId, WeightUpdateRequest request) {
        const string query = @"
            UPDATE weight_measurements
            SET weight = @weight,
                fat = @fat,
                note = @note
            WHERE
                user_id = @userId AND
                measured_on = @measuredOn";
        await using var connection = DbConnection;
        return await connection.ExecuteAsync(query, new { request.MeasuredOn, request.Weight, request.Fat, request.Note, userId });
    }

    public async Task<int> ImportWeightMeasurements(IList<WeightMeasurement> measurements, bool overwrite) {
        // measuredOn is truncated by a DB trigger, to ensure all timestamps stop at the second's level
        const string skipExistingQuery = @"
            INSERT INTO weight_measurements (user_id, measured_on, weight, impedance, fat, note)
            VALUES (@userId, @measuredOn, @weight, @impedance, @fat, @note) ON CONFLICT DO NOTHING";

        const string overwriteQuery = @"
            INSERT INTO weight_measurements (user_id, measured_on, weight, impedance, fat, note)
            VALUES (@userId, @measuredOn, @weight, @impedance, @fat, @note) ON CONFLICT (user_id, measured_on) 
                DO UPDATE SET weight = @weight, impedance = @impedance, note = @note";

        await using var connection = DbConnection;

        // use transaction to process bulk inserts
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var rows = await connection.ExecuteAsync(overwrite ? overwriteQuery : skipExistingQuery, measurements,
            transaction: transaction);
        if (!overwrite || (overwrite && rows == measurements.Count))
            await transaction.CommitAsync();
        else {
            await transaction.RollbackAsync();
        }

        return rows;
    }

    public async Task<int> DeleteWeightMeasurement(Guid userId, DateTimeOffset measuredOn) {
        const string query = @"DELETE FROM weight_measurements WHERE measured_on = @measuredOn AND user_id = @userId;";
        await using var connection = DbConnection;
        return await connection.ExecuteAsync(query, new { measuredOn, userId });
    }

    public async Task<IEnumerable<ContiguousWeightResponse>> FindDuplicates(Guid userId, WeightStatRequest request) {
        var query = $@"
            SELECT
                COUNT(successor.measured_on) OVER() AS total,
                successor.measured_on AS measured_on,
                successor.weight AS weight,
                successor.fat AS fat,
                successor.note,
                ROUND(EXTRACT(EPOCH FROM(successor.measured_on - predecessor.measured_on))) as seconds_after,
                successor.weight - predecessor.weight AS weight_difference,
                successor.fat - predecessor.fat AS fat_difference
            FROM
                weight_measurements AS successor,
                weight_measurements AS predecessor
            WHERE
                successor.user_id = @userId AND
                predecessor.user_id = @userId AND
                age(successor.measured_on, predecessor.measured_on) BETWEEN '1 millisecond' AND '20 minute'
            ORDER BY {GetSortProperty(request.Sort)} {request.Direction}
            FETCH FIRST @pageSize ROWS ONLY
            OFFSET @offset";
           
        
        await using var connection = DbConnection;
        return await connection.QueryAsync<ContiguousWeightResponse>(query, 
            new { userId, 
                request.Start, 
                end = request.End.AddDays(1),
                request.PageSize, 
                offset = request.PageIndex * request.PageSize });
    }

    private static string GetSortProperty(SortAttributes attribute) {
        return attribute switch {
            SortAttributes.MeasuredOn => "measured_on",
            SortAttributes.WeightDifference => "weight_difference",
            SortAttributes.FatDifference => "fat_difference",
            SortAttributes.SecondsAfter => "seconds_after",
            _ => attribute.ToString()
        };
    }
}

