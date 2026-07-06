using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260706120000_AddCollectionAssignmentExecutionState")]
    public partial class AddCollectionAssignmentExecutionState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AppliedAt",
                table: "EdgeCollectionAssignments",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliedConfigurationHash",
                table: "EdgeCollectionAssignments",
                type: "NVARCHAR2(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppliedConfigurationVersion",
                table: "EdgeCollectionAssignments",
                type: "NUMBER(10)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExecutionAt",
                table: "EdgeCollectionAssignments",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastExecutionMessage",
                table: "EdgeCollectionAssignments",
                type: "NVARCHAR2(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastExecutionProgress",
                table: "EdgeCollectionAssignments",
                type: "NUMBER(10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastExecutionStatus",
                table: "EdgeCollectionAssignments",
                type: "NVARCHAR2(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastExecutionTaskId",
                table: "EdgeCollectionAssignments",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EdgeColAssign_GwAppliedVer",
                table: "EdgeCollectionAssignments",
                columns: new[] { "GatewayId", "AppliedConfigurationVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_EdgeColAssign_LastExecTask",
                table: "EdgeCollectionAssignments",
                column: "LastExecutionTaskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_EdgeColAssign_GwAppliedVer", table: "EdgeCollectionAssignments");
            migrationBuilder.DropIndex(name: "IX_EdgeColAssign_LastExecTask", table: "EdgeCollectionAssignments");

            migrationBuilder.DropColumn(name: "AppliedAt", table: "EdgeCollectionAssignments");
            migrationBuilder.DropColumn(name: "AppliedConfigurationHash", table: "EdgeCollectionAssignments");
            migrationBuilder.DropColumn(name: "AppliedConfigurationVersion", table: "EdgeCollectionAssignments");
            migrationBuilder.DropColumn(name: "LastExecutionAt", table: "EdgeCollectionAssignments");
            migrationBuilder.DropColumn(name: "LastExecutionMessage", table: "EdgeCollectionAssignments");
            migrationBuilder.DropColumn(name: "LastExecutionProgress", table: "EdgeCollectionAssignments");
            migrationBuilder.DropColumn(name: "LastExecutionStatus", table: "EdgeCollectionAssignments");
            migrationBuilder.DropColumn(name: "LastExecutionTaskId", table: "EdgeCollectionAssignments");
        }
    }
}
