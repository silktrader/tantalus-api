using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tantalus.Migrations
{
    public partial class ReplaceDateOnlyWithDatetime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_portion_diary_entries_diary_entry_temp_id",
                table: "portion");

            migrationBuilder.DropForeignKey(
                name: "fk_portion_foods_food_id",
                table: "portion");

            migrationBuilder.DropPrimaryKey(
                name: "pk_portion",
                table: "portion");

            migrationBuilder.RenameTable(
                name: "portion",
                newName: "portions");

            migrationBuilder.RenameIndex(
                name: "ix_portion_food_id",
                table: "portions",
                newName: "ix_portions_food_id");

            migrationBuilder.RenameIndex(
                name: "ix_portion_date_user_id",
                table: "portions",
                newName: "ix_portions_date_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_portions",
                table: "portions",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_portions_diary_entries_diary_entry_temp_id",
                table: "portions",
                columns: new[] { "date", "user_id" },
                principalTable: "diary_entries",
                principalColumns: new[] { "date", "user_id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_portions_foods_food_id",
                table: "portions",
                column: "food_id",
                principalTable: "foods",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_portions_diary_entries_diary_entry_temp_id",
                table: "portions");

            migrationBuilder.DropForeignKey(
                name: "fk_portions_foods_food_id",
                table: "portions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_portions",
                table: "portions");

            migrationBuilder.RenameTable(
                name: "portions",
                newName: "portion");

            migrationBuilder.RenameIndex(
                name: "ix_portions_food_id",
                table: "portion",
                newName: "ix_portion_food_id");

            migrationBuilder.RenameIndex(
                name: "ix_portions_date_user_id",
                table: "portion",
                newName: "ix_portion_date_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_portion",
                table: "portion",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_portion_diary_entries_diary_entry_temp_id",
                table: "portion",
                columns: new[] { "date", "user_id" },
                principalTable: "diary_entries",
                principalColumns: new[] { "date", "user_id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_portion_foods_food_id",
                table: "portion",
                column: "food_id",
                principalTable: "foods",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
