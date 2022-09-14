using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
{
    public partial class Produce : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProduceId",
                table: "Device",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "DataStorage",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Produce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DefaultTimeout = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DefaultIdentityType = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
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
                name: "IX_DataStorage_OwnerId",
                table: "DataStorage",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Produce_CustomerId",
                table: "Produce",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Produce_TenantId",
                table: "Produce",
                column: "TenantId");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Produce_OwnerId",
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
