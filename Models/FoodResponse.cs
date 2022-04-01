﻿namespace Tantalus.Models; 

public record FoodResponse {
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string ShortUrl { get; set; }

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

    public Entities.Food.VisibleState Visibility { get; set; }
    public DateTime Created { get; set; }
}