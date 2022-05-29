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
    public SortAttributes Sort { get; init; }
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
}

public enum SortAttributes {
    MeasuredOn,
    Weight,
    Fat,
    SecondsAfter,
    WeightDifference,
    FatDifference
}

public enum SortDirection {
    Asc,
    Desc
}