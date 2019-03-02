using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class AddDiscriminator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttributeData_Device_DeviceId1",
                table: "AttributeData");

            migrationBuilder.DropForeignKey(
                name: "FK_AttributeData_Device_DeviceId",
                table: "AttributeData");

            migrationBuilder.DropForeignKey(
                name: "FK_AttributeData_Device_TelemetryData_DeviceId",
                table: "AttributeData");

            migrationBuilder.DropForeignKey(
                name: "FK_AttributeData_Device_TelemetryLatest_DeviceId",
                table: "AttributeData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttributeData",
                table: "AttributeData");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AttributeData");

            migrationBuilder.RenameTable(
                name: "AttributeData",
                newName: "DataStorage");

            migrationBuilder.RenameIndex(
                name: "IX_AttributeData_TelemetryLatest_DeviceId",
                table: "DataStorage",
                newName: "IX_DataStorage_TelemetryLatest_DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_AttributeData_TelemetryData_DeviceId",
                table: "DataStorage",
                newName: "IX_DataStorage_TelemetryData_DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_AttributeData_DeviceId",
                table: "DataStorage",
                newName: "IX_DataStorage_DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_AttributeData_DeviceId1",
                table: "DataStorage",
                newName: "IX_DataStorage_DeviceId1");

            migrationBuilder.AddColumn<int>(
                name: "Catalog",
                table: "DataStorage",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataStorage",
                table: "DataStorage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Device_DeviceId",
                table: "DataStorage",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Device_DeviceId1",
                table: "DataStorage",
                column: "DeviceId1",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Device_TelemetryData_DeviceId",
                table: "DataStorage",
                column: "TelemetryData_DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Device_TelemetryLatest_DeviceId",
                table: "DataStorage",
                column: "TelemetryLatest_DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Device_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Device_DeviceId1",
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

            migrationBuilder.DropColumn(
                name: "Catalog",
                table: "DataStorage");

            migrationBuilder.RenameTable(
                name: "DataStorage",
                newName: "AttributeData");

            migrationBuilder.RenameIndex(
                name: "IX_DataStorage_TelemetryLatest_DeviceId",
                table: "AttributeData",
                newName: "IX_AttributeData_TelemetryLatest_DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_DataStorage_TelemetryData_DeviceId",
                table: "AttributeData",
                newName: "IX_AttributeData_TelemetryData_DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_DataStorage_DeviceId1",
                table: "AttributeData",
                newName: "IX_AttributeData_DeviceId1");

            migrationBuilder.RenameIndex(
                name: "IX_DataStorage_DeviceId",
                table: "AttributeData",
                newName: "IX_AttributeData_DeviceId");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AttributeData",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttributeData",
                table: "AttributeData",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeData_Device_DeviceId1",
                table: "AttributeData",
                column: "DeviceId1",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeData_Device_DeviceId",
                table: "AttributeData",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeData_Device_TelemetryData_DeviceId",
                table: "AttributeData",
                column: "TelemetryData_DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeData_Device_TelemetryLatest_DeviceId",
                table: "AttributeData",
                column: "TelemetryLatest_DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
