namespace Controllers;

public record PortionResourceResponse {
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public int Priority { get; init; }
    public bool? IsRecipe { get; init; }
}