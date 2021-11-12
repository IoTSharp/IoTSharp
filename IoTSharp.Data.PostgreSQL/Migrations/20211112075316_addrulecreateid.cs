using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class addrulecreateid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreateId",
                table: "FlowRules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateId",
                table: "FlowRules");
        }
    }
}
