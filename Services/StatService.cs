using Dapper;
using Npgsql;
using Tantalus.Models;

namespace Tantalus.Services; 

public interface IStatService {
       Task<HighMoodFoodsResponse> GetHighMoodFoods(Guid userId, GetStatsParameters parameters);
}

public class StatService : IStatService {
       private readonly string _connectionString;

       private NpgsqlConnection DbConnection => new(_connectionString);
       
       public StatService(IConfiguration configuration) {
              _connectionString = configuration.GetConnectionString("Database") ?? throw new InvalidOperationException();
       }

    public async Task<HighMoodFoodsResponse> GetHighMoodFoods(Guid userId, GetStatsParameters parameters) {

        const string query = @"
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
                                   WHERE  mood > 3
                                   GROUP  BY id) AS happy_occurrences
                     USING(id)) AS combined
                     JOIN foods USING(id)
                     ORDER BY (percent, total) DESC
                     LIMIT @records";
        await using var connection = DbConnection;
        return new HighMoodFoodsResponse {
               Foods = await connection.QueryAsync<HighMoodFood>(query, 
                      new { userId, records = parameters.Records, startDate = parameters.StartDate, endDate = parameters.EndDate}) 
        };

    }
    
}

