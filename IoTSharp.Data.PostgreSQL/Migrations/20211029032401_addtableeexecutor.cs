using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class addtableeexecutor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExecutorId",
                table: "Flows",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RuleTaskExecutors",
                columns: table => new
                {
                    ExecutorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutorName = table.Column<string>(type: "text", nullable: true),
                    ExecutorDesc = table.Column<string>(type: "text", nullable: true),
                    Path = table.Column<string>(type: "text", nullable: true),
                    TypeName = table.Column<string>(type: "text", nullable: true),
                    DefaultConfig = table.Column<string>(type: "text", nullable: true),
                    MataData = table.Column<string>(type: "text", nullable: true),
                    Tag = table.Column<string>(type: "text", nullable: true),
                    ExecutorStatus = table.Column<int>(type: "integer", nullable: false),
                    AddDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleTaskExecutors", x => x.ExecutorId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flows_ExecutorId",
                table: "Flows",
                column: "ExecutorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flows_RuleTaskExecutors_ExecutorId",
                table: "Flows",
                column: "ExecutorId",
                principalTable: "RuleTaskExecutors",
                principalColumn: "ExecutorId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flows_RuleTaskExecutors_ExecutorId",
                table: "Flows");

            migrationBuilder.DropTable(
                name: "RuleTaskExecutors");

            migrationBuilder.DropIndex(
                name: "IX_Flows_ExecutorId",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "ExecutorId",
                table: "Flows");
        }
    }
}
