using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tantalus.Migrations
{
    /// <inheritdoc />
    public partial class RefactorRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Contents",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "ReplacedByToken",
                table: "RefreshTokens",
                newName: "ReplacedBy");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "RefreshTokens",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Value",
                table: "RefreshTokens",
                column: "Value",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_Value",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "ReplacedBy",
                table: "RefreshTokens",
                newName: "ReplacedByToken");

            migrationBuilder.AddColumn<string>(
                name: "Contents",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
