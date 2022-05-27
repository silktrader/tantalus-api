namespace Tantalus.Models; 

public sealed record WeightUpdateRequest {
    public DateTimeOffset MeasuredOn { get; init; }
    public int Weight { get; init; }
    public float? Fat { get; init; }
    public string? Note { get; init; }
}