using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
{
    public partial class AddOracle2022 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceIdentities_Device_De~",
                table: "DeviceIdentities");

            migrationBuilder.DropIndex(
                name: "IX_DeviceIdentities_DeviceId",
                table: "DeviceIdentities");

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

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "TelemetryData",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<int>(
                name: "DataSide",
                table: "TelemetryData",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "TelemetryData",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "TelemetryData",
                type: "NVARCHAR2(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "TelemetryData",
                type: "RAW(16)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowId",
                table: "Flows",
                type: "RAW(16)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Flows",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateId",
                table: "Flows",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Createor",
                table: "Flows",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Flows",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ExecutorId",
                table: "Flows",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FlowRuleRuleId",
                table: "Flows",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FlowStatus",
                table: "Flows",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Flows",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "TestStatus",
                table: "Flows",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "Tester",
                table: "Flows",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "TesterDateTime",
                table: "Flows",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "RuleId",
                table: "FlowRules",
                type: "RAW(16)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AddColumn<Guid>(
                name: "CreateId",
                table: "FlowRules",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "FlowRules",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "MountType",
                table: "FlowRules",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentRuleId",
                table: "FlowRules",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<double>(
                name: "SubVersion",
                table: "FlowRules",
                type: "BINARY_DOUBLE",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "FlowRules",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<double>(
                name: "Version",
                table: "FlowRules",
                type: "BINARY_DOUBLE",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowId",
                table: "FlowOperations",
                type: "RAW(16)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

            migrationBuilder.AlterColumn<Guid>(
                name: "OperationId",
                table: "FlowOperations",
                type: "RAW(16)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

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

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormInfos",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormInfos",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormFieldValueInfos",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormFieldValueInfos",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormFieldInfos",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormFieldInfos",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceIdentities",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "RAW(16)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceModelId",
                table: "Device",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Device",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "DataStorage",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "DataStorage",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "DataSide",
                table: "DataStorage",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "DataStorage",
                type: "NVARCHAR2(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DataStorage",
                type: "RAW(16)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "Catalog",
                table: "DataStorage",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "EventId",
                table: "BaseEvents",
                type: "RAW(16)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AddColumn<string>(
                name: "BizData",
                table: "BaseEvents",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "BaseEvents",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FlowRuleRuleId",
                table: "BaseEvents",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BaseEvents",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "DeviceDiagrams",
                columns: table => new
                {
                    DiagramId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DiagramName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DiagramDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DiagramStatus = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    DiagramImage = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDefault = table.Column<bool>(type: "NUMBER(1)", nullable: true),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceDiagrams", x => x.DiagramId);
                    table.ForeignKey(
                        name: "FK_DeviceDiagrams_Customer_Cu~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceDiagrams_Tenant_Tena~",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceGraphToolBoxes",
                columns: table => new
                {
                    ToolBoxId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ToolBoxName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ToolBoxIcon = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ToolBoxStatus = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ToolBoxRequestUri = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ToolBoxType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DeviceId = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    ToolBoxOffsetX = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    ToolBoxOffsetY = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    ToolBoxOffsetTopPer = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    ToolBoxOffsetLeftPer = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CommondParam = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CommondType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGraphToolBoxes", x => x.ToolBoxId);
                    table.ForeignKey(
                        name: "FK_DeviceGraphToolBoxes_Custo~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceGraphToolBoxes_Tenan~",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceModels",
                columns: table => new
                {
                    DeviceModelId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ModelName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ModelDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ModelStatus = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceModels", x => x.DeviceModelId);
                });

            migrationBuilder.CreateTable(
                name: "DevicePortMappings",
                columns: table => new
                {
                    MappingId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SourceId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TargeId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SourceElementId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TargetElementId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    MappingStatus = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    MappingIndex = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    SourceDeviceId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TargetDeviceId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePortMappings", x => x.MappingId);
                });

            migrationBuilder.CreateTable(
                name: "DevicePorts",
                columns: table => new
                {
                    PortId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PortName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PortDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PortPic = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PortType = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PortPhyType = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PortStatus = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DeviceId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Creator = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    PortElementId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePorts", x => x.PortId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceRules",
                columns: table => new
                {
                    DeviceRuleId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DeviceId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    FlowRuleRuleId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    ConfigUser = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ConfigDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    EnableTrace = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceRules", x => x.DeviceRuleId);
                    table.ForeignKey(
                        name: "FK_DeviceRules_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceRules_FlowRules_Flow~",
                        column: x => x.FlowRuleRuleId,
                        principalTable: "FlowRules",
                        principalColumn: "RuleId");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserId = table.Column<string>(type: "NVARCHAR2(450)", nullable: true),
                    Token = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    JwtId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsUsed = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    IsRevorked = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_~",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RuleTaskExecutors",
                columns: table => new
                {
                    ExecutorId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ExecutorName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ExecutorDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Path = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TypeName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DefaultConfig = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    MataData = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Tag = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ExecutorStatus = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    AddDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TestStatus = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Tester = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TesterDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleTaskExecutors", x => x.ExecutorId);
                    table.ForeignKey(
                        name: "FK_RuleTaskExecutors_Customer~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RuleTaskExecutors_Tenant_T~",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionEvents",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EventName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EventDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EventNameSpace = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EventStatus = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EventParam = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EventTag = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_SubscriptionEvents_Custome~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionEvents_Tenant_~",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceGraphs",
                columns: table => new
                {
                    GraphId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DeviceId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    GraphShape = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphWidth = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    GraphHeight = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    GraphPostionX = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    GraphPostionY = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    GraphElementId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DeviceDiagramDiagramId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    GraphFill = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphStroke = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphStrokeWidth = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    GraphTextFill = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphTextFontSize = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    GraphTextRefX = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    GraphTextAnchor = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphTextVerticalAnchor = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphTextFontFamily = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GraphTextRefY = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGraphs", x => x.GraphId);
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_Customer_Cust~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_DeviceDiagram~",
                        column: x => x.DeviceDiagramDiagramId,
                        principalTable: "DeviceDiagrams",
                        principalColumn: "DiagramId");
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_Tenant_Tenant~",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceModelCommands",
                columns: table => new
                {
                    CommandId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CommandTitle = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CommandI18N = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CommandType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CommandParams = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CommandName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CommandTemplate = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DeviceModelId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CommandStatus = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceModelCommands", x => x.CommandId);
                    table.ForeignKey(
                        name: "FK_DeviceModelCommands_Device~",
                        column: x => x.DeviceModelId,
                        principalTable: "DeviceModels",
                        principalColumn: "DeviceModelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionTasks",
                columns: table => new
                {
                    BindId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EventId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SubscriptionEventId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    RuleTaskExecutorExecutorId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TaskConfig = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionTasks", x => x.BindId);
                    table.ForeignKey(
                        name: "FK_SubscriptionTasks_RuleTask~",
                        column: x => x.RuleTaskExecutorExecutorId,
                        principalTable: "RuleTaskExecutors",
                        principalColumn: "ExecutorId");
                    table.ForeignKey(
                        name: "FK_SubscriptionTasks_Subscrip~",
                        column: x => x.SubscriptionEventId,
                        principalTable: "SubscriptionEvents",
                        principalColumn: "EventId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flows_CustomerId",
                table: "Flows",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_ExecutorId",
                table: "Flows",
                column: "ExecutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_FlowRuleRuleId",
                table: "Flows",
                column: "FlowRuleRuleId");

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
                name: "IX_FlowOperations_BaseEventId",
                table: "FlowOperations",
                column: "BaseEventId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_FlowId",
                table: "FlowOperations",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowOperations_FlowRuleId",
                table: "FlowOperations",
                column: "FlowRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormInfos_CustomerId",
                table: "DynamicFormInfos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormInfos_TenantId",
                table: "DynamicFormInfos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldValueInfo~1",
                table: "DynamicFormFieldValueInfos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldValueInfos~",
                table: "DynamicFormFieldValueInfos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldInfos_Cust~",
                table: "DynamicFormFieldInfos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldInfos_Tena~",
                table: "DynamicFormFieldInfos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIdentities_DeviceId",
                table: "DeviceIdentities",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Device_DeviceModelId",
                table: "Device",
                column: "DeviceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEvents_CustomerId",
                table: "BaseEvents",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEvents_FlowRuleRuleId",
                table: "BaseEvents",
                column: "FlowRuleRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEvents_TenantId",
                table: "BaseEvents",
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
                name: "IX_DeviceGraphs_CustomerId",
                table: "DeviceGraphs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_DeviceDiagram~",
                table: "DeviceGraphs",
                column: "DeviceDiagramDiagramId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_TenantId",
                table: "DeviceGraphs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphToolBoxes_Custo~",
                table: "DeviceGraphToolBoxes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphToolBoxes_Tenan~",
                table: "DeviceGraphToolBoxes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceModelCommands_Device~",
                table: "DeviceModelCommands",
                column: "DeviceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceRules_DeviceId",
                table: "DeviceRules",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceRules_FlowRuleRuleId",
                table: "DeviceRules",
                column: "FlowRuleRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleTaskExecutors_Customer~",
                table: "RuleTaskExecutors",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleTaskExecutors_TenantId",
                table: "RuleTaskExecutors",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionEvents_Custome~",
                table: "SubscriptionEvents",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionEvents_TenantId",
                table: "SubscriptionEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTasks_RuleTask~",
                table: "SubscriptionTasks",
                column: "RuleTaskExecutorExecutorId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTasks_Subscrip~",
                table: "SubscriptionTasks",
                column: "SubscriptionEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEvents_Customer_Custom~",
                table: "BaseEvents",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEvents_FlowRules_FlowR~",
                table: "BaseEvents",
                column: "FlowRuleRuleId",
                principalTable: "FlowRules",
                principalColumn: "RuleId");

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
                name: "FK_DeviceIdentities_Device_De~",
                table: "DeviceIdentities",
                column: "DeviceId",
                principalTable: "Device",
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
                name: "FK_DynamicFormFieldValueInfo~1",
                table: "DynamicFormFieldValueInfos",
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
                name: "FK_Flows_FlowRules_FlowRuleRu~",
                table: "Flows",
                column: "FlowRuleRuleId",
                principalTable: "FlowRules",
                principalColumn: "RuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flows_RuleTaskExecutors_Ex~",
                table: "Flows",
                column: "ExecutorId",
                principalTable: "RuleTaskExecutors",
                principalColumn: "ExecutorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flows_Tenant_TenantId",
                table: "Flows",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseEvents_Customer_Custom~",
                table: "BaseEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseEvents_FlowRules_FlowR~",
                table: "BaseEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseEvents_Tenant_TenantId",
                table: "BaseEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Device_DeviceModels_Device~",
                table: "Device");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceIdentities_Device_De~",
                table: "DeviceIdentities");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldInfos_Cust~",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldInfos_Tena~",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldValueInfo~1",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldValueInfos~",
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
                name: "FK_Flows_FlowRules_FlowRuleRu~",
                table: "Flows");

            migrationBuilder.DropForeignKey(
                name: "FK_Flows_RuleTaskExecutors_Ex~",
                table: "Flows");

            migrationBuilder.DropForeignKey(
                name: "FK_Flows_Tenant_TenantId",
                table: "Flows");

            migrationBuilder.DropTable(
                name: "DeviceGraphs");

            migrationBuilder.DropTable(
                name: "DeviceGraphToolBoxes");

            migrationBuilder.DropTable(
                name: "DeviceModelCommands");

            migrationBuilder.DropTable(
                name: "DevicePortMappings");

            migrationBuilder.DropTable(
                name: "DevicePorts");

            migrationBuilder.DropTable(
                name: "DeviceRules");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SubscriptionTasks");

            migrationBuilder.DropTable(
                name: "DeviceDiagrams");

            migrationBuilder.DropTable(
                name: "DeviceModels");

            migrationBuilder.DropTable(
                name: "RuleTaskExecutors");

            migrationBuilder.DropTable(
                name: "SubscriptionEvents");

            migrationBuilder.DropIndex(
                name: "IX_Flows_CustomerId",
                table: "Flows");

            migrationBuilder.DropIndex(
                name: "IX_Flows_ExecutorId",
                table: "Flows");

            migrationBuilder.DropIndex(
                name: "IX_Flows_FlowRuleRuleId",
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
                name: "IX_FlowOperations_BaseEventId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_FlowId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_FlowOperations_FlowRuleId",
                table: "FlowOperations");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormInfos_CustomerId",
                table: "DynamicFormInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormInfos_TenantId",
                table: "DynamicFormInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormFieldValueInfo~1",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormFieldValueInfos~",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormFieldInfos_Cust~",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormFieldInfos_Tena~",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropIndex(
                name: "IX_DeviceIdentities_DeviceId",
                table: "DeviceIdentities");

            migrationBuilder.DropIndex(
                name: "IX_Device_DeviceModelId",
                table: "Device");

            migrationBuilder.DropIndex(
                name: "IX_BaseEvents_CustomerId",
                table: "BaseEvents");

            migrationBuilder.DropIndex(
                name: "IX_BaseEvents_FlowRuleRuleId",
                table: "BaseEvents");

            migrationBuilder.DropIndex(
                name: "IX_BaseEvents_TenantId",
                table: "BaseEvents");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "CreateId",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "Createor",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "ExecutorId",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "FlowRuleRuleId",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "FlowStatus",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "TestStatus",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "Tester",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "TesterDateTime",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "CreateId",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "MountType",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "ParentRuleId",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "SubVersion",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "BaseEventId",
                table: "FlowOperations");

            migrationBuilder.DropColumn(
                name: "FlowRuleId",
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
                name: "DeviceModelId",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "BizData",
                table: "BaseEvents");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "BaseEvents");

            migrationBuilder.DropColumn(
                name: "FlowRuleRuleId",
                table: "BaseEvents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BaseEvents");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "TelemetryData",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<int>(
                name: "DataSide",
                table: "TelemetryData",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "TelemetryData",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "TelemetryData",
                type: "NVARCHAR2(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "TelemetryData",
                type: "RAW(16)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<long>(
                name: "FlowId",
                table: "Flows",
                type: "NUMBER(19)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AddColumn<long>(
                name: "RuleId",
                table: "Flows",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "RuleId",
                table: "FlowRules",
                type: "NUMBER(19)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<long>(
                name: "FlowId",
                table: "FlowOperations",
                type: "NUMBER(19)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<long>(
                name: "OperationId",
                table: "FlowOperations",
                type: "NUMBER(19)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AddColumn<long>(
                name: "EventId",
                table: "FlowOperations",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RuleId",
                table: "FlowOperations",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceIdentities",
                type: "RAW(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "DataStorage",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "DataStorage",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "DataSide",
                table: "DataStorage",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "DataStorage",
                type: "NVARCHAR2(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DataStorage",
                type: "RAW(16)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "Catalog",
                table: "DataStorage",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AlterColumn<long>(
                name: "EventId",
                table: "BaseEvents",
                type: "NUMBER(19)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "RAW(16)")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AddColumn<long>(
                name: "RuleId",
                table: "BaseEvents",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIdentities_DeviceId",
                table: "DeviceIdentities",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceIdentities_Device_De~",
                table: "DeviceIdentities",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id");
        }
    }
}
