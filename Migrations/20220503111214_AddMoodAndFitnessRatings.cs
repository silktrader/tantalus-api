using Microsoft.EntityFrameworkCore.Migrations;
using Tantalus.Entities;

#nullable disable

namespace Tantalus.Migrations
{
    public partial class AddMoodAndFitnessRatings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "visibility",
            //     table: "foods");

            // migrationBuilder.AlterDatabase()
            //     .Annotation("Npgsql:Enum:access", "private,shared,editable")
            //     .Annotation("Npgsql:Enum:meal", "breakfast,morning,lunch,afternoon,dinner")
            //     .Annotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
            //     .OldAnnotation("Npgsql:Enum:meal", "breakfast,morning,lunch,afternoon,dinner")
            //     .OldAnnotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
            //     .OldAnnotation("Npgsql:Enum:visible_state", "private,shared,editable");
            //
            // migrationBuilder.AlterColumn<Access>(
            //     name: "access",
            //     table: "recipes",
            //     type: "access",
            //     nullable: false,
            //     defaultValue: Access.Private,
            //     oldClrType: typeof(Access),
            //     oldType: "visible_state",
            //     oldDefaultValue: Access.Private);
            //
            // migrationBuilder.AddColumn<Access>(
            //     name: "access",
            //     table: "foods",
            //     type: "access",
            //     nullable: false,
            //     defaultValue: Access.Private);

            migrationBuilder.AddColumn<short>(
                name: "fitness",
                table: "diary_entries",
                type: "smallint",
                nullable: false,
                defaultValue: (short)3);

            migrationBuilder.AddColumn<short>(
                name: "mood",
                table: "diary_entries",
                type: "smallint",
                nullable: false,
                defaultValue: (short)3);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "access",
            //     table: "foods");

            migrationBuilder.DropColumn(
                name: "fitness",
                table: "diary_entries");

            migrationBuilder.DropColumn(
                name: "mood",
                table: "diary_entries");

            // migrationBuilder.AlterDatabase()
            //     .Annotation("Npgsql:Enum:meal", "breakfast,morning,lunch,afternoon,dinner")
            //     .Annotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor")
            //     .Annotation("Npgsql:Enum:visible_state", "private,shared,editable")
            //     .OldAnnotation("Npgsql:Enum:access", "private,shared,editable")
            //     .OldAnnotation("Npgsql:Enum:meal", "breakfast,morning,lunch,afternoon,dinner")
            //     .OldAnnotation("Npgsql:Enum:revocation_reason", "replaced,manual,revoked_ancestor");
            //
            // migrationBuilder.AlterColumn<Access>(
            //     name: "access",
            //     table: "recipes",
            //     type: "visible_state",
            //     nullable: false,
            //     defaultValue: Access.Private,
            //     oldClrType: typeof(Access),
            //     oldType: "access",
            //     oldDefaultValue: Access.Private);
            //
            // migrationBuilder.AddColumn<Access>(
            //     name: "visibility",
            //     table: "foods",
            //     type: "visible_state",
            //     nullable: false,
            //     defaultValue: Access.Private);
        }
    }
}
