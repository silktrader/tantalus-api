using System.ComponentModel.DataAnnotations;

namespace Models;

public record PaginatedRequest {
    
    [Range(0, int.MaxValue)]
    public int PageIndex { get; init; }
    
    [Range(10, 100)] 
    public int PageSize { get; init; }
    
    [Required] 
    public SortDirection Direction { get; init; }
}

public sealed record WeightStatRequest : PaginatedRequest {
    public SortAttributes Sort { get; init; }               // Required won't work as the deserializer assigns a default value before validation
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
}

public enum SortAttributes {
    None,
    MeasuredOn,
    Weight,
    Fat,
    SecondsAfter,
    WeightChange,
    FatChange,
    Month,                 // used in monthly stats
    MonthlyAvgCalories,
    CaloriesChange,
    RecordedDays,           // used in weight monthly stats
    RecordedMeasures        // used in weight monthly stats
}

public enum SortDirection {
    Asc,
    Desc
}