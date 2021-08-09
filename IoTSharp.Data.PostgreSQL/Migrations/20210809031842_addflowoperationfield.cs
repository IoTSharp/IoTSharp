using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class addflowoperationfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Step",
                table: "FlowOperations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "FlowOperations",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Step",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "FlowOperations");
        }
    }
}
