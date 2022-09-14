using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.MySql.Migrations
{
    public partial class Produce : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProduceId",
                table: "Device",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "DataStorage",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Produce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultTimeout = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DefaultIdentityType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
