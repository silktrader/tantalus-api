using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tantalus.Entities;

namespace Tantalus.Data;

public class DataContext : DbContext {
    private readonly IConfiguration _configuration;

    static DataContext() {
        NpgsqlConnection.GlobalTypeMapper.MapEnum<RefreshToken.RevocationReason>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Food.VisibleState>();
    }

    public DataContext(IConfiguration configuration) {
        _configuration = configuration;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Food> Foods { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseNpgsql(
                _configuration.GetConnectionString("Database") ?? throw new InvalidOperationException())
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.HasPostgresEnum<RefreshToken.RevocationReason>();
        builder.HasPostgresEnum<Food.VisibleState>();

        builder.Entity<User>(entity => {
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
        builder.Entity<User>().HasIndex(user => user.Name).IsUnique();

        builder.Entity<RefreshToken>(entity => {
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
        builder.Entity<RefreshToken>().HasIndex(token => token.Value).IsUnique();

        builder.Entity<Food>(entity => {
            entity.Property(food => food.FullName).IsRequired().HasMaxLength(100);
            entity.Property(food => food.ShortUrl).HasMaxLength(50);

            entity.Property(food => food.Proteins).HasDefaultValue(0);
            entity.Property(food => food.Carbs).HasDefaultValue(0);
            entity.Property(food => food.Fats).HasDefaultValue(0);

            entity.Property(food => food.Fibres).HasDefaultValue(0);
            entity.Property(food => food.Sugar).HasDefaultValue(0);
            entity.Property(food => food.Starch).HasDefaultValue(0);

            entity.Property(food => food.Saturated).HasDefaultValue(0);
            entity.Property(food => food.Monounsaturated).HasDefaultValue(0);
            entity.Property(food => food.Polyunsaturated).HasDefaultValue(0);
            entity.Property(food => food.Trans).HasDefaultValue(0);
            entity.Property(food => food.Cholesterol).HasDefaultValue(0);
            entity.Property(food => food.Omega3).HasDefaultValue(0);
            entity.Property(food => food.Omega6).HasDefaultValue(0);

            entity.Property(food => food.Sodium).HasDefaultValue(0);
            entity.Property(food => food.Potassium).HasDefaultValue(0);
            entity.Property(food => food.Magnesium).HasDefaultValue(0);
            entity.Property(food => food.Calcium).HasDefaultValue(0);
            entity.Property(food => food.Zinc).HasDefaultValue(0);
            entity.Property(food => food.Iron).HasDefaultValue(0);
            entity.Property(food => food.Alcohol).HasDefaultValue(0);

            entity.Property(food => food.Created).IsRequired().HasDefaultValueSql("NOW()");

            entity.Property(food => food.Visibility).IsRequired().HasDefaultValue(Food.VisibleState.Private);

            entity.HasOne(food => food.User)
                .WithMany(user => user.Foods)
                .HasForeignKey(food => food.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        builder.Entity<Food>().HasIndex(food => food.ShortUrl).IsUnique();
    }
}