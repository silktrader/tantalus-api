namespace Tantalus.Entities; 

    public class Recipe {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Name { get; set; }
        public string? Notes { get; set; }
        public Access Access { get; set; }
        public DateTime Created { get; init; }

        public User User { get; set; }
        public List<RecipeIngredient> Ingredients { get; set; } = new();
    }
