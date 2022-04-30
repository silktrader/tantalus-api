namespace Tantalus.Models; 

public record GetFoodStatsResponse {
    public IEnumerable<FrequentFood> FrequentFoods { get; init; }
}

public record FrequentFood {
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Url { get; init; }
    public int Frequency { get; init; }
}