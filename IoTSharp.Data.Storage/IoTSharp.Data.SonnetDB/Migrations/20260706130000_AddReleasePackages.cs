using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SonnetDB.Migrations
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    ContractVersion = table.Column<string>(type: "STRING", maxLength: 64, nullable: true),
                    PackageType = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    PackageKey = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    Name = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    Version = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    TargetRuntimeType = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    TargetRuntimeVersion = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    FileName = table.Column<string>(type: "STRING", maxLength: 512, nullable: true),
                    ContentType = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    Size = table.Column<long>(type: "BIGINT", nullable: false),
                    Sha256 = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    BlobPath = table.Column<string>(type: "STRING", maxLength: 1024, nullable: true),
                    DownloadToken = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    Metadata = table.Column<string>(type: "STRING", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    CreatedBy = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                });

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
