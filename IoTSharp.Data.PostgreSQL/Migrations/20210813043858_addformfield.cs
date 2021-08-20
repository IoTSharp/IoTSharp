using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class addformfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormLayout",
                table: "DynamicFormInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompact",
                table: "DynamicFormInfos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "RuleId",
                table: "BaseEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormLayout",
                table: "DynamicFormInfos");

            migrationBuilder.DropColumn(
                name: "IsCompact",
                table: "DynamicFormInfos");

            migrationBuilder.DropColumn(
                name: "RuleId",
                table: "BaseEvents");
        }
    }
}
