using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SqlServer.Migrations
{
    public partial class Produce : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProduceId",
                table: "Device",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Produce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultTimeout = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DefaultIdentityType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produce", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produce_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Produce_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Device_ProduceId",
                table: "Device",
                column: "ProduceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_DeviceId",
                table: "DataStorage",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Produce_CustomerId",
                table: "Produce",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Produce_TenantId",
                table: "Produce",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Produce_DeviceId",
                table: "DataStorage",
                column: "DeviceId",
                principalTable: "Produce",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Device_Produce_ProduceId",
                table: "Device",
                column: "ProduceId",
                principalTable: "Produce",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Produce_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_Device_Produce_ProduceId",
                table: "Device");

            migrationBuilder.DropTable(
                name: "Produce");

            migrationBuilder.DropIndex(
                name: "IX_Device_ProduceId",
                table: "Device");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_DeviceId",
                table: "DataStorage");

            migrationBuilder.DropColumn(
                name: "ProduceId",
                table: "Device");
        }
    }
}
