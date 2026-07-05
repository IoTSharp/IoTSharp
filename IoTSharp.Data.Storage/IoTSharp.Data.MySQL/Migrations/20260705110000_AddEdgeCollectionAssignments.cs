using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.MySQL.Migrations
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
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ContractVersion = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewayId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EdgeNodeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TargetKey = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuntimeType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstanceId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConfigurationVersion = table.Column<int>(type: "int", nullable: false),
                    ConfigurationHash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaskCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceVersion = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Metadata = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastPulledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TenantId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdgeCollectionAssignments", x => x.Id);
                    table.ForeignKey(name: "FK_EdgeCollectionAssignments_Customer_CustomerId", column: x => x.CustomerId, principalTable: "Customer", principalColumn: "Id");
                    table.ForeignKey(name: "FK_EdgeCollectionAssignments_Device_GatewayId", column: x => x.GatewayId, principalTable: "Device", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(name: "FK_EdgeCollectionAssignments_Tenant_TenantId", column: x => x.TenantId, principalTable: "Tenant", principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
