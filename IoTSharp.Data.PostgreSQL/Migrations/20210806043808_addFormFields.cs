using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class addFormFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModelClass",
                table: "DynamicFormInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "DynamicFormInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "DynamicFormFieldInfos",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DictionaryTag",
                table: "BaseDictionaries",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelClass",
                table: "DynamicFormInfos");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "DynamicFormInfos");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropColumn(
                name: "DictionaryTag",
                table: "BaseDictionaries");
        }
    }
}
