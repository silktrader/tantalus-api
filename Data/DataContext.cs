using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tantalus.Entities;

namespace Tantalus.Data;

public class DataContext : DbContext {
    private readonly IConfiguration _configuration;

    static DataContext() {
        NpgsqlConnection.GlobalTypeMapper.MapEnum<RefreshToken.RevocationReason>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Access>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Meal>();
    }

    public DataContext(IConfiguration configuration) {
        _configuration = configuration;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Food> Foods { get; set; } = null!;
    public DbSet<DiaryEntry> DiaryEntries { get; set; } = null!;
    public DbSet<Portion> Portions { get; set; } = null!;
    public DbSet<Recipe> Recipes { get; set; } = null!;
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; } = null!;
    public DbSet<WeightMeasurement> WeightMeasurements { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseNpgsql(
                _configuration.GetConnectionString("Database") ?? throw new InvalidOperationException())
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.HasPostgresEnum<RefreshToken.RevocationReason>();
        builder.HasPostgresEnum<Access>();

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
            
            entity.HasIndex(user => user.Name).IsUnique();
        });

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
            
            entity.HasIndex(token => token.Value).IsUnique();
        });

        builder.Entity<Food>(entity => {
            entity.Property(food => food.Name).IsRequired().HasMaxLength(100);
            entity.Property(food => food.ShortUrl).HasMaxLength(50);

            entity.Property(food => food.Proteins).HasDefaultValue(0);
            entity.Property(food => food.Carbs).HasDefaultValue(0);
            entity.Property(food => food.Fats).HasDefaultValue(0);

            entity.Property(food => food.Fibres).HasDefaultValue(null);
            entity.Property(food => food.Sugar).HasDefaultValue(null);
            entity.Property(food => food.Starch).HasDefaultValue(null);

            entity.Property(food => food.Saturated).HasDefaultValue(null);
            entity.Property(food => food.Monounsaturated).HasDefaultValue(null);
            entity.Property(food => food.Polyunsaturated).HasDefaultValue(null);
            entity.Property(food => food.Trans).HasDefaultValue(null);
            entity.Property(food => food.Cholesterol).HasDefaultValue(null);
            entity.Property(food => food.Omega3).HasDefaultValue(null);
            entity.Property(food => food.Omega6).HasDefaultValue(null);

            entity.Property(food => food.Sodium).HasDefaultValue(null);
            entity.Property(food => food.Potassium).HasDefaultValue(null);
            entity.Property(food => food.Magnesium).HasDefaultValue(null);
            entity.Property(food => food.Calcium).HasDefaultValue(null);
            entity.Property(food => food.Zinc).HasDefaultValue(null);
            entity.Property(food => food.Iron).HasDefaultValue(null);
            entity.Property(food => food.Alcohol).HasDefaultValue(null);

            entity.Property(food => food.Created).IsRequired().HasDefaultValueSql("NOW()");

            entity.Property(food => food.Access).IsRequired().HasDefaultValue(Access.Private);

            entity.HasOne(food => food.User)
                .WithMany(user => user.Foods)
                .HasForeignKey(food => food.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(food => food.ShortUrl).IsUnique();
        });

        builder.Entity<Portion>(entity => {
            entity.Property(portion => portion.Date).IsRequired().HasColumnType("date");
            // tk define BRIN index to migrate?
            entity.Property(portion => portion.UserId).IsRequired();
            entity.Property(portion => portion.FoodId).IsRequired();
            entity.Property(portion => portion.Quantity).IsRequired();
            entity.Property(portion => portion.Meal).IsRequired();
            entity.HasOne(portion => portion.DiaryEntry)
                .WithMany(entry => entry.Portions)
                .HasForeignKey(portion => new { DiaryEntryDate = portion.Date, DiaryEntryUserId = portion.UserId });
        });
        builder.HasPostgresEnum<Meal>();

        builder.Entity<DiaryEntry>(entity => {
            entity.HasKey(entry => new { entry.Date, entry.UserId });
            entity.Property(entry => entry.Date).IsRequired().HasColumnType("date");
            entity.Property(entry => entry.UserId).IsRequired();
            entity.Property(entry => entry.Mood).IsRequired().HasDefaultValue(3);
            entity.Property(entry => entry.Fitness).IsRequired().HasDefaultValue(3);
        });

        builder.Entity<Recipe>(entity => {
            entity.Property(recipe => recipe.UserId).IsRequired();
            entity.Property(recipe => recipe.Name).HasMaxLength(50).IsRequired();
            entity.Property(recipe => recipe.Created).IsRequired().HasColumnType("Date");
            entity.Property(recipe => recipe.Access).IsRequired().HasDefaultValue(Access.Private);
            entity.HasOne(recipe => recipe.User)
                .WithMany(user => user.Recipes)
                .HasForeignKey(recipe => recipe.UserId)
                .OnDelete(DeleteBehavior.SetNull);          // remember to retrieve orphans on user deletion tk
        });
        
        builder.Entity<RecipeIngredient>(entity => {
            entity.Property(ingredient => ingredient.Quantity).IsRequired();
            entity.HasKey(ingredient => new { ingredient.RecipeId, ingredient.FoodId });
            entity.HasOne(ingredient => ingredient.Recipe)
                .WithMany(recipe => recipe.Ingredients)
                .HasForeignKey(ingredient => ingredient.RecipeId);
            entity.HasOne(ingredient => ingredient.Food)
                .WithMany(food => food.Ingredients)
                .HasForeignKey(ingredient => ingredient.FoodId);
        });

        builder.Entity<WeightMeasurement>(entity => {
            entity.Property(measurement => measurement.MeasuredOn).IsRequired().HasColumnType("timestamptz");
            entity.Property(measurement => measurement.Weight).IsRequired();
            entity.HasKey(measurement => new { measurement.UserId, measurement.MeasuredOn });
            entity.HasOne(measurement => measurement.User)
                .WithMany(user => user.WeightMeasurements)
                .HasForeignKey(measurement => measurement.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}