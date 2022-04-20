﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Tantalus.Data;
using Tantalus.Entities;

#nullable disable

namespace Tantalus.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20220417182946_AddDiaryEntryAndPortion")]
    partial class AddDiaryEntryAndPortion
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "meal", new[] { "breakfast", "morning", "lunch", "afternoon", "dinner" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "revocation_reason", new[] { "replaced", "manual", "revoked_ancestor" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "visible_state", new[] { "private", "shared", "editable" });
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Tantalus.Entities.DiaryEntry", b =>
                {
                    b.Property<DateOnly>("Date")
                        .HasColumnType("date")
                        .HasColumnName("date");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("comment");

                    b.HasKey("Date", "UserId")
                        .HasName("pk_diary_entries");

                    b.ToTable("diary_entries", (string)null);
                });

            modelBuilder.Entity("Tantalus.Entities.Food", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<float?>("Alcohol")
                        .HasColumnType("real")
                        .HasColumnName("alcohol");

                    b.Property<float?>("Calcium")
                        .HasColumnType("real")
                        .HasColumnName("calcium");

                    b.Property<float>("Carbs")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(0f)
                        .HasColumnName("carbs");

                    b.Property<float?>("Cholesterol")
                        .HasColumnType("real")
                        .HasColumnName("cholesterol");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created")
                        .HasDefaultValueSql("NOW()");

                    b.Property<float>("Fats")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(0f)
                        .HasColumnName("fats");

                    b.Property<float?>("Fibres")
                        .HasColumnType("real")
                        .HasColumnName("fibres");

                    b.Property<float?>("Iron")
                        .HasColumnType("real")
                        .HasColumnName("iron");

                    b.Property<float?>("Magnesium")
                        .HasColumnType("real")
                        .HasColumnName("magnesium");

                    b.Property<float?>("Monounsaturated")
                        .HasColumnType("real")
                        .HasColumnName("monounsaturated");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.Property<string>("Notes")
                        .HasColumnType("text")
                        .HasColumnName("notes");

                    b.Property<float?>("Omega3")
                        .HasColumnType("real")
                        .HasColumnName("omega3");

                    b.Property<float?>("Omega6")
                        .HasColumnType("real")
                        .HasColumnName("omega6");

                    b.Property<float?>("Polyunsaturated")
                        .HasColumnType("real")
                        .HasColumnName("polyunsaturated");

                    b.Property<float?>("Potassium")
                        .HasColumnType("real")
                        .HasColumnName("potassium");

                    b.Property<float>("Proteins")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(0f)
                        .HasColumnName("proteins");

                    b.Property<float?>("Saturated")
                        .HasColumnType("real")
                        .HasColumnName("saturated");

                    b.Property<string>("ShortUrl")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("short_url");

                    b.Property<float?>("Sodium")
                        .HasColumnType("real")
                        .HasColumnName("sodium");

                    b.Property<string>("Source")
                        .HasColumnType("text")
                        .HasColumnName("source");

                    b.Property<float?>("Starch")
                        .HasColumnType("real")
                        .HasColumnName("starch");

                    b.Property<float?>("Sugar")
                        .HasColumnType("real")
                        .HasColumnName("sugar");

                    b.Property<float?>("Trans")
                        .HasColumnType("real")
                        .HasColumnName("trans");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<Food.VisibleState>("Visibility")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("visible_state")
                        .HasDefaultValue(Food.VisibleState.Private)
                        .HasColumnName("visibility");

                    b.Property<float?>("Zinc")
                        .HasColumnType("real")
                        .HasColumnName("zinc");

                    b.HasKey("Id")
                        .HasName("pk_foods");

                    b.HasIndex("ShortUrl")
                        .IsUnique()
                        .HasDatabaseName("ix_foods_short_url");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_foods_user_id");

                    b.ToTable("foods", (string)null);
                });

            modelBuilder.Entity("Tantalus.Entities.Portion", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date")
                        .HasColumnName("date");

                    b.Property<Guid>("FoodId")
                        .HasColumnType("uuid")
                        .HasColumnName("food_id");

                    b.Property<Meal>("Meal")
                        .HasColumnType("meal")
                        .HasColumnName("meal");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer")
                        .HasColumnName("quantity");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_portion");

                    b.HasIndex("FoodId")
                        .HasDatabaseName("ix_portion_food_id");

                    b.HasIndex("Date", "UserId")
                        .HasDatabaseName("ix_portion_date_user_id");

                    b.ToTable("portion", (string)null);
                });

            modelBuilder.Entity("Tantalus.Entities.RefreshToken", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_date");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expiry_date");

                    b.Property<RefreshToken.RevocationReason?>("ReasonRevoked")
                        .HasColumnType("revocation_reason")
                        .HasColumnName("reason_revoked");

                    b.Property<string>("ReplacedBy")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("replaced_by");

                    b.Property<DateTime?>("RevocationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("revocation_date");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("value");

                    b.HasKey("UserId", "CreationDate")
                        .HasName("pk_refresh_tokens");

                    b.HasIndex("Value")
                        .IsUnique()
                        .HasDatabaseName("ix_refresh_tokens_value");

                    b.ToTable("refresh_tokens", (string)null);
                });

            modelBuilder.Entity("Tantalus.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_date");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(254)
                        .HasColumnType("character varying(254)")
                        .HasColumnName("email");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("full_name");

                    b.Property<string>("HashedPassword")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("hashed_password");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("character varying(16)")
                        .HasColumnName("name");

                    b.Property<string>("PasswordSalt")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("password_salt");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_users_name");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("Tantalus.Entities.Food", b =>
                {
                    b.HasOne("Tantalus.Entities.User", "User")
                        .WithMany("Foods")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired()
                        .HasConstraintName("fk_foods_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Tantalus.Entities.Portion", b =>
                {
                    b.HasOne("Tantalus.Entities.Food", "Food")
                        .WithMany()
                        .HasForeignKey("FoodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_portion_foods_food_id");

                    b.HasOne("Tantalus.Entities.DiaryEntry", "DiaryEntry")
                        .WithMany("Portions")
                        .HasForeignKey("Date", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_portion_diary_entries_diary_entry_temp_id");

                    b.Navigation("DiaryEntry");

                    b.Navigation("Food");
                });

            modelBuilder.Entity("Tantalus.Entities.RefreshToken", b =>
                {
                    b.HasOne("Tantalus.Entities.User", "User")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_refresh_tokens_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Tantalus.Entities.DiaryEntry", b =>
                {
                    b.Navigation("Portions");
                });

            modelBuilder.Entity("Tantalus.Entities.User", b =>
                {
                    b.Navigation("Foods");

                    b.Navigation("RefreshTokens");
                });
#pragma warning restore 612, 618
        }
    }
}
