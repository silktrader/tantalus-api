using Tantalus.Entities;

namespace Tantalus.Models; 

public record GetFoodStatsResponse {
    public int Count { get; init; }
    public int Quantity { get; init; }
    public int Max { get; init; }
    public DateOnly? LastEaten { get; init; }
    public IEnumerable<FrequentFood> FrequentFoods { get; init; }
    public IEnumerable<FrequentMeal> FrequentMeals { get; init; }
    public IEnumerable<RecipeFoodStat> Recipes { get; init; }
}

public record FrequentFood {
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string ShortUrl { get; init; }
    public int Frequency { get; init; }
}

public record FrequentMeal {
    public int Frequency { get; init; }
    public Meal Meal { get; init; }
}

public record RecipeFoodStat {
    public Guid Id { get; init; }
    public string Name { get; init; }
    public int Quantity { get; init; }
}