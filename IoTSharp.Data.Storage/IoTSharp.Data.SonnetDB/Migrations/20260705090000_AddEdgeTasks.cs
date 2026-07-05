using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SonnetDB.Migrations
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    ContractVersion = table.Column<string>(type: "STRING", maxLength: 64, nullable: true),
                    TaskType = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    TargetType = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    GatewayId = table.Column<Guid>(type: "STRING", nullable: false),
                    EdgeNodeId = table.Column<Guid>(type: "STRING", nullable: true),
                    TargetKey = table.Column<string>(type: "STRING", maxLength: 450, nullable: true),
                    RuntimeType = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    InstanceId = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "STRING", maxLength: 1024, nullable: true),
                    Progress = table.Column<int>(type: "INT", nullable: true),
                    Parameters = table.Column<string>(type: "STRING", nullable: true),
                    Metadata = table.Column<string>(type: "STRING", nullable: true),
                    RequestPayload = table.Column<string>(type: "STRING", nullable: true),
                    LastReceiptPayload = table.Column<string>(type: "STRING", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    SentAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    LastReceiptAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdgeTasks", x => x.Id);
                    table.ForeignKey(name: "FK_EdgeTasks_Customer_CustomerId", column: x => x.CustomerId, principalTable: "Customer", principalColumn: "Id");
                    table.ForeignKey(name: "FK_EdgeTasks_Device_GatewayId", column: x => x.GatewayId, principalTable: "Device", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_EdgeTasks_Tenant_TenantId", column: x => x.TenantId, principalTable: "Tenant", principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_CustomerId", table: "EdgeTasks", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_CustomerId_TenantId_Deleted", table: "EdgeTasks", columns: new[] { "CustomerId", "TenantId", "Deleted" });
            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_EdgeNodeId", table: "EdgeTasks", column: "EdgeNodeId");
            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_ExpireAt", table: "EdgeTasks", column: "ExpireAt");
            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_GatewayId_Status_CreatedAt", table: "EdgeTasks", columns: new[] { "GatewayId", "Status", "CreatedAt" });
            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_TargetKey_Status", table: "EdgeTasks", columns: new[] { "TargetKey", "Status" });
            migrationBuilder.CreateIndex(name: "IX_EdgeTasks_TenantId", table: "EdgeTasks", column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EdgeTasks");
        }
    }
}
