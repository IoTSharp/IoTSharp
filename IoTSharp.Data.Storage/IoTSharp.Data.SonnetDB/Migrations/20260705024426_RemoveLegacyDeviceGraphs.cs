using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SonnetDB.Migrations
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
                    DiagramId = table.Column<Guid>(type: "STRING", nullable: false),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "DATETIME", nullable: true),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    DiagramDesc = table.Column<string>(type: "STRING", nullable: true),
                    DiagramImage = table.Column<string>(type: "STRING", nullable: true),
                    DiagramName = table.Column<string>(type: "STRING", nullable: true),
                    DiagramStatus = table.Column<int>(type: "INT", nullable: false),
                    IsDefault = table.Column<bool>(type: "BOOL", nullable: false)
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
                    ToolBoxId = table.Column<Guid>(type: "STRING", nullable: false),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CommondParam = table.Column<string>(type: "STRING", nullable: true),
                    CommondType = table.Column<string>(type: "STRING", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    DeviceId = table.Column<long>(type: "INT", nullable: false),
                    ToolBoxIcon = table.Column<string>(type: "STRING", nullable: true),
                    ToolBoxName = table.Column<string>(type: "STRING", nullable: true),
                    ToolBoxOffsetLeftPer = table.Column<int>(type: "INT", nullable: false),
                    ToolBoxOffsetTopPer = table.Column<int>(type: "INT", nullable: false),
                    ToolBoxOffsetX = table.Column<int>(type: "INT", nullable: false),
                    ToolBoxOffsetY = table.Column<int>(type: "INT", nullable: false),
                    ToolBoxRequestUri = table.Column<string>(type: "STRING", nullable: true),
                    ToolBoxStatus = table.Column<int>(type: "INT", nullable: false),
                    ToolBoxType = table.Column<string>(type: "STRING", nullable: true)
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
                    GraphId = table.Column<Guid>(type: "STRING", nullable: false),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true),
                    DeviceDiagramDiagramId = table.Column<Guid>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    DeviceId = table.Column<Guid>(type: "STRING", nullable: false),
                    GraphElementId = table.Column<string>(type: "STRING", nullable: true),
                    GraphFill = table.Column<string>(type: "STRING", nullable: true),
                    GraphHeight = table.Column<int>(type: "INT", nullable: false),
                    GraphPostionX = table.Column<int>(type: "INT", nullable: false),
                    GraphPostionY = table.Column<int>(type: "INT", nullable: false),
                    GraphShape = table.Column<string>(type: "STRING", nullable: true),
                    GraphStroke = table.Column<string>(type: "STRING", nullable: true),
                    GraphStrokeWidth = table.Column<int>(type: "INT", nullable: false),
                    GraphTextAnchor = table.Column<string>(type: "STRING", nullable: true),
                    GraphTextFill = table.Column<string>(type: "STRING", nullable: true),
                    GraphTextFontFamily = table.Column<string>(type: "STRING", nullable: true),
                    GraphTextFontSize = table.Column<int>(type: "INT", nullable: false),
                    GraphTextRefX = table.Column<int>(type: "INT", nullable: false),
                    GraphTextRefY = table.Column<int>(type: "INT", nullable: false),
                    GraphTextVerticalAnchor = table.Column<string>(type: "STRING", nullable: true),
                    GraphWidth = table.Column<int>(type: "INT", nullable: false)
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
