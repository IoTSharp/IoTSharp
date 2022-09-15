using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
{
    public partial class Produce2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Produce_OwnerId",
                table: "DataStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_Device_Produce_ProduceId",
                table: "Device");

            migrationBuilder.DropForeignKey(
                name: "FK_Produce_Customer_CustomerId",
                table: "Produce");

            migrationBuilder.DropForeignKey(
                name: "FK_Produce_Tenant_TenantId",
                table: "Produce");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Produce",
                table: "Produce");

            migrationBuilder.RenameTable(
                name: "Produce",
                newName: "Produces");

            migrationBuilder.RenameIndex(
                name: "IX_Produce_TenantId",
                table: "Produces",
                newName: "IX_Produces_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Produce_CustomerId",
                table: "Produces",
                newName: "IX_Produces_CustomerId");

            migrationBuilder.AddColumn<int>(
                name: "DefaultDeviceType",
                table: "Produces",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Produces",
                table: "Produces",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Produces_OwnerId",
                table: "DataStorage",
                column: "OwnerId",
                principalTable: "Produces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_Produces_ProduceId",
                table: "Device",
                column: "ProduceId",
                principalTable: "Produces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Produces_Customer_CustomerId",
                table: "Produces",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Produces_Tenant_TenantId",
                table: "Produces",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Produces_OwnerId",
                table: "DataStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_Device_Produces_ProduceId",
                table: "Device");

            migrationBuilder.DropForeignKey(
                name: "FK_Produces_Customer_CustomerId",
                table: "Produces");

            migrationBuilder.DropForeignKey(
                name: "FK_Produces_Tenant_TenantId",
                table: "Produces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Produces",
                table: "Produces");

            migrationBuilder.DropColumn(
                name: "DefaultDeviceType",
                table: "Produces");

            migrationBuilder.RenameTable(
                name: "Produces",
                newName: "Produce");

            migrationBuilder.RenameIndex(
                name: "IX_Produces_TenantId",
                table: "Produce",
                newName: "IX_Produce_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Produces_CustomerId",
                table: "Produce",
                newName: "IX_Produce_CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Produce",
                table: "Produce",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Produce_OwnerId",
                table: "DataStorage",
                column: "OwnerId",
                principalTable: "Produce",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_Produce_ProduceId",
                table: "Device",
                column: "ProduceId",
                principalTable: "Produce",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Produce_Customer_CustomerId",
                table: "Produce",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Produce_Tenant_TenantId",
                table: "Produce",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");
        }
    }
}
