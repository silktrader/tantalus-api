namespace Tantalus.Entities; 

public sealed class WeightMeasurement {
    public Guid UserId { get; set; }
    public DateTime MeasuredOn { get; init; }
    public int Weight { get; init; }          // measured in grams
    public short? Impedance { get; init; }      // measured in ohms
    public string? Note { get; set; }
    public User User { get; }
}