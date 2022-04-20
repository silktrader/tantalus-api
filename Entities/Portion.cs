namespace Tantalus.Entities;

public class Portion {
    public Guid Id { get; init; }
    public Guid FoodId { get; init; }
    public Guid UserId { get; init; }
    public DateTime Date { get; private set; }
    public int Quantity { get; set; }
    public Meal Meal { get; set; }

    public DiaryEntry DiaryEntry { get; set; }
    public Food Food { get; set; }
}