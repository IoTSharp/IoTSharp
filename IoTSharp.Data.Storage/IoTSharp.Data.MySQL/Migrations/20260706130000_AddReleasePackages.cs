using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.MySQL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260706130000_AddReleasePackages")]
    public partial class AddReleasePackages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReleasePackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ContractVersion = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    PackageType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
                    PackageKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    TargetRuntimeType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    TargetRuntimeVersion = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    FileName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    ContentType = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    BlobPath = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    DownloadToken = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    Metadata = table.Column<string>(type: "longtext", nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
                    Deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TenantId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleasePackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleasePackages_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReleasePackages_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(name: "IX_ReleasePackages_CreatedAt", table: "ReleasePackages", column: "CreatedAt");
            migrationBuilder.CreateIndex(name: "IX_ReleasePackages_CustomerId", table: "ReleasePackages", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_ReleasePackages_CustomerId_TenantId_Deleted", table: "ReleasePackages", columns: new[] { "CustomerId", "TenantId", "Deleted" });
            migrationBuilder.CreateIndex(name: "IX_ReleasePackages_PackageKey_Version_TargetRuntimeType_Deleted", table: "ReleasePackages", columns: new[] { "PackageKey", "Version", "TargetRuntimeType", "Deleted" });
            migrationBuilder.CreateIndex(name: "IX_ReleasePackages_PackageType", table: "ReleasePackages", column: "PackageType");
            migrationBuilder.CreateIndex(name: "IX_ReleasePackages_Sha256", table: "ReleasePackages", column: "Sha256");
            migrationBuilder.CreateIndex(name: "IX_ReleasePackages_TenantId", table: "ReleasePackages", column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ReleasePackages");
        }
    }
}
