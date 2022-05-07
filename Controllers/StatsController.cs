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
    public async Task<ActionResult<HighMoodFoodsResponse>> GetHighMoodFoods([FromQuery] GetStatsParameters parameters) {
        return Ok(await _statService.GetHighMoodFoods(UserGuid, parameters));
    }
    
}