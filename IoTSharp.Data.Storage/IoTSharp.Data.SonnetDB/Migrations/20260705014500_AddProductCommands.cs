using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SonnetDB.Migrations
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
                    CommandId = table.Column<Guid>(type: "STRING", nullable: false),
                    ProductId = table.Column<Guid>(type: "STRING", nullable: false),
                    CommandTitle = table.Column<string>(type: "STRING", nullable: true),
                    CommandType = table.Column<int>(type: "INT", nullable: false),
                    CommandParams = table.Column<string>(type: "STRING", nullable: true),
                    CommandName = table.Column<string>(type: "STRING", nullable: true),
                    CommandTemplate = table.Column<string>(type: "STRING", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    CommandStatus = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCommands", x => x.CommandId);
                    table.ForeignKey(
                        name: "FK_ProductCommands_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCommands_ProductId_CommandStatus",
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
