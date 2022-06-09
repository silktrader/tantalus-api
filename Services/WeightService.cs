using System.Net.Cache;
using Dapper;
using Microsoft.AspNetCore.Server.HttpSys;
using Models;
using Npgsql;
using Tantalus.Entities;
using Tantalus.Models;

namespace Tantalus.Services;

public interface IWeightService {
    Task<IEnumerable<WeightResponse>> GetWeightMeasurements(Guid userId, WeightStatRequest parameters);
    Task<WeightResponse?> GetWeightMeasurement(DateTime date, Guid userId);
    Task<int> UpdateWeightMeasurement(Guid userId, WeightUpdateRequest request);
    Task<int> ImportWeightMeasurements(IList<WeightMeasurement> measurements, bool overwrite);
    Task<int> DeleteWeightMeasurement(Guid userId, DateTimeOffset measuredOn);
    Task<IEnumerable<ContiguousWeightResponse>> FindDuplicates(Guid userId, WeightStatRequest request);
    Task<IEnumerable<WeightMonthlyChange>> GetMonthlyChanges(Guid userId, WeightStatRequest request);
}

public class WeightService : IWeightService {
    
    private readonly string _connectionString;
    private NpgsqlConnection DbConnection => new(_connectionString);

    public WeightService(IConfiguration configuration) {
        _connectionString = configuration.GetConnectionString("Database") ?? throw new InvalidOperationException();
    }

    public async Task<IEnumerable<WeightResponse>> GetWeightMeasurements(Guid userId, WeightStatRequest parameters) {

        var query = $@"
            SELECT count(*) OVER() as total, weight, fat, measured_on, note
            FROM weight_measurements
            WHERE
                user_id = @userId AND
                measured_on >= @start AND
                measured_on < @end
            ORDER BY {GetSortProperty(parameters.Sort)} {parameters.Direction}
            FETCH FIRST @pageSize ROWS ONLY OFFSET @offset";
        
        // the end date is inclusive
        var queryParameters = new {
            userId,
            start = parameters.Start, 
            end = parameters.End.AddDays(1),
            pageSize = parameters.PageSize,
            offset = parameters.PageIndex * parameters.PageSize
        };
        await using var connection = DbConnection;
        return await connection.QueryAsync<WeightResponse>(query, queryParameters);
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
        const string query = @"
            SELECT
                COUNT(successor.measured_on) OVER() AS total,
                successor.measured_on AS measured_on,
                successor.weight AS weight,
                successor.fat AS fat,
                successor.note,
                ROUND(EXTRACT(EPOCH FROM(successor.measured_on - predecessor.measured_on))) as seconds_after,
                successor.weight - predecessor.weight AS weight_change,
                successor.fat - predecessor.fat AS fat_change
            FROM
                weight_measurements AS successor,
                weight_measurements AS predecessor
            WHERE
                successor.user_id = @userId AND
                predecessor.user_id = @userId AND
                age(successor.measured_on, predecessor.measured_on) BETWEEN '1 millisecond' AND '20 minute'
            ";

        var paginationExtra = $@"
            ORDER BY {GetSortProperty(request.Sort)} {request.Direction}
            FETCH FIRST @pageSize ROWS ONLY
            OFFSET @offset";
        
        await using var connection = DbConnection;
        return await connection.QueryAsync<ContiguousWeightResponse>(query + paginationExtra, 
            new { userId, 
                request.Start, 
                end = request.End.AddDays(1),
                request.PageSize, 
                offset = request.PageIndex * request.PageSize });
    }

    public async Task<IEnumerable<WeightMonthlyChange>> GetMonthlyChanges(Guid userId, WeightStatRequest request) {
        const string query = @"
            SELECT
	            count(*) OVER() AS total,
	            period::date, 
	            ROUND(weight) AS weight, 
	            ROUND(weight - lead(weight, 1, NULL) OVER(ORDER BY period DESC)) AS weight_change, 
	            ROUND(fat:: NUMERIC, 2) AS fat,
	            ROUND((fat - lead(fat, 1, NULL) OVER(ORDER BY period DESC)):: NUMERIC, 2) AS fat_change,
	            recorded_measures,
	            monthly_avg_calories,
                monthly_avg_calories - lead(monthly_avg_calories, 1, NULL) OVER(ORDER BY period DESC) calories_change,
	            recorded_days
            FROM generate_series(date_trunc('month', @start), date_trunc('month', @end), '1 month') as period
                LEFT JOIN (
                    SELECT date_trunc('month', measured_on) AS period,
                        AVG(weight) AS weight,
                        AVG(fat) AS fat,
                        count(*) as recorded_measures
                    FROM weight_measurements
                    WHERE user_id = @userId
                    GROUP BY period
                ) monthly_measurements_averages USING (period)
                LEFT JOIN (
                    SELECT date_trunc('month', date) AS period,
                        ROUND(AVG(daily_calories)) AS monthly_avg_calories,
                        count(*) as recorded_days
                    FROM(
                            SELECT date,
                                SUM(quantity * calories(foods) / 100) AS daily_calories
                            FROM portions
                                JOIN foods ON portions.food_id = foods.id
                            WHERE portions.user_id = @userId
                            GROUP BY date
                        ) daily_cals
                    WHERE daily_calories > 1500
                    GROUP BY period
                ) monthly_caloric_averages USING (period)";
        
            var paginationExtra = $@"
                ORDER BY {(request.Sort == SortAttributes.None ? "period" : GetSortProperty(request.Sort))} {request.Direction}
                FETCH FIRST @pageSize ROWS ONLY OFFSET @offset";
            
        await using var connection = DbConnection;
        return await connection.QueryAsync<WeightMonthlyChange>(query + paginationExtra, new {
            userId,
            start = request.Start,
            end = request.End,
            request.PageSize,
            offset = request.PageIndex * request.PageSize
        });
    }

    private static string GetSortProperty(SortAttributes attribute) {
        return attribute switch {
            SortAttributes.MeasuredOn => "measured_on",
            SortAttributes.WeightChange => "weight_change",
            SortAttributes.FatChange => "fat_change",
            SortAttributes.SecondsAfter => "seconds_after",
            SortAttributes.MonthlyAvgCalories => "monthly_avg_calories",
            SortAttributes.CaloriesChange => "calories_change",
            SortAttributes.RecordedDays => "recorded_days",
            SortAttributes.RecordedMeasures => "recorded_measures",
            SortAttributes.Month => "period",                   // not ideal, due to JSON des.
            _ => attribute.ToString()
        };
    }
}

