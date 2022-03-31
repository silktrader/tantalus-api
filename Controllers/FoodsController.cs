using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tantalus.Models;
using Tantalus.Services;

namespace Controllers; 

[Route("api/[controller]")]
[ApiController]
public class FoodsController : TantalusController {

    private readonly IFoodService _foodService;

    public FoodsController(IFoodService foodService) {
        _foodService = foodService;
    }
    
    [Authorize]
    [HttpPost]
    public async Task<ActionResult> AddFood(FoodRequest foodRequest) {
        var response = await _foodService.AddFood(foodRequest, UserGuid);
        return Ok(response);        // tk add createdAt
    }
}