using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tantalus.Models;
using Tantalus.Services;

namespace Controllers; 

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class DiaryController : TantalusController {
    
    private readonly IMapper _mapper;
    private readonly IDiaryService _diaryService;
    private readonly IFoodService _foodService;
    private readonly IRecipeService _recipeService;

    public DiaryController(IMapper mapper, IDiaryService diaryService, IFoodService foodService, IRecipeService recipeService) {
        _mapper = mapper;
        _diaryService = diaryService;
        _foodService = foodService;
        _recipeService = recipeService;
    }

    [HttpGet("{date}")]
    public async Task<ActionResult<DiaryEntryResponse>> GetDiary(DateOnly date) {
        var userGuid = UserGuid;
        // nasty conversion until JSON parsing of the new DateOnly gets better in .NET
        var diary = await _diaryService.GetDiary(date, userGuid);
        if (diary == null)
            return NoContent();
        return Ok(diary);
    }
    
    [HttpDelete("{date}")]
    public async Task<ActionResult> DeleteDiary(DateOnly date) {
        return await _diaryService.DeleteDiary(date, UserGuid) == 1 ? Ok() : BadRequest();
    }
    
    [HttpPost("{date}/portions")]
    public async Task<IActionResult> AddPortions([FromBody] PortionRequest[] portionRequests, [FromRoute] DateOnly date) {

        var userGuid = UserGuid;
        // create a daily entry when it's missing to meet the foreign key constraints
        await _diaryService.CreateDailyEntry(date, userGuid);
        
        // gather all referenced foods and ascertain they are accessible by the user; can't use hashsets
        var foodsIds = portionRequests.Select(request => request.FoodId).Distinct().ToArray();
        var foods = (await _foodService.GetFoods(foodsIds, userGuid)).ToArray();

        // don't proceed with the insertion when even one food reference is missing
        if (foods.Length != foodsIds.Length)
            return BadRequest("Missing foods references.");

        return Ok( new {
            foods,
            portions = await _diaryService.AddPortions(portionRequests, date, userGuid)
        });
    }
    
    [HttpPost("{date}/recipes")]
    public async Task<IActionResult> AddRecipePortions([FromBody] AddRecipePortionsRequest addRecipePortionsRequest, [FromRoute] DateOnly date) {

        var userGuid = UserGuid;
        // create a daily entry when it's missing to meet the foreign key constraints
        await _diaryService.CreateDailyEntry(date, userGuid);
        
        // fetch recipe
        var recipe = await _recipeService.GetRecipe(addRecipePortionsRequest.Id, userGuid);
        if (recipe == null)
            return NotFound();
        
        // create single portion requests, to piggy back on existing service methods
        var portionRequests = new List<PortionRequest>();
        var foodsIds = new List<Guid>();
        foreach (var ingredient in recipe.Ingredients) {
            portionRequests.Add(new PortionRequest {
                    Id = Guid.NewGuid(),
                    FoodId = ingredient.Food.Id,
                    Quantity = ingredient.Quantity,
                    Meal = addRecipePortionsRequest.Meal
                } );
            foodsIds.Add(ingredient.Food.Id);
        }
        
        // gather all referenced foods and ascertain they are accessible by the user; can't use hashsets
        var foods = (await _foodService.GetFoods(foodsIds, userGuid)).ToArray();

        // don't proceed with the insertion when even one food reference is missing
        if (foods.Length != foodsIds.Count)
            return BadRequest("Missing foods references");

        return Ok( new {
            foods,
            portions = await _diaryService.AddPortions(portionRequests, date, userGuid)
        });
    }

    [HttpPut("{date}/portions/{id:guid}")]
    public async Task<ActionResult> UpdatePortion(DateOnly date, Guid id, PortionRequest portionRequest) {
        
        // fetch the portion's details first and check whether the user is authorised to change it
        var portion = await _diaryService.GetPortion(id);

        if (portion == null) 
            return NotFound();

        if (portion.UserId != UserGuid)
            return Unauthorized();

        if (DateOnly.FromDateTime(portion.Date) != date)
            return BadRequest();

        await _diaryService.UpdatePortion(portion, portionRequest);
        return NoContent();
    }
    
    [HttpDelete("{date}/portions")]
    public async Task<ActionResult> RemovePortions(DateOnly date, [FromQuery] Guid[] ids) {
        var deletedRows = await _diaryService.DeletePortions(UserGuid, ids);
        if (deletedRows > 0)
            return Ok();
        
        return BadRequest();
    }

    [HttpPut("{date}/mood")]
    public async Task<ActionResult> UpdateMood(DateOnly date, MoodPutRequest request) {
        // create a daily entry when it's missing to meet the foreign key constraints
        await _diaryService.UpdateMood(date, UserGuid, request.Mood);
        return Ok();
    }
    
    [HttpPut("{date}/fitness")]
    public async Task<ActionResult> UpdateFitness(DateOnly date, FitnessPutRequest request) {
        // create a daily entry when it's missing to meet the foreign key constraints
        await _diaryService.UpdateFitness(date, UserGuid, request.Fitness);
        return Ok();
    }

    [HttpPost("weight")]
    public async Task<ActionResult> AddWeightMeasurement(WeightReportResponse request) {
        await _diaryService.AddWeightMeasurement(UserGuid, request);
        return Ok();
    }

    [HttpGet("{date}/weight-report")]
    public async Task<ActionResult> GetWeightReport(DateOnly date) {

        return Ok();
    }
}