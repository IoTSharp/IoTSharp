using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260706110000_AddCollectionConfigurationVersions")]
    public partial class AddCollectionConfigurationVersions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollectionConfigVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ContractVersion = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    GatewayId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EdgeNodeId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    Version = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ConfigurationHash = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    TaskCount = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SourceType = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    SourceId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    SourceVersion = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    SourceMetadata = table.Column<string>(type: "NCLOB", nullable: true),
                    Payload = table.Column<string>(type: "NCLOB", nullable: true),
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
                    table.PrimaryKey("PK_CollectionConfigVersions", x => x.Id);
                    table.ForeignKey(name: "FK_ColCfgVer_Customer", column: x => x.CustomerId, principalTable: "Customer", principalColumn: "Id");
                    table.ForeignKey(name: "FK_ColCfgVer_Device", column: x => x.GatewayId, principalTable: "Device", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(name: "FK_ColCfgVer_Tenant", column: x => x.TenantId, principalTable: "Tenant", principalColumn: "Id");
                });

            migrationBuilder.AddColumn<Guid>(
                name: "CollectionConfigVersionId",
                table: "EdgeCollectionAssignments",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.CreateIndex(name: "IX_ColCfgVer_Hash", table: "CollectionConfigVersions", column: "ConfigurationHash");
            migrationBuilder.CreateIndex(name: "IX_ColCfgVer_Customer", table: "CollectionConfigVersions", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_ColCfgVer_CustTenDel", table: "CollectionConfigVersions", columns: new[] { "CustomerId", "TenantId", "Deleted" });
            migrationBuilder.CreateIndex(name: "IX_ColCfgVer_EdgeNode", table: "CollectionConfigVersions", column: "EdgeNodeId");
            migrationBuilder.CreateIndex(name: "IX_ColCfgVer_GwCreated", table: "CollectionConfigVersions", columns: new[] { "GatewayId", "CreatedAt" });
            migrationBuilder.CreateIndex(name: "IX_ColCfgVer_GwVersion", table: "CollectionConfigVersions", columns: new[] { "GatewayId", "Version" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ColCfgVer_Source", table: "CollectionConfigVersions", columns: new[] { "SourceType", "SourceId", "SourceVersion" });
            migrationBuilder.CreateIndex(name: "IX_ColCfgVer_Tenant", table: "CollectionConfigVersions", column: "TenantId");
            migrationBuilder.CreateIndex(name: "IX_EdgeColAssign_CfgVer", table: "EdgeCollectionAssignments", column: "CollectionConfigVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EdgeColAssign_CfgVer",
                table: "EdgeCollectionAssignments",
                column: "CollectionConfigVersionId",
                principalTable: "CollectionConfigVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EdgeColAssign_CfgVer",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropTable(name: "CollectionConfigVersions");

            migrationBuilder.DropIndex(
                name: "IX_EdgeColAssign_CfgVer",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropColumn(
                name: "CollectionConfigVersionId",
                table: "EdgeCollectionAssignments");
        }
    }
}
