using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tantalus.Migrations
{
    public partial class AddWeightMeasurement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "weight_measurement",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    measured_on = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    weight = table.Column<short>(type: "smallint", nullable: false),
                    impedance = table.Column<short>(type: "smallint", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_weight_measurement", x => new { x.user_id, x.measured_on });
                    table.ForeignKey(
                        name: "fk_weight_measurement_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "weight_measurement");
        }
    }
}
