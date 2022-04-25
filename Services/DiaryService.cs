using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tantalus.Data;
using Tantalus.Entities;
using Tantalus.Models;

namespace Tantalus.Services; 

public class DiaryService : IDiaryService {
    private readonly string _connectionString;
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;
    private NpgsqlConnection DbConnection => new(_connectionString);

    public DiaryService(DataContext dataContext, IMapper mapper, IConfiguration configuration) {
        _dataContext = dataContext;
        _mapper = mapper;
        _connectionString = configuration.GetConnectionString("Database") ?? throw new InvalidOperationException();
    }
    
    public async Task<DiaryEntryResponse?> GetDiary(DateOnly dateOnly, Guid userId) {
        var date = dateOnly.ToDateTime(TimeOnly.MinValue);
        const string diaryEntryQuery = "SELECT comment FROM diary_entries WHERE date=@date AND user_id=@userId";
        const string portionsQuery = "SELECT * FROM portions WHERE date=@date AND user_id=@userId";
        await using var connection = DbConnection;
        
        // check whether a diary entry exists, which guarantees that portions were recorded for the relevant date
        var diaryEntry = await connection.QueryFirstOrDefaultAsync<DiaryEntry>(diaryEntryQuery, new { date, userId });
        if (diaryEntry == null)
            return null;

        var portions = await connection.QueryAsync<Portion>(portionsQuery, new { date, userId });
        var foodsIds = portions.Select(portion => portion.FoodId).Distinct().ToArray();
        
        const string foodsQuery = "SELECT * FROM foods where id = ANY(@foodsIds)";
        // for some reason `id IN @foodsIds` doesn't work
        // an alternative query to the the one above, while removing the portions iteration, would be:
        // const string foodsQuery = "SELECT foods.* FROM foods JOIN portions ON foods.id = portions.food_id WHERE portions.date=@date";
        return new DiaryEntryResponse {
            Comment = diaryEntry.Comment,
            Portions = _mapper.Map<PortionResponse[]>(portions),
            Foods = _mapper.Map<FoodResponse[]>(await connection.QueryAsync<Food>(foodsQuery, new { foodsIds }))
        };
    }

    public async Task<bool> CreateDailyEntry(DateOnly dateOnly, Guid userId) {
        var date = dateOnly.ToDateTime(TimeOnly.MinValue);
        const string query = "INSERT INTO diary_entries (date, user_id) VALUES (@date, @userId) ON CONFLICT DO NOTHING";
        await using var connection = DbConnection;
        return await connection.ExecuteAsync(query, new { date, userId }) == 1;
    }
    
    public async Task<IEnumerable<Portion>> AddPortions(IEnumerable<PortionRequest> portionRequests, DateOnly dateOnly, Guid userId) {
        var portions = portionRequests.Select(request => new Portion {
            Id = request.Id,
            FoodId = request.FoodId,
            Quantity = request.Quantity,
            Meal = request.Meal,
            Date = dateOnly.ToDateTime(TimeOnly.MinValue),
            UserId = userId
        }).ToList();
        
        // EF tracked inserts are more efficient than manual inserts and allow safe and easy enum mapping, example:
        // "INSERT INTO portions (id, food_id, quantity, meal, date, user_id) VALUES (@Id, @FoodId, @Quantity, @Meal, @date, @userId) RETURNING *";

        await _dataContext.AddRangeAsync(portions);
        await _dataContext.SaveChangesAsync();
        return portions;
    }

    public async Task<Portion?> GetPortion(Guid portionId) {
        const string query = "SELECT * FROM portions WHERE id = @portionId";
        await using var connection = DbConnection;
        return await connection.QuerySingleOrDefaultAsync<Portion>(query, new { portionId });
    }

    public async Task UpdatePortion(Portion portion, PortionRequest portionRequest) {
        //const string query = "UPDATE portions SET quantity = @quantity, meal = @meal WHERE id = @portionId";
        // Dapper won't properly map C# enums to Postgresql enums; rather than hack around it's preferable to resort to EF
        _mapper.Map(portionRequest, portion);
        _dataContext.Entry(portion).State = EntityState.Modified;
        await _dataContext.SaveChangesAsync();
    }
}

public interface IDiaryService {
    Task<bool> CreateDailyEntry(DateOnly date, Guid userId);
    Task<IEnumerable<Portion>> AddPortions(IEnumerable<PortionRequest> portionRequests, DateOnly date, Guid userId);
    Task<DiaryEntryResponse?> GetDiary(DateOnly date, Guid userId);
    Task<Portion?> GetPortion(Guid portionId);
    Task UpdatePortion(Portion portion, PortionRequest portionRequest);
}

