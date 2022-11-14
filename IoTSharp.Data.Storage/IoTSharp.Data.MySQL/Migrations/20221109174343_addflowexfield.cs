using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.MySql.Migrations
{
    public partial class addflowexfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FlowClass",
                table: "Flows",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FlowIcon",
                table: "Flows",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FlowNameSpace",
                table: "Flows",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FlowShapeType",
                table: "Flows",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FlowTag",
                table: "Flows",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Left",
                table: "Flows",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Top",
                table: "Flows",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
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
        }
    }
}
