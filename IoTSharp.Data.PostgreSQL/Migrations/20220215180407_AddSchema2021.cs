using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IoTSharp.Migrations
{
    public partial class AddSchema2021 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceIdentities_Device_DeviceId",
                table: "DeviceIdentities");

            migrationBuilder.DropIndex(
                name: "IX_TelemetryData_DeviceId_KeyName_DateTime",
                table: "TelemetryData");

            migrationBuilder.DropIndex(
                name: "IX_DeviceIdentities_DeviceId",
                table: "DeviceIdentities");

            migrationBuilder.AlterColumn<long>(
                name: "Value_Long",
                table: "TelemetryData",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<double>(
                name: "Value_Double",
                table: "TelemetryData",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "TelemetryData",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "TelemetryData",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "TelemetryData",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<int>(
                name: "DataSide",
                table: "TelemetryData",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "TelemetryData",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "TelemetryData",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "TelemetryData",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceIdentities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastActive",
                table: "Device",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceModelId",
                table: "Device",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Device",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<long>(
                name: "Value_Long",
                table: "DataStorage",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<double>(
                name: "Value_Double",
                table: "DataStorage",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "DataStorage",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "DataStorage",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "DataStorage",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "DataStorage",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "DataSide",
                table: "DataStorage",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "DataStorage",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DataStorage",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "Catalog",
                table: "DataStorage",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActiveDateTime",
                table: "AuditLog",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateTable(
                name: "BaseDictionaries",
                columns: table => new
                {
                    DictionaryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DictionaryName = table.Column<string>(type: "text", nullable: true),
                    DictionaryValue = table.Column<string>(type: "text", nullable: true),
                    Dictionary18NKeyName = table.Column<string>(type: "text", nullable: true),
                    DictionaryStatus = table.Column<int>(type: "integer", nullable: true),
                    DictionaryValueType = table.Column<int>(type: "integer", nullable: true),
                    DictionaryValueTypeName = table.Column<string>(type: "text", nullable: true),
                    DictionaryGroupId = table.Column<long>(type: "bigint", nullable: true),
                    DictionaryPattern = table.Column<string>(type: "text", nullable: true),
                    DictionaryDesc = table.Column<string>(type: "text", nullable: true),
                    DictionaryColor = table.Column<string>(type: "text", nullable: true),
                    DictionaryIcon = table.Column<string>(type: "text", nullable: true),
                    DictionaryTag = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseDictionaries", x => x.DictionaryId);
                });

            migrationBuilder.CreateTable(
                name: "BaseDictionaryGroups",
                columns: table => new
                {
                    DictionaryGroupId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DictionaryGroupName = table.Column<string>(type: "text", nullable: true),
                    DictionaryGroupKey = table.Column<string>(type: "text", nullable: true),
                    DictionaryGroupValueType = table.Column<int>(type: "integer", nullable: true),
                    DictionaryGroupStatus = table.Column<int>(type: "integer", nullable: true),
                    DictionaryGroupValueTypeName = table.Column<string>(type: "text", nullable: true),
                    DictionaryGroupDesc = table.Column<string>(type: "text", nullable: true),
                    DictionaryGroup18NKeyName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseDictionaryGroups", x => x.DictionaryGroupId);
                });

            migrationBuilder.CreateTable(
                name: "BaseI18Ns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    KeyName = table.Column<string>(type: "text", nullable: true),
                    ValueBG = table.Column<string>(type: "text", nullable: true),
                    ValueCS = table.Column<string>(type: "text", nullable: true),
                    ValueDA = table.Column<string>(type: "text", nullable: true),
                    ValueDEDE = table.Column<string>(type: "text", nullable: true),
                    ValueESES = table.Column<string>(type: "text", nullable: true),
                    ValueENUS = table.Column<string>(type: "text", nullable: true),
                    ValueENGR = table.Column<string>(type: "text", nullable: true),
                    ValueELGR = table.Column<string>(type: "text", nullable: true),
                    ValueFI = table.Column<string>(type: "text", nullable: true),
                    ValueFRFR = table.Column<string>(type: "text", nullable: true),
                    ValueHE = table.Column<string>(type: "text", nullable: true),
                    ValueHRHR = table.Column<string>(type: "text", nullable: true),
                    ValueHU = table.Column<string>(type: "text", nullable: true),
                    ValueITIT = table.Column<string>(type: "text", nullable: true),
                    ValueJAJP = table.Column<string>(type: "text", nullable: true),
                    ValueKOKR = table.Column<string>(type: "text", nullable: true),
                    ValueNL = table.Column<string>(type: "text", nullable: true),
                    ValuePLPL = table.Column<string>(type: "text", nullable: true),
                    ValuePT = table.Column<string>(type: "text", nullable: true),
                    ValueSLSL = table.Column<string>(type: "text", nullable: true),
                    ValueTRTR = table.Column<string>(type: "text", nullable: true),
                    ValueSR = table.Column<string>(type: "text", nullable: true),
                    ValueSV = table.Column<string>(type: "text", nullable: true),
                    ValueUK = table.Column<string>(type: "text", nullable: true),
                    ValueVI = table.Column<string>(type: "text", nullable: true),
                    ValueZHCN = table.Column<string>(type: "text", nullable: true),
                    ValueZHTW = table.Column<string>(type: "text", nullable: true),
                    ResourceType = table.Column<int>(type: "integer", nullable: true),
                    ResourceId = table.Column<long>(type: "bigint", nullable: true),
                    ResourceKey = table.Column<string>(type: "text", nullable: true),
                    ResourceTag = table.Column<string>(type: "text", nullable: true),
                    ResouceDesc = table.Column<string>(type: "text", nullable: true),
                    ResouceGroupId = table.Column<int>(type: "integer", nullable: true),
                    AddDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseI18Ns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceDiagrams",
                columns: table => new
                {
                    DiagramId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiagramName = table.Column<string>(type: "text", nullable: true),
                    DiagramDesc = table.Column<string>(type: "text", nullable: true),
                    DiagramStatus = table.Column<int>(type: "integer", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DiagramImage = table.Column<string>(type: "text", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceDiagrams", x => x.DiagramId);
                    table.ForeignKey(
                        name: "FK_DeviceDiagrams_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceDiagrams_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceGraphToolBoxes",
                columns: table => new
                {
                    ToolBoxId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToolBoxName = table.Column<string>(type: "text", nullable: true),
                    ToolBoxIcon = table.Column<string>(type: "text", nullable: true),
                    ToolBoxStatus = table.Column<int>(type: "integer", nullable: false),
                    ToolBoxRequestUri = table.Column<string>(type: "text", nullable: true),
                    ToolBoxType = table.Column<string>(type: "text", nullable: true),
                    DeviceId = table.Column<long>(type: "bigint", nullable: false),
                    ToolBoxOffsetX = table.Column<int>(type: "integer", nullable: false),
                    ToolBoxOffsetY = table.Column<int>(type: "integer", nullable: false),
                    ToolBoxOffsetTopPer = table.Column<int>(type: "integer", nullable: false),
                    ToolBoxOffsetLeftPer = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    CommondParam = table.Column<string>(type: "text", nullable: true),
                    CommondType = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGraphToolBoxes", x => x.ToolBoxId);
                    table.ForeignKey(
                        name: "FK_DeviceGraphToolBoxes_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceGraphToolBoxes_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceModels",
                columns: table => new
                {
                    DeviceModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: true),
                    ModelDesc = table.Column<string>(type: "text", nullable: true),
                    ModelStatus = table.Column<int>(type: "integer", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceModels", x => x.DeviceModelId);
                });

            migrationBuilder.CreateTable(
                name: "DevicePortMappings",
                columns: table => new
                {
                    MappingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<string>(type: "text", nullable: true),
                    TargeId = table.Column<string>(type: "text", nullable: true),
                    SourceElementId = table.Column<string>(type: "text", nullable: true),
                    TargetElementId = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    MappingStatus = table.Column<int>(type: "integer", nullable: false),
                    MappingIndex = table.Column<int>(type: "integer", nullable: false),
                    SourceDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetDeviceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePortMappings", x => x.MappingId);
                });

            migrationBuilder.CreateTable(
                name: "DevicePorts",
                columns: table => new
                {
                    PortId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortName = table.Column<string>(type: "text", nullable: true),
                    PortDesc = table.Column<string>(type: "text", nullable: true),
                    PortPic = table.Column<string>(type: "text", nullable: true),
                    PortType = table.Column<int>(type: "integer", nullable: false),
                    PortPhyType = table.Column<int>(type: "integer", nullable: false),
                    PortStatus = table.Column<int>(type: "integer", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Creator = table.Column<long>(type: "bigint", nullable: false),
                    PortElementId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePorts", x => x.PortId);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormFieldInfos",
                columns: table => new
                {
                    FieldId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FieldName = table.Column<string>(type: "text", nullable: true),
                    FieldValue = table.Column<string>(type: "text", nullable: true),
                    FieldValueType = table.Column<int>(type: "integer", nullable: false),
                    FormId = table.Column<long>(type: "bigint", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FieldEditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FieldCode = table.Column<string>(type: "text", nullable: true),
                    FieldUnit = table.Column<string>(type: "text", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    FieldStatus = table.Column<int>(type: "integer", nullable: false),
                    FieldI18nKey = table.Column<string>(type: "text", nullable: true),
                    FieldValueDataSource = table.Column<string>(type: "text", nullable: true),
                    FieldValueLocalDataSource = table.Column<string>(type: "text", nullable: true),
                    FieldPattern = table.Column<string>(type: "text", nullable: true),
                    FieldMaxLength = table.Column<int>(type: "integer", nullable: false),
                    FieldValueTypeName = table.Column<string>(type: "text", nullable: true),
                    FieldUIElement = table.Column<long>(type: "bigint", nullable: false),
                    FieldUIElementSchema = table.Column<string>(type: "text", nullable: true),
                    FieldPocoTypeName = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormFieldInfos", x => x.FieldId);
                    table.ForeignKey(
                        name: "FK_DynamicFormFieldInfos_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicFormFieldInfos_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormFieldValueInfos",
                columns: table => new
                {
                    FieldValueId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FieldId = table.Column<long>(type: "bigint", nullable: false),
                    FieldName = table.Column<string>(type: "text", nullable: true),
                    FieldValue = table.Column<string>(type: "text", nullable: true),
                    FromId = table.Column<long>(type: "bigint", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FieldCode = table.Column<string>(type: "text", nullable: true),
                    FieldUnit = table.Column<string>(type: "text", nullable: true),
                    FieldValueType = table.Column<long>(type: "bigint", nullable: false),
                    BizId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormFieldValueInfos", x => x.FieldValueId);
                    table.ForeignKey(
                        name: "FK_DynamicFormFieldValueInfos_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicFormFieldValueInfos_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormInfos",
                columns: table => new
                {
                    FormId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BizId = table.Column<long>(type: "bigint", nullable: false),
                    FormCreator = table.Column<long>(type: "bigint", nullable: false),
                    FormName = table.Column<string>(type: "text", nullable: true),
                    FormDesc = table.Column<string>(type: "text", nullable: true),
                    FormStatus = table.Column<int>(type: "integer", nullable: false),
                    FormSchame = table.Column<string>(type: "text", nullable: true),
                    ModelClass = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    FromCreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FormLayout = table.Column<string>(type: "text", nullable: true),
                    IsCompact = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormInfos", x => x.FormId);
                    table.ForeignKey(
                        name: "FK_DynamicFormInfos_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicFormInfos_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FlowRules",
                columns: table => new
                {
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Describes = table.Column<string>(type: "text", nullable: true),
                    Runner = table.Column<string>(type: "text", nullable: true),
                    ExecutableCode = table.Column<string>(type: "text", nullable: true),
                    Creator = table.Column<string>(type: "text", nullable: true),
                    RuleDesc = table.Column<string>(type: "text", nullable: true),
                    RuleStatus = table.Column<int>(type: "integer", nullable: true),
                    CreatTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DefinitionsXml = table.Column<string>(type: "text", nullable: true),
                    ParentRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubVersion = table.Column<double>(type: "double precision", nullable: false),
                    Version = table.Column<double>(type: "double precision", nullable: false),
                    CreateId = table.Column<Guid>(type: "uuid", nullable: false),
                    MountType = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowRules", x => x.RuleId);
                    table.ForeignKey(
                        name: "FK_FlowRules_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FlowRules_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Token = table.Column<string>(type: "text", nullable: true),
                    JwtId = table.Column<string>(type: "text", nullable: true),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    IsRevorked = table.Column<bool>(type: "boolean", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RuleTaskExecutors",
                columns: table => new
                {
                    ExecutorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutorName = table.Column<string>(type: "text", nullable: true),
                    ExecutorDesc = table.Column<string>(type: "text", nullable: true),
                    Path = table.Column<string>(type: "text", nullable: true),
                    TypeName = table.Column<string>(type: "text", nullable: true),
                    DefaultConfig = table.Column<string>(type: "text", nullable: true),
                    MataData = table.Column<string>(type: "text", nullable: true),
                    Tag = table.Column<string>(type: "text", nullable: true),
                    ExecutorStatus = table.Column<int>(type: "integer", nullable: false),
                    AddDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    TestStatus = table.Column<int>(type: "integer", nullable: false),
                    Tester = table.Column<Guid>(type: "uuid", nullable: false),
                    TesterDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleTaskExecutors", x => x.ExecutorId);
                    table.ForeignKey(
                        name: "FK_RuleTaskExecutors_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RuleTaskExecutors_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionEvents",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventName = table.Column<string>(type: "text", nullable: true),
                    EventDesc = table.Column<string>(type: "text", nullable: true),
                    EventNameSpace = table.Column<string>(type: "text", nullable: true),
                    EventStatus = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EventParam = table.Column<string>(type: "text", nullable: true),
                    EventTag = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_SubscriptionEvents_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubscriptionEvents_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceGraphs",
                columns: table => new
                {
                    GraphId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    GraphShape = table.Column<string>(type: "text", nullable: true),
                    GraphWidth = table.Column<int>(type: "integer", nullable: false),
                    GraphHeight = table.Column<int>(type: "integer", nullable: false),
                    GraphPostionX = table.Column<int>(type: "integer", nullable: false),
                    GraphPostionY = table.Column<int>(type: "integer", nullable: false),
                    GraphElementId = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceDiagramDiagramId = table.Column<Guid>(type: "uuid", nullable: true),
                    GraphFill = table.Column<string>(type: "text", nullable: true),
                    GraphStroke = table.Column<string>(type: "text", nullable: true),
                    GraphStrokeWidth = table.Column<int>(type: "integer", nullable: false),
                    GraphTextFill = table.Column<string>(type: "text", nullable: true),
                    GraphTextFontSize = table.Column<int>(type: "integer", nullable: false),
                    GraphTextRefX = table.Column<int>(type: "integer", nullable: false),
                    GraphTextAnchor = table.Column<string>(type: "text", nullable: true),
                    GraphTextVerticalAnchor = table.Column<string>(type: "text", nullable: true),
                    GraphTextFontFamily = table.Column<string>(type: "text", nullable: true),
                    GraphTextRefY = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGraphs", x => x.GraphId);
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_DeviceDiagrams_DeviceDiagramDiagramId",
                        column: x => x.DeviceDiagramDiagramId,
                        principalTable: "DeviceDiagrams",
                        principalColumn: "DiagramId");
                    table.ForeignKey(
                        name: "FK_DeviceGraphs_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceModelCommands",
                columns: table => new
                {
                    CommandId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommandTitle = table.Column<string>(type: "text", nullable: true),
                    CommandI18N = table.Column<string>(type: "text", nullable: true),
                    CommandType = table.Column<int>(type: "integer", nullable: false),
                    CommandParams = table.Column<string>(type: "text", nullable: true),
                    CommandName = table.Column<string>(type: "text", nullable: true),
                    CommandTemplate = table.Column<string>(type: "text", nullable: true),
                    DeviceModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    CommandStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceModelCommands", x => x.CommandId);
                    table.ForeignKey(
                        name: "FK_DeviceModelCommands_DeviceModels_DeviceModelId",
                        column: x => x.DeviceModelId,
                        principalTable: "DeviceModels",
                        principalColumn: "DeviceModelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaseEvents",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventName = table.Column<string>(type: "text", nullable: true),
                    EventDesc = table.Column<string>(type: "text", nullable: true),
                    EventStaus = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    MataData = table.Column<string>(type: "text", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    FlowRuleRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Bizid = table.Column<string>(type: "text", nullable: true),
                    CreaterDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BizData = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_BaseEvents_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaseEvents_FlowRules_FlowRuleRuleId",
                        column: x => x.FlowRuleRuleId,
                        principalTable: "FlowRules",
                        principalColumn: "RuleId");
                    table.ForeignKey(
                        name: "FK_BaseEvents_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceRules",
                columns: table => new
                {
                    DeviceRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: true),
                    FlowRuleRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfigUser = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnableTrace = table.Column<int>(type: "integer", nullable: false)
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
                        name: "FK_DeviceRules_FlowRules_FlowRuleRuleId",
                        column: x => x.FlowRuleRuleId,
                        principalTable: "FlowRules",
                        principalColumn: "RuleId");
                });

            migrationBuilder.CreateTable(
                name: "Flows",
                columns: table => new
                {
                    FlowId = table.Column<Guid>(type: "uuid", nullable: false),
                    bpmnid = table.Column<string>(type: "text", nullable: true),
                    Flowname = table.Column<string>(type: "text", nullable: true),
                    FlowRuleRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Flowdesc = table.Column<string>(type: "text", nullable: true),
                    ObjectId = table.Column<string>(type: "text", nullable: true),
                    FlowType = table.Column<string>(type: "text", nullable: true),
                    SourceId = table.Column<string>(type: "text", nullable: true),
                    TargetId = table.Column<string>(type: "text", nullable: true),
                    NodeProcessClass = table.Column<string>(type: "text", nullable: true),
                    Conditionexpression = table.Column<string>(type: "text", nullable: true),
                    NodeProcessMethod = table.Column<string>(type: "text", nullable: true),
                    NodeProcessParams = table.Column<string>(type: "text", nullable: true),
                    NodeProcessType = table.Column<string>(type: "text", nullable: true),
                    NodeProcessScriptType = table.Column<string>(type: "text", nullable: true),
                    NodeProcessScript = table.Column<string>(type: "text", nullable: true),
                    Incoming = table.Column<string>(type: "text", nullable: true),
                    Outgoing = table.Column<string>(type: "text", nullable: true),
                    FlowStatus = table.Column<int>(type: "integer", nullable: false),
                    TestStatus = table.Column<int>(type: "integer", nullable: false),
                    Tester = table.Column<Guid>(type: "uuid", nullable: false),
                    TesterDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Createor = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutorId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flows", x => x.FlowId);
                    table.ForeignKey(
                        name: "FK_Flows_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Flows_FlowRules_FlowRuleRuleId",
                        column: x => x.FlowRuleRuleId,
                        principalTable: "FlowRules",
                        principalColumn: "RuleId");
                    table.ForeignKey(
                        name: "FK_Flows_RuleTaskExecutors_ExecutorId",
                        column: x => x.ExecutorId,
                        principalTable: "RuleTaskExecutors",
                        principalColumn: "ExecutorId");
                    table.ForeignKey(
                        name: "FK_Flows_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionTasks",
                columns: table => new
                {
                    BindId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    RuleTaskExecutorExecutorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TaskConfig = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionTasks", x => x.BindId);
                    table.ForeignKey(
                        name: "FK_SubscriptionTasks_RuleTaskExecutors_RuleTaskExecutorExecuto~",
                        column: x => x.RuleTaskExecutorExecutorId,
                        principalTable: "RuleTaskExecutors",
                        principalColumn: "ExecutorId");
                    table.ForeignKey(
                        name: "FK_SubscriptionTasks_SubscriptionEvents_SubscriptionEventId",
                        column: x => x.SubscriptionEventId,
                        principalTable: "SubscriptionEvents",
                        principalColumn: "EventId");
                });

            migrationBuilder.CreateTable(
                name: "FlowOperations",
                columns: table => new
                {
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NodeStatus = table.Column<int>(type: "integer", nullable: false),
                    OperationDesc = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    BizId = table.Column<string>(type: "text", nullable: true),
                    bpmnid = table.Column<string>(type: "text", nullable: true),
                    FlowId = table.Column<Guid>(type: "uuid", nullable: true),
                    FlowRuleRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    BaseEventEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    Step = table.Column<int>(type: "integer", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowOperations", x => x.OperationId);
                    table.ForeignKey(
                        name: "FK_FlowOperations_BaseEvents_BaseEventEventId",
                        column: x => x.BaseEventEventId,
                        principalTable: "BaseEvents",
                        principalColumn: "EventId");
                    table.ForeignKey(
                        name: "FK_FlowOperations_FlowRules_FlowRuleRuleId",
                        column: x => x.FlowRuleRuleId,
                        principalTable: "FlowRules",
                        principalColumn: "RuleId");
                    table.ForeignKey(
                        name: "FK_FlowOperations_Flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Flows",
                        principalColumn: "FlowId");
                });

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
                name: "IX_DeviceGraphs_DeviceDiagramDiagramId",
                table: "DeviceGraphs",
                column: "DeviceDiagramDiagramId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_TenantId",
                table: "DeviceGraphs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphToolBoxes_CustomerId",
                table: "DeviceGraphToolBoxes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphToolBoxes_TenantId",
                table: "DeviceGraphToolBoxes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceModelCommands_DeviceModelId",
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
                name: "IX_DynamicFormFieldInfos_CustomerId",
                table: "DynamicFormFieldInfos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldInfos_TenantId",
                table: "DynamicFormFieldInfos",
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
                name: "IX_DynamicFormInfos_CustomerId",
                table: "DynamicFormInfos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormInfos_TenantId",
                table: "DynamicFormInfos",
                column: "TenantId");

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

            migrationBuilder.CreateIndex(
                name: "IX_FlowRules_CustomerId",
                table: "FlowRules",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowRules_TenantId",
                table: "FlowRules",
                column: "TenantId");

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
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleTaskExecutors_CustomerId",
                table: "RuleTaskExecutors",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleTaskExecutors_TenantId",
                table: "RuleTaskExecutors",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionEvents_CustomerId",
                table: "SubscriptionEvents",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionEvents_TenantId",
                table: "SubscriptionEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTasks_RuleTaskExecutorExecutorId",
                table: "SubscriptionTasks",
                column: "RuleTaskExecutorExecutorId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTasks_SubscriptionEventId",
                table: "SubscriptionTasks",
                column: "SubscriptionEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_DeviceModels_DeviceModelId",
                table: "Device",
                column: "DeviceModelId",
                principalTable: "DeviceModels",
                principalColumn: "DeviceModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceIdentities_Device_DeviceId",
                table: "DeviceIdentities",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Device_DeviceModels_DeviceModelId",
                table: "Device");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceIdentities_Device_DeviceId",
                table: "DeviceIdentities");

            migrationBuilder.DropTable(
                name: "BaseDictionaries");

            migrationBuilder.DropTable(
                name: "BaseDictionaryGroups");

            migrationBuilder.DropTable(
                name: "BaseI18Ns");

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
                name: "DynamicFormFieldInfos");

            migrationBuilder.DropTable(
                name: "DynamicFormFieldValueInfos");

            migrationBuilder.DropTable(
                name: "DynamicFormInfos");

            migrationBuilder.DropTable(
                name: "FlowOperations");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SubscriptionTasks");

            migrationBuilder.DropTable(
                name: "DeviceDiagrams");

            migrationBuilder.DropTable(
                name: "DeviceModels");

            migrationBuilder.DropTable(
                name: "BaseEvents");

            migrationBuilder.DropTable(
                name: "Flows");

            migrationBuilder.DropTable(
                name: "SubscriptionEvents");

            migrationBuilder.DropTable(
                name: "FlowRules");

            migrationBuilder.DropTable(
                name: "RuleTaskExecutors");

            migrationBuilder.DropIndex(
                name: "IX_DeviceIdentities_DeviceId",
                table: "DeviceIdentities");

            migrationBuilder.DropIndex(
                name: "IX_Device_DeviceModelId",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "DeviceModelId",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Device");

            migrationBuilder.AlterColumn<long>(
                name: "Value_Long",
                table: "TelemetryData",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Value_Double",
                table: "TelemetryData",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "TelemetryData",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "TelemetryData",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "TelemetryData",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<int>(
                name: "DataSide",
                table: "TelemetryData",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "TelemetryData",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "TelemetryData",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "TelemetryData",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceIdentities",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastActive",
                table: "Device",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<long>(
                name: "Value_Long",
                table: "DataStorage",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Value_Double",
                table: "DataStorage",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "DataStorage",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "DataStorage",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "DataStorage",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "DataStorage",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "DataSide",
                table: "DataStorage",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "DataStorage",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DataStorage",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "Catalog",
                table: "DataStorage",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActiveDateTime",
                table: "AuditLog",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId_KeyName_DateTime",
                table: "TelemetryData",
                columns: new[] { "DeviceId", "KeyName", "DateTime" });

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
        }
    }
}
