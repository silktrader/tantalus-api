﻿using System.ComponentModel.DataAnnotations;

namespace Models;

public record PaginatedRequest {
    
    [Range(0, int.MaxValue)]
    public int PageIndex { get; init; }
    
    [Range(10, 100)] 
    public int PageSize { get; init; }
    
    [Required] 
    public SortDirection Direction { get; init; }
}

public record WeightStatRequest : PaginatedRequest {

    public WeightAttributes Sort { get; init; }
    
    public DateTime? Start { get; init; }
    public DateTime? End { get; init; }
}

public enum WeightAttributes {
    MeasuredOn,
    Weight,
    Fat
}

public enum SortDirection {
    Asc,
    Desc
}