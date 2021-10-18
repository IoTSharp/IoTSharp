using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IoTSharp.Migrations
{
    public partial class modifycolumnkey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RuleId",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "RuleId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "RuleId",
                table: "BaseEvents");

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowId",
                table: "Flows",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "FlowRuleRuleId",
                table: "Flows",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "RuleId",
                table: "FlowRules",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowId",
                table: "FlowOperations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "OperationId",
                table: "FlowOperations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

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

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowRuleRuleId",
                table: "DeviceRules",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceRuleId",
                table: "DeviceRules",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "EventId",
                table: "BaseEvents",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_Flows_FlowRuleRuleId",
                table: "Flows",
                column: "FlowRuleRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_BaseEventEventId",
                table: "FlowOperations",
                column: "BaseEventEventId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_FlowId",
                table: "FlowOperations",
                column: "FlowId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Flows_FlowRules_FlowRuleRuleId",
                table: "Flows",
                column: "FlowRuleRuleId",
                principalTable: "FlowRules",
                principalColumn: "RuleId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropForeignKey(
                name: "FK_Flows_FlowRules_FlowRuleRuleId",
                table: "Flows");

            migrationBuilder.DropIndex(
                name: "IX_Flows_FlowRuleRuleId",
                table: "Flows");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_BaseEventEventId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_FlowId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_FlowRuleRuleId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "FlowRuleRuleId",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "BaseEventEventId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "FlowRuleRuleId",
                table: "FlowOperations");

            migrationBuilder.AlterColumn<long>(
                name: "FlowId",
                table: "Flows",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "RuleId",
                table: "Flows",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "RuleId",
                table: "FlowRules",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "FlowId",
                table: "FlowOperations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "OperationId",
                table: "FlowOperations",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "EventId",
                table: "FlowOperations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RuleId",
                table: "FlowOperations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "FlowRuleRuleId",
                table: "DeviceRules",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "DeviceRuleId",
                table: "DeviceRules",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "EventId",
                table: "BaseEvents",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "RuleId",
                table: "BaseEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
