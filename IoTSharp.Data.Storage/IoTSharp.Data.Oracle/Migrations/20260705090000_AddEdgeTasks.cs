using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260705090000_AddEdgeTasks")]
    public partial class AddEdgeTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EdgeTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ContractVersion = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    TaskType = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    TargetType = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    GatewayId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EdgeNodeId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    TargetKey = table.Column<string>(type: "NVARCHAR2(450)", maxLength: 450, nullable: true),
                    RuntimeType = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    InstanceId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "NVARCHAR2(1024)", maxLength: 1024, nullable: true),
                    Progress = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Parameters = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Metadata = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RequestPayload = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    LastReceiptPayload = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    LastReceiptAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdgeTasks", x => x.Id);
                    table.ForeignKey(name: "FK_EdgeTasks_Customer_C~", column: x => x.CustomerId, principalTable: "Customer", principalColumn: "Id");
                    table.ForeignKey(name: "FK_EdgeTasks_Device_Gate~", column: x => x.GatewayId, principalTable: "Device", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_EdgeTasks_Tenant_Tena~", column: x => x.TenantId, principalTable: "Tenant", principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_CustomerId", table: "EdgeTasks", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_Cust_Ten_Del", table: "EdgeTasks", columns: new[] { "CustomerId", "TenantId", "Deleted" });
            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_EdgeNodeId", table: "EdgeTasks", column: "EdgeNodeId");
            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_ExpireAt", table: "EdgeTasks", column: "ExpireAt");
            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_Gw_St_Created", table: "EdgeTasks", columns: new[] { "GatewayId", "Status", "CreatedAt" });
            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_Target_Status", table: "EdgeTasks", columns: new[] { "TargetKey", "Status" });
            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_TenantId", table: "EdgeTasks", column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EdgeTasks");
        }
    }
}
