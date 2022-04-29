﻿using System.Data.Common;
using AutoMapper;
using Dapper;
using Models;
using Npgsql;
using Tantalus.Data;
using Tantalus.Entities;
using Tantalus.Models;

namespace Tantalus.Services;

public interface IRecipeService {
    Task<(IEnumerable<RecipeResponse>, int count)> GetRecipes(GetFoodsParameters parameters, Guid userId);
    Task<bool> Exists(string name, Guid userId);
    Task CreateRecipe(RecipePostRequest recipeRequest, Guid userId);
    Task<RecipeResponse?> GetRecipe(Guid id, Guid userId);
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

    public async Task<(IEnumerable<RecipeResponse>, int count)> GetRecipes(GetFoodsParameters parameters, Guid userId) {
        const string countQuery = "SELECT COUNT(*) FROM user_recipes(@userId)";

        // filtering is optional and case insensitive
        var nameFilter = parameters.NameFilter == null ? string.Empty : $"WHERE name ILIKE '%{parameters.NameFilter}%'";

        var query = $@"
            SELECT * FROM user_recipes(@userId) AS recipes {nameFilter}
            JOIN recipe_ingredients ON recipe_id = recipes.id
            JOIN foods ON recipe_ingredients.food_id = foods.id
            FETCH FIRST @pageSize ROWS ONLY OFFSET @offset";

        await using var connection = DbConnection;
        var recipesDictionary = new Dictionary<Guid, RecipeResponse>();
        var recipes = await connection.QueryAsync<RecipeResponse, IngredientResponse, FoodResponse, RecipeResponse>(query, map:
            (recipe, ingredient, food) => {
                if (!recipesDictionary.TryGetValue(recipe.Id, out var recipeRecord)) {
                    recipeRecord = recipe;
                    recipeRecord.Ingredients = new List<IngredientResponse>();
                    recipesDictionary.Add(recipeRecord.Id, recipeRecord);
                }

                ingredient.Food = food;
                recipeRecord.Ingredients.Add(ingredient);
                return recipeRecord;
            },
            splitOn: "food_id,id",
            param: new {
                userId,
                pageSize = parameters.PageSize,
                offset = parameters.PageIndex * parameters.PageSize
            });
        var count = await connection.ExecuteScalarAsync<int>(countQuery, new { userId });
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
}