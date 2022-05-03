using System.ComponentModel.DataAnnotations;
using System.Text;
using AutoMapper;
using Controllers;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Models;
using Npgsql;
using Tantalus.Data;
using Tantalus.Entities;
using Tantalus.Models;

namespace Tantalus.Services;

public interface IFoodService {
    Task<Food> AddFood(FoodAddRequest foodRequest, Guid userId);
    Task<Food?> GetFood(Guid foodId);
    Task<Food?> GetFood(string shortUrl);
    Task<Food> UpdateFood(FoodUpdateRequest foodRequest, Food food);
    Task<bool> Delete(Guid foodId, Guid userId);
    Task<(IEnumerable<Food> foods, int count)> GetFoods(GetFoodsParameters parameters, Guid userId);
    Task<IEnumerable<Food>> GetFoods(IList<Guid> foodIds, Guid userId);
    Task<IEnumerable<PortionResourceResponse>> GetPortionResourceHints(string name, Guid userId, int limit);
    Task<GetFoodStatsResponse> GetFoodStats(Guid foodId, Guid userId);
    Task<bool> CanViewFood(Guid foodId, Guid userId);
}

public class FoodService : IFoodService {
    private readonly string _connectionString;

    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;
    private NpgsqlConnection DbConnection => new(_connectionString);

    public FoodService(DataContext dataContext, IMapper mapper, IConfiguration configuration) {
        _dataContext = dataContext;
        _mapper = mapper;
        _connectionString = configuration.GetConnectionString("Database") ?? throw new InvalidOperationException();
    }

    public async Task<Food> AddFood(FoodAddRequest foodRequest, Guid userId) {
        // add missing data when necessary
        var food = _mapper.Map<Food>(foodRequest);
        food.Id = Guid.NewGuid(); // can delegate Guid generation to Postgres
        food.UserId = userId;

        // generate an appropriate short URL, possibly with a randomly generated ID
        var shortUrl = ShortenUrl(food.Name);
        if (await Exists(shortUrl))
            shortUrl = $"{shortUrl}-{await Nanoid.Nanoid.GenerateAsync(size: 4)}";
        food.ShortUrl = shortUrl;

        // insert into "Foods" ("Id", "FullName", "ShortUrl", "UserId") values (gen_random_uuid (), 'Banana', 'banana', 'e1bcbc54-52bd-4618-9f27-deef344f9f57')
        await _dataContext.Foods.AddAsync(food);
        await _dataContext.SaveChangesAsync();
        return food;
    }

    public async Task<Food?> GetFood(string shortUrl) {
        // delegates the user check to the controller
        const string query = "SELECT * FROM foods WHERE short_url=@shortUrl";
        await using var connection = DbConnection;
        return await connection.QueryFirstOrDefaultAsync<Food>(query, new { shortUrl });
    }

    public async Task<Food?> GetFood(Guid foodId) {
        const string query = "SELECT * FROM foods WHERE id=@foodId";
        await using var connection = DbConnection;
        return await connection.QueryFirstOrDefaultAsync<Food>(query, new { foodId });
    }

    /// <summary>
    /// Determines whether a user has access to a food, given its id. Checks whether the user created the food
    /// or the latter's access is set to either 'shared', or 'editable', by way of a DB function check.
    /// </summary>
    public async Task<bool> CanViewFood(Guid foodId, Guid userId) {
        const string query = "SELECT EXISTS (SELECT TRUE FROM user_foods(@userId) WHERE id=@foodId)";
        await using var connection = DbConnection;
        return await connection.ExecuteScalarAsync<bool>(query, new { foodId, userId });
    }

    public async Task<Food> UpdateFood(FoodUpdateRequest foodRequest, Food food) {
        _mapper.Map(foodRequest, food);
        _dataContext.Entry(food).State = EntityState.Modified;
        await _dataContext.SaveChangesAsync();
        return food;
    }

