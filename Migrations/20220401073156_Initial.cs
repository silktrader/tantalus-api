using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Tantalus.Entities;

#nullable disable

namespace Tantalus.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
                .Annotation("Npgsql:Enum:visible_state", "private,shared,editable");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    hashed_password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_salt = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "foods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    short_url = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    proteins = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    carbs = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    fats = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    fibres = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    sugar = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    starch = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    saturated = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    monounsaturated = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    polyunsaturated = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    trans = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    cholesterol = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    omega3 = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    omega6 = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    sodium = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    potassium = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    magnesium = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    calcium = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    zinc = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    iron = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    alcohol = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    source = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    visibility = table.Column<VisibleState>(type: "visible_state", nullable: false, defaultValue: VisibleState.Private),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_foods", x => x.id);
                    table.ForeignKey(
                        name: "fk_foods_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revocation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reason_revoked = table.Column<RefreshToken.RevocationReason>(type: "revocation_reason", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => new { x.user_id, x.creation_date });
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_foods_short_url",
                table: "foods",
                column: "short_url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_foods_user_id",
                table: "foods",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_value",
                table: "refresh_tokens",
                column: "value",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_name",
                table: "users",
                column: "name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "foods");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
