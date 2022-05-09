using Dapper;
using Npgsql;
using Tantalus.Models;

namespace Tantalus.Services;

public interface IStatService {
    Task<MoodFoodsResponse> GetMoodFoods(Guid userId, GetStatsParameters parameters, bool high);
    Task<MoodPerCaloricRange> GetMoodPerCaloricRange(Guid userId, GetStatsParameters parameters);
    Task<MoodFoodsResponse> GetFoodsAverageMood(Guid userId, GetStatsParameters parameters, bool highest);
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
                       JOIN   diary_entries USING (date)
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
                    userId, records = parameters.Records, 
                    startDate = parameters.StartDate,
                    endDate = parameters.EndDate
                })
        };
    }

    public async Task<MoodPerCaloricRange> GetMoodPerCaloricRange(Guid userId, GetStatsParameters parameters) {

        // the diary entries join might be clarified with a WHERE clause, but requires user_id to be carried over
        // carrying it over though conflicts with the group by (which in turn requires inclusion or use in an aggregate)
        const string query = @"
            SELECT lower_limit, upper_limit, ROUND(AVG(mood), 2) AS average_mood
            FROM (
                SELECT mood, lower_limit, upper_limit
                FROM ( (
                        SELECT date, ROUND(SUM(quantity * (
                                        SELECT calories(foods)
                                        FROM foods
                                        WHERE id = selected_portions.food_id)
                                ) / 100
                            ) calories
                        FROM portions AS selected_portions
                        WHERE user_id = @userId
                        AND date BETWEEN @startDate AND @endDate
                        GROUP BY date
                    ) calories_aggregates
                    JOIN diary_entries ON diary_entries.date = calories_aggregates.date AND diary_entries.user_id = @userId                  
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
            Ranges = await connection.QueryAsync<CaloricRange>(query, new {
                userId,
                startDate = parameters.StartDate,
                endDate = parameters.EndDate
            })
        };
    }

    public async Task<MoodFoodsResponse> GetFoodsAverageMood(Guid userId, GetStatsParameters parameters, bool highest) {
        
        // when average ratings are the same, for two or more foods, one could order by an addition property
        // (ie. total count, grams, etc.) as a tie breaker
        // in large tables ties are unlikely to occur
        
        // the second order by is required to restore order after a join and sort the forcefully included foods
        
        const string query = @"
            SELECT id, name, short_url, average_mood
            FROM (
                SELECT food_id AS id, ROUND(AVG(mood), 2) AS average_mood
                FROM(
                    SELECT food_id, mood
                    FROM ( (
                            SELECT date, food_id, user_id
                            FROM portions
                            WHERE
                                date BETWEEN @startDate AND @endDate
                                AND user_id = @userId
                            GROUP BY (date, food_id, user_id)
                        ) foods_eaten
                        JOIN diary_entries USING(date, user_id)
                    ) foods_moods
                ) AS foods_averages
                GROUP BY food_id
                ORDER BY
                    food_id IN (
                        '56f54f4c-601d-4be9-8dba-aa572ee0ab42',
                        '56f54f4c-601d-4be9-8dba-aa572ee0ab42'
                    ) DESC,
                    average_mood DESC
                LIMIT @records
            ) sorted_foods_averages
            JOIN foods USING (id)
            ORDER BY average_mood DESC";

        await using var connection = DbConnection;
        return new MoodFoodsResponse {
            Foods = await connection.QueryAsync<MoodFood>(query, new {userId, records = parameters.Records, startDate = parameters.StartDate, endDate = parameters.EndDate})
        };
    }
}