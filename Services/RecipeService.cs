using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Models;
using Npgsql;
using Tantalus.Data;
using Tantalus.Entities;
using Tantalus.Models;

namespace Tantalus.Services;

public interface IRecipeService {
    Task<(IEnumerable<RecipeResponse>, long count)> GetPaginatedRecipes(GetFoodsParameters parameters, Guid userId);
    Task<bool> Exists(string name, Guid userId);
    Task CreateRecipe(RecipePostRequest recipeRequest, Guid userId);
    Task<RecipeResponse?> GetRecipe(Guid id, Guid userId);

    /// <summary>
    /// Find a recipe and track it, for later EF manipulation, preferable to Dapper in case of updates.
    /// </summary>
    Task<Recipe?> TrackRecipe(Guid id, Guid userId);

    Task UpdateRecipe(Recipe recipe, RecipePostRequest request);
}

public class RecipeService : IRecipeService {
    private readonly string _connectionString;

    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;
    private NpgsqlConnection DbConnection => new(_connectionString);

    public RecipeService(DataContext dataContext, IMapper mapper, IConfiguration configuration) {
        _dataContext = dataContext;
        _mapper = mapper;
        _connectionString = configuration.GetConnectionString("Database") ?? throw new InvalidOperationException();
    }

    public async Task<(IEnumerable<RecipeResponse>, long count)> GetPaginatedRecipes(GetFoodsParameters parameters, Guid userId) {
        
        // avoid aliasing columns to ensure correctness of mapping
        const string query = @"
            SELECT *
            FROM (
                SELECT id, name, access, created, count
                FROM (
                    SELECT *, COUNT(*) OVER() AS count
                    FROM user_recipes(@userId)
                ) counted_recipes
                WHERE @unfiltered OR name ILIKE @nameFilter
            ) recipes
            JOIN recipe_ingredients ON recipe_id = recipes.id
            JOIN foods ON recipe_ingredients.food_id = foods.id
            FETCH FIRST @pageSize ROWS ONLY
            OFFSET @offset";

        await using var connection = DbConnection;
        var recipesDictionary = new Dictionary<Guid, RecipeResponse>();
        
        // must map to Int64, or cast to a smaller int in the query, due to Dapper 
        long count = 0;
        await connection.QueryAsync<RecipeResponse, long, IngredientResponse, FoodResponse, RecipeResponse>(query, map:
            (recipe, recipesCount, ingredient, food) => {
                count = recipesCount;
                if (!recipesDictionary.TryGetValue(recipe.Id, out var recipeRecord)) {
                    recipeRecord = recipe;
                    recipeRecord.Ingredients = new List<IngredientResponse>();
                    recipesDictionary.Add(recipeRecord.Id, recipeRecord);
                }

                ingredient.Food = food;
                recipeRecord.Ingredients.Add(ingredient);
                return recipeRecord;
            },
            splitOn: "count,food_id,id",
            param: new {
                userId,
                unfiltered = parameters.NameFilter == null, 
                nameFilter = $"%{parameters.NameFilter}%",
                pageSize = parameters.PageSize,
                offset = parameters.PageIndex * parameters.PageSize
            });
        return (recipesDictionary.Values, count);
    }

    public async Task<RecipeResponse?> GetRecipe(Guid id, Guid userId) {
        const string query = @"
            SELECT * FROM user_recipes(@userId) AS recipes 
            JOIN recipe_ingredients ON recipe_id = recipes.id 
            JOIN foods ON recipe_ingredients.food_id = foods.id 
            WHERE recipes.id = @id";
        await using var connection = DbConnection;
        RecipeResponse? recipeResponse = null; 
        return (await connection.QueryAsync<RecipeResponse, IngredientResponse, FoodResponse, RecipeResponse>(query,
            map: (recipe, recipeIngredient, food) => {
                if (recipeResponse == null) {
                    recipeResponse = recipe;
                    recipeResponse.Ingredients = new List<IngredientResponse>();        // tk use auto constructor
                }

                recipeIngredient.Food = food;
                recipeResponse.Ingredients.Add(recipeIngredient);
                return recipeResponse;
            },
            splitOn: "food_id,id",
            param: new { id, userId })).FirstOrDefault();
    }

    public async Task<bool> Exists(string name, Guid userId) {
        // the alternative would be to include the recipe's name among the relation key's attributes
        const string query = "SELECT TRUE WHERE EXISTS (SELECT 1 FROM user_recipes(@userId) WHERE name=@name)";
        await using var connection = DbConnection;
        return await connection.ExecuteScalarAsync<bool>(query, new {name, userId});
    }

    public async Task CreateRecipe(RecipePostRequest recipeRequest, Guid userId) {
        
        // rely on EF for simple inserts
        await _dataContext.Recipes.AddAsync(new Recipe {
            Id = recipeRequest.Id,
            Name = recipeRequest.Name,
            UserId = userId,
            Created = DateTime.Now
        });
        await _dataContext.SaveChangesAsync();

        await _dataContext.RecipeIngredients.AddRangeAsync(
            recipeRequest.Ingredients.Select(response => new RecipeIngredient {
                FoodId = response.FoodId,
                RecipeId = recipeRequest.Id,
                Quantity = response.Quantity
            }));

        await _dataContext.SaveChangesAsync();
    }

    /// <summary>
    /// Find a recipe and track it, for later EF manipulation, preferable to Dapper in case of updates.
    /// </summary>
    public async Task<Recipe?> TrackRecipe(Guid id, Guid userId) {
        return await _dataContext.Recipes.Include(recipe => recipe.Ingredients).FirstOrDefaultAsync(recipe => recipe.Id == id && recipe.UserId == userId);
    }

    public async Task UpdateRecipe(Recipe recipe, RecipePostRequest request) {
        recipe.Name = request.Name;
        // tk add notes, etc. later
        
        var removedIngredients = new List<RecipeIngredient>();
        var newIngredients = request.Ingredients.ToDictionary(
            keySelector: ingredient => ingredient.FoodId,
            elementSelector: ingredient => ingredient);

        // change ingredients quantities and signal those that will be removed
        foreach (var ingredient in recipe.Ingredients) {
            // check whether quantities where changed
            if (newIngredients.TryGetValue(ingredient.FoodId, out var updatedIngredient)) {
                ingredient.Quantity = updatedIngredient.Quantity;
                newIngredients.Remove(ingredient.FoodId);
            }

            // the ingredient was removed during the edit
            else removedIngredients.Add(ingredient);
        }

        // remove ingredients no longer present, defer removal to avoid up changing iterating list
        _dataContext.RecipeIngredients.RemoveRange(removedIngredients);

        // add new ingredients, whose quantities weren't processed in the previous loop
        await _dataContext.RecipeIngredients.AddRangeAsync(newIngredients.Values.Select(ingredient => new RecipeIngredient {
            FoodId = ingredient.FoodId,
            RecipeId = recipe.Id,
            Quantity = ingredient.Quantity
        }));

        await _dataContext.SaveChangesAsync();
    }
}