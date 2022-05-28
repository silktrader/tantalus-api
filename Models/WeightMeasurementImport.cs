namespace Controllers;

public sealed record WeightMeasurementImport {
    public bool Overwrite { get; init; }
    public IFormFile Data { get; init; }
}