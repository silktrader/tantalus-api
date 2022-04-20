using System.Collections.Immutable;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tantalus.Models;
using Tantalus.Services;

namespace Controllers; 

[Route("api")]
[Authorize]
[ApiController]
public class DiaryController : TantalusController {
    
    private readonly IMapper _mapper;
    private readonly IDiaryService _diaryService;
    private readonly IFoodService _foodService;

    public DiaryController(IMapper mapper, IDiaryService diaryService, IFoodService foodService) {
        _mapper = mapper;
        _diaryService = diaryService;
        _foodService = foodService;
    }

    [HttpGet("{date:datetime}")]
    public async Task<ActionResult<DiaryEntryResponse>> GetDiary(DateOnly date) {
        var userGuid = UserGuid;
        // nasty conversion until JSON parsing of the new DateOnly gets better in .NET
        var diary = await _diaryService.GetDiary(date, userGuid);
        if (diary == null)
            return NoContent();
        return Ok(diary);
    }
    
    [HttpPost("{date}/portions")]
    public async Task<IActionResult> AddPortions([FromBody] PortionRequest[] portionRequests, [FromRoute] DateOnly date) {

        var userGuid = UserGuid;
        // create a daily entry when it's missing to meet the foreign key constraints
        await _diaryService.CreateDailyEntry(date, userGuid);
        
        // gather all referenced foods and ascertain they are accessible by the user
        var foodsIds = portionRequests.Select(request => request.FoodId).ToImmutableHashSet();
        var foods = await _foodService.GetFoods(foodsIds, userGuid);

        // don't proceed with the insertion when even one food reference is missing
        if (foods.Count() != foodsIds.Count)
            return BadRequest();

        await _diaryService.AddPortions(portionRequests, date, userGuid);
        return Ok(); // tk created at
        
       
    }
}