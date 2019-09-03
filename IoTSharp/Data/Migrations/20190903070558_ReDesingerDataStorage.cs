using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class ReDesingerDataStorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Device_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Device_AttributeLatest_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Device_TelemetryData_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Device_TelemetryLatest_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataStorage",
                table: "DataStorage");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_AttributeLatest_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_TelemetryData_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_TelemetryLatest_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropColumn(
                name: "AttributeLatest_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DataStorage");

            migrationBuilder.DropColumn(
                name: "TelemetryData_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropColumn(
                name: "TelemetryLatest_DeviceId",
                table: "DataStorage");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DataStorage",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataStorage",
                table: "DataStorage",
                columns: new[] { "Catalog", "DeviceId", "KeyName", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_Catalog",
                table: "DataStorage",
                column: "Catalog");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_Catalog_DeviceId",
                table: "DataStorage",
                columns: new[] { "Catalog", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_Catalog_DeviceId_KeyName",
                table: "DataStorage",
                columns: new[] { "Catalog", "DeviceId", "KeyName" });

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_Catalog_DeviceId_KeyName_DateTime",
                table: "DataStorage",
                columns: new[] { "Catalog", "DeviceId", "KeyName", "DateTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DataStorage",
                table: "DataStorage");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_Catalog",
                table: "DataStorage");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_Catalog_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_Catalog_DeviceId_KeyName",
                table: "DataStorage");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_Catalog_DeviceId_KeyName_DateTime",
                table: "DataStorage");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DataStorage",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<Guid>(
                name: "AttributeLatest_DeviceId",
                table: "DataStorage",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "DataStorage",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TelemetryData_DeviceId",
                table: "DataStorage",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TelemetryLatest_DeviceId",
                table: "DataStorage",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataStorage",
                table: "DataStorage",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_DeviceId",
                table: "DataStorage",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_AttributeLatest_DeviceId",
                table: "DataStorage",
                column: "AttributeLatest_DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_TelemetryData_DeviceId",
                table: "DataStorage",
                column: "TelemetryData_DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_TelemetryLatest_DeviceId",
                table: "DataStorage",
                column: "TelemetryLatest_DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Device_DeviceId",
                table: "DataStorage",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Device_AttributeLatest_DeviceId",
                table: "DataStorage",
                column: "AttributeLatest_DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Device_TelemetryData_DeviceId",
                table: "DataStorage",
                column: "TelemetryData_DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Device_TelemetryLatest_DeviceId",
                table: "DataStorage",
                column: "TelemetryLatest_DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
