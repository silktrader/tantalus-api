namespace Tantalus.Entities; 

public sealed class WeightMeasurement {
    public Guid UserId { get; init; }
    public DateTime MeasuredOn { get; init; }
    public short Weight { get; init; }          // measured in grams
    public short? Impedance { get; init; }      // measured in ohms
    public string? Note { get; set; }
    public User User { get; }
}