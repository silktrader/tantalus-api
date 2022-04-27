using System.ComponentModel.DataAnnotations;
using Tantalus.Entities;

namespace Tantalus.Models;

public abstract record FoodRequest {
    // macronutrients
    public float? Proteins { get; set; }
    public float? Carbs { get; set; }
    public float? Fats { get; set; }

    // carbohydrates
    public float? Fibres { get; set; }
    public float? Sugar { get; set; }
    public float? Starch { get; set; }

    // fats
    public float? Saturated { get; set; }
    public float? Monounsaturated { get; set; }
    public float? Polyunsaturated { get; set; }
    public float? Trans { get; set; }
    public float? Cholesterol { get; set; }
    public float? Omega3 { get; set; }
    public float? Omega6 { get; set; }

    // minerals
    public float? Sodium { get; set; }
    public float? Potassium { get; set; }
    public float? Magnesium { get; set; }
    public float? Calcium { get; set; }
    public float? Zinc { get; set; }
    public float? Iron { get; set; }

    public float? Alcohol { get; set; }

    public string? Source { get; set; }
    public string? Notes { get; set; }
}

public sealed record FoodAddRequest : FoodRequest {
    [Required] public string Name { get; init; }
    [Required] public VisibleState Visibility { get; init; }
}

public sealed record FoodUpdateRequest : FoodRequest {
    [Required] public Guid Id { get; init; }
    public string? Name { get; init; }
    public VisibleState? Visibility { get; init; }
}