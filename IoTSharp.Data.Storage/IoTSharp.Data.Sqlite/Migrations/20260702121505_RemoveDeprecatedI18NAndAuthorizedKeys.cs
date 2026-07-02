using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
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
                type: "TEXT",
                nullable: true,
                collation: "NOCASE");

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorizedKeyId",
                table: "Device",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuthorizedKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AuthToken = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
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
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AddDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    KeyName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ResouceDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ResouceGroupId = table.Column<int>(type: "INTEGER", nullable: true),
                    ResourceId = table.Column<long>(type: "INTEGER", nullable: true),
                    ResourceKey = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ResourceTag = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ResourceType = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValueBG = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueCS = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueDA = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueDEDE = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueELGR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueENGR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueENUS = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueESES = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueFI = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueFRFR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueHE = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueHRHR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueHU = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueITIT = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueJAJP = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueKOKR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueNL = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValuePLPL = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValuePT = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueSLSL = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueSR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueSV = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueTRTR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueUK = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueVI = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueZHCN = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueZHTW = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
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