    /// <summary> Fetch foods details, inclusive of nutrients quantities, on the basis of search parameters.</summary>
    public async Task<(IEnumerable<Food> foods, int count)> GetFoods(GetFoodsParameters parameters, Guid userId) {
        // could `SELECT *, count(*) OVER() AS foods_count`, but it requires normalisation and inevitable memory allocations
        const string query = "SELECT COUNT(*) FROM user_foods(@userId)";
        
        // filtering is optional and case insensitive
        var nameFilter = parameters.NameFilter == null ? string.Empty : $"WHERE name ILIKE '%{parameters.NameFilter}%'";
        
        // handle special cases when ordering is based on calculated row properties
        var calculatedProperty = parameters.SortProperty switch {
            FoodAttribute.Calories => "calories(foods)",
            FoodAttribute.FatsPercentage => "fats * 9 / Greatest(calories(foods), 0.01)",
            FoodAttribute.ProteinsPercentage => "proteins * 4 / Greatest(calories(foods), 0.01)",
            FoodAttribute.CarbsPercentage => "carbs * 4 / Greatest(calories(foods), 0.01)",
            FoodAttribute.DetailsPercentage => $@"num_nulls({FoodAttributes.Nullable})",
            _ => null                    
        };

        var filter = $@"
            SELECT * FROM user_foods(@userId) {nameFilter} 
            ORDER BY {calculatedProperty ?? parameters.SortProperty.ToString()} {(parameters.Ascending ? "asc" : "desc")} 
            FETCH FIRST @pageSize ROWS ONLY OFFSET @offset";
        
        await using var connection = DbConnection;
        var foods = await connection.QueryAsync<Food>(filter, new { 
            userId,
            pageSize = parameters.PageSize,
            offset = parameters.PageIndex * parameters.PageSize });
        var count = await connection.ExecuteScalarAsync<int>(query, new {userId});        // returns the first column of the first row
        return (foods, count);
    }
    
    public async Task<IEnumerable<Food>> GetFoods(IList<Guid> foodIds, Guid userId) {
        // Dapper won't allow the `IN` operator to work on array parameters, hence the resort to `ANY` 
        const string query = "SELECT * FROM user_foods(@userId) WHERE id = ANY(@foodIds)";
        await using var connection = DbConnection;
        return await connection.QueryAsync<Food>(query, new { foodIds, userId });
    }

    public async Task<bool> Delete(Guid foodId, Guid userId) {
        await using var connection = DbConnection;
        return connection.ExecuteAsync("DELETE FROM foods WHERE id = @foodId AND user_id = @userId",
            new { foodId, userId }).Result > 0;
    }

    private async Task<bool> Exists(string shortUrl) {
        // given the unique constraint on shortUrl a `SELECT TRUE FROM foods` would suffice
        const string query = "SELECT EXISTS (SELECT 1 FROM foods WHERE short_url=@shortUrl)";
        await using var connection = DbConnection;
        return await connection.QueryFirstAsync<bool>(query, new { shortUrl });
    }
    
    public async Task<IEnumerable<PortionResourceResponse>> GetPortionResourceHints(string name, Guid userId, int limit) {
        var pattern = $"%{name}%";
        var today = DateTime.Now;
        var monthAgo = today.AddMonths(-1);
        const string query = @"
            SELECT   id, name, (
                SELECT Count(*)
                FROM   portions
                WHERE  food_id = foods.id
                AND    date <= @today
                AND    date >= @monthAgo) AS priority
            FROM     user_foods(@userId) AS foods
            WHERE    name ilike @pattern
            ORDER BY priority DESC
            LIMIT    @limit";
        await using var connection = DbConnection;
        return await connection.QueryAsync<PortionResourceResponse>(query, new { pattern, userId, limit, today, monthAgo });
    }

