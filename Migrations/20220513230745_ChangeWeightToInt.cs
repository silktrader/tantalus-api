using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tantalus.Migrations
{
    public partial class ChangeWeightToInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_weight_measurement",
                table: "weight_measurements");

            migrationBuilder.AlterColumn<int>(
                name: "weight",
                table: "weight_measurements",
                type: "integer",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddPrimaryKey(
                name: "pk_weight_measurements",
                table: "weight_measurements",
                columns: new[] { "user_id", "measured_on" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_weight_measurements",
                table: "weight_measurements");

            migrationBuilder.AlterColumn<short>(
                name: "weight",
                table: "weight_measurements",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "pk_weight_measurement",
                table: "weight_measurements",
                columns: new[] { "user_id", "measured_on" });
        }
    }
}
