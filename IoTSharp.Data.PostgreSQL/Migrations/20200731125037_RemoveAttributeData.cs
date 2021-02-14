using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class RemoveAttributeData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DataStorage_Catalog_DeviceId_KeyName_DateTime",
                table: "DataStorage");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "DataStorage",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "DataStorage",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateTable(
                name: "TelemetryData",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(nullable: false),
                    KeyName = table.Column<string>(nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataSide = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Value_Boolean = table.Column<bool>(nullable: false),
                    Value_String = table.Column<string>(nullable: true),
                    Value_Long = table.Column<long>(nullable: false),
                    Value_DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Value_Double = table.Column<double>(nullable: false),
                    Value_Json = table.Column<string>(type: "jsonb", nullable: true),
                    Value_XML = table.Column<string>(type: "xml", nullable: true),
                    Value_Binary = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryData", x => new { x.DeviceId, x.KeyName, x.DateTime });
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId_KeyName_DateTime",
                table: "TelemetryData",
                columns: new[] { "DeviceId", "KeyName", "DateTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelemetryData");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "DataStorage",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "DataStorage",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_Catalog_DeviceId_KeyName_DateTime",
                table: "DataStorage",
                columns: new[] { "Catalog", "DeviceId", "KeyName", "DateTime" });
        }
    }
}
