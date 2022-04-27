using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Tantalus.Entities;

#nullable disable

namespace Tantalus.Migrations
{
    public partial class AddRecipes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recipes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    access = table.Column<Access>(type: "visible_state", nullable: false, defaultValue: Access.Private),
                    created = table.Column<DateTime>(type: "Date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipes", x => x.id);
                    table.ForeignKey(
                        name: "fk_recipes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "recipe_ingredients",
                columns: table => new
                {
                    food_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipe_ingredients", x => new { x.recipe_id, x.food_id });
                    table.ForeignKey(
                        name: "fk_recipe_ingredients_foods_food_id",
                        column: x => x.food_id,
                        principalTable: "foods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_recipe_ingredients_recipes_recipe_id",
                        column: x => x.recipe_id,
                        principalTable: "recipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_recipe_ingredients_food_id",
                table: "recipe_ingredients",
                column: "food_id");

            migrationBuilder.CreateIndex(
                name: "ix_recipes_user_id",
                table: "recipes",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recipe_ingredients");

            migrationBuilder.DropTable(
                name: "recipes");
        }
    }
}
