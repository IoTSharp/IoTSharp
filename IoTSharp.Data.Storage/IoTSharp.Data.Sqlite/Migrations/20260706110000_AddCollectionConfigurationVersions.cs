using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContractVersion = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, collation: "NOCASE"),
                    GatewayId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EdgeNodeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfigurationHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    TaskCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    SourceId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    SourceVersion = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, collation: "NOCASE"),
                    SourceMetadata = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Payload = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionConfigVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ColCfgVer_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ColCfgVer_Device",
                        column: x => x.GatewayId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ColCfgVer_Tenant",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.AddColumn<Guid>(
                name: "CollectionConfigVersionId",
                table: "EdgeCollectionAssignments",
                type: "TEXT",
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

            // SQLite 不支持对已有表执行 AddForeignKeyOperation，版本引用列和索引先落表；
            // 其他关系型 provider 仍在对应迁移中创建外键约束。
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
