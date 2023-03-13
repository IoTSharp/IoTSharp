using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
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
                type: "TEXT",
                nullable: true,
                collation: "NOCASE");
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
