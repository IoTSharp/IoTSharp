using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
{
    /// <inheritdoc />
    public partial class Oracle2024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseEvents_Customer_Custom~",
                table: "BaseEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseEvents_Tenant_TenantId",
                table: "BaseEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Device_DeviceModels_Device~",
                table: "Device");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceDiagrams_Customer_Cu~",
                table: "DeviceDiagrams");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceDiagrams_Tenant_Tena~",
                table: "DeviceDiagrams");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphs_Customer_Cust~",
                table: "DeviceGraphs");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphs_Tenant_Tenant~",
                table: "DeviceGraphs");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphToolBoxes_Custo~",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphToolBoxes_Tenan~",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldInfos_Cust~",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldInfos_Tena~",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldValueInfos~",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldValueInfo~1",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormInfos_Customer_~",
                table: "DynamicFormInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormInfos_Tenant_Te~",
                table: "DynamicFormInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_BaseEvents_~",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_FlowRules_F~",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_Flows_FlowId",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowRules_Customer_Custome~",
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
                name: "FK_RuleTaskExecutors_Customer~",
                table: "RuleTaskExecutors");

            migrationBuilder.DropForeignKey(
                name: "FK_RuleTaskExecutors_Tenant_T~",
                table: "RuleTaskExecutors");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionEvents_Custome~",
                table: "SubscriptionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionEvents_Tenant_~",
                table: "SubscriptionEvents");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_BaseEventId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_FlowRuleId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_Device_DeviceModelId",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "BaseEventId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "FlowRuleId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "LastActive",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "Online",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Device");

            migrationBuilder.RenameColumn(
                name: "EMail",
                table: "Tenant",
                newName: "Email");

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Tenant",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "TelemetryData",
                type: "BOOLEAN",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "SubscriptionEvents",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "SubscriptionEvents",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "RuleTaskExecutors",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "RuleTaskExecutors",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsed",
                table: "RefreshTokens",
                type: "BOOLEAN",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRevorked",
                table: "RefreshTokens",
                type: "BOOLEAN",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "Flows",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Flows",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AddColumn<string>(
                name: "FlowClass",
                table: "Flows",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlowIcon",
                table: "Flows",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlowNameSpace",
                table: "Flows",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlowShapeType",
                table: "Flows",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlowTag",
                table: "Flows",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Left",
                table: "Flows",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Top",
                table: "Flows",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "FlowRules",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "FlowRules",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<int>(
                name: "NodeStatus",
                table: "FlowOperations",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowId",
                table: "FlowOperations",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AddColumn<Guid>(
                name: "BaseEventEventId",
                table: "FlowOperations",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FlowRuleRuleId",
                table: "FlowOperations",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormInfos",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompact",
                table: "DynamicFormInfos",
                type: "BOOLEAN",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");

            migrationBuilder.AlterColumn<int>(
                name: "FormStatus",
                table: "DynamicFormInfos",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FormCreator",
                table: "DynamicFormInfos",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormInfos",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<long>(
                name: "BizId",
                table: "DynamicFormInfos",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormFieldValueInfos",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<long>(
                name: "FromId",
                table: "DynamicFormFieldValueInfos",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FieldValueType",
                table: "DynamicFormFieldValueInfos",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FieldId",
                table: "DynamicFormFieldValueInfos",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormFieldValueInfos",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<long>(
                name: "BizId",
                table: "DynamicFormFieldValueInfos",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormFieldInfos",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "DynamicFormFieldInfos",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "DynamicFormFieldInfos",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FormId",
                table: "DynamicFormFieldInfos",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FieldValueType",
                table: "DynamicFormFieldInfos",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FieldUIElement",
                table: "DynamicFormFieldInfos",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FieldStatus",
                table: "DynamicFormFieldInfos",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FieldMaxLength",
                table: "DynamicFormFieldInfos",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormFieldInfos",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<int>(
                name: "PortType",
                table: "DevicePorts",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PortStatus",
                table: "DevicePorts",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PortPhyType",
                table: "DevicePorts",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Creator",
                table: "DevicePorts",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MappingStatus",
                table: "DevicePortMappings",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MappingIndex",
                table: "DevicePortMappings",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ToolBoxStatus",
                table: "DeviceGraphToolBoxes",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ToolBoxOffsetY",
                table: "DeviceGraphToolBoxes",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ToolBoxOffsetX",
                table: "DeviceGraphToolBoxes",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ToolBoxOffsetTopPer",
                table: "DeviceGraphToolBoxes",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ToolBoxOffsetLeftPer",
                table: "DeviceGraphToolBoxes",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "DeviceGraphToolBoxes",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<long>(
                name: "DeviceId",
                table: "DeviceGraphToolBoxes",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DeviceGraphToolBoxes",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "DeviceGraphs",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<int>(
                name: "GraphWidth",
                table: "DeviceGraphs",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphTextRefY",
                table: "DeviceGraphs",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphTextRefX",
                table: "DeviceGraphs",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphTextFontSize",
                table: "DeviceGraphs",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphStrokeWidth",
                table: "DeviceGraphs",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphPostionY",
                table: "DeviceGraphs",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphPostionX",
                table: "DeviceGraphs",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraphHeight",
                table: "DeviceGraphs",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DeviceGraphs",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "DeviceDiagrams",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDefault",
                table: "DeviceDiagrams",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DiagramStatus",
                table: "DeviceDiagrams",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DeviceDiagrams",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Device",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ProduceId",
                table: "Device",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "DataStorage",
                type: "BOOLEAN",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "DataStorage",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Customer",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "BaseEvents",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "BaseEvents",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "AuthorizedKeys",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "TwoFactorEnabled",
                table: "AspNetUsers",
                type: "BOOLEAN",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "AspNetUsers",
                type: "BOOLEAN",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "LockoutEnabled",
                table: "AspNetUsers",
                type: "BOOLEAN",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "EmailConfirmed",
                table: "AspNetUsers",
                type: "BOOLEAN",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");

            migrationBuilder.CreateTable(
                name: "Alarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    AlarmType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    AlarmDetail = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    AckDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ClearDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    AlarmStatus = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Serverity = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Propagate = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    OriginatorId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OriginatorType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alarms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alarms_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Alarms_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    AssetType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Assets_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Produces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Icon = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GatewayType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GatewayConfiguration = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DefaultTimeout = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    DefaultIdentityType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DefaultDeviceType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ProduceToken = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produces_Customer_Customer~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Produces_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DeviceId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DataCatalog = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    KeyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    AssetId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetRelations_Assets_Asse~",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProduceDictionaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    KeyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DisplayName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Unit = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    UnitExpression = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    UnitConvert = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    KeyDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DefaultValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Display = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    Place0 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PlaceOrder0 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Place1 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PlaceOrder1 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Place2 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PlaceOrder2 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Place3 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PlaceOrder3 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Place4 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PlaceOrder4 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Place5 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PlaceOrder5 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DataType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Tag = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Customer = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    ProduceId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduceDictionaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProduceDictionaries_Produc~",
                        column: x => x.ProduceId,
                        principalTable: "Produces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_BaseEventEv~",
                table: "FlowOperations",
                column: "BaseEventEventId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_FlowRuleRul~",
                table: "FlowOperations",
                column: "FlowRuleRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_ProduceId",
                table: "Device",
                column: "ProduceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_OwnerId",
                table: "DataStorage",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_CustomerId",
                table: "Alarms",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_TenantId",
                table: "Alarms",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRelations_AssetId",
                table: "AssetRelations",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CustomerId",
                table: "Assets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_TenantId",
                table: "Assets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProduceDictionaries_Produc~",
                table: "ProduceDictionaries",
                column: "ProduceId");

            migrationBuilder.CreateIndex(
                name: "IX_Produces_CustomerId",
                table: "Produces",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Produces_TenantId",
                table: "Produces",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEvents_Customer_Custom~",
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
                name: "FK_DataStorage_Produces_Owner~",
                table: "DataStorage",
                column: "OwnerId",
                principalTable: "Produces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_Produces_ProduceId",
                table: "Device",
                column: "ProduceId",
                principalTable: "Produces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceDiagrams_Customer_Cu~",
                table: "DeviceDiagrams",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceDiagrams_Tenant_Tena~",
                table: "DeviceDiagrams",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphs_Customer_Cust~",
                table: "DeviceGraphs",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphs_Tenant_Tenant~",
                table: "DeviceGraphs",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphToolBoxes_Custo~",
                table: "DeviceGraphToolBoxes",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphToolBoxes_Tenan~",
                table: "DeviceGraphToolBoxes",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldInfos_Cust~",
                table: "DynamicFormFieldInfos",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldInfos_Tena~",
                table: "DynamicFormFieldInfos",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldValueInfos~",
                table: "DynamicFormFieldValueInfos",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldValueInfo~1",
                table: "DynamicFormFieldValueInfos",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormInfos_Customer_~",
                table: "DynamicFormInfos",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormInfos_Tenant_Te~",
                table: "DynamicFormInfos",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_BaseEvents_~",
                table: "FlowOperations",
                column: "BaseEventEventId",
                principalTable: "BaseEvents",
                principalColumn: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_FlowRules_F~",
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
                name: "FK_FlowRules_Customer_Custome~",
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
                name: "FK_RuleTaskExecutors_Customer~",
                table: "RuleTaskExecutors",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RuleTaskExecutors_Tenant_T~",
                table: "RuleTaskExecutors",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionEvents_Custome~",
                table: "SubscriptionEvents",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionEvents_Tenant_~",
                table: "SubscriptionEvents",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseEvents_Customer_Custom~",
                table: "BaseEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseEvents_Tenant_TenantId",
                table: "BaseEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DataStorage_Produces_Owner~",
                table: "DataStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_Device_Produces_ProduceId",
                table: "Device");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceDiagrams_Customer_Cu~",
                table: "DeviceDiagrams");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceDiagrams_Tenant_Tena~",
                table: "DeviceDiagrams");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphs_Customer_Cust~",
                table: "DeviceGraphs");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphs_Tenant_Tenant~",
                table: "DeviceGraphs");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphToolBoxes_Custo~",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphToolBoxes_Tenan~",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldInfos_Cust~",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldInfos_Tena~",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldValueInfos~",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldValueInfo~1",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormInfos_Customer_~",
                table: "DynamicFormInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormInfos_Tenant_Te~",
                table: "DynamicFormInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_BaseEvents_~",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_FlowRules_F~",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowOperations_Flows_FlowId",
                table: "FlowOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowRules_Customer_Custome~",
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
                name: "FK_RuleTaskExecutors_Customer~",
                table: "RuleTaskExecutors");

            migrationBuilder.DropForeignKey(
                name: "FK_RuleTaskExecutors_Tenant_T~",
                table: "RuleTaskExecutors");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionEvents_Custome~",
                table: "SubscriptionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionEvents_Tenant_~",
                table: "SubscriptionEvents");

            migrationBuilder.DropTable(
                name: "Alarms");

            migrationBuilder.DropTable(
                name: "AssetRelations");

            migrationBuilder.DropTable(
                name: "ProduceDictionaries");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Produces");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_BaseEventEv~",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_FlowRuleRul~",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_Device_ProduceId",
                table: "Device");

            migrationBuilder.DropIndex(
                name: "IX_DataStorage_OwnerId",
                table: "DataStorage");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "FlowClass",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "FlowIcon",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "FlowNameSpace",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "FlowShapeType",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "FlowTag",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "Left",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "Top",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "BaseEventEventId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "FlowRuleRuleId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "ProduceId",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "DataStorage");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "AuthorizedKeys");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Tenant",
                newName: "EMail");

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "TelemetryData",
                type: "NUMBER(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "SubscriptionEvents",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "SubscriptionEvents",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "RuleTaskExecutors",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "RuleTaskExecutors",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsed",
                table: "RefreshTokens",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRevorked",
                table: "RefreshTokens",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "Flows",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Flows",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "FlowRules",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "FlowRules",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NodeStatus",
                table: "FlowOperations",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowId",
                table: "FlowOperations",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BaseEventId",
                table: "FlowOperations",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FlowRuleId",
                table: "FlowOperations",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormInfos",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompact",
                table: "DynamicFormInfos",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN");

            migrationBuilder.AlterColumn<int>(
                name: "FormStatus",
                table: "DynamicFormInfos",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<long>(
                name: "FormCreator",
                table: "DynamicFormInfos",
                type: "NUMBER(19)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormInfos",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "BizId",
                table: "DynamicFormInfos",
                type: "NUMBER(19)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormFieldValueInfos",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FromId",
                table: "DynamicFormFieldValueInfos",
                type: "NUMBER(19)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

            migrationBuilder.AlterColumn<long>(
                name: "FieldValueType",
                table: "DynamicFormFieldValueInfos",
                type: "NUMBER(19)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

            migrationBuilder.AlterColumn<long>(
                name: "FieldId",
                table: "DynamicFormFieldValueInfos",
                type: "NUMBER(19)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormFieldValueInfos",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "BizId",
                table: "DynamicFormFieldValueInfos",
                type: "NUMBER(19)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormFieldInfos",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "DynamicFormFieldInfos",
                type: "NUMBER(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN");

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "DynamicFormFieldInfos",
                type: "NUMBER(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN");

            migrationBuilder.AlterColumn<long>(
                name: "FormId",
                table: "DynamicFormFieldInfos",
                type: "NUMBER(19)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

            migrationBuilder.AlterColumn<int>(
                name: "FieldValueType",
                table: "DynamicFormFieldInfos",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<long>(
                name: "FieldUIElement",
                table: "DynamicFormFieldInfos",
                type: "NUMBER(19)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

            migrationBuilder.AlterColumn<int>(
                name: "FieldStatus",
                table: "DynamicFormFieldInfos",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<int>(
                name: "FieldMaxLength",
                table: "DynamicFormFieldInfos",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormFieldInfos",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PortType",
                table: "DevicePorts",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<int>(
                name: "PortStatus",
                table: "DevicePorts",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<int>(
                name: "PortPhyType",
                table: "DevicePorts",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<long>(
                name: "Creator",
                table: "DevicePorts",
                type: "NUMBER(19)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

            migrationBuilder.AlterColumn<int>(
                name: "MappingStatus",
                table: "DevicePortMappings",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MappingIndex",
                table: "DevicePortMappings",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<int>(
                name: "ToolBoxStatus",
                table: "DeviceGraphToolBoxes",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ToolBoxOffsetY",
                table: "DeviceGraphToolBoxes",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ToolBoxOffsetX",
                table: "DeviceGraphToolBoxes",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ToolBoxOffsetTopPer",
                table: "DeviceGraphToolBoxes",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ToolBoxOffsetLeftPer",
                table: "DeviceGraphToolBoxes",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "DeviceGraphToolBoxes",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "DeviceId",
                table: "DeviceGraphToolBoxes",
                type: "NUMBER(19)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DeviceGraphToolBoxes",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "DeviceGraphs",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphWidth",
                table: "DeviceGraphs",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphTextRefY",
                table: "DeviceGraphs",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphTextRefX",
                table: "DeviceGraphs",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphTextFontSize",
                table: "DeviceGraphs",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphStrokeWidth",
                table: "DeviceGraphs",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphPostionY",
                table: "DeviceGraphs",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphPostionX",
                table: "DeviceGraphs",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "GraphHeight",
                table: "DeviceGraphs",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DeviceGraphs",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "DeviceDiagrams",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDefault",
                table: "DeviceDiagrams",
                type: "NUMBER(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN");

            migrationBuilder.AlterColumn<int>(
                name: "DiagramStatus",
                table: "DeviceDiagrams",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DeviceDiagrams",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActive",
                table: "Device",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Online",
                table: "Device",
                type: "NUMBER(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Device",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "DataStorage",
                type: "NUMBER(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "BaseEvents",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "BaseEvents",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "TwoFactorEnabled",
                table: "AspNetUsers",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN");

            migrationBuilder.AlterColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "AspNetUsers",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN");

            migrationBuilder.AlterColumn<bool>(
                name: "LockoutEnabled",
                table: "AspNetUsers",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN");

            migrationBuilder.AlterColumn<bool>(
                name: "EmailConfirmed",
                table: "AspNetUsers",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_BaseEventId",
                table: "FlowOperations",
                column: "BaseEventId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_FlowRuleId",
                table: "FlowOperations",
                column: "FlowRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_DeviceModelId",
                table: "Device",
                column: "DeviceModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEvents_Customer_Custom~",
                table: "BaseEvents",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEvents_Tenant_TenantId",
                table: "BaseEvents",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Device_DeviceModels_Device~",
                table: "Device",
                column: "DeviceModelId",
                principalTable: "DeviceModels",
                principalColumn: "DeviceModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceDiagrams_Customer_Cu~",
                table: "DeviceDiagrams",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceDiagrams_Tenant_Tena~",
                table: "DeviceDiagrams",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphs_Customer_Cust~",
                table: "DeviceGraphs",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphs_Tenant_Tenant~",
                table: "DeviceGraphs",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphToolBoxes_Custo~",
                table: "DeviceGraphToolBoxes",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphToolBoxes_Tenan~",
                table: "DeviceGraphToolBoxes",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldInfos_Cust~",
                table: "DynamicFormFieldInfos",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldInfos_Tena~",
                table: "DynamicFormFieldInfos",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldValueInfos~",
                table: "DynamicFormFieldValueInfos",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldValueInfo~1",
                table: "DynamicFormFieldValueInfos",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormInfos_Customer_~",
                table: "DynamicFormInfos",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormInfos_Tenant_Te~",
                table: "DynamicFormInfos",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_BaseEvents_~",
                table: "FlowOperations",
                column: "BaseEventId",
                principalTable: "BaseEvents",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlowOperations_FlowRules_F~",
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
                name: "FK_FlowRules_Customer_Custome~",
                table: "FlowRules",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlowRules_Tenant_TenantId",
                table: "FlowRules",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Flows_Customer_CustomerId",
                table: "Flows",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Flows_Tenant_TenantId",
                table: "Flows",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RuleTaskExecutors_Customer~",
                table: "RuleTaskExecutors",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RuleTaskExecutors_Tenant_T~",
                table: "RuleTaskExecutors",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionEvents_Custome~",
                table: "SubscriptionEvents",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionEvents_Tenant_~",
                table: "SubscriptionEvents",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
