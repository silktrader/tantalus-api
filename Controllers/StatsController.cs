using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tantalus.Models;
using Tantalus.Services;

namespace Controllers; 

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class StatsController : TantalusController {
    
    private readonly IStatService _statService;

    public StatsController(IStatService statService) {
        _statService = statService;
    }

    [HttpGet("mood/high-mood-foods")]
    public async Task<ActionResult<MoodFoodsResponse>> GetHighMoodFoods([FromQuery] GetStatsParameters parameters) {
        return Ok(await _statService.GetMoodFoods(UserGuid, parameters, high: true));
    }
    
    [HttpGet("mood/low-mood-foods")]
    public async Task<ActionResult<MoodFoodsResponse>> GetLowMoodFoods([FromQuery] GetStatsParameters parameters) {
        return Ok(await _statService.GetMoodFoods(UserGuid, parameters, high: false));
    }
    
    [HttpGet("mood/mood-per-caloric-range")]
    public async Task<ActionResult<MoodPerCaloricRange>> GetMoodPerCaloricRange([FromQuery] GetStatsParameters parameters) {
        return Ok(await _statService.GetMoodPerCaloricRange(UserGuid, parameters));
    }
    
    [HttpGet("mood/foods-highest-average-mood")]
    public async Task<ActionResult<MoodFoodsResponse>> GetFoodsHighestAverageMood([FromQuery] GetStatsParameters parameters) {
        return Ok(await _statService.GetFoodsAverageMood(UserGuid, parameters, highest: true));
    }
    
    [HttpGet("mood/foods-lowest-average-mood")]
    public async Task<ActionResult<MoodFoodsResponse>> GetFoodsLowestAverageMood([FromQuery] GetStatsParameters parameters) {
        return Ok(await _statService.GetFoodsAverageMood(UserGuid, parameters, highest: false));
    }
    
}