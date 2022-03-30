// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace Tantalus.Entities;

public class RefreshToken {
    public RefreshToken(string value, Guid userId, DateTime creationDate, DateTime expiryDate) {
        Value = value;
        UserId = userId;
        CreationDate = creationDate;
        ExpiryDate = expiryDate;
    }

    public string Value { get; set; }           // might be hashed in the future, don't use as key
    public User User { get; set; }
    public Guid UserId { get; set; }
    
    public DateTime CreationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? RevocationDate { get; set; }
    
    public string? ReplacedBy { get; set; }
    public RevocationReason? ReasonRevoked { get; set; }
    
    public bool HasExpired => DateTime.UtcNow >= ExpiryDate;
    public bool WasRevoked => RevocationDate != null;
    public bool IsActive => !WasRevoked && !HasExpired;
    
    public enum RevocationReason {
        Replaced,
        Manual,
        RevokedAncestor
    }
}