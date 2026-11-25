using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.MySQL.Migrations
{
    /// <summary>持久化按租户、时间和采集实例划分的仪表盘趋势样本。</summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261125090000_AddDashboardTrendSamples")]
    public sealed class AddDashboardTrendSamples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardTrendSamples",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StartUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CollectorId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Received = table.Column<long>(type: "bigint", nullable: true),
                    Processed = table.Column<long>(type: "bigint", nullable: true),
                    Telemetry = table.Column<long>(type: "bigint", nullable: true),
                    Attributes = table.Column<long>(type: "bigint", nullable: true),
                    OnlineDevices = table.Column<int>(type: "int", nullable: true),
                    SnapshotUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardTrendSamples", x => new { x.TenantId, x.StartUtc, x.CollectorId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardTrend_StartUtc",
                table: "DashboardTrendSamples",
                column: "StartUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DashboardTrendSamples");
        }
    }
}
