using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class addteststatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TestStatus",
                table: "RuleTaskExecutors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "Tester",
                table: "RuleTaskExecutors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "TesterDateTime",
                table: "RuleTaskExecutors",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TestStatus",
                table: "Flows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "Tester",
                table: "Flows",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "TesterDateTime",
                table: "Flows",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestStatus",
                table: "RuleTaskExecutors");

            migrationBuilder.DropColumn(
                name: "Tester",
                table: "RuleTaskExecutors");

            migrationBuilder.DropColumn(
                name: "TesterDateTime",
                table: "RuleTaskExecutors");

            migrationBuilder.DropColumn(
                name: "TestStatus",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "Tester",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "TesterDateTime",
                table: "Flows");
        }
    }
}
