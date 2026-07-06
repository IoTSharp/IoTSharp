using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class AddReleaseCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReleasePlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlanType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PackageId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RollbackPackageId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ConfirmationPolicy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BatchSize = table.Column<int>(type: "int", nullable: false),
                    ContinueOnFailure = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CurrentBatchNo = table.Column<int>(type: "int", nullable: false),
                    TotalTaskCount = table.Column<int>(type: "int", nullable: false),
                    PendingTaskCount = table.Column<int>(type: "int", nullable: false),
                    RunningTaskCount = table.Column<int>(type: "int", nullable: false),
                    SucceededTaskCount = table.Column<int>(type: "int", nullable: false),
                    FailedTaskCount = table.Column<int>(type: "int", nullable: false),
                    Metadata = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_ReleasePlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleasePlans_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReleasePlans_ReleasePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ReleasePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReleasePlans_ReleasePackages_RollbackPackageId",
                        column: x => x.RollbackPackageId,
                        principalTable: "ReleasePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReleasePlans_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReleaseTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlanId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PackageId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TargetType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    GatewayId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    EdgeNodeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TargetKey = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuntimeType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstanceId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BatchNo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsRollback = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EdgeTaskId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Message = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Progress = table.Column<int>(type: "int", nullable: true),
                    Metadata = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DispatchedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastReceiptAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TenantId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseTasks_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReleaseTasks_EdgeTasks_EdgeTaskId",
                        column: x => x.EdgeTaskId,
                        principalTable: "EdgeTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReleaseTasks_ReleasePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ReleasePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReleaseTasks_ReleasePlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "ReleasePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReleaseTasks_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReleaseReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlanId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReleaseTaskId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EdgeTaskId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TargetType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    GatewayId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    EdgeNodeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TargetKey = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuntimeType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstanceId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Progress = table.Column<int>(type: "int", nullable: true),
                    Result = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Metadata = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Payload = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TenantId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseReceipts_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReleaseReceipts_ReleasePlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "ReleasePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReleaseReceipts_ReleaseTasks_ReleaseTaskId",
                        column: x => x.ReleaseTaskId,
                        principalTable: "ReleaseTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReleaseReceipts_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");







            migrationBuilder.CreateIndex(
                name: "IX_ReleasePlans_CreatedAt",
                table: "ReleasePlans",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReleasePlans_CustomerId_TenantId_Deleted",
                table: "ReleasePlans",
                columns: new[] { "CustomerId", "TenantId", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ReleasePlans_PackageId",
                table: "ReleasePlans",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleasePlans_RollbackPackageId",
                table: "ReleasePlans",
                column: "RollbackPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleasePlans_TenantId_Status_CreatedAt",
                table: "ReleasePlans",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseReceipts_CustomerId_TenantId_Deleted",
                table: "ReleaseReceipts",
                columns: new[] { "CustomerId", "TenantId", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseReceipts_EdgeNodeId",
                table: "ReleaseReceipts",
                column: "EdgeNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseReceipts_EdgeTaskId",
                table: "ReleaseReceipts",
                column: "EdgeTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseReceipts_GatewayId",
                table: "ReleaseReceipts",
                column: "GatewayId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseReceipts_PlanId_ReportedAt",
                table: "ReleaseReceipts",
                columns: new[] { "PlanId", "ReportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseReceipts_ReleaseTaskId_ReportedAt",
                table: "ReleaseReceipts",
                columns: new[] { "ReleaseTaskId", "ReportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseReceipts_TargetKey_Status",
                table: "ReleaseReceipts",
                columns: new[] { "TargetKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseReceipts_TenantId",
                table: "ReleaseReceipts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseTasks_CustomerId_TenantId_Deleted",
                table: "ReleaseTasks",
                columns: new[] { "CustomerId", "TenantId", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseTasks_EdgeNodeId",
                table: "ReleaseTasks",
                column: "EdgeNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseTasks_EdgeTaskId",
                table: "ReleaseTasks",
                column: "EdgeTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseTasks_GatewayId",
                table: "ReleaseTasks",
                column: "GatewayId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseTasks_PackageId",
                table: "ReleaseTasks",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseTasks_PlanId_BatchNo_Status",
                table: "ReleaseTasks",
                columns: new[] { "PlanId", "BatchNo", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseTasks_TargetKey_Status",
                table: "ReleaseTasks",
                columns: new[] { "TargetKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseTasks_TenantId",
                table: "ReleaseTasks",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReleaseReceipts");

            migrationBuilder.DropTable(
                name: "ReleaseTasks");

            migrationBuilder.DropTable(
                name: "ReleasePlans");

        }
    }
}
