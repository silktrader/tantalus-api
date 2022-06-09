using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Tantalus.Entities;

#nullable disable

namespace Tantalus.Migrations
{
    public partial class AddExtraUserColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:access", "private,shared,editable")
                .Annotation("Npgsql:Enum:meal", "breakfast,morning,lunch,afternoon,dinner")
                .Annotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
                .Annotation("Npgsql:Enum:sex", "unspecified,male,female")
                .OldAnnotation("Npgsql:Enum:access", "private,shared,editable")
                .OldAnnotation("Npgsql:Enum:meal", "breakfast,morning,lunch,afternoon,dinner")
                .OldAnnotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor");

            migrationBuilder.AddColumn<DateTime>(
                name: "birth_date",
                table: "users",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "height",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Sex>(
                name: "sex",
                table: "users",
                type: "sex",
                nullable: false,
                defaultValue: Sex.Unspecified);

            migrationBuilder.AddColumn<int>(
                name: "step_length",
                table: "users",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "birth_date",
                table: "users");

            migrationBuilder.DropColumn(
                name: "height",
                table: "users");

            migrationBuilder.DropColumn(
                name: "sex",
                table: "users");

            migrationBuilder.DropColumn(
                name: "step_length",
                table: "users");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:access", "private,shared,editable")
                .Annotation("Npgsql:Enum:meal", "breakfast,morning,lunch,afternoon,dinner")
                .Annotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
                .OldAnnotation("Npgsql:Enum:access", "private,shared,editable")
                .OldAnnotation("Npgsql:Enum:meal", "breakfast,morning,lunch,afternoon,dinner")
                .OldAnnotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
                .OldAnnotation("Npgsql:Enum:sex", "unspecified,male,female");
        }
    }
}
