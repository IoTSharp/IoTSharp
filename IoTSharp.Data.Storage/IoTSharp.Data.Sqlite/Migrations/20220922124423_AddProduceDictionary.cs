using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
{
    public partial class AddProduceDictionary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GatewayConfiguration",
                table: "Produces",
                type: "TEXT",
                nullable: true,
                collation: "NOCASE");

            migrationBuilder.AddColumn<int>(
                name: "GatewayType",
                table: "Produces",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Produces",
                type: "TEXT",
                nullable: true,
                collation: "NOCASE");

            migrationBuilder.CreateTable(
                name: "ProduceDictionaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KeyName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Unit = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    UnitExpression = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    UnitConvert = table.Column<bool>(type: "INTEGER", nullable: false),
                    KeyDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DefaultValue = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Display = table.Column<bool>(type: "INTEGER", nullable: false),
                    Place0 = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    PlaceOrder0 = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Place1 = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    PlaceOrder1 = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Place2 = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    PlaceOrder2 = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Place3 = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    PlaceOrder3 = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Place4 = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    PlaceOrder4 = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Place5 = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    PlaceOrder5 = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DataType = table.Column<int>(type: "INTEGER", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Customer = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProduceId = table.Column<Guid>(type: "TEXT", nullable: true)
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
