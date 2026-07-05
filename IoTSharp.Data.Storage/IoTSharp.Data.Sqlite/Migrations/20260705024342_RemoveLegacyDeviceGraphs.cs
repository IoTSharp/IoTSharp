using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyDeviceGraphs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceGraphs");

            migrationBuilder.DropTable(
                name: "DeviceGraphToolBoxes");

            migrationBuilder.DropTable(
                name: "DeviceDiagrams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceDiagrams",
                columns: table => new
                {
                    DiagramId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiagramDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DiagramImage = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DiagramName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DiagramStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceDiagrams", x => x.DiagramId);
                    table.ForeignKey(
                        name: "FK_DeviceDiagrams_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceDiagrams_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceGraphToolBoxes",
                columns: table => new
                {
                    ToolBoxId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CommondParam = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CommondType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<long>(type: "INTEGER", nullable: false),
                    ToolBoxIcon = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ToolBoxName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ToolBoxOffsetLeftPer = table.Column<int>(type: "INTEGER", nullable: false),
                    ToolBoxOffsetTopPer = table.Column<int>(type: "INTEGER", nullable: false),
                    ToolBoxOffsetX = table.Column<int>(type: "INTEGER", nullable: false),
                    ToolBoxOffsetY = table.Column<int>(type: "INTEGER", nullable: false),
                    ToolBoxRequestUri = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ToolBoxStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ToolBoxType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGraphToolBoxes", x => x.ToolBoxId);
                    table.ForeignKey(
                        name: "FK_DeviceGraphToolBoxes_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceGraphToolBoxes_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceGraphs",
                columns: table => new
                {
                    GraphId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeviceDiagramDiagramId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GraphElementId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphFill = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphHeight = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphPostionX = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphPostionY = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphShape = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphStroke = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphStrokeWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphTextAnchor = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphTextFill = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphTextFontFamily = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphTextFontSize = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphTextRefX = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphTextRefY = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphTextVerticalAnchor = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphWidth = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGraphs", x => x.GraphId);
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_DeviceDiagrams_DeviceDiagramDiagramId",
                        column: x => x.DeviceDiagramDiagramId,
                        principalTable: "DeviceDiagrams",
                        principalColumn: "DiagramId");
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceDiagrams_CustomerId",
                table: "DeviceDiagrams",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceDiagrams_TenantId",
                table: "DeviceDiagrams",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_CustomerId",
                table: "DeviceGraphs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_DeviceDiagramDiagramId",
                table: "DeviceGraphs",
                column: "DeviceDiagramDiagramId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_TenantId",
                table: "DeviceGraphs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphToolBoxes_CustomerId",
                table: "DeviceGraphToolBoxes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphToolBoxes_TenantId",
                table: "DeviceGraphToolBoxes",
                column: "TenantId");
        }
    }
}
