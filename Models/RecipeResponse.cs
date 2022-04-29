using System.ComponentModel.DataAnnotations;

namespace Tantalus.Models; 

public record RecipeResponse {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Notes { get; set; }
        public List<IngredientResponse> Ingredients { get; set; }
}

public record IngredientResponse {
        public int Quantity { get; set; }
        public FoodResponse Food { get; set; }
}

public record RecipePostRequest {
        [Required] public Guid Id { get; init; }
        [Required, StringLength(50, MinimumLength = 5)] public string Name { get; init; }
        [Required] public List<IngredientPostRequest> Ingredients { get; init; }
}

public record IngredientPostRequest {
        [Required] public Guid FoodId { get; init; }
        [Required, Range(1, 999)] public int Quantity { get; init; }
}