using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class ModifyRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropForeignKey(
                name: "FK_Relationship_AspNetUsers_IdentityId",
                table: "Relationship");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_DeviceId1",
                table: "DataStorage");

            migrationBuilder.DropColumn(
                name: "DeviceId1",
                table: "DataStorage");

            migrationBuilder.RenameColumn(
                name: "IdentityId",
                table: "Relationship",
                newName: "IdentityUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Relationship_IdentityId",
                table: "Relationship",
                newName: "IX_Relationship_IdentityUserId");

            migrationBuilder.AddColumn<Guid>(
                name: "AttributeLatest_DeviceId",
                table: "DataStorage",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_AttributeLatest_DeviceId",
                table: "DataStorage",
                column: "AttributeLatest_DeviceId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Relationship_AspNetUsers_IdentityUserId",
                table: "Relationship",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropForeignKey(
                name: "FK_Relationship_AspNetUsers_IdentityUserId",
                table: "Relationship");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_AttributeLatest_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropColumn(
                name: "AttributeLatest_DeviceId",
                table: "DataStorage");

            migrationBuilder.RenameColumn(
                name: "IdentityUserId",
                table: "Relationship",
                newName: "IdentityId");

            migrationBuilder.RenameIndex(
                name: "IX_Relationship_IdentityUserId",
                table: "Relationship",
                newName: "IX_Relationship_IdentityId");

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceId1",
                table: "DataStorage",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_DeviceId1",
                table: "DataStorage",
                column: "DeviceId1");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Relationship_AspNetUsers_IdentityId",
                table: "Relationship",
                column: "IdentityId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
