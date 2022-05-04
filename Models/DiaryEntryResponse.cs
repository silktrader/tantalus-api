using System.ComponentModel.DataAnnotations;

namespace Tantalus.Models; 

public sealed record DiaryEntryResponse {
    public IEnumerable<FoodResponse> Foods { get; init; }
    public IEnumerable<PortionResponse> Portions { get; init; }
    public string? Comment { get; init; }
    public short Mood { get; init; }
    public short Fitness { get; init; }
}

public sealed record MoodPutRequest([Required, Range(1, 5)] short Mood);
public sealed record FitnessPutRequest([Required, Range(1, 5)] short Fitness);