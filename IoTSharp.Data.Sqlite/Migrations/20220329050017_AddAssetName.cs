using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
{
    public partial class AddAssetName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AssetRelations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AssetRelations",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "AssetRelations");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AssetRelations");
        }
    }
}
