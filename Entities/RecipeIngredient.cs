namespace Tantalus.Entities; 

public class RecipeIngredient {
    public Guid FoodId { get; init; }
    public Guid RecipeId { get; init; }
    public int Quantity { get; set; }

    public Food Food { get; set; }
    public Recipe Recipe { get; set; }
}