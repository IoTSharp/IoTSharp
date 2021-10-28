using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class addtaskexecutor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentRuleId",
                table: "FlowRules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<double>(
                name: "SubVersion",
                table: "FlowRules",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "BizData",
                table: "BaseEvents",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentRuleId",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "SubVersion",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "BizData",
                table: "BaseEvents");
        }
    }
}
