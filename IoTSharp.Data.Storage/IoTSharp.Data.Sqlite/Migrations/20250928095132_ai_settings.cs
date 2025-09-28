using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class ai_settings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AISettingsId",
                table: "Tenant",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AISettingsId",
                table: "Customer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AISettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    MCP_API_KEY = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Enable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AISettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_AISettingsId",
                table: "Tenant",
                column: "AISettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_AISettingsId",
                table: "Customer",
                column: "AISettingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_AISettings_AISettingsId",
                table: "Customer",
                column: "AISettingsId",
                principalTable: "AISettings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenant_AISettings_AISettingsId",
                table: "Tenant",
                column: "AISettingsId",
                principalTable: "AISettings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_AISettings_AISettingsId",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenant_AISettings_AISettingsId",
                table: "Tenant");

            migrationBuilder.DropTable(
                name: "AISettings");

            migrationBuilder.DropIndex(
                name: "IX_Tenant_AISettingsId",
                table: "Tenant");

            migrationBuilder.DropIndex(
                name: "IX_Customer_AISettingsId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "AISettingsId",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "AISettingsId",
                table: "Customer");
        }
    }
}
