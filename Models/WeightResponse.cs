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
    public int WeightChange { get; init; }
    public float FatChange { get; init; }
}

public record WeightMonthlyChange {
    [JsonIgnore]
    public int Total { get; init; }
    [JsonIgnore]
    public DateTime Period { get; init; }
    
    public DateOnly Month => DateOnly.FromDateTime(Period);
    public int? Weight { get; init; }
    public int? WeightChange { get; init; }
    public float? Fat { get; init; }
    public float? FatChange { get; init; }
    public int RecordedMeasures { get; init; }
    public int MonthlyAvgCalories { get; init; }
    public int CaloriesChange { get; init; }
    public int RecordedDays { get; init; }
}