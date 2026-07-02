using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SonnetDB.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDeprecatedI18NAndAuthorizedKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Device_AuthorizedKeys_AuthorizedKeyId",
                table: "Device");

            migrationBuilder.DropTable(
                name: "AuthorizedKeys");

            migrationBuilder.DropTable(
                name: "BaseI18Ns");

            migrationBuilder.DropIndex(
                name: "IX_Device_AuthorizedKeyId",
                table: "Device");

            migrationBuilder.DropIndex(
                name: "IX_Device_CustomerId",
                table: "Device");

            migrationBuilder.DropIndex(
                name: "IX_Device_TenantId",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "CommandI18N",
                table: "DeviceModelCommands");

            migrationBuilder.DropColumn(
                name: "AuthorizedKeyId",
                table: "Device");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommandI18N",
                table: "DeviceModelCommands",
                type: "STRING",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorizedKeyId",
                table: "Device",
                type: "STRING",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuthorizedKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    AuthToken = table.Column<string>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    Name = table.Column<string>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizedKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizedKeys_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuthorizedKeys_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaseI18Ns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INT", nullable: false),
                    AddDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    KeyName = table.Column<string>(type: "STRING", nullable: true),
                    ResouceDesc = table.Column<string>(type: "STRING", nullable: true),
                    ResouceGroupId = table.Column<int>(type: "INT", nullable: true),
                    ResourceId = table.Column<long>(type: "INT", nullable: true),
                    ResourceKey = table.Column<string>(type: "STRING", nullable: true),
                    ResourceTag = table.Column<string>(type: "STRING", nullable: true),
                    ResourceType = table.Column<int>(type: "INT", nullable: true),
                    Status = table.Column<int>(type: "INT", nullable: false),
                    UserId = table.Column<Guid>(type: "STRING", nullable: false),
                    ValueBG = table.Column<string>(type: "STRING", nullable: true),
                    ValueCS = table.Column<string>(type: "STRING", nullable: true),
                    ValueDA = table.Column<string>(type: "STRING", nullable: true),
                    ValueDEDE = table.Column<string>(type: "STRING", nullable: true),
                    ValueELGR = table.Column<string>(type: "STRING", nullable: true),
                    ValueENGR = table.Column<string>(type: "STRING", nullable: true),
                    ValueENUS = table.Column<string>(type: "STRING", nullable: true),
                    ValueESES = table.Column<string>(type: "STRING", nullable: true),
                    ValueFI = table.Column<string>(type: "STRING", nullable: true),
                    ValueFRFR = table.Column<string>(type: "STRING", nullable: true),
                    ValueHE = table.Column<string>(type: "STRING", nullable: true),
                    ValueHRHR = table.Column<string>(type: "STRING", nullable: true),
                    ValueHU = table.Column<string>(type: "STRING", nullable: true),
                    ValueITIT = table.Column<string>(type: "STRING", nullable: true),
                    ValueJAJP = table.Column<string>(type: "STRING", nullable: true),
                    ValueKOKR = table.Column<string>(type: "STRING", nullable: true),
                    ValueNL = table.Column<string>(type: "STRING", nullable: true),
                    ValuePLPL = table.Column<string>(type: "STRING", nullable: true),
                    ValuePT = table.Column<string>(type: "STRING", nullable: true),
                    ValueSLSL = table.Column<string>(type: "STRING", nullable: true),
                    ValueSR = table.Column<string>(type: "STRING", nullable: true),
                    ValueSV = table.Column<string>(type: "STRING", nullable: true),
                    ValueTRTR = table.Column<string>(type: "STRING", nullable: true),
                    ValueUK = table.Column<string>(type: "STRING", nullable: true),
                    ValueVI = table.Column<string>(type: "STRING", nullable: true),
                    ValueZHCN = table.Column<string>(type: "STRING", nullable: true),
                    ValueZHTW = table.Column<string>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseI18Ns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Device_AuthorizedKeyId",
                table: "Device",
                column: "AuthorizedKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_CustomerId",
                table: "Device",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_TenantId",
                table: "Device",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedKeys_CustomerId",
                table: "AuthorizedKeys",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedKeys_TenantId",
                table: "AuthorizedKeys",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_AuthorizedKeys_AuthorizedKeyId",
                table: "Device",
                column: "AuthorizedKeyId",
                principalTable: "AuthorizedKeys",
                principalColumn: "Id");
        }
    }
}
