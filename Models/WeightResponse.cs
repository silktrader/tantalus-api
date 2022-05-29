using System.Text.Json.Serialization;

namespace Tantalus.Models; 

public record WeightResponse {
    [JsonIgnore]
    public int Total { get; init; }
    public DateTimeOffset MeasuredOn { get; init; }
    public int Weight { get; init; }           
    public float? Fat { get; init; }
    public string? Note { get; set; }
}

public record ContiguousWeightResponse : WeightResponse {
    public int SecondsAfter { get; init; }
    public int WeightDifference { get; init; }
    public float FatDifference { get; init; }
}