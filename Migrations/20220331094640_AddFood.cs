using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tantalus.Migrations
{
    /// <inheritdoc />
    public partial class AddFood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
                .Annotation("Npgsql:Enum:visible_state", "private,shared,editable")
                .OldAnnotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor");

            migrationBuilder.CreateTable(
                name: "Foods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortUrl = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    Proteins = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Carbs = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Fats = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Fibres = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Sugar = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Starch = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Saturated = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Monounsaturated = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Polyunsaturated = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Trans = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Cholesterol = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Omega3 = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Omega6 = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Sodium = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Potassium = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Magnesium = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Calcium = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Zinc = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Iron = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Alcohol = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Foods_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Foods_ShortUrl",
                table: "Foods",
                column: "ShortUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Foods_UserId",
                table: "Foods",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Foods");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
                .OldAnnotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
                .OldAnnotation("Npgsql:Enum:visible_state", "private,shared,editable");
        }
    }
}
