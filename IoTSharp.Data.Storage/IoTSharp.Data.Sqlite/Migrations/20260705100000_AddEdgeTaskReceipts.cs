using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260705100000_AddEdgeTaskReceipts")]
    public partial class AddEdgeTaskReceipts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EdgeTaskReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContractVersion = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, collation: "NOCASE"),
                    TargetType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    GatewayId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EdgeNodeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TargetKey = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, collation: "NOCASE"),
                    RuntimeType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    InstanceId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true, collation: "NOCASE"),
                    Progress = table.Column<int>(type: "INTEGER", nullable: true),
                    Result = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Payload = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ReportedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdgeTaskReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EdgeTaskReceipts_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EdgeTaskReceipts_Device_GatewayId",
                        column: x => x.GatewayId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EdgeTaskReceipts_EdgeTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "EdgeTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EdgeTaskReceipts_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(name: "IX_EdgeTaskReceipts_CustomerId", table: "EdgeTaskReceipts", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_EdgeTaskReceipts_CustomerId_TenantId_Deleted", table: "EdgeTaskReceipts", columns: new[] { "CustomerId", "TenantId", "Deleted" });
            migrationBuilder.CreateIndex(name: "IX_EdgeTaskReceipts_EdgeNodeId", table: "EdgeTaskReceipts", column: "EdgeNodeId");
            migrationBuilder.CreateIndex(name: "IX_EdgeTaskReceipts_GatewayId_ReportedAt", table: "EdgeTaskReceipts", columns: new[] { "GatewayId", "ReportedAt" });
            migrationBuilder.CreateIndex(name: "IX_EdgeTaskReceipts_GatewayId_Status_ReportedAt", table: "EdgeTaskReceipts", columns: new[] { "GatewayId", "Status", "ReportedAt" });
            migrationBuilder.CreateIndex(name: "IX_EdgeTaskReceipts_TargetKey_Status", table: "EdgeTaskReceipts", columns: new[] { "TargetKey", "Status" });
            migrationBuilder.CreateIndex(name: "IX_EdgeTaskReceipts_TaskId_ReportedAt", table: "EdgeTaskReceipts", columns: new[] { "TaskId", "ReportedAt" });
            migrationBuilder.CreateIndex(name: "IX_EdgeTaskReceipts_TenantId", table: "EdgeTaskReceipts", column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EdgeTaskReceipts");
        }
    }
}
