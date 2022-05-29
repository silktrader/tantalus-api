using System.Text.Json.Serialization;

namespace Tantalus.Models; 

public sealed record AllWeightsResponse {
    public IEnumerable<WeightResponse> Measurements { get; init; }
    public int Count { get; init; }
}

public record WeightResponse {
    public DateTimeOffset MeasuredOn { get; init; }
    public int Weight { get; init; }           
    public float? Fat { get; init; }
    public string? Note { get; set; }
}

public record ContiguousWeightResponse : WeightResponse {
    [JsonIgnore]
    public int Total { get; init; }
    public int SecondsAfter { get; init; }
    public int WeightDifference { get; init; }
    public float FatDifference { get; init; }
}