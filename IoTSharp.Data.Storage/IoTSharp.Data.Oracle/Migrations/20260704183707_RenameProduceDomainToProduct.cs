using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
{
    /// <inheritdoc />
    public partial class RenameProduceDomainToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Produces_Owner~",
                table: "DataStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_Device_Produces_ProduceId",
                table: "Device");

            migrationBuilder.Sql("""
                BEGIN
                    EXECUTE IMMEDIATE 'DROP TABLE "ProduceDataMappings"';
                EXCEPTION
                    WHEN OTHERS THEN
                        IF SQLCODE != -942 THEN
                            RAISE;
                        END IF;
                END;
                """);

            migrationBuilder.DropTable(
                name: "ProduceDictionaries");

            migrationBuilder.DropTable(
                name: "Produces");

            migrationBuilder.RenameColumn(
                name: "ProduceId",
                table: "Device",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Device_ProduceId",
                table: "Device",
                newName: "IX_Device_ProductId");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Icon = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GatewayType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GatewayConfiguration = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DefaultTimeout = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    DefaultIdentityType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DefaultDeviceType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ProductToken = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Customer_Customer~",
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
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ProductId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    ProductKeyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DataCatalog = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DeviceId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DeviceKeyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDataMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDataMappings_Produc~",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductDictionaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    KeyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DisplayName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Unit = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    UnitExpression = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    UnitConvert = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    KeyDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DefaultValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Display = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    Place0 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PlaceOrder0 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Place1 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PlaceOrder1 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Place2 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PlaceOrder2 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Place3 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PlaceOrder3 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Place4 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PlaceOrder4 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Place5 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PlaceOrder5 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DataType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Tag = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Customer = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    ProductId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDictionaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDictionaries_Produc~",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductDataMappings_Produc~",
                table: "ProductDataMappings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDictionaries_Produc~",
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

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Products_Owner~",
                table: "DataStorage",
                column: "OwnerId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_Products_ProductId",
                table: "Device",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Products_Owner~",
                table: "DataStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_Device_Products_ProductId",
                table: "Device");

            migrationBuilder.DropTable(
                name: "ProductDataMappings");

            migrationBuilder.DropTable(
                name: "ProductDictionaries");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Device",
                newName: "ProduceId");

            migrationBuilder.RenameIndex(
                name: "IX_Device_ProductId",
                table: "Device",
                newName: "IX_Device_ProduceId");

            migrationBuilder.CreateTable(
                name: "Produces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    DefaultDeviceType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DefaultIdentityType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DefaultTimeout = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GatewayConfiguration = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GatewayType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Icon = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ProduceToken = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produces_Customer_Customer~",
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
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ProduceId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    DataCatalog = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DeviceId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DeviceKeyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ProduceKeyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduceDataMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProduceDataMappings_Produc~",
                        column: x => x.ProduceId,
                        principalTable: "Produces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProduceDictionaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Customer = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    DataType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DefaultValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    Display = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    DisplayName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    KeyDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    KeyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Place0 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Place1 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Place2 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Place3 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Place4 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Place5 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PlaceOrder0 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PlaceOrder1 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PlaceOrder2 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PlaceOrder3 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PlaceOrder4 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PlaceOrder5 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ProduceId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    Tag = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Unit = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    UnitConvert = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    UnitExpression = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduceDictionaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProduceDictionaries_Produc~",
                        column: x => x.ProduceId,
                        principalTable: "Produces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProduceDataMappings_Produc~",
                table: "ProduceDataMappings",
                column: "ProduceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProduceDictionaries_Produc~",
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

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Produces_Owner~",
                table: "DataStorage",
                column: "OwnerId",
                principalTable: "Produces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_Produces_ProduceId",
                table: "Device",
                column: "ProduceId",
                principalTable: "Produces",
                principalColumn: "Id");
        }
    }
}
