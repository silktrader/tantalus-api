using System.Text;
using AutoMapper;
using Controllers;
using Dapper;
using Microsoft.EntityFrameworkCore;
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
    Task<IEnumerable<Food>> GetFoods(FoodsController.GetFoodsParameters parameters, Guid userId);
    Task<IEnumerable<Food>> GetFoods(IEnumerable<Guid> foodIds, Guid userId);
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
        const string query = "SELECT * FROM foods WHERE short_url=@shortUrl";
        await using var connection = new NpgsqlConnection(_connectionString);
        return (await connection.QueryAsync<Food>(query, new { shortUrl })).FirstOrDefault();
        // return await _dataContext.Foods.FirstOrDefaultAsync(food => food.ShortUrl == shortUrl);
    }

    public async Task<Food?> GetFood(Guid foodId) {
        const string query = "SELECT * FROM foods WHERE id=@foodId";
        await using var connection = DbConnection;
        return await connection.QueryFirstOrDefaultAsync<Food>(query, new { foodId });
    }

    public async Task<Food> UpdateFood(FoodUpdateRequest foodRequest, Food food) {
        _mapper.Map(foodRequest, food);
        _dataContext.Entry(food).State = EntityState.Modified;
        await _dataContext.SaveChangesAsync();
        return food;
    }

    public async Task<IEnumerable<Food>> GetFoods(FoodsController.GetFoodsParameters parameters, Guid userId) {
        // tk substitute enum values
        const string query = "SELECT * FROM foods WHERE user_id = @userId or visibility in ('shared', 'editable')";
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Food>(query, new { userId });
    }
    
    public async Task<IEnumerable<Food>> GetFoods(IEnumerable<Guid> foodIds, Guid userId) {
        const string query = "SELECT * FROM foods WHERE id IN @foodIds and (user_id = @userId OR visibility in ('shared', 'editable'))";
        // id = ANY(@foodIds) is an alternative
        await using var connection = DbConnection;
        return await connection.QueryAsync<Food>(query, new { foodIds, userId });
    }

    public async Task<bool> Delete(Guid foodId, Guid userId) {
        await using var connection = DbConnection;
        return connection.ExecuteAsync("DELETE FROM foods WHERE id = @foodId AND user_id = @userId",
            new { foodId, userId }).Result > 0;
    }

    private async Task<bool> Exists(string shortUrl) {
        const string query = "SELECT EXISTS (SELECT 1 FROM foods WHERE short_url=@shortUrl)";
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstAsync<bool>(query, new { shortUrl });
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