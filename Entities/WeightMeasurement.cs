namespace Tantalus.Entities; 

public sealed class WeightMeasurement {
    public Guid UserId { get; set; }
    public DateTime MeasuredOn { get; init; }
    public int Weight { get; init; }           // grams
    public short? Impedance { get; init; }     // ohms
    public float? Fat { get; init; }           // percentage
    public string? Note { get; set; }
    public User User { get; }
}