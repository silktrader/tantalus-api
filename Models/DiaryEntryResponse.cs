namespace Tantalus.Models; 

public sealed record DiaryEntryResponse {
    public IEnumerable<FoodResponse> Foods { get; init; }
    public IEnumerable<PortionResponse> Portions { get; init; }
    public string? Comment { get; init; }
}