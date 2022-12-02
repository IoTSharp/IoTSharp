using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Migrations
{
    public partial class ModifyEmailSpell : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EMail",
                table: "Tenant",
                newName: "Email");

            migrationBuilder.AddColumn<string>(
                name: "FlowClass",
                table: "Flows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlowIcon",
                table: "Flows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlowNameSpace",
                table: "Flows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlowShapeType",
                table: "Flows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlowTag",
                table: "Flows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Left",
                table: "Flows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Top",
                table: "Flows",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlowClass",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "FlowIcon",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "FlowNameSpace",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "FlowShapeType",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "FlowTag",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "Left",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "Top",
                table: "Flows");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Tenant",
                newName: "EMail");
        }
    }
}
