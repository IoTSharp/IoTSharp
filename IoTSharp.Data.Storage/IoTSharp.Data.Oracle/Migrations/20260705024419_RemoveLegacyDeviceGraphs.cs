using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
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
                    DiagramId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DiagramDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DiagramImage = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DiagramName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DiagramStatus = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsDefault = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceDiagrams", x => x.DiagramId);
                    table.ForeignKey(
                        name: "FK_DeviceDiagrams_Customer_Cu~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceDiagrams_Tenant_Tena~",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceGraphToolBoxes",
                columns: table => new
                {
                    ToolBoxId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CommondParam = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CommondType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DeviceId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ToolBoxIcon = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ToolBoxName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ToolBoxOffsetLeftPer = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ToolBoxOffsetTopPer = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ToolBoxOffsetX = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ToolBoxOffsetY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ToolBoxRequestUri = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ToolBoxStatus = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ToolBoxType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGraphToolBoxes", x => x.ToolBoxId);
                    table.ForeignKey(
                        name: "FK_DeviceGraphToolBoxes_Custo~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceGraphToolBoxes_Tenan~",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceGraphs",
                columns: table => new
                {
                    GraphId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    DeviceDiagramDiagramId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DeviceId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    GraphElementId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphFill = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphHeight = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GraphPostionX = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GraphPostionY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GraphShape = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphStroke = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphStrokeWidth = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GraphTextAnchor = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphTextFill = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphTextFontFamily = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphTextFontSize = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GraphTextRefX = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GraphTextRefY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GraphTextVerticalAnchor = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphWidth = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGraphs", x => x.GraphId);
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_Customer_Cust~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_DeviceDiagram~",
                        column: x => x.DeviceDiagramDiagramId,
                        principalTable: "DeviceDiagrams",
                        principalColumn: "DiagramId");
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_Tenant_Tenant~",
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
                name: "IX_DeviceGraphs_DeviceDiagram~",
                table: "DeviceGraphs",
                column: "DeviceDiagramDiagramId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_TenantId",
                table: "DeviceGraphs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphToolBoxes_Custo~",
                table: "DeviceGraphToolBoxes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphToolBoxes_Tenan~",
                table: "DeviceGraphToolBoxes",
                column: "TenantId");
        }
    }
}
