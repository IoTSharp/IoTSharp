using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.MySql.Migrations
{
    public partial class FixTenantInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceIdentities_Device_DeviceId",
                table: "DeviceIdentities");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_BaseEvents_BaseEventId",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_FlowRules_FlowRuleId",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_Flows_FlowId",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionTasks_Customer_CustomerId",
                table: "SubscriptionTasks");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionTasks_CustomerId",
                table: "SubscriptionTasks");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_BaseEventId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_FlowRuleId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_DeviceIdentities_DeviceId",
                table: "DeviceIdentities");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "SubscriptionTasks");

            migrationBuilder.DropColumn(
                name: "BaseEventId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "FlowRuleId",
                table: "FlowOperations");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "TelemetryData",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<int>(
                name: "DataSide",
                table: "TelemetryData",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "TelemetryData",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "TelemetryData",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "TelemetryData",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "SubscriptionEvents",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SubscriptionEvents",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "RuleTaskExecutors",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "RuleTaskExecutors",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Flows",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Flows",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "FlowRules",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "FlowRules",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<double>(
                name: "Version",
                table: "FlowRules",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<int>(
                name: "NodeStatus",
                table: "FlowOperations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowId",
                table: "FlowOperations",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BaseEventEventId",
                table: "FlowOperations",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "FlowRuleRuleId",
                table: "FlowOperations",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "FormStatus",
                table: "DynamicFormInfos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FormCreator",
                table: "DynamicFormInfos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "BizId",
                table: "DynamicFormInfos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormInfos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormInfos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<long>(
                name: "FromId",
                table: "DynamicFormFieldValueInfos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FieldValueType",
                table: "DynamicFormFieldValueInfos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FieldId",
                table: "DynamicFormFieldValueInfos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "BizId",
                table: "DynamicFormFieldValueInfos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormFieldValueInfos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormFieldValueInfos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "DynamicFormFieldInfos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "DynamicFormFieldInfos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FormId",
                table: "DynamicFormFieldInfos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FieldValueType",
                table: "DynamicFormFieldInfos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FieldUIElement",
                table: "DynamicFormFieldInfos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FieldStatus",
                table: "DynamicFormFieldInfos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FieldMaxLength",
                table: "DynamicFormFieldInfos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormFieldInfos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormFieldInfos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "PortType",
                table: "DevicePorts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PortStatus",
                table: "DevicePorts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PortPhyType",
                table: "DevicePorts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Creator",
                table: "DevicePorts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MappingStatus",
                table: "DevicePortMappings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MappingIndex",
                table: "DevicePortMappings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceIdentities",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "ToolBoxStatus",
                table: "DeviceGraphToolBoxes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ToolBoxOffsetY",
                table: "DeviceGraphToolBoxes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ToolBoxOffsetX",
                table: "DeviceGraphToolBoxes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ToolBoxOffsetTopPer",
                table: "DeviceGraphToolBoxes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ToolBoxOffsetLeftPer",
                table: "DeviceGraphToolBoxes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "DeviceId",
                table: "DeviceGraphToolBoxes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DeviceGraphToolBoxes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DeviceGraphToolBoxes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "GraphWidth",
                table: "DeviceGraphs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphTextRefY",
                table: "DeviceGraphs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphTextRefX",
                table: "DeviceGraphs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphTextFontSize",
                table: "DeviceGraphs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphStrokeWidth",
                table: "DeviceGraphs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphPostionY",
                table: "DeviceGraphs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphPostionX",
                table: "DeviceGraphs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphHeight",
                table: "DeviceGraphs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DeviceGraphs",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DeviceGraphs",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDefault",
                table: "DeviceDiagrams",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DiagramStatus",
                table: "DeviceDiagrams",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DeviceDiagrams",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DeviceDiagrams",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "DataStorage",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "DataStorage",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "DataSide",
                table: "DataStorage",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "DataStorage",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DataStorage",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "Catalog",
                table: "DataStorage",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "BaseEvents",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BaseEvents",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Token = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JwtId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsUsed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsRevorked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionEvents_CustomerId",
                table: "SubscriptionEvents",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionEvents_TenantId",
                table: "SubscriptionEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleTaskExecutors_CustomerId",
                table: "RuleTaskExecutors",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleTaskExecutors_TenantId",
                table: "RuleTaskExecutors",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_CustomerId",
                table: "Flows",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_TenantId",
                table: "Flows",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowRules_CustomerId",
                table: "FlowRules",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowRules_TenantId",
                table: "FlowRules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_BaseEventEventId",
                table: "FlowOperations",
                column: "BaseEventEventId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_FlowRuleRuleId",
                table: "FlowOperations",
                column: "FlowRuleRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormInfos_CustomerId",
                table: "DynamicFormInfos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormInfos_TenantId",
                table: "DynamicFormInfos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldValueInfos_CustomerId",
                table: "DynamicFormFieldValueInfos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldValueInfos_TenantId",
                table: "DynamicFormFieldValueInfos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldInfos_CustomerId",
                table: "DynamicFormFieldInfos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldInfos_TenantId",
                table: "DynamicFormFieldInfos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIdentities_DeviceId",
                table: "DeviceIdentities",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphToolBoxes_CustomerId",
                table: "DeviceGraphToolBoxes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphToolBoxes_TenantId",
                table: "DeviceGraphToolBoxes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_CustomerId",
                table: "DeviceGraphs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_TenantId",
                table: "DeviceGraphs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceDiagrams_CustomerId",
                table: "DeviceDiagrams",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceDiagrams_TenantId",
                table: "DeviceDiagrams",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEvents_CustomerId",
                table: "BaseEvents",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEvents_TenantId",
                table: "BaseEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEvents_Customer_CustomerId",
                table: "BaseEvents",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEvents_Tenant_TenantId",
                table: "BaseEvents",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceDiagrams_Customer_CustomerId",
                table: "DeviceDiagrams",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceDiagrams_Tenant_TenantId",
                table: "DeviceDiagrams",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphs_Customer_CustomerId",
                table: "DeviceGraphs",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphs_Tenant_TenantId",
                table: "DeviceGraphs",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphToolBoxes_Customer_CustomerId",
                table: "DeviceGraphToolBoxes",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphToolBoxes_Tenant_TenantId",
                table: "DeviceGraphToolBoxes",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceIdentities_Device_DeviceId",
                table: "DeviceIdentities",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldInfos_Customer_CustomerId",
                table: "DynamicFormFieldInfos",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldInfos_Tenant_TenantId",
                table: "DynamicFormFieldInfos",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldValueInfos_Customer_CustomerId",
                table: "DynamicFormFieldValueInfos",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldValueInfos_Tenant_TenantId",
                table: "DynamicFormFieldValueInfos",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormInfos_Customer_CustomerId",
                table: "DynamicFormInfos",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormInfos_Tenant_TenantId",
                table: "DynamicFormInfos",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_BaseEvents_BaseEventEventId",
                table: "FlowOperations",
                column: "BaseEventEventId",
                principalTable: "BaseEvents",
                principalColumn: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_FlowRules_FlowRuleRuleId",
                table: "FlowOperations",
                column: "FlowRuleRuleId",
                principalTable: "FlowRules",
                principalColumn: "RuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_Flows_FlowId",
                table: "FlowOperations",
                column: "FlowId",
                principalTable: "Flows",
                principalColumn: "FlowId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlowRules_Customer_CustomerId",
                table: "FlowRules",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FlowRules_Tenant_TenantId",
                table: "FlowRules",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Flows_Customer_CustomerId",
                table: "Flows",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Flows_Tenant_TenantId",
                table: "Flows",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RuleTaskExecutors_Customer_CustomerId",
                table: "RuleTaskExecutors",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RuleTaskExecutors_Tenant_TenantId",
                table: "RuleTaskExecutors",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionEvents_Customer_CustomerId",
                table: "SubscriptionEvents",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionEvents_Tenant_TenantId",
                table: "SubscriptionEvents",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseEvents_Customer_CustomerId",
                table: "BaseEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseEvents_Tenant_TenantId",
                table: "BaseEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceDiagrams_Customer_CustomerId",
                table: "DeviceDiagrams");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceDiagrams_Tenant_TenantId",
                table: "DeviceDiagrams");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphs_Customer_CustomerId",
                table: "DeviceGraphs");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphs_Tenant_TenantId",
                table: "DeviceGraphs");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphToolBoxes_Customer_CustomerId",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphToolBoxes_Tenant_TenantId",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceIdentities_Device_DeviceId",
                table: "DeviceIdentities");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldInfos_Customer_CustomerId",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldInfos_Tenant_TenantId",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldValueInfos_Customer_CustomerId",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldValueInfos_Tenant_TenantId",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormInfos_Customer_CustomerId",
                table: "DynamicFormInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormInfos_Tenant_TenantId",
                table: "DynamicFormInfos");

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
                name: "FK_FlowRules_Customer_CustomerId",
                table: "FlowRules");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowRules_Tenant_TenantId",
                table: "FlowRules");

            migrationBuilder.DropForeignKey(
                name: "FK_Flows_Customer_CustomerId",
                table: "Flows");

            migrationBuilder.DropForeignKey(
                name: "FK_Flows_Tenant_TenantId",
                table: "Flows");

            migrationBuilder.DropForeignKey(
                name: "FK_RuleTaskExecutors_Customer_CustomerId",
                table: "RuleTaskExecutors");

            migrationBuilder.DropForeignKey(
                name: "FK_RuleTaskExecutors_Tenant_TenantId",
                table: "RuleTaskExecutors");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionEvents_Customer_CustomerId",
                table: "SubscriptionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionEvents_Tenant_TenantId",
                table: "SubscriptionEvents");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionEvents_CustomerId",
                table: "SubscriptionEvents");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionEvents_TenantId",
                table: "SubscriptionEvents");

            migrationBuilder.DropIndex(
                name: "IX_RuleTaskExecutors_CustomerId",
                table: "RuleTaskExecutors");

            migrationBuilder.DropIndex(
                name: "IX_RuleTaskExecutors_TenantId",
                table: "RuleTaskExecutors");

            migrationBuilder.DropIndex(
                name: "IX_Flows_CustomerId",
                table: "Flows");

            migrationBuilder.DropIndex(
                name: "IX_Flows_TenantId",
                table: "Flows");

            migrationBuilder.DropIndex(
                name: "IX_FlowRules_CustomerId",
                table: "FlowRules");

            migrationBuilder.DropIndex(
                name: "IX_FlowRules_TenantId",
                table: "FlowRules");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_BaseEventEventId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_FlowRuleRuleId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormInfos_CustomerId",
                table: "DynamicFormInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormInfos_TenantId",
                table: "DynamicFormInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormFieldValueInfos_CustomerId",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormFieldValueInfos_TenantId",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormFieldInfos_CustomerId",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormFieldInfos_TenantId",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropIndex(
                name: "IX_DeviceIdentities_DeviceId",
                table: "DeviceIdentities");

            migrationBuilder.DropIndex(
                name: "IX_DeviceGraphToolBoxes_CustomerId",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropIndex(
                name: "IX_DeviceGraphToolBoxes_TenantId",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropIndex(
                name: "IX_DeviceGraphs_CustomerId",
                table: "DeviceGraphs");

            migrationBuilder.DropIndex(
                name: "IX_DeviceGraphs_TenantId",
                table: "DeviceGraphs");

            migrationBuilder.DropIndex(
                name: "IX_DeviceDiagrams_CustomerId",
                table: "DeviceDiagrams");

            migrationBuilder.DropIndex(
                name: "IX_DeviceDiagrams_TenantId",
                table: "DeviceDiagrams");

            migrationBuilder.DropIndex(
                name: "IX_BaseEvents_CustomerId",
                table: "BaseEvents");

            migrationBuilder.DropIndex(
                name: "IX_BaseEvents_TenantId",
                table: "BaseEvents");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "SubscriptionEvents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SubscriptionEvents");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "RuleTaskExecutors");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "RuleTaskExecutors");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "BaseEventEventId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "FlowRuleRuleId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DynamicFormInfos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DynamicFormInfos");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DeviceGraphs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DeviceGraphs");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DeviceDiagrams");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DeviceDiagrams");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "BaseEvents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BaseEvents");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "TelemetryData",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<int>(
                name: "DataSide",
                table: "TelemetryData",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "TelemetryData",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "TelemetryData",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "TelemetryData",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "SubscriptionTasks",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "NodeStatus",
                table: "FlowOperations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowId",
                table: "FlowOperations",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BaseEventId",
                table: "FlowOperations",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "FlowRuleId",
                table: "FlowOperations",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "FormStatus",
                table: "DynamicFormInfos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "FormCreator",
                table: "DynamicFormInfos",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "BizId",
                table: "DynamicFormInfos",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "FromId",
                table: "DynamicFormFieldValueInfos",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "FieldValueType",
                table: "DynamicFormFieldValueInfos",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "FieldId",
                table: "DynamicFormFieldValueInfos",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "BizId",
                table: "DynamicFormFieldValueInfos",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "DynamicFormFieldInfos",
                type: "tinyint(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "DynamicFormFieldInfos",
                type: "tinyint(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<long>(
                name: "FormId",
                table: "DynamicFormFieldInfos",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "FieldValueType",
                table: "DynamicFormFieldInfos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "FieldUIElement",
                table: "DynamicFormFieldInfos",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "FieldStatus",
                table: "DynamicFormFieldInfos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "FieldMaxLength",
                table: "DynamicFormFieldInfos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PortType",
                table: "DevicePorts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PortStatus",
                table: "DevicePorts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PortPhyType",
                table: "DevicePorts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "Creator",
                table: "DevicePorts",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "MappingStatus",
                table: "DevicePortMappings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "MappingIndex",
                table: "DevicePortMappings",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceIdentities",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "ToolBoxStatus",
                table: "DeviceGraphToolBoxes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "ToolBoxOffsetY",
                table: "DeviceGraphToolBoxes",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "ToolBoxOffsetX",
                table: "DeviceGraphToolBoxes",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "ToolBoxOffsetTopPer",
                table: "DeviceGraphToolBoxes",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "ToolBoxOffsetLeftPer",
                table: "DeviceGraphToolBoxes",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "DeviceId",
                table: "DeviceGraphToolBoxes",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphWidth",
                table: "DeviceGraphs",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphTextRefY",
                table: "DeviceGraphs",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphTextRefX",
                table: "DeviceGraphs",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphTextFontSize",
                table: "DeviceGraphs",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphStrokeWidth",
                table: "DeviceGraphs",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphPostionY",
                table: "DeviceGraphs",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphPostionX",
                table: "DeviceGraphs",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphHeight",
                table: "DeviceGraphs",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDefault",
                table: "DeviceDiagrams",
                type: "tinyint(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<int>(
                name: "DiagramStatus",
                table: "DeviceDiagrams",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "DataStorage",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "DataStorage",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "DataSide",
                table: "DataStorage",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "DataStorage",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DataStorage",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "Catalog",
                table: "DataStorage",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTasks_CustomerId",
                table: "SubscriptionTasks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_BaseEventId",
                table: "FlowOperations",
                column: "BaseEventId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_FlowRuleId",
                table: "FlowOperations",
                column: "FlowRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIdentities_DeviceId",
                table: "DeviceIdentities",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceIdentities_Device_DeviceId",
                table: "DeviceIdentities",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionTasks_Customer_CustomerId",
                table: "SubscriptionTasks",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
