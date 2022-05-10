using System.ComponentModel.DataAnnotations;

namespace Tantalus.Models; 

public sealed record GetStatsParameters {
    [Required, Range(10, 50)]
    public int Records { get; init; }

    [Required]
    public DateTime StartDate { get; init; }
    
    [Required]
    public DateTime EndDate { get; init; }

    public Guid[]? Included { get; init; }
}