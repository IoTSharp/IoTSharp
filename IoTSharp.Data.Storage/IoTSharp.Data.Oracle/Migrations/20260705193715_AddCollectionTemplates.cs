using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
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
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ProductId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TemplateKey = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    Name = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SemanticModelId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    Version = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Enabled = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    ReportPolicy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Metadata = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionTemplates_Custom~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollectionTemplates_Produc~",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionTemplates_Tenant~",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });


            migrationBuilder.CreateTable(
                name: "CollectionConnectionTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CollectionTemplateId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ConnectionKey = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    ConnectionName = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    Transport = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    EndpointRef = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    Host = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    Port = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    SerialPort = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    AuthType = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    TimeoutMs = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RetryCount = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ProtocolOptions = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Metadata = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionConnectionTempla~", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionConnectionTempla~",
                        column: x => x.CollectionTemplateId,
                        principalTable: "CollectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionPointTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CollectionTemplateId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ConnectionKey = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    PointKey = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    SemanticId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    BindingId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    Name = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    DisplayName = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    SourceType = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    Address = table.Column<string>(type: "NVARCHAR2(512)", maxLength: 512, nullable: true),
                    FieldPath = table.Column<string>(type: "NVARCHAR2(512)", maxLength: 512, nullable: true),
                    RawValueType = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    ValueType = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Access = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Length = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    Unit = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    Enabled = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    ProtocolOptions = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    QualityPolicy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Metadata = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionPointTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionPointTemplates_C~",
                        column: x => x.CollectionTemplateId,
                        principalTable: "CollectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionProtocolTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CollectionTemplateId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Protocol = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    ProtocolKind = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    Parameters = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Metadata = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionProtocolTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionProtocolTemplate~",
                        column: x => x.CollectionTemplateId,
                        principalTable: "CollectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionMappingPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PointTemplateId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TargetType = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    TargetName = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    ValueType = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    Unit = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    Group = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionMappingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionMappingPolicies_~",
                        column: x => x.PointTemplateId,
                        principalTable: "CollectionPointTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionSamplingPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PointTemplateId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ReadPeriodMs = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TimeoutMs = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Trigger = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Deadband = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    ReportOnQualityChange = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    Subscription = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    AggregateHint = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    Group = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionSamplingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionSamplingPolicies~",
                        column: x => x.PointTemplateId,
                        principalTable: "CollectionPointTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionTransformTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PointTemplateId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TransformType = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Order = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Parameters = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionTransformTemplat~", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionTransformTemplat~",
                        column: x => x.PointTemplateId,
                        principalTable: "CollectionPointTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionConnectionTempla~",
                table: "CollectionConnectionTemplates",
                columns: new[] { "CollectionTemplateId", "ConnectionKey" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionMappingPolicies_~",
                table: "CollectionMappingPolicies",
                column: "PointTemplateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPointTemplates_~1",
                table: "CollectionPointTemplates",
                columns: new[] { "CollectionTemplateId", "BindingId" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPointTemplates_~2",
                table: "CollectionPointTemplates",
                columns: new[] { "CollectionTemplateId", "PointKey" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPointTemplates_~3",
                table: "CollectionPointTemplates",
                columns: new[] { "CollectionTemplateId", "SemanticId" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPointTemplates_C~",
                table: "CollectionPointTemplates",
                column: "ConnectionKey");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionProtocolTemplate~",
                table: "CollectionProtocolTemplates",
                column: "CollectionTemplateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSamplingPolicies~",
                table: "CollectionSamplingPolicies",
                column: "PointTemplateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTemplates_Custom~",
                table: "CollectionTemplates",
                columns: new[] { "CustomerId", "TenantId", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTemplates_Produ~1",
                table: "CollectionTemplates",
                columns: new[] { "ProductId", "TemplateKey", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTemplates_Produc~",
                table: "CollectionTemplates",
                columns: new[] { "ProductId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTemplates_Semant~",
                table: "CollectionTemplates",
                column: "SemanticModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTemplates_Tenant~",
                table: "CollectionTemplates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTransformTemplat~",
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
