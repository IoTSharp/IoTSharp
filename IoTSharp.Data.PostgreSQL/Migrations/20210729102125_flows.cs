using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IoTSharp.Migrations
{
    public partial class flows : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TelemetryData_DeviceId_KeyName_DateTime",
                table: "TelemetryData");

            migrationBuilder.CreateTable(
                name: "FlowOperations",
                columns: table => new
                {
                    OperationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    NodeStatus = table.Column<int>(type: "integer", nullable: true),
                    OperationDesc = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    BizId = table.Column<string>(type: "text", nullable: true),
                    FlowId = table.Column<long>(type: "bigint", nullable: false),
                    RuleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowOperations", x => x.OperationId);
                });

            migrationBuilder.CreateTable(
                name: "FlowRules",
                columns: table => new
                {
                    RuleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RuleType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Describes = table.Column<string>(type: "text", nullable: true),
                    Runner = table.Column<string>(type: "text", nullable: true),
                    ExecutableCode = table.Column<string>(type: "text", nullable: true),
                    Creator = table.Column<string>(type: "text", nullable: true),
                    RuleDesc = table.Column<string>(type: "text", nullable: true),
                    RuleStatus = table.Column<int>(type: "integer", nullable: true),
                    CreatTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DefinitionsXml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowRules", x => x.RuleId);
                });

            migrationBuilder.CreateTable(
                name: "Flows",
                columns: table => new
                {
                    FlowId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bpmnid = table.Column<string>(type: "text", nullable: true),
                    Flowname = table.Column<string>(type: "text", nullable: true),
                    RuleId = table.Column<long>(type: "bigint", nullable: false),
                    Flowdesc = table.Column<string>(type: "text", nullable: true),
                    ObjectId = table.Column<string>(type: "text", nullable: true),
                    FlowType = table.Column<string>(type: "text", nullable: true),
                    SourceId = table.Column<string>(type: "text", nullable: true),
                    TargetId = table.Column<string>(type: "text", nullable: true),
                    NodeProcessClass = table.Column<string>(type: "text", nullable: true),
                    Conditionexpression = table.Column<string>(type: "text", nullable: true),
                    NodeProcessMethod = table.Column<string>(type: "text", nullable: true),
                    NodeProcessParams = table.Column<string>(type: "text", nullable: true),
                    NodeProcessType = table.Column<string>(type: "text", nullable: true),
                    NodeProcessScript = table.Column<string>(type: "text", nullable: true),
                    Incoming = table.Column<string>(type: "text", nullable: true),
                    Outgoing = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flows", x => x.FlowId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlowOperations");

            migrationBuilder.DropTable(
                name: "FlowRules");

            migrationBuilder.DropTable(
                name: "Flows");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId_KeyName_DateTime",
                table: "TelemetryData",
                columns: new[] { "DeviceId", "KeyName", "DateTime" });
        }
    }
}
