using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
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

            migrationBuilder.Sql("DROP TABLE IF EXISTS \"ProduceDataMappings\";");

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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Icon = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GatewayType = table.Column<int>(type: "INTEGER", nullable: false),
                    GatewayConfiguration = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DefaultTimeout = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DefaultIdentityType = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DefaultDeviceType = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductToken = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProductKeyName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DataCatalog = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceKeyName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Description = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KeyName = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Unit = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    UnitExpression = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    UnitConvert = table.Column<bool>(type: "INTEGER", nullable: false),
                    KeyDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DefaultValue = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Display = table.Column<bool>(type: "INTEGER", nullable: false),
                    Place0 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    PlaceOrder0 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Place1 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    PlaceOrder1 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Place2 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    PlaceOrder2 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Place3 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    PlaceOrder3 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Place4 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    PlaceOrder4 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Place5 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    PlaceOrder5 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    DataType = table.Column<int>(type: "INTEGER", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Customer = table.Column<Guid>(type: "TEXT", nullable: true),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: true)
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

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Products_OwnerId",
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
                name: "FK_DataStorage_Products_OwnerId",
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DefaultDeviceType = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultIdentityType = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultTimeout = table.Column<int>(type: "INTEGER", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GatewayConfiguration = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GatewayType = table.Column<int>(type: "INTEGER", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Name = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ProduceToken = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProduceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DataCatalog = table.Column<int>(type: "INTEGER", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceKeyName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ProduceKeyName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Customer = table.Column<Guid>(type: "TEXT", nullable: true),
                    DataType = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultValue = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Display = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    KeyDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    KeyName = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Place0 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Place1 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Place2 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Place3 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Place4 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Place5 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    PlaceOrder0 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    PlaceOrder1 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    PlaceOrder2 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    PlaceOrder3 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    PlaceOrder4 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    PlaceOrder5 = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    ProduceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Tag = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Unit = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    UnitConvert = table.Column<bool>(type: "INTEGER", nullable: false),
                    UnitExpression = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE")
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

            migrationBuilder.AddForeignKey(
                name: "FK_DataStorage_Produces_OwnerId",
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
