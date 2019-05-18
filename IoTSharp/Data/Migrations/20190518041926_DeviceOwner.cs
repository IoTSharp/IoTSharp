using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class DeviceOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Device_Device_GatewayId",
                table: "Device");

            migrationBuilder.RenameColumn(
                name: "GatewayId",
                table: "Device",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Device_GatewayId",
                table: "Device",
                newName: "IX_Device_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_Device_OwnerId",
                table: "Device",
                column: "OwnerId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Device_Device_OwnerId",
                table: "Device");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Device",
                newName: "GatewayId");

            migrationBuilder.RenameIndex(
                name: "IX_Device_OwnerId",
                table: "Device",
                newName: "IX_Device_GatewayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_Device_GatewayId",
                table: "Device",
                column: "GatewayId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
