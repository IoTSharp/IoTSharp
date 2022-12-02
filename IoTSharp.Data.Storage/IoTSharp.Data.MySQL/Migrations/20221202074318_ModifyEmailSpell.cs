using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.MySql.Migrations
{
    public partial class ModifyEmailSpell : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EMail",
                table: "Tenant",
                newName: "Email");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Tenant",
                newName: "EMail");
        }
    }
}
