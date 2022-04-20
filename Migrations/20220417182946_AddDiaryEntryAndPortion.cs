using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Tantalus.Entities;

#nullable disable

namespace Tantalus.Migrations
{
    public partial class AddDiaryEntryAndPortion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:meal", "breakfast,morning,lunch,afternoon,dinner")
                .Annotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
                .Annotation("Npgsql:Enum:visible_state", "private,shared,editable")
                .OldAnnotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
                .OldAnnotation("Npgsql:Enum:visible_state", "private,shared,editable");

            migrationBuilder.CreateTable(
                name: "diary_entries",
                columns: table => new
                {
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_diary_entries", x => new { x.date, x.user_id });
                });

            migrationBuilder.CreateTable(
                name: "portion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    food_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    meal = table.Column<Meal>(type: "meal", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portion", x => x.id);
                    table.ForeignKey(
                        name: "fk_portion_diary_entries_diary_entry_temp_id",
                        columns: x => new { x.date, x.user_id },
                        principalTable: "diary_entries",
                        principalColumns: new[] { "date", "user_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_portion_foods_food_id",
                        column: x => x.food_id,
                        principalTable: "foods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_portion_date_user_id",
                table: "portion",
                columns: new[] { "date", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_portion_food_id",
                table: "portion",
                column: "food_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portion");

            migrationBuilder.DropTable(
                name: "diary_entries");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
                .Annotation("Npgsql:Enum:visible_state", "private,shared,editable")
                .OldAnnotation("Npgsql:Enum:meal", "breakfast,morning,lunch,afternoon,dinner")
                .OldAnnotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
                .OldAnnotation("Npgsql:Enum:visible_state", "private,shared,editable");
        }
    }
}
