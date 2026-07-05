using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260705110000_AddEdgeCollectionAssignments")]
    public partial class AddEdgeCollectionAssignments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EdgeCollectionAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ContractVersion = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    TargetType = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    GatewayId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EdgeNodeId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    TargetKey = table.Column<string>(type: "NVARCHAR2(450)", maxLength: 450, nullable: true),
                    RuntimeType = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    InstanceId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    ConfigurationVersion = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ConfigurationHash = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    TaskCount = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    SourceType = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    SourceId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    SourceVersion = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    Metadata = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    LastPulledAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdgeCollectionAssignments", x => x.Id);
                    table.ForeignKey(name: "FK_EdgeColAssign_Customer", column: x => x.CustomerId, principalTable: "Customer", principalColumn: "Id");
                    table.ForeignKey(name: "FK_EdgeColAssign_Device", column: x => x.GatewayId, principalTable: "Device", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(name: "FK_EdgeColAssign_Tenant", column: x => x.TenantId, principalTable: "Tenant", principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(name: "IX_EdgeColAssign_AssignedAt", table: "EdgeCollectionAssignments", column: "AssignedAt");
            migrationBuilder.CreateIndex(name: "IX_EdgeColAssign_Hash", table: "EdgeCollectionAssignments", column: "ConfigurationHash");
            migrationBuilder.CreateIndex(name: "IX_EdgeColAssign_Customer", table: "EdgeCollectionAssignments", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_EdgeColAssign_Cust_Ten_Del", table: "EdgeCollectionAssignments", columns: new[] { "CustomerId", "TenantId", "Deleted" });
            migrationBuilder.CreateIndex(name: "IX_EdgeColAssign_EdgeNode", table: "EdgeCollectionAssignments", column: "EdgeNodeId");
            migrationBuilder.CreateIndex(name: "IX_EdgeColAssign_Gw_Ver", table: "EdgeCollectionAssignments", columns: new[] { "GatewayId", "ConfigurationVersion" });
            migrationBuilder.CreateIndex(name: "IX_EdgeColAssign_Gw_St_Ver", table: "EdgeCollectionAssignments", columns: new[] { "GatewayId", "Status", "ConfigurationVersion" });
            migrationBuilder.CreateIndex(name: "IX_EdgeColAssign_Target_St", table: "EdgeCollectionAssignments", columns: new[] { "TargetKey", "Status" });
            migrationBuilder.CreateIndex(name: "IX_EdgeColAssign_Tenant", table: "EdgeCollectionAssignments", column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EdgeCollectionAssignments");
        }
    }
}
