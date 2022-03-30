using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tantalus.Entities;

namespace Tantalus.Data;

public class DataContext : DbContext {
    private readonly IConfiguration _configuration;
    
    static DataContext() => NpgsqlConnection.GlobalTypeMapper.MapEnum<RefreshToken.RevocationReason>();

    public DataContext(IConfiguration configuration) {
        _configuration = configuration;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseNpgsql(
            _configuration.GetConnectionString("Database") ?? throw new InvalidOperationException());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {

        modelBuilder.HasPostgresEnum<RefreshToken.RevocationReason>();
        
        modelBuilder.Entity<RefreshToken>(entity => {
            entity.HasKey(token => new {
                token.UserId, token.CreationDate
            });

            entity.Property(token => token.Value)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(token => token.ReplacedBy)
                .HasMaxLength(100);

            entity.Property(token => token.ExpiryDate)
                .IsRequired();

            entity.Property(token => token.CreationDate)
                .IsRequired();

            entity.HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity => {
            entity.Property(user => user.Name)
                .IsRequired()
                .HasMaxLength(16);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(254);

            entity.Property(user => user.FullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(user => user.HashedPassword)
                .IsRequired()
                .HasMaxLength(255);

            // must contain 32 bytes
            entity.Property(user => user.PasswordSalt)
                .IsRequired()
                .HasMaxLength(64);
        });

        modelBuilder.Entity<User>().HasIndex(user => user.Name).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(token => token.Value).IsUnique();
    }
}