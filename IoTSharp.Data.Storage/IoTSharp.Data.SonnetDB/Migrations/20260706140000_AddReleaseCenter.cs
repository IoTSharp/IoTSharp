using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SonnetDB.Migrations
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    Name = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "STRING", maxLength: 1024, nullable: true),
                    PlanType = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    PackageId = table.Column<Guid>(type: "STRING", nullable: true),
                    RollbackPackageId = table.Column<Guid>(type: "STRING", nullable: true),
                    ConfirmationPolicy = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    BatchSize = table.Column<int>(type: "INT", nullable: false),
                    ContinueOnFailure = table.Column<bool>(type: "BOOL", nullable: false),
                    CurrentBatchNo = table.Column<int>(type: "INT", nullable: false),
                    TotalTaskCount = table.Column<int>(type: "INT", nullable: false),
                    PendingTaskCount = table.Column<int>(type: "INT", nullable: false),
                    RunningTaskCount = table.Column<int>(type: "INT", nullable: false),
                    SucceededTaskCount = table.Column<int>(type: "INT", nullable: false),
                    FailedTaskCount = table.Column<int>(type: "INT", nullable: false),
                    Metadata = table.Column<string>(type: "STRING", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    CreatedBy = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "ReleaseTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    PlanId = table.Column<Guid>(type: "STRING", nullable: false),
                    PackageId = table.Column<Guid>(type: "STRING", nullable: true),
                    TargetType = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    TargetId = table.Column<Guid>(type: "STRING", nullable: true),
                    GatewayId = table.Column<Guid>(type: "STRING", nullable: true),
                    EdgeNodeId = table.Column<Guid>(type: "STRING", nullable: true),
                    TargetKey = table.Column<string>(type: "STRING", maxLength: 450, nullable: true),
                    RuntimeType = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    InstanceId = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    BatchNo = table.Column<int>(type: "INT", nullable: false),
                    Status = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    IsRollback = table.Column<bool>(type: "BOOL", nullable: false),
                    EdgeTaskId = table.Column<Guid>(type: "STRING", nullable: true),
                    Message = table.Column<string>(type: "STRING", maxLength: 1024, nullable: true),
                    Progress = table.Column<int>(type: "INT", nullable: true),
                    Metadata = table.Column<string>(type: "STRING", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    DispatchedAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    LastReceiptAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "ReleaseReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    PlanId = table.Column<Guid>(type: "STRING", nullable: false),
                    ReleaseTaskId = table.Column<Guid>(type: "STRING", nullable: false),
                    EdgeTaskId = table.Column<Guid>(type: "STRING", nullable: true),
                    TargetType = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    TargetId = table.Column<Guid>(type: "STRING", nullable: true),
                    GatewayId = table.Column<Guid>(type: "STRING", nullable: true),
                    EdgeNodeId = table.Column<Guid>(type: "STRING", nullable: true),
                    TargetKey = table.Column<string>(type: "STRING", maxLength: 450, nullable: true),
                    RuntimeType = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    InstanceId = table.Column<string>(type: "STRING", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "STRING", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "STRING", maxLength: 1024, nullable: true),
                    Progress = table.Column<int>(type: "INT", nullable: true),
                    Result = table.Column<string>(type: "STRING", nullable: true),
                    Metadata = table.Column<string>(type: "STRING", nullable: true),
                    Payload = table.Column<string>(type: "STRING", nullable: true),
                    ReportedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                });







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
