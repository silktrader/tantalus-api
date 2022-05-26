namespace Tantalus.Models; 

public sealed record AllWeightMeasurementsResponse {
    public IEnumerable<WeightMeasurementResponse> Measurements { get; init; }
    public int Count { get; init; }
}

public sealed record WeightMeasurementResponse {
    public DateTime MeasuredOn { get; init; }
    public int Weight { get; init; }           
    public float? Fat { get; init; }
    public string? Note { get; set; }
}