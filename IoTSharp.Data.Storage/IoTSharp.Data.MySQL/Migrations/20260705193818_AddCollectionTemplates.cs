using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.MySQL.Migrations
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
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProductId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TemplateKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SemanticModelId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReportPolicy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Metadata = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");


            migrationBuilder.CreateTable(
                name: "CollectionConnectionTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CollectionTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ConnectionKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConnectionName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Transport = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EndpointRef = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Host = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Port = table.Column<int>(type: "int", nullable: true),
                    SerialPort = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuthType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TimeoutMs = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ProtocolOptions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Metadata = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionConnectionTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionConnectionTemplates_CollectionTemplates_Collection~",
                        column: x => x.CollectionTemplateId,
                        principalTable: "CollectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CollectionPointTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CollectionTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ConnectionKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PointKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SemanticId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BindingId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FieldPath = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawValueType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValueType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Access = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Length = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Unit = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProtocolOptions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QualityPolicy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Metadata = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionPointTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionPointTemplates_CollectionTemplates_CollectionTempl~",
                        column: x => x.CollectionTemplateId,
                        principalTable: "CollectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CollectionProtocolTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CollectionTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Protocol = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProtocolKind = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Parameters = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Metadata = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionProtocolTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionProtocolTemplates_CollectionTemplates_CollectionTe~",
                        column: x => x.CollectionTemplateId,
                        principalTable: "CollectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CollectionMappingPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PointTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TargetType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValueType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Unit = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Group = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionMappingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionMappingPolicies_CollectionPointTemplates_PointTemp~",
                        column: x => x.PointTemplateId,
                        principalTable: "CollectionPointTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CollectionSamplingPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PointTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReadPeriodMs = table.Column<int>(type: "int", nullable: false),
                    TimeoutMs = table.Column<int>(type: "int", nullable: true),
                    Trigger = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Deadband = table.Column<double>(type: "double", nullable: true),
                    ReportOnQualityChange = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Subscription = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AggregateHint = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Group = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionSamplingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionSamplingPolicies_CollectionPointTemplates_PointTem~",
                        column: x => x.PointTemplateId,
                        principalTable: "CollectionPointTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CollectionTransformTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PointTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TransformType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Parameters = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionTransformTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionTransformTemplates_CollectionPointTemplates_PointT~",
                        column: x => x.PointTemplateId,
                        principalTable: "CollectionPointTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionConnectionTemplates_CollectionTemplateId_Connectio~",
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
