using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Data.MySQL.Migrations
{
    public partial class Add20211210 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreateId",
                table: "FlowRules",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "EnableTrace",
                table: "DeviceRules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DeviceDiagrams",
                columns: table => new
                {
                    DiagramId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DiagramName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiagramDesc = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiagramStatus = table.Column<int>(type: "int", nullable: true),
                    Creator = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    DiagramImage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceDiagrams", x => x.DiagramId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeviceGraphToolBoxes",
                columns: table => new
                {
                    ToolBoxId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ToolBoxName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ToolBoxIcon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ToolBoxStatus = table.Column<int>(type: "int", nullable: true),
                    ToolBoxRequestUri = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ToolBoxType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceId = table.Column<long>(type: "bigint", nullable: true),
                    ToolBoxOffsetX = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ToolBoxOffsetY = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ToolBoxOffsetTopPer = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ToolBoxOffsetLeftPer = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Creator = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CommondParam = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommondType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGraphToolBoxes", x => x.ToolBoxId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DevicePortMappings",
                columns: table => new
                {
                    MappingId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SourceId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargeId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceElementId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetElementId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Creator = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MappingStatus = table.Column<int>(type: "int", nullable: true),
                    MappingIndex = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    SourceDeviceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TargetDeviceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePortMappings", x => x.MappingId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DevicePorts",
                columns: table => new
                {
                    PortId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PortName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PortDesc = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PortPic = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PortType = table.Column<int>(type: "int", nullable: true),
                    PortPhyType = table.Column<int>(type: "int", nullable: true),
                    PortStatus = table.Column<int>(type: "int", nullable: true),
                    DeviceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Creator = table.Column<long>(type: "bigint", nullable: true),
                    PortElementId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePorts", x => x.PortId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SubscriptionEvents",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EventName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventDesc = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventNameSpace = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventStatus = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    EventParam = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventTag = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreateDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Creator = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionEvents", x => x.EventId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeviceGraphs",
                columns: table => new
                {
                    GraphId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DeviceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    GraphShape = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GraphWidth = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    GraphHeight = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    GraphPostionX = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    GraphPostionY = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    GraphElementId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Creator = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DeviceDiagramDiagramId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    GraphFill = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GraphStroke = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GraphStrokeWidth = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    GraphTextFill = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GraphTextFontSize = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    GraphTextRefX = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    GraphTextAnchor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GraphTextVerticalAnchor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GraphTextFontFamily = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GraphTextRefY = table.Column<decimal>(type: "decimal(65,30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGraphs", x => x.GraphId);
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_DeviceDiagrams_DeviceDiagramDiagramId",
                        column: x => x.DeviceDiagramDiagramId,
                        principalTable: "DeviceDiagrams",
                        principalColumn: "DiagramId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SubscriptionTasks",
                columns: table => new
                {
                    BindId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EventId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SubscriptionEventId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RuleTaskExecutorExecutorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TaskConfig = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionTasks", x => x.BindId);
                    table.ForeignKey(
                        name: "FK_SubscriptionTasks_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionTasks_RuleTaskExecutors_RuleTaskExecutorExecutor~",
                        column: x => x.RuleTaskExecutorExecutorId,
                        principalTable: "RuleTaskExecutors",
                        principalColumn: "ExecutorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionTasks_SubscriptionEvents_SubscriptionEventId",
                        column: x => x.SubscriptionEventId,
                        principalTable: "SubscriptionEvents",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_DeviceDiagramDiagramId",
                table: "DeviceGraphs",
                column: "DeviceDiagramDiagramId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTasks_CustomerId",
                table: "SubscriptionTasks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTasks_RuleTaskExecutorExecutorId",
                table: "SubscriptionTasks",
                column: "RuleTaskExecutorExecutorId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTasks_SubscriptionEventId",
                table: "SubscriptionTasks",
                column: "SubscriptionEventId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceGraphs");

            migrationBuilder.DropTable(
                name: "DeviceGraphToolBoxes");

            migrationBuilder.DropTable(
                name: "DevicePortMappings");

            migrationBuilder.DropTable(
                name: "DevicePorts");

            migrationBuilder.DropTable(
                name: "SubscriptionTasks");

            migrationBuilder.DropTable(
                name: "DeviceDiagrams");

            migrationBuilder.DropTable(
                name: "SubscriptionEvents");

            migrationBuilder.DropColumn(
                name: "CreateId",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "EnableTrace",
                table: "DeviceRules");
        }
    }
}
