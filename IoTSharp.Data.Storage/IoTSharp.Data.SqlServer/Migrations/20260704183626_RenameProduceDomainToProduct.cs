using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SqlServer.Migrations
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

            migrationBuilder.Sql("DROP TABLE IF EXISTS [ProduceDataMappings];");

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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayType = table.Column<int>(type: "int", nullable: false),
                    GatewayConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultTimeout = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DefaultIdentityType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultDeviceType = table.Column<int>(type: "int", nullable: false),
                    ProductToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductKeyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataCatalog = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceKeyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitExpression = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitConvert = table.Column<bool>(type: "bit", nullable: false),
                    KeyDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Display = table.Column<bool>(type: "bit", nullable: false),
                    Place0 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOrder0 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOrder1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOrder2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOrder3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOrder4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOrder5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<int>(type: "int", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Customer = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DefaultDeviceType = table.Column<int>(type: "int", nullable: false),
                    DefaultIdentityType = table.Column<int>(type: "int", nullable: false),
                    DefaultTimeout = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayType = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProduceToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProduceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DataCatalog = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceKeyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProduceKeyName = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customer = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DataType = table.Column<int>(type: "int", nullable: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Display = table.Column<bool>(type: "bit", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KeyDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KeyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place0 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOrder0 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOrder1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOrder2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOrder3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOrder4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOrder5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProduceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitConvert = table.Column<bool>(type: "bit", nullable: false),
                    UnitExpression = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
