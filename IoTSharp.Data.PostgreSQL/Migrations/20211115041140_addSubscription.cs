using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class addSubscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceDiagrams",
                columns: table => new
                {
                    DiagramId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiagramName = table.Column<string>(type: "text", nullable: true),
                    DiagramDesc = table.Column<string>(type: "text", nullable: true),
                    DiagramStatus = table.Column<int>(type: "integer", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DiagramImage = table.Column<string>(type: "text", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceDiagrams", x => x.DiagramId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceGraphToolBoxes",
                columns: table => new
                {
                    ToolBoxId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToolBoxName = table.Column<string>(type: "text", nullable: true),
                    ToolBoxIcon = table.Column<string>(type: "text", nullable: true),
                    ToolBoxStatus = table.Column<int>(type: "integer", nullable: true),
                    ToolBoxRequestUri = table.Column<string>(type: "text", nullable: true),
                    ToolBoxType = table.Column<string>(type: "text", nullable: true),
                    DeviceId = table.Column<long>(type: "bigint", nullable: true),
                    ToolBoxOffsetX = table.Column<decimal>(type: "numeric", nullable: true),
                    ToolBoxOffsetY = table.Column<decimal>(type: "numeric", nullable: true),
                    ToolBoxOffsetTopPer = table.Column<decimal>(type: "numeric", nullable: true),
                    ToolBoxOffsetLeftPer = table.Column<decimal>(type: "numeric", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    CommondParam = table.Column<string>(type: "text", nullable: true),
                    CommondType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGraphToolBoxes", x => x.ToolBoxId);
                });

            migrationBuilder.CreateTable(
                name: "DevicePortMappings",
                columns: table => new
                {
                    MappingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<string>(type: "text", nullable: true),
                    TargeId = table.Column<string>(type: "text", nullable: true),
                    SourceElementId = table.Column<string>(type: "text", nullable: true),
                    TargetElementId = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    MappingStatus = table.Column<int>(type: "integer", nullable: true),
                    MappingIndex = table.Column<decimal>(type: "numeric", nullable: true),
                    SourceDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetDeviceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePortMappings", x => x.MappingId);
                });

            migrationBuilder.CreateTable(
                name: "DevicePorts",
                columns: table => new
                {
                    PortId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortName = table.Column<string>(type: "text", nullable: true),
                    PortDesc = table.Column<string>(type: "text", nullable: true),
                    PortPic = table.Column<string>(type: "text", nullable: true),
                    PortType = table.Column<int>(type: "integer", nullable: true),
                    PortPhyType = table.Column<int>(type: "integer", nullable: true),
                    PortStatus = table.Column<int>(type: "integer", nullable: true),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Creator = table.Column<long>(type: "bigint", nullable: true),
                    PortElementId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePorts", x => x.PortId);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionEvents",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventName = table.Column<string>(type: "text", nullable: true),
                    EventDesc = table.Column<string>(type: "text", nullable: true),
                    EventNameSpace = table.Column<string>(type: "text", nullable: true),
                    EventStatus = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EventParam = table.Column<string>(type: "text", nullable: true),
                    EventTag = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionEvents", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceGraphs",
                columns: table => new
                {
                    GraphId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    GraphShape = table.Column<string>(type: "text", nullable: true),
                    GraphWidth = table.Column<decimal>(type: "numeric", nullable: true),
                    GraphHeight = table.Column<decimal>(type: "numeric", nullable: true),
                    GraphPostionX = table.Column<decimal>(type: "numeric", nullable: true),
                    GraphPostionY = table.Column<decimal>(type: "numeric", nullable: true),
                    GraphElementId = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceDiagramDiagramId = table.Column<Guid>(type: "uuid", nullable: true),
                    GraphFill = table.Column<string>(type: "text", nullable: true),
                    GraphStroke = table.Column<string>(type: "text", nullable: true),
                    GraphStrokeWidth = table.Column<decimal>(type: "numeric", nullable: true),
                    GraphTextFill = table.Column<string>(type: "text", nullable: true),
                    GraphTextFontSize = table.Column<decimal>(type: "numeric", nullable: true),
                    GraphTextRefX = table.Column<decimal>(type: "numeric", nullable: true),
                    GraphTextAnchor = table.Column<string>(type: "text", nullable: true),
                    GraphTextVerticalAnchor = table.Column<string>(type: "text", nullable: true),
                    GraphTextFontFamily = table.Column<string>(type: "text", nullable: true),
                    GraphTextRefY = table.Column<decimal>(type: "numeric", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionTasks",
                columns: table => new
                {
                    BindId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    RuleTaskExecutorExecutorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskConfig = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionTasks", x => x.BindId);
                    table.ForeignKey(
                        name: "FK_SubscriptionTasks_RuleTaskExecutors_RuleTaskExecutorExecuto~",
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_DeviceDiagramDiagramId",
                table: "DeviceGraphs",
                column: "DeviceDiagramDiagramId");

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
        }
    }
}
