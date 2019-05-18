using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class Gateway : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Device");

            migrationBuilder.AddColumn<int>(
                name: "DeviceType",
                table: "Device",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "GatewayId",
                table: "Device",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Device_GatewayId",
                table: "Device",
                column: "GatewayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_Device_GatewayId",
                table: "Device",
                column: "GatewayId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Device_Device_GatewayId",
                table: "Device");

            migrationBuilder.DropIndex(
                name: "IX_Device_GatewayId",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "GatewayId",
                table: "Device");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Device",
                nullable: true);
        }
    }
}
