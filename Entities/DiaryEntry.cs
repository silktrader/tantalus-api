namespace Tantalus.Entities;

public class DiaryEntry {
    public DateTime Date { get; init; }
    public Guid UserId { get; init; }
    public string? Comment { get; set; }

    public User User { get; }
    public ICollection<Portion> Portions { get; set; } = new HashSet<Portion>();
}