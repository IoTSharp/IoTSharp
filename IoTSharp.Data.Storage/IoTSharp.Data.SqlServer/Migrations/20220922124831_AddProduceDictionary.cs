using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SqlServer.Migrations
{
    public partial class AddProduceDictionary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GatewayConfiguration",
                table: "Produces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GatewayType",
                table: "Produces",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Produces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProduceDictionaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitExpression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitConvert = table.Column<bool>(type: "bit", nullable: false),
                    KeyDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Display = table.Column<bool>(type: "bit", nullable: false),
                    Place0 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceOrder0 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Place1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceOrder1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Place2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceOrder2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Place3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceOrder3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Place4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceOrder4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Place5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceOrder5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataType = table.Column<int>(type: "int", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Customer = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProduceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
