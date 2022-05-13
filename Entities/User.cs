namespace Tantalus.Entities;

public class User {
    public User(Guid id, string name, string fullName, string email, string hashedPassword, string passwordSalt, DateTime creationDate) {
        Id = id;
        Name = name;
        FullName = fullName;
        Email = email;
        HashedPassword = hashedPassword;
        PasswordSalt = passwordSalt;
        CreationDate = creationDate;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string HashedPassword { get; set; }
    public string PasswordSalt { get; set; }
    public DateTime CreationDate { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public List<Food> Foods { get; set; } = new();
    public List<Recipe> Recipes { get; set; } = new();
    public List<WeightMeasurement> WeightMeasurements { get; set; } = new();
}
