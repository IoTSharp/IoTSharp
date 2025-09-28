using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Migrations
{
    /// <inheritdoc />
    public partial class ai_settings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Device");

            migrationBuilder.AddColumn<Guid>(
                name: "AISettingsId",
                table: "Tenant",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AISettingsId",
                table: "Customer",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AISettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    MCP_API_KEY = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Enable = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Device",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");
        }
    }
}
