using Microsoft.EntityFrameworkCore.Migrations;
using Tantalus.Entities;

#nullable disable

namespace Tantalus.Migrations
{
    /// <inheritdoc />
    public partial class AddVisibleFoodState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql("alter table \"Foods\" drop column \"Visibility\"; " +
                                 "alter table \"Foods\" add column \"Visibility\" visible_state;" +
                                 "alter table \"Foods\" alter column \"Visibility\" set default 'private'::visible_state;");
            
            // the code below didn't work due an impossible automatic cast
            // migrationBuilder.AlterColumn<Food.VisibleState>(
            //     name: "Visibility",
            //     table: "Foods",
            //     type: "visible_state",
            //     nullable: false,
            //     defaultValue: Food.VisibleState.Private,
            //     oldClrType: typeof(int),
            //     oldType: "integer",
            //     oldDefaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Visibility",
                table: "Foods",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(Food.VisibleState),
                oldType: "visible_state",
                oldDefaultValue: Food.VisibleState.Private);
        }
    }
}
