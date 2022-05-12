using System.ComponentModel.DataAnnotations;
using Tantalus.Entities;

namespace Tantalus.Models;

public sealed record AddRecipePortionsRequest {
    [Required]
    public Guid Id { get; init; }
    
    [Required]
    public Meal Meal { get; init; }
}