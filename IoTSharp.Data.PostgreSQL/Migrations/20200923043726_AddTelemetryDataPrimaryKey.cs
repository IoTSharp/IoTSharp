using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class AddTelemetryDataPrimaryKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TelemetryData",
                table: "TelemetryData");

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "TelemetryData",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "TelemetryData",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TelemetryData",
                table: "TelemetryData",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId",
                table: "TelemetryData",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_KeyName",
                table: "TelemetryData",
                column: "KeyName");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId_KeyName",
                table: "TelemetryData",
                columns: new[] { "DeviceId", "KeyName" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TelemetryData",
                table: "TelemetryData");

            migrationBuilder.DropIndex(
                name: "IX_TelemetryData_DeviceId",
                table: "TelemetryData");

            migrationBuilder.DropIndex(
                name: "IX_TelemetryData_KeyName",
                table: "TelemetryData");

            migrationBuilder.DropIndex(
                name: "IX_TelemetryData_DeviceId_KeyName",
                table: "TelemetryData");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TelemetryData");

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "TelemetryData",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TelemetryData",
                table: "TelemetryData",
                columns: new[] { "DeviceId", "KeyName", "DateTime" });
        }
    }
}
