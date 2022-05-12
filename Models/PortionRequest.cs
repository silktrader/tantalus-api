using System.ComponentModel.DataAnnotations;
using Tantalus.Entities;

namespace Tantalus.Models; 

public record PortionRequest {
    
    [Required]
    public Guid Id { get; init; }
    
    [Required]
    public Guid FoodId { get; init; }

    [Required, Range(0, 1000)]
    public int Quantity { get; init; }

    [Required] // tk not sufficient for deserialisation, look into it
    public Meal Meal { get; init; }
}

public record PortionResponse : PortionRequest;