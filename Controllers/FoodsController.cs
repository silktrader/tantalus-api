using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Tantalus.Entities;
using Tantalus.Models;
using Tantalus.Services;

namespace Controllers; 

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class FoodsController : TantalusController {

    private readonly IFoodService _foodService;
    private readonly IMapper _mapper;

    public FoodsController(IFoodService foodService, IMapper mapper) {
        _foodService = foodService;
        _mapper = mapper;
    }
    
    [HttpPost]
    public async Task<ActionResult> AddFood(FoodAddRequest foodRequest) {
        var food = await _foodService.AddFood(foodRequest, UserGuid);
        return CreatedAtAction(nameof(GetFood), new {shortUrl = food.ShortUrl }, _mapper.Map<FoodResponse>(food));
    }
    
    [HttpPut("{foodId:guid}")]
    public async Task<ActionResult> UpdateFood(FoodUpdateRequest foodRequest, Guid foodId) {
        var food = await _foodService.GetFood(foodId);
        if (food == null)
            return NotFound();

        if (food.UserId != UserGuid && food.Access == Access.Private)
            return BadRequest();

        await _foodService.UpdateFood(foodRequest, food);
        return NoContent();
    }

    [HttpGet("{shortUrl:maxlength(50)}")]
    public async Task<ActionResult<FoodResponse>> GetFood(string shortUrl) {
        var food = await _foodService.GetFood(shortUrl);
        if (food == null) return NotFound();
        if (food.UserId != UserGuid) return Unauthorized();
        return Ok(_mapper.Map<FoodResponse>(food));
    }

    [HttpGet]
    public async Task<ActionResult> GetFoods([FromQuery] GetFoodsParameters parameters) {
        var (foodsData, count) = await _foodService.GetFoods(parameters, UserGuid);
        var foods = _mapper.Map<FoodResponse[]>(foodsData);
        return Ok(new { foods, count});
    }

    [HttpDelete("{foodId:guid}")]
    public async Task<ActionResult<FoodResponse>> DeleteFood(Guid foodId) {
        return await _foodService.Delete(foodId, UserGuid) ? NoContent() : BadRequest();
    }

    [HttpGet("names")]
    public async Task<ActionResult<IEnumerable<PortionResourceResponse>>> GetNames(string filter) {
        if (string.IsNullOrEmpty(filter))
            return NoContent();

        return Ok(await _foodService.GetPortionResourceHints(filter, UserGuid, 5));
    }

    [HttpGet("{id:guid}/stats")]
    public async Task<ActionResult<GetFoodStatsResponse>> GetFoodStats(Guid id) {
        var userId = UserGuid;
        return (await _foodService.CanViewFood(id, userId)) ? await _foodService.GetFoodStats(id, userId) : NotFound();
    }
}