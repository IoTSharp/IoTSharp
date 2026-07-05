using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SonnetDB.Migrations
{
    /// <inheritdoc />
    public partial class RenameProduceDomainToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Produces_OwnerId",
                table: "DataStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_Device_Produces_ProduceId",
                table: "Device");

            migrationBuilder.DropTable(
                name: "ProduceDataMappings");

            migrationBuilder.DropTable(
                name: "ProduceDictionaries");

            migrationBuilder.DropTable(
                name: "Produces");

            migrationBuilder.DropIndex(
                name: "IX_Device_ProduceId",
                table: "Device");

            migrationBuilder.RenameColumn(
                name: "ProduceId",
                table: "Device",
                newName: "ProductId");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    Name = table.Column<string>(type: "STRING", nullable: true),
                    Icon = table.Column<string>(type: "STRING", nullable: true),
                    GatewayType = table.Column<int>(type: "INT", nullable: false),
                    GatewayConfiguration = table.Column<string>(type: "STRING", nullable: true),
                    DefaultTimeout = table.Column<int>(type: "INT", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true),
                    DefaultIdentityType = table.Column<int>(type: "INT", nullable: false),
                    Description = table.Column<string>(type: "STRING", nullable: true),
                    DefaultDeviceType = table.Column<int>(type: "INT", nullable: false),
                    ProductToken = table.Column<string>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductDataMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    ProductId = table.Column<Guid>(type: "STRING", nullable: true),
                    ProductKeyName = table.Column<string>(type: "STRING", nullable: true),
                    DataCatalog = table.Column<int>(type: "INT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "STRING", nullable: false),
                    DeviceKeyName = table.Column<string>(type: "STRING", nullable: true),
                    Description = table.Column<string>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDataMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDataMappings_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductDictionaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    KeyName = table.Column<string>(type: "STRING", nullable: false),
                    DisplayName = table.Column<string>(type: "STRING", nullable: false),
                    Unit = table.Column<string>(type: "STRING", nullable: false),
                    UnitExpression = table.Column<string>(type: "STRING", nullable: false),
                    UnitConvert = table.Column<bool>(type: "BOOL", nullable: false),
                    KeyDesc = table.Column<string>(type: "STRING", nullable: true),
                    DefaultValue = table.Column<string>(type: "STRING", nullable: true),
                    Display = table.Column<bool>(type: "BOOL", nullable: false),
                    Place0 = table.Column<string>(type: "STRING", nullable: false),
                    PlaceOrder0 = table.Column<string>(type: "STRING", nullable: false),
                    Place1 = table.Column<string>(type: "STRING", nullable: false),
                    PlaceOrder1 = table.Column<string>(type: "STRING", nullable: false),
                    Place2 = table.Column<string>(type: "STRING", nullable: false),
                    PlaceOrder2 = table.Column<string>(type: "STRING", nullable: false),
                    Place3 = table.Column<string>(type: "STRING", nullable: false),
                    PlaceOrder3 = table.Column<string>(type: "STRING", nullable: false),
                    Place4 = table.Column<string>(type: "STRING", nullable: false),
                    PlaceOrder4 = table.Column<string>(type: "STRING", nullable: false),
                    Place5 = table.Column<string>(type: "STRING", nullable: false),
                    PlaceOrder5 = table.Column<string>(type: "STRING", nullable: false),
                    DataType = table.Column<int>(type: "INT", nullable: false),
                    Tag = table.Column<string>(type: "STRING", nullable: true),
                    Customer = table.Column<Guid>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    ProductId = table.Column<Guid>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDictionaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDictionaries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Device_ProductId",
                table: "Device",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDataMappings_ProductId",
                table: "ProductDataMappings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDictionaries_ProductId",
                table: "ProductDictionaries",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CustomerId",
                table: "Products",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId",
                table: "Products",
                column: "TenantId");

            // SonnetDB 当前不支持 ALTER TABLE 追加外键；关系由 EF 模型保持。
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductDataMappings");

            migrationBuilder.DropTable(
                name: "ProductDictionaries");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Device_ProductId",
                table: "Device");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Device",
                newName: "ProduceId");

            migrationBuilder.CreateTable(
                name: "Produces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    DefaultDeviceType = table.Column<int>(type: "INT", nullable: false),
                    DefaultIdentityType = table.Column<int>(type: "INT", nullable: false),
                    DefaultTimeout = table.Column<int>(type: "INT", nullable: false),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    Description = table.Column<string>(type: "STRING", nullable: true),
                    GatewayConfiguration = table.Column<string>(type: "STRING", nullable: true),
                    GatewayType = table.Column<int>(type: "INT", nullable: false),
                    Icon = table.Column<string>(type: "STRING", nullable: true),
                    Name = table.Column<string>(type: "STRING", nullable: true),
                    ProduceToken = table.Column<string>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produces_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Produces_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProduceDataMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    ProduceId = table.Column<Guid>(type: "STRING", nullable: true),
                    DataCatalog = table.Column<int>(type: "INT", nullable: false),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    Description = table.Column<string>(type: "STRING", nullable: true),
                    DeviceId = table.Column<Guid>(type: "STRING", nullable: false),
                    DeviceKeyName = table.Column<string>(type: "STRING", nullable: true),
                    ProduceKeyName = table.Column<string>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduceDataMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProduceDataMappings_Produces_ProduceId",
                        column: x => x.ProduceId,
                        principalTable: "Produces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProduceDictionaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    Customer = table.Column<Guid>(type: "STRING", nullable: true),
                    DataType = table.Column<int>(type: "INT", nullable: false),
                    DefaultValue = table.Column<string>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    Display = table.Column<bool>(type: "BOOL", nullable: false),
                    DisplayName = table.Column<string>(type: "STRING", nullable: false),
                    KeyDesc = table.Column<string>(type: "STRING", nullable: true),
                    KeyName = table.Column<string>(type: "STRING", nullable: false),
                    Place0 = table.Column<string>(type: "STRING", nullable: false),
                    Place1 = table.Column<string>(type: "STRING", nullable: false),
                    Place2 = table.Column<string>(type: "STRING", nullable: false),
                    Place3 = table.Column<string>(type: "STRING", nullable: false),
                    Place4 = table.Column<string>(type: "STRING", nullable: false),
                    Place5 = table.Column<string>(type: "STRING", nullable: false),
                    PlaceOrder0 = table.Column<string>(type: "STRING", nullable: false),
                    PlaceOrder1 = table.Column<string>(type: "STRING", nullable: false),
                    PlaceOrder2 = table.Column<string>(type: "STRING", nullable: false),
                    PlaceOrder3 = table.Column<string>(type: "STRING", nullable: false),
                    PlaceOrder4 = table.Column<string>(type: "STRING", nullable: false),
                    PlaceOrder5 = table.Column<string>(type: "STRING", nullable: false),
                    ProduceId = table.Column<Guid>(type: "STRING", nullable: true),
                    Tag = table.Column<string>(type: "STRING", nullable: true),
                    Unit = table.Column<string>(type: "STRING", nullable: false),
                    UnitConvert = table.Column<bool>(type: "BOOL", nullable: false),
                    UnitExpression = table.Column<string>(type: "STRING", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduceDictionaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProduceDictionaries_Produces_ProduceId",
                        column: x => x.ProduceId,
                        principalTable: "Produces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Device_ProduceId",
                table: "Device",
                column: "ProduceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProduceDataMappings_ProduceId",
                table: "ProduceDataMappings",
                column: "ProduceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProduceDictionaries_ProduceId",
                table: "ProduceDictionaries",
                column: "ProduceId");

            migrationBuilder.CreateIndex(
                name: "IX_Produces_CustomerId",
                table: "Produces",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Produces_TenantId",
                table: "Produces",
                column: "TenantId");

            // SonnetDB 当前不支持 ALTER TABLE 追加外键；关系由 EF 模型保持。
        }
    }
}
