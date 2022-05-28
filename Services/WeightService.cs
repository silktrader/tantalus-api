using Dapper;
using Models;
using Npgsql;
using Tantalus.Models;

namespace Tantalus.Services;

public interface IWeightService {
    Task<AllWeightMeasurementsResponse> GetWeightMeasurements(Guid userId, WeightStatRequest parameters);
    Task<WeightMeasurementResponse?> GetWeightMeasurement(DateTime date, Guid userId);
    Task<int> UpdateWeightMeasurement(Guid userId, WeightUpdateRequest request);
}

public class WeightService : IWeightService {
    
    private readonly string _connectionString;
    private NpgsqlConnection DbConnection => new(_connectionString);

    public WeightService(IConfiguration configuration) {
        _connectionString = configuration.GetConnectionString("Database") ?? throw new InvalidOperationException();
    }

    public async Task<AllWeightMeasurementsResponse> GetWeightMeasurements(Guid userId, WeightStatRequest parameters) {

        var sortProperty = parameters.Sort == WeightAttributes.MeasuredOn ? "measured_on" : parameters.Sort.ToString();
        
        var query = $@"
            SELECT weight, fat, measured_on, note
            FROM weight_measurements
            WHERE
                user_id = @userId AND
                @unbound OR (
                    measured_on >= @start AND
                    measured_on < @end
                )
            ORDER BY {sortProperty} {parameters.Direction}
            FETCH FIRST @pageSize ROWS ONLY OFFSET @offset";
        // brackets around parameters.Direction required to avoid Rider issues

        const string countQuery = @"
            SELECT count(*)
            FROM weight_measurements
            WHERE
                user_id = @userId AND
                @unbound OR (
                    measured_on >= @start AND
                    measured_on < @end
                )";

        var queryParameters = new {
            userId,
            unbound = parameters.Start == null,
            start = parameters.Start, 
            end = parameters.End,
            pageSize = parameters.PageSize,
            offset = parameters.PageIndex * parameters.PageSize
        };
        await using var connection = DbConnection;
        return new AllWeightMeasurementsResponse {
            Measurements = await connection.QueryAsync<WeightMeasurementResponse>(query, queryParameters),
            Count = await connection.ExecuteScalarAsync<int>(countQuery, queryParameters)
        };
    }
    
    public async Task<WeightMeasurementResponse?> GetWeightMeasurement(DateTime date, Guid userId) {
        const string query = "SELECT * FROM weight_measurements WHERE measured_on = @date AND user_id = @userId";
        await using var connection = DbConnection;
        return await connection.QueryFirstOrDefaultAsync<WeightMeasurementResponse>(query, new { date, userId });
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
}