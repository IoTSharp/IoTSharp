using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IoTSharp.Migrations
{
    public partial class modifycolumnkey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TelemetryData_DeviceId_KeyName_DateTime",
                table: "TelemetryData");

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
                    AddDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseI18Ns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormFieldInfos",
                columns: table => new
                {
                    FieldId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FieldName = table.Column<string>(type: "text", nullable: true),
                    FieldValue = table.Column<string>(type: "text", nullable: true),
                    FieldValueType = table.Column<int>(type: "integer", nullable: true),
                    FormId = table.Column<long>(type: "bigint", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FieldEditDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FieldCode = table.Column<string>(type: "text", nullable: true),
                    FieldUnit = table.Column<string>(type: "text", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    FieldStatus = table.Column<int>(type: "integer", nullable: true),
                    FieldI18nKey = table.Column<string>(type: "text", nullable: true),
                    FieldValueDataSource = table.Column<string>(type: "text", nullable: true),
                    FieldValueLocalDataSource = table.Column<string>(type: "text", nullable: true),
                    FieldPattern = table.Column<string>(type: "text", nullable: true),
                    FieldMaxLength = table.Column<int>(type: "integer", nullable: true),
                    FieldValueTypeName = table.Column<string>(type: "text", nullable: true),
                    FieldUIElement = table.Column<long>(type: "bigint", nullable: true),
                    FieldUIElementSchema = table.Column<string>(type: "text", nullable: true),
                    FieldPocoTypeName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormFieldInfos", x => x.FieldId);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormFieldValueInfos",
                columns: table => new
                {
                    FieldValueId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FieldId = table.Column<long>(type: "bigint", nullable: true),
                    FieldName = table.Column<string>(type: "text", nullable: true),
                    FieldValue = table.Column<string>(type: "text", nullable: true),
                    FromId = table.Column<long>(type: "bigint", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FieldCode = table.Column<string>(type: "text", nullable: true),
                    FieldUnit = table.Column<string>(type: "text", nullable: true),
                    FieldValueType = table.Column<long>(type: "bigint", nullable: true),
                    BizId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormFieldValueInfos", x => x.FieldValueId);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormInfos",
                columns: table => new
                {
                    FormId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BizId = table.Column<long>(type: "bigint", nullable: true),
                    FormCreator = table.Column<long>(type: "bigint", nullable: true),
                    FormName = table.Column<string>(type: "text", nullable: true),
                    FormDesc = table.Column<string>(type: "text", nullable: true),
                    FormStatus = table.Column<int>(type: "integer", nullable: true),
                    FormSchame = table.Column<string>(type: "text", nullable: true),
                    ModelClass = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    FromCreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FormLayout = table.Column<string>(type: "text", nullable: true),
                    IsCompact = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormInfos", x => x.FormId);
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
                    CreatTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DefinitionsXml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowRules", x => x.RuleId);
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
                    CreaterDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_BaseEvents_FlowRules_FlowRuleRuleId",
                        column: x => x.FlowRuleRuleId,
                        principalTable: "FlowRules",
                        principalColumn: "RuleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceRules",
                columns: table => new
                {
                    DeviceRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: true),
                    FlowRuleRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfigUser = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceRules", x => x.DeviceRuleId);
                    table.ForeignKey(
                        name: "FK_DeviceRules_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceRules_FlowRules_FlowRuleRuleId",
                        column: x => x.FlowRuleRuleId,
                        principalTable: "FlowRules",
                        principalColumn: "RuleId",
                        onDelete: ReferentialAction.Restrict);
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
                    Outgoing = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flows", x => x.FlowId);
                    table.ForeignKey(
                        name: "FK_Flows_FlowRules_FlowRuleRuleId",
                        column: x => x.FlowRuleRuleId,
                        principalTable: "FlowRules",
                        principalColumn: "RuleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlowOperations",
                columns: table => new
                {
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    NodeStatus = table.Column<int>(type: "integer", nullable: true),
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
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlowOperations_FlowRules_FlowRuleRuleId",
                        column: x => x.FlowRuleRuleId,
                        principalTable: "FlowRules",
                        principalColumn: "RuleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlowOperations_Flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Flows",
                        principalColumn: "FlowId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseEvents_FlowRuleRuleId",
                table: "BaseEvents",
                column: "FlowRuleRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceRules_DeviceId",
                table: "DeviceRules",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceRules_FlowRuleRuleId",
                table: "DeviceRules",
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

            migrationBuilder.CreateIndex(
                name: "IX_Flows_FlowRuleRuleId",
                table: "Flows",
                column: "FlowRuleRuleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaseDictionaries");

            migrationBuilder.DropTable(
                name: "BaseDictionaryGroups");

            migrationBuilder.DropTable(
                name: "BaseI18Ns");

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
                name: "BaseEvents");

            migrationBuilder.DropTable(
                name: "Flows");

            migrationBuilder.DropTable(
                name: "FlowRules");

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

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId_KeyName_DateTime",
                table: "TelemetryData",
                columns: new[] { "DeviceId", "KeyName", "DateTime" });
        }
    }
}
