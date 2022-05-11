using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Tantalus.Models;
using Tantalus.Services;

namespace Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class RecipesController : TantalusController {

    private readonly IRecipeService _recipeService;
    private readonly IMapper _mapper;

    public RecipesController(IRecipeService recipeService, IMapper mapper) {
        _recipeService = recipeService;
        _mapper = mapper;
    }
    
    [HttpGet]
    public async Task<ActionResult> GetPaginatedRecipes([FromQuery] GetFoodsParameters parameters) {
        var (recipesData, count) = await _recipeService.GetPaginatedRecipes(parameters, UserGuid);
        var recipes = _mapper.Map<RecipeResponse[]>(recipesData);
        return Ok(new { recipes, count});
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RecipeResponse>> GetRecipe(Guid id) {
        var recipe = await _recipeService.GetRecipe(id, UserGuid);
        if (recipe == null)
            return NotFound();
        return Ok(recipe);
    }
    
    [HttpPost]
    public async Task<ActionResult> AddRecipe(RecipePostRequest recipe) {
        var userGuid = UserGuid;
        if (await _recipeService.Exists(recipe.Name, userGuid))
            return BadRequest(new { message = "A recipe with the same name already exists" });

        // internal errors are handled with adequate HTTP responses
        await _recipeService.CreateRecipe(recipe, userGuid);
        return Ok();
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> EditRecipe(Guid id, RecipePostRequest request) {
        var userId = UserGuid;
        var recipe = await _recipeService.TrackRecipe(id, userId);
        
        if (recipe == null)
            return NotFound();

        await _recipeService.UpdateRecipe(recipe, request);
        return Ok();
    }
}