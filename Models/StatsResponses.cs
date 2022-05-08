namespace Tantalus.Models; 

public record MoodFoodsResponse {
    public IEnumerable<MoodFood> Foods { get; init; }
}

public sealed record MoodFood {
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string ShortUrl { get; init; }
    public int Total { get; init; }
    public float Percent { get; init; }
} 

public sealed record MoodPerCaloricRange {
    public IEnumerable<CaloricRange> Ranges { get; init; }
}

public sealed record CaloricRange {
    public int LowerLimit { get; init; }
    public int UpperLimit { get; init; }
    public float AverageMood { get; init; }
}