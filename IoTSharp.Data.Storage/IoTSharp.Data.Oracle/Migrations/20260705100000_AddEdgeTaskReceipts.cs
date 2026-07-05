using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
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
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TaskId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ContractVersion = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    TargetType = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    GatewayId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EdgeNodeId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    TargetKey = table.Column<string>(type: "NVARCHAR2(450)", maxLength: 450, nullable: true),
                    RuntimeType = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    InstanceId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "NVARCHAR2(1024)", maxLength: 1024, nullable: true),
                    Progress = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Result = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Metadata = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Payload = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ReportedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdgeTaskReceipts", x => x.Id);
                    table.ForeignKey(name: "FK_EdgeReceipts_Customer", column: x => x.CustomerId, principalTable: "Customer", principalColumn: "Id");
                    table.ForeignKey(name: "FK_EdgeReceipts_Device", column: x => x.GatewayId, principalTable: "Device", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(name: "FK_EdgeReceipts_Task", column: x => x.TaskId, principalTable: "EdgeTasks", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(name: "FK_EdgeReceipts_Tenant", column: x => x.TenantId, principalTable: "Tenant", principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(name: "IX_EdgeReceipts_CustomerId", table: "EdgeTaskReceipts", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_EdgeReceipts_Cust_Ten_Del", table: "EdgeTaskReceipts", columns: new[] { "CustomerId", "TenantId", "Deleted" });
            migrationBuilder.CreateIndex(name: "IX_EdgeReceipts_EdgeNodeId", table: "EdgeTaskReceipts", column: "EdgeNodeId");
            migrationBuilder.CreateIndex(name: "IX_EdgeReceipts_Gw_Reported", table: "EdgeTaskReceipts", columns: new[] { "GatewayId", "ReportedAt" });
            migrationBuilder.CreateIndex(name: "IX_EdgeReceipts_Gw_St_Report", table: "EdgeTaskReceipts", columns: new[] { "GatewayId", "Status", "ReportedAt" });
            migrationBuilder.CreateIndex(name: "IX_EdgeReceipts_Target_Status", table: "EdgeTaskReceipts", columns: new[] { "TargetKey", "Status" });
            migrationBuilder.CreateIndex(name: "IX_EdgeReceipts_Task_Reported", table: "EdgeTaskReceipts", columns: new[] { "TaskId", "ReportedAt" });
            migrationBuilder.CreateIndex(name: "IX_EdgeReceipts_TenantId", table: "EdgeTaskReceipts", column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EdgeTaskReceipts");
        }
    }
}
