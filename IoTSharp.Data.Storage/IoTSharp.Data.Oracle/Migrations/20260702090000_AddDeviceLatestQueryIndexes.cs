using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceLatestQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_Catalog_KeyName_DeviceId",
                table: "DataStorage",
                columns: new[] { "Catalog", "KeyName", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Device_CustomerId_TenantId_Deleted",
                table: "Device",
                columns: new[] { "CustomerId", "TenantId", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Device_TenantId_Deleted",
                table: "Device",
                columns: new[] { "TenantId", "Deleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DataStorage_Catalog_KeyName_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropIndex(
                name: "IX_Device_CustomerId_TenantId_Deleted",
                table: "Device");

            migrationBuilder.DropIndex(
                name: "IX_Device_TenantId_Deleted",
                table: "Device");
        }
    }
}
