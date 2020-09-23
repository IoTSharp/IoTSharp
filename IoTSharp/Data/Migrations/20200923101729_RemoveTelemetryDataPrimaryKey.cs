using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class RemoveTelemetryDataPrimaryKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TelemetryData",
                table: "TelemetryData");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TelemetryData");

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "TelemetryData",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TelemetryData",
                table: "TelemetryData",
                columns: new[] { "DeviceId", "KeyName", "DateTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TelemetryData",
                table: "TelemetryData");

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "TelemetryData",
                type: "text",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "TelemetryData",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TelemetryData",
                table: "TelemetryData",
                column: "Id");
        }
    }
}
