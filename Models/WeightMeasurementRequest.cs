using System.ComponentModel.DataAnnotations;

namespace Tantalus.Models;

public sealed record WeightMeasurementRequest {
    [Required, Range(10000, 300000)]
    public short Weight { get; init; }
    public DateTime MeasuredOn { get; init; }
    [Required, Range(1, 1000)]
    public short? Impedance { get; init; }
    public string? Note { get; init; }
}