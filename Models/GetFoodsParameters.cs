using System.ComponentModel.DataAnnotations;

namespace Models;

public record GetFoodsParameters(
    [Range(0, int.MaxValue)] int PageIndex,
    [Range(10, 100)] int PageSize,
    [Required] FoodAttribute SortProperty,
    [Required] bool Ascending,
    string? NameFilter);
    