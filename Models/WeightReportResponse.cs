namespace Tantalus.Models;

public sealed record WeightReportResponse {
    public int Weight { get; init; }                    // weight in grams
    public float? Fat { get; init; }                    // average body fat percentage
    public short Measurements { get; init; }
    public int PreviousWeightChange { get; init; }      // difference, in grams, compared to the previous day
    public float PreviousFatChange { get; init; }       // difference, in %, compared to the previous day
    public int Last30DaysWeightChange { get; init; }
    public float Last30DaysFatChange { get; init; }
}