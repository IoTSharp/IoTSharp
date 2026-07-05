using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SonnetDB.Migrations
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    TaskId = table.Column<Guid>(type: "STRING", nullable: false),
                    ContractVersion = table.Column<string>(type: "STRING", maxLength: 64, nullable: true),
                    TargetType = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    GatewayId = table.Column<Guid>(type: "STRING", nullable: false),
                    EdgeNodeId = table.Column<Guid>(type: "STRING", nullable: true),
                    TargetKey = table.Column<string>(type: "STRING", maxLength: 450, nullable: true),
                    RuntimeType = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    InstanceId = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "STRING", maxLength: 1024, nullable: true),
                    Progress = table.Column<int>(type: "INT", nullable: true),
                    Result = table.Column<string>(type: "STRING", nullable: true),
                    Metadata = table.Column<string>(type: "STRING", nullable: true),
                    Payload = table.Column<string>(type: "STRING", nullable: true),
                    ReportedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdgeTaskReceipts", x => x.Id);
                    table.ForeignKey(name: "FK_EdgeTaskReceipts_Customer_CustomerId", column: x => x.CustomerId, principalTable: "Customer", principalColumn: "Id");
                    table.ForeignKey(name: "FK_EdgeTaskReceipts_Device_GatewayId", column: x => x.GatewayId, principalTable: "Device", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(name: "FK_EdgeTaskReceipts_EdgeTasks_TaskId", column: x => x.TaskId, principalTable: "EdgeTasks", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(name: "FK_EdgeTaskReceipts_Tenant_TenantId", column: x => x.TenantId, principalTable: "Tenant", principalColumn: "Id");
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
