using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class modifycolumdevrule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlowId",
                table: "DeviceRules");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceRules",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<long>(
                name: "FlowRuleRuleId",
                table: "DeviceRules",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceRules_DeviceId",
                table: "DeviceRules",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceRules_FlowRuleRuleId",
                table: "DeviceRules",
                column: "FlowRuleRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceRules_Device_DeviceId",
                table: "DeviceRules",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceRules_FlowRules_FlowRuleRuleId",
                table: "DeviceRules",
                column: "FlowRuleRuleId",
                principalTable: "FlowRules",
                principalColumn: "RuleId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceRules_Device_DeviceId",
                table: "DeviceRules");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceRules_FlowRules_FlowRuleRuleId",
                table: "DeviceRules");

            migrationBuilder.DropIndex(
                name: "IX_DeviceRules_DeviceId",
                table: "DeviceRules");

            migrationBuilder.DropIndex(
                name: "IX_DeviceRules_FlowRuleRuleId",
                table: "DeviceRules");

            migrationBuilder.DropColumn(
                name: "FlowRuleRuleId",
                table: "DeviceRules");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceRules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FlowId",
                table: "DeviceRules",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