    public async Task<GetFoodStatsResponse> GetFoodStats(Guid foodId, Guid userId) {

        var parameters = new { foodId, userId };
        // tk limit to certain date range
        const string associatedFoodsQuery = @"
            SELECT id, name, short_url, Count(*) AS frequency
            FROM (
                SELECT foods.id, foods.short_url, name, date
                FROM   portions
                JOIN   foods
                ON     food_id = foods.id
                WHERE  food_id != @foodId
                AND    portions.user_id = @userId
                AND    (date, meal) IN (
                  SELECT date, meal
                  FROM   foods
                  JOIN   portions
                  ON     foods.id = portions.food_id
                  WHERE  foods.id = @foodId)) AS frequent_foods
            GROUP BY (id, name, short_url)
            ORDER BY frequency DESC
            LIMIT    5";

        // the results could be included in other queries; this is more legible, maintainable
        const string consumptionQuery = @"
            SELECT Count(*)      AS count,
                   Sum(quantity) AS quantity,
                   Max(quantity) AS max,
                   Max(date)     AS last_eaten
            FROM   portions
            WHERE  food_id = @foodId
                   AND date BETWEEN '2022-04-01' AND Now();";

        // use view tk!
        const string frequentMealsQuery = @"
            SELECT *
            FROM   (SELECT Count(*) AS frequency,
                           meal
                    FROM   portions
                    WHERE  food_id = @foodId
                           AND date BETWEEN '2022-04-01' AND Now()
                    GROUP  BY meal) AS frequent_meals
            WHERE  frequency > 0
            ORDER  BY frequency DESC
            LIMIT  3;";

        const string recipesQuery = @"
            SELECT id, quantity, name
            FROM   recipe_ingredients
                   JOIN recipes
                     ON recipe_id = id
            WHERE  food_id = @foodId;";
        
        await using var connection = DbConnection;
        var (count, quantity, max, lastEaten) = await connection.QueryFirstAsync<(int count, int quantity, int max, DateTime lastEaten)>(consumptionQuery, parameters);

        return new GetFoodStatsResponse {
            Count = count,
            Quantity = quantity,
            Max = max,
            LastEaten = lastEaten == DateTime.MinValue ? null : DateOnly.FromDateTime(lastEaten),
            FrequentFoods = await connection.QueryAsync<FrequentFood>(associatedFoodsQuery, parameters),
            FrequentMeals = await connection.QueryAsync<FrequentMeal>(frequentMealsQuery, parameters),
            Recipes = await connection.QueryAsync<RecipeFoodStat>(recipesQuery, parameters)
        };
    }

    private static string ShortenUrl(string url) {
        const int maxLength = 50; // tk export
        var previousDash = false;
        var stringBuilder = new StringBuilder(url.Length);

        for (var i = 0; i < url.Length; i++) {
            var character = url[i];
            switch (character) {
                case >= 'a' and <= 'z' or >= '0' and <= '9':
                    stringBuilder.Append(character);
                    previousDash = false;
                    break;
                case >= 'A' and <= 'Z':
                    stringBuilder.Append((char)(character | 32)); // convert to lowercase
                    previousDash = false;
                    break;
                case ' ':
                case ',':
                case '.':
                case '/':
                case '\\':
                case '-':
                case '_':
                case '=': {
                    if (!previousDash && stringBuilder.Length > 0) {
                        stringBuilder.Append('-');
                        previousDash = true;
                    }

                    break;
                }
                default: {
                    if (character >= 128) {
                        var previousLength = stringBuilder.Length;
                        stringBuilder.Append(AsciiSubstitute(character));
                        if (previousLength != stringBuilder.Length) previousDash = false;
                    }

                    break;
                }
            }

            if (i == maxLength) break;
        }

        return previousDash ? stringBuilder.ToString()[..(stringBuilder.Length - 1)] : stringBuilder.ToString();
    }

    // not the fastest implementation, but easy to maintain and update
    // tk look for builtin methods
    private static string AsciiSubstitute(char character) {
        var s = character.ToString().ToLowerInvariant();

        if ("àåáâäãåą".Contains(s)) return "a";
        if ("èéêëę".Contains(s)) return "e";
        if ("ìíîïı".Contains(s)) return "i";
        if ("òóôõöøőð".Contains(s)) return "o";
        if ("ùúûüŭů".Contains(s)) return "u";
        if ("çćčĉ".Contains(s)) return "c";
        if ("żźž".Contains(s)) return "z";
        if ("śşšŝ".Contains(s)) return "s";
        if ("ñń".Contains(s)) return "n";
        if ("ýÿ".Contains(s)) return "y";
        if ("ğĝ".Contains(s)) return "g";

        return character switch {
            'ß' => "ss",
            'Þ' => "th",
            'ĥ' => "h",
            'ĵ' => "j",
            _ => ""
        };
    }
}