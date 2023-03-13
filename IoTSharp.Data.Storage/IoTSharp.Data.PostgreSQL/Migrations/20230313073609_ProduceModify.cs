using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Migrations
{
    /// <inheritdoc />
    public partial class ProduceModify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProduceToken",
                table: "Produces",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProduceToken",
                table: "Produces");
        }
    }
}
