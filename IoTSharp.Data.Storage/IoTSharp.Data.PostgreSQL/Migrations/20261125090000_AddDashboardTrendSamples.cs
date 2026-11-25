using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.PostgreSQL.Migrations
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
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CollectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Received = table.Column<long>(type: "bigint", nullable: true),
                    Processed = table.Column<long>(type: "bigint", nullable: true),
                    Telemetry = table.Column<long>(type: "bigint", nullable: true),
                    Attributes = table.Column<long>(type: "bigint", nullable: true),
                    OnlineDevices = table.Column<int>(type: "integer", nullable: true),
                    SnapshotUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardTrendSamples", x => new { x.TenantId, x.StartUtc, x.CollectorId });
                });

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
