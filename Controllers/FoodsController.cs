using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tantalus.Entities;
using Tantalus.Models;
using Tantalus.Services;

namespace Controllers; 

[Route("api/[controller]")]
[ApiController]
public class FoodsController : TantalusController {

    private readonly IFoodService _foodService;
    private readonly IMapper _mapper;

    public FoodsController(IFoodService foodService, IMapper mapper) {
        _foodService = foodService;
        _mapper = mapper;
    }
    
    [Authorize]
    [HttpPost]
    public async Task<ActionResult> AddFood(FoodAddRequest foodRequest) {
        var food = await _foodService.AddFood(foodRequest, UserGuid);
        return CreatedAtAction(nameof(GetFood), new {shortUrl = food.ShortUrl }, _mapper.Map<FoodResponse>(food));
    }
    
    [Authorize]
    [HttpPut("{foodId:guid}")]
    public async Task<ActionResult> UpdateFood(FoodUpdateRequest foodRequest, Guid foodId) {
        var food = await _foodService.GetFood(foodId);
        if (food == null)
            return NotFound();

        if (food.UserId != UserGuid && food.Visibility == Food.VisibleState.Private)
            return BadRequest();

        await _foodService.UpdateFood(foodRequest, food);
        return NoContent();
    }

    [Authorize]
    [HttpGet("{shortUrl:maxlength(50)}")]
    public async Task<ActionResult<FoodResponse>> GetFood(string shortUrl) {
        var food = await _foodService.GetFood(shortUrl);
        if (food == null) return NotFound();
        if (food.UserId != UserGuid) return Unauthorized();
        return Ok(_mapper.Map<FoodResponse>(food));
    }

    public record GetFoodsParameters(int PageIndex, int PageSize, string SortProperty, string SortOrder, string? NameFilter);
    
    [Authorize]
    public async Task<ActionResult> GetFoods([FromQuery] GetFoodsParameters parameters) {
        var foods = _mapper.Map<FoodResponse[]>(await _foodService.GetFoods(parameters, UserGuid));
        return Ok(new { foods, count = foods.Length});
    }

    [Authorize]
    [HttpDelete("{foodId:guid}")]
    public async Task<ActionResult<FoodResponse>> DeleteFood(Guid foodId) {
        return await _foodService.Delete(foodId, UserGuid) ? NoContent() : BadRequest();
    } 
}