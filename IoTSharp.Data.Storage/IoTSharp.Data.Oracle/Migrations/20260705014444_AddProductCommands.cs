using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCommands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductCommands",
                columns: table => new
                {
                    CommandId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ProductId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CommandTitle = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CommandType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CommandParams = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CommandName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CommandTemplate = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CommandStatus = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCommands", x => x.CommandId);
                    table.ForeignKey(
                        name: "FK_ProductCommands_Products_P~",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCommands_ProductId_~",
                table: "ProductCommands",
                columns: new[] { "ProductId", "CommandStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCommands");
        }
    }
}
