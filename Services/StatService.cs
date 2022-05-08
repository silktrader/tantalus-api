using Dapper;
using Npgsql;
using Tantalus.Models;

namespace Tantalus.Services;

public interface IStatService {
    Task<MoodFoodsResponse> GetMoodFoods(Guid userId, GetStatsParameters parameters, bool high);
    Task<MoodPerCaloricRange> GetMoodPerCaloricRange(Guid userId, GetStatsParameters parameters);
}

public class StatService : IStatService {
    private readonly string _connectionString;

    private NpgsqlConnection DbConnection => new(_connectionString);

    public StatService(IConfiguration configuration) {
        _connectionString = configuration.GetConnectionString("Database") ?? throw new InvalidOperationException();
    }

    public async Task<MoodFoodsResponse> GetMoodFoods(Guid userId, GetStatsParameters parameters, bool high) {
        var query = @$"
              WITH foods_moods
                   AS (SELECT food_id AS id,
                              mood
                       FROM   portions
                              join diary_entries USING (date)
                       WHERE  date BETWEEN @startDate AND @endDate
                       AND    portions.user_id = @userId
                       AND    diary_entries.user_id = @userId
                       GROUP  BY id, date, mood)

              SELECT id, name, short_url, total, percent
              FROM  (SELECT id, total, happy / total :: FLOAT AS percent
                     FROM   (SELECT id, Count(*) AS total
                                   FROM   foods_moods
                                   GROUP  BY id) AS total_occurrences
                     JOIN   (SELECT id, Count(*) AS happy
                                   FROM   foods_moods
                                   WHERE  mood {(high ? ">" : "<")} 3
                                   GROUP  BY id) AS happy_occurrences
                     USING(id)) AS combined
                     JOIN foods USING(id)
                     ORDER BY (percent, total) DESC
                     LIMIT @records";
        await using var connection = DbConnection;
        return new MoodFoodsResponse {
            Foods = await connection.QueryAsync<MoodFood>(query,
                new {
                    userId, records = parameters.Records, startDate = parameters.StartDate,
                    endDate = parameters.EndDate
                })
        };
    }

    public async Task<MoodPerCaloricRange> GetMoodPerCaloricRange(Guid userId, GetStatsParameters parameters) {

        const string query = @"
            SELECT lower_limit, upper_limit, ROUND(AVG(mood), 2) AS average_mood
            FROM (
                SELECT mood, lower_limit, upper_limit
                FROM ( (
                        SELECT DATE, ROUND(SUM(quantity * (
                                        SELECT calories(foods)
                                        FROM foods
                                        WHERE id = selected_portions.food_id)
                                ) / 100
                            ) calories
                        FROM portions AS selected_portions
                        GROUP BY DATE
                    ) calories_aggregates
                    JOIN diary_entries USING(DATE)
                )
                JOIN (
                    SELECT *
                    FROM (
                        VALUES (0, 1500), (1501, 2000), (2001, 2500), (2501, 3000), (3001, 3500), (3501, 4000), (4001, 10000)
                    ) calories_ranges (lower_limit, upper_limit)
                ) ranges
                ON calories BETWEEN lower_limit AND upper_limit
            ) averages
            GROUP BY (lower_limit, upper_limit)";

        await using var connection = DbConnection;
        return new MoodPerCaloricRange {
            Ranges = await connection.QueryAsync<CaloricRange>(query, new {userId})
        };
    }
}