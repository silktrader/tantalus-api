using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Tantalus.Models;
using Tantalus.Services;

namespace Controllers; 

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class WeightController: TantalusController {
    private readonly IWeightService _weightService;

    public WeightController(IWeightService weightService) {
        _weightService = weightService;
    }
    
    [HttpGet]
    public async Task<ActionResult<AllWeightMeasurementsResponse>> GetWeightMeasurements([FromQuery] WeightStatRequest parameters) {
        return Ok(await _weightService.GetWeightMeasurements(UserGuid, parameters));
    }
    
}