using System.ComponentModel.DataAnnotations;

namespace Tantalus.Models; 

public sealed record GetStatsParameters {
    [Required, Range(10, 30)]
    public int Records { get; init; }
}