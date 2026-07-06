using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollectionTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TemplateKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    Description = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    SemanticModelId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReportPolicy = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionTemplates_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollectionTemplates_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionTemplates_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });


            migrationBuilder.CreateTable(
                name: "CollectionConnectionTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CollectionTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConnectionKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    ConnectionName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    Transport = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, collation: "NOCASE"),
                    EndpointRef = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    Host = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    Port = table.Column<int>(type: "INTEGER", nullable: true),
                    SerialPort = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    AuthType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    TimeoutMs = table.Column<int>(type: "INTEGER", nullable: false),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ProtocolOptions = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionConnectionTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionConnectionTemplates_CollectionTemplates_CollectionTemplateId",
                        column: x => x.CollectionTemplateId,
                        principalTable: "CollectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionPointTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CollectionTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConnectionKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    PointKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    SemanticId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    BindingId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    Address = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true, collation: "NOCASE"),
                    FieldPath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true, collation: "NOCASE"),
                    RawValueType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, collation: "NOCASE"),
                    ValueType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Access = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Length = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, collation: "NOCASE"),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProtocolOptions = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    QualityPolicy = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionPointTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionPointTemplates_CollectionTemplates_CollectionTemplateId",
                        column: x => x.CollectionTemplateId,
                        principalTable: "CollectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionProtocolTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CollectionTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Protocol = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProtocolKind = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    Parameters = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionProtocolTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionProtocolTemplates_CollectionTemplates_CollectionTemplateId",
                        column: x => x.CollectionTemplateId,
                        principalTable: "CollectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionMappingPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PointTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TargetName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    ValueType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, collation: "NOCASE"),
                    Group = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionMappingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionMappingPolicies_CollectionPointTemplates_PointTemplateId",
                        column: x => x.PointTemplateId,
                        principalTable: "CollectionPointTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionSamplingPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PointTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReadPeriodMs = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeoutMs = table.Column<int>(type: "INTEGER", nullable: true),
                    Trigger = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Deadband = table.Column<double>(type: "REAL", nullable: true),
                    ReportOnQualityChange = table.Column<bool>(type: "INTEGER", nullable: false),
                    Subscription = table.Column<bool>(type: "INTEGER", nullable: false),
                    AggregateHint = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE"),
                    Group = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionSamplingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionSamplingPolicies_CollectionPointTemplates_PointTemplateId",
                        column: x => x.PointTemplateId,
                        principalTable: "CollectionPointTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionTransformTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PointTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransformType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Parameters = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionTransformTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionTransformTemplates_CollectionPointTemplates_PointTemplateId",
                        column: x => x.PointTemplateId,
                        principalTable: "CollectionPointTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionConnectionTemplates_CollectionTemplateId_ConnectionKey",
                table: "CollectionConnectionTemplates",
                columns: new[] { "CollectionTemplateId", "ConnectionKey" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionMappingPolicies_PointTemplateId",
                table: "CollectionMappingPolicies",
                column: "PointTemplateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPointTemplates_CollectionTemplateId_BindingId",
                table: "CollectionPointTemplates",
                columns: new[] { "CollectionTemplateId", "BindingId" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPointTemplates_CollectionTemplateId_PointKey",
                table: "CollectionPointTemplates",
                columns: new[] { "CollectionTemplateId", "PointKey" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPointTemplates_CollectionTemplateId_SemanticId",
                table: "CollectionPointTemplates",
                columns: new[] { "CollectionTemplateId", "SemanticId" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPointTemplates_ConnectionKey",
                table: "CollectionPointTemplates",
                column: "ConnectionKey");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionProtocolTemplates_CollectionTemplateId",
                table: "CollectionProtocolTemplates",
                column: "CollectionTemplateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSamplingPolicies_PointTemplateId",
                table: "CollectionSamplingPolicies",
                column: "PointTemplateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTemplates_CustomerId_TenantId_Deleted",
                table: "CollectionTemplates",
                columns: new[] { "CustomerId", "TenantId", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTemplates_ProductId_Status",
                table: "CollectionTemplates",
                columns: new[] { "ProductId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTemplates_ProductId_TemplateKey_Deleted",
                table: "CollectionTemplates",
                columns: new[] { "ProductId", "TemplateKey", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTemplates_SemanticModelId",
                table: "CollectionTemplates",
                column: "SemanticModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTemplates_TenantId",
                table: "CollectionTemplates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTransformTemplates_PointTemplateId_Order",
                table: "CollectionTransformTemplates",
                columns: new[] { "PointTemplateId", "Order" });








        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionConnectionTemplates");

            migrationBuilder.DropTable(
                name: "CollectionMappingPolicies");

            migrationBuilder.DropTable(
                name: "CollectionProtocolTemplates");

            migrationBuilder.DropTable(
                name: "CollectionSamplingPolicies");

            migrationBuilder.DropTable(
                name: "CollectionTransformTemplates");


            migrationBuilder.DropTable(
                name: "CollectionPointTemplates");

            migrationBuilder.DropTable(
                name: "CollectionTemplates");
        }
    }
}
