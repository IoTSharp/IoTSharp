using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.MySQL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260704090000_AddEdgeNodes")]
    public partial class AddEdgeNodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EdgeNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    GatewayId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(nullable: true),
                    RuntimeType = table.Column<string>(type: "varchar(255)", nullable: true),
                    RuntimeName = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: true),
                    InstanceId = table.Column<string>(type: "varchar(255)", nullable: true),
                    Platform = table.Column<string>(nullable: true),
                    HostName = table.Column<string>(nullable: true),
                    IpAddress = table.Column<string>(nullable: true),
                    Status = table.Column<string>(type: "varchar(255)", nullable: true),
                    Healthy = table.Column<bool>(nullable: true),
                    UptimeSeconds = table.Column<long>(nullable: true),
                    LastRegistrationDateTime = table.Column<DateTime>(nullable: true),
                    LastHeartbeatDateTime = table.Column<DateTime>(nullable: true),
                    Capabilities = table.Column<string>(nullable: true),
                    Metadata = table.Column<string>(nullable: true),
                    Metrics = table.Column<string>(nullable: true),
                    Deleted = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    TenantId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdgeNodes", x => x.Id);
                    table.ForeignKey(name: "FK_EdgeNodes_Customer_CustomerId", column: x => x.CustomerId, principalTable: "Customer", principalColumn: "Id");
                    table.ForeignKey(name: "FK_EdgeNodes_Device_GatewayId", column: x => x.GatewayId, principalTable: "Device", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_EdgeNodes_Tenant_TenantId", column: x => x.TenantId, principalTable: "Tenant", principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(name: "IX_EdgeNodes_CustomerId", table: "EdgeNodes", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_EdgeNodes_CustomerId_TenantId_Deleted", table: "EdgeNodes", columns: new[] { "CustomerId", "TenantId", "Deleted" });
            migrationBuilder.CreateIndex(name: "IX_EdgeNodes_GatewayId", table: "EdgeNodes", column: "GatewayId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_EdgeNodes_InstanceId", table: "EdgeNodes", column: "InstanceId");
            migrationBuilder.CreateIndex(name: "IX_EdgeNodes_RuntimeType_Status", table: "EdgeNodes", columns: new[] { "RuntimeType", "Status" });
            migrationBuilder.CreateIndex(name: "IX_EdgeNodes_TenantId", table: "EdgeNodes", column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EdgeNodes");
        }
    }
}
