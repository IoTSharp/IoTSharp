using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SqlServer.Migrations
{
    public partial class Produce2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProduceId",
                table: "Device",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "DataStorage",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Produces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultTimeout = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DefaultIdentityType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultDeviceType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produces_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Produces_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Device_ProduceId",
                table: "Device",
                column: "ProduceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_OwnerId",
                table: "DataStorage",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Produces_CustomerId",
                table: "Produces",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Produces_TenantId",
                table: "Produces",
                column: "TenantId");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Produces_OwnerId",
                table: "DataStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_Device_Produces_ProduceId",
                table: "Device");

            migrationBuilder.DropTable(
                name: "Produces");

            migrationBuilder.DropIndex(
                name: "IX_Device_ProduceId",
                table: "Device");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_OwnerId",
                table: "DataStorage");

            migrationBuilder.DropColumn(
                name: "ProduceId",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "DataStorage");
        }
    }
}
