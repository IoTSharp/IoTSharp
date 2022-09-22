using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Migrations
{
    public partial class AddProduceDictionary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GatewayConfiguration",
                table: "Produces",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GatewayType",
                table: "Produces",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Produces",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProduceDictionaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KeyName = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Unit = table.Column<string>(type: "text", nullable: true),
                    UnitExpression = table.Column<string>(type: "text", nullable: true),
                    UnitConvert = table.Column<bool>(type: "boolean", nullable: false),
                    KeyDesc = table.Column<string>(type: "text", nullable: true),
                    DefaultValue = table.Column<string>(type: "text", nullable: true),
                    Display = table.Column<bool>(type: "boolean", nullable: false),
                    Place0 = table.Column<string>(type: "text", nullable: true),
                    PlaceOrder0 = table.Column<string>(type: "text", nullable: true),
                    Place1 = table.Column<string>(type: "text", nullable: true),
                    PlaceOrder1 = table.Column<string>(type: "text", nullable: true),
                    Place2 = table.Column<string>(type: "text", nullable: true),
                    PlaceOrder2 = table.Column<string>(type: "text", nullable: true),
                    Place3 = table.Column<string>(type: "text", nullable: true),
                    PlaceOrder3 = table.Column<string>(type: "text", nullable: true),
                    Place4 = table.Column<string>(type: "text", nullable: true),
                    PlaceOrder4 = table.Column<string>(type: "text", nullable: true),
                    Place5 = table.Column<string>(type: "text", nullable: true),
                    PlaceOrder5 = table.Column<string>(type: "text", nullable: true),
                    DataType = table.Column<int>(type: "integer", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: true),
                    Customer = table.Column<Guid>(type: "uuid", nullable: true),
                    ProduceId = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "IX_ProduceDictionaries_ProduceId",
                table: "ProduceDictionaries",
                column: "ProduceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProduceDictionaries");

            migrationBuilder.DropColumn(
                name: "GatewayConfiguration",
                table: "Produces");

            migrationBuilder.DropColumn(
                name: "GatewayType",
                table: "Produces");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Produces");
        }
    }
}
