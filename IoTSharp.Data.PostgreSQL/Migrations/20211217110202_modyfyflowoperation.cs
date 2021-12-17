using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class modyfyflowoperation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_BaseEvents_BaseEventEventId",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_FlowRules_FlowRuleRuleId",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_Flows_FlowId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_BaseEventEventId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_FlowRuleRuleId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "BaseEventEventId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "FlowRuleRuleId",
                table: "FlowOperations");

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowId",
                table: "FlowOperations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BaseEventId",
                table: "FlowOperations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FlowRuleId",
                table: "FlowOperations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_BaseEventId",
                table: "FlowOperations",
                column: "BaseEventId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_FlowRuleId",
                table: "FlowOperations",
                column: "FlowRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_BaseEvents_BaseEventId",
                table: "FlowOperations",
                column: "BaseEventId",
                principalTable: "BaseEvents",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_FlowRules_FlowRuleId",
                table: "FlowOperations",
                column: "FlowRuleId",
                principalTable: "FlowRules",
                principalColumn: "RuleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_Flows_FlowId",
                table: "FlowOperations",
                column: "FlowId",
                principalTable: "Flows",
                principalColumn: "FlowId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_BaseEvents_BaseEventId",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_FlowRules_FlowRuleId",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_Flows_FlowId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_BaseEventId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_FlowRuleId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "BaseEventId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "FlowRuleId",
                table: "FlowOperations");

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowId",
                table: "FlowOperations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "BaseEventEventId",
                table: "FlowOperations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FlowRuleRuleId",
                table: "FlowOperations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_BaseEventEventId",
                table: "FlowOperations",
                column: "BaseEventEventId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_FlowRuleRuleId",
                table: "FlowOperations",
                column: "FlowRuleRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_BaseEvents_BaseEventEventId",
                table: "FlowOperations",
                column: "BaseEventEventId",
                principalTable: "BaseEvents",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_FlowRules_FlowRuleRuleId",
                table: "FlowOperations",
                column: "FlowRuleRuleId",
                principalTable: "FlowRules",
                principalColumn: "RuleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_Flows_FlowId",
                table: "FlowOperations",
                column: "FlowId",
                principalTable: "Flows",
                principalColumn: "FlowId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
