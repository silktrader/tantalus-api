namespace Tantalus.Models; 

public record HighMoodFoodsResponse {
    public IEnumerable<HighMoodFood> Foods { get; init; }
}

public sealed record HighMoodFood {
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string ShortUrl { get; init; }
    public int Total { get; init; }
    public float Percent { get; init; }
} 