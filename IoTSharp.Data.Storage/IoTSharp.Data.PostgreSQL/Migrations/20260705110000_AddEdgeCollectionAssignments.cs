using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.PostgreSQL.Migrations
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    GatewayId = table.Column<Guid>(type: "uuid", nullable: false),
                    EdgeNodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetKey = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    RuntimeType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstanceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ConfigurationVersion = table.Column<int>(type: "integer", nullable: false),
                    ConfigurationHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TaskCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastPulledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdgeCollectionAssignments", x => x.Id);
                    table.ForeignKey(name: "FK_EdgeCollectionAssignments_Customer_CustomerId", column: x => x.CustomerId, principalTable: "Customer", principalColumn: "Id");
                    table.ForeignKey(name: "FK_EdgeCollectionAssignments_Device_GatewayId", column: x => x.GatewayId, principalTable: "Device", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(name: "FK_EdgeCollectionAssignments_Tenant_TenantId", column: x => x.TenantId, principalTable: "Tenant", principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(name: "IX_EdgeCollectionAssignments_AssignedAt", table: "EdgeCollectionAssignments", column: "AssignedAt");
            migrationBuilder.CreateIndex(name: "IX_EdgeCollectionAssignments_ConfigurationHash", table: "EdgeCollectionAssignments", column: "ConfigurationHash");
            migrationBuilder.CreateIndex(name: "IX_EdgeCollectionAssignments_CustomerId", table: "EdgeCollectionAssignments", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_EdgeCollectionAssignments_CustomerId_TenantId_Deleted", table: "EdgeCollectionAssignments", columns: new[] { "CustomerId", "TenantId", "Deleted" });
            migrationBuilder.CreateIndex(name: "IX_EdgeCollectionAssignments_EdgeNodeId", table: "EdgeCollectionAssignments", column: "EdgeNodeId");
            migrationBuilder.CreateIndex(name: "IX_EdgeCollectionAssignments_GatewayId_ConfigurationVersion", table: "EdgeCollectionAssignments", columns: new[] { "GatewayId", "ConfigurationVersion" });
            migrationBuilder.CreateIndex(name: "IX_EdgeCollectionAssignments_GatewayId_Status_ConfigurationVersion", table: "EdgeCollectionAssignments", columns: new[] { "GatewayId", "Status", "ConfigurationVersion" });
            migrationBuilder.CreateIndex(name: "IX_EdgeCollectionAssignments_TargetKey_Status", table: "EdgeCollectionAssignments", columns: new[] { "TargetKey", "Status" });
            migrationBuilder.CreateIndex(name: "IX_EdgeCollectionAssignments_TenantId", table: "EdgeCollectionAssignments", column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EdgeCollectionAssignments");
        }
    }
}
