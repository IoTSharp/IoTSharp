using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Data.Sqlite.Migrations
{
    public partial class modify_somevalue_can_nullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TelemetryData_DeviceId_KeyName_DateTime",
                table: "TelemetryData");

            migrationBuilder.AlterColumn<long>(
                name: "Value_Long",
                table: "TelemetryData",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<double>(
                name: "Value_Double",
                table: "TelemetryData",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "TelemetryData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "TelemetryData",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "Value_Long",
                table: "DataStorage",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<double>(
                name: "Value_Double",
                table: "DataStorage",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "DataStorage",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "DataStorage",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.CreateTable(
                name: "BaseDictionaries",
                columns: table => new
                {
                    DictionaryId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DictionaryName = table.Column<string>(type: "TEXT", nullable: true),
                    DictionaryValue = table.Column<string>(type: "TEXT", nullable: true),
                    Dictionary18NKeyName = table.Column<string>(type: "TEXT", nullable: true),
                    DictionaryStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    DictionaryValueType = table.Column<int>(type: "INTEGER", nullable: true),
                    DictionaryValueTypeName = table.Column<string>(type: "TEXT", nullable: true),
                    DictionaryGroupId = table.Column<long>(type: "INTEGER", nullable: true),
                    DictionaryPattern = table.Column<string>(type: "TEXT", nullable: true),
                    DictionaryDesc = table.Column<string>(type: "TEXT", nullable: true),
                    DictionaryColor = table.Column<string>(type: "TEXT", nullable: true),
                    DictionaryIcon = table.Column<string>(type: "TEXT", nullable: true),
                    DictionaryTag = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseDictionaries", x => x.DictionaryId);
                });

            migrationBuilder.CreateTable(
                name: "BaseDictionaryGroups",
                columns: table => new
                {
                    DictionaryGroupId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DictionaryGroupName = table.Column<string>(type: "TEXT", nullable: true),
                    DictionaryGroupKey = table.Column<string>(type: "TEXT", nullable: true),
                    DictionaryGroupValueType = table.Column<int>(type: "INTEGER", nullable: true),
                    DictionaryGroupStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    DictionaryGroupValueTypeName = table.Column<string>(type: "TEXT", nullable: true),
                    DictionaryGroupDesc = table.Column<string>(type: "TEXT", nullable: true),
                    DictionaryGroup18NKeyName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseDictionaryGroups", x => x.DictionaryGroupId);
                });

            migrationBuilder.CreateTable(
                name: "BaseEvents",
                columns: table => new
                {
                    EventId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventName = table.Column<string>(type: "TEXT", nullable: true),
                    EventDesc = table.Column<string>(type: "TEXT", nullable: true),
                    EventStaus = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    MataData = table.Column<string>(type: "TEXT", nullable: true),
                    RuleId = table.Column<long>(type: "INTEGER", nullable: false),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bizid = table.Column<string>(type: "TEXT", nullable: true),
                    CreaterDateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseEvents", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "BaseI18Ns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    KeyName = table.Column<string>(type: "TEXT", nullable: true),
                    ValueBG = table.Column<string>(type: "TEXT", nullable: true),
                    ValueCS = table.Column<string>(type: "TEXT", nullable: true),
                    ValueDA = table.Column<string>(type: "TEXT", nullable: true),
                    ValueDEDE = table.Column<string>(type: "TEXT", nullable: true),
                    ValueESES = table.Column<string>(type: "TEXT", nullable: true),
                    ValueENUS = table.Column<string>(type: "TEXT", nullable: true),
                    ValueENGR = table.Column<string>(type: "TEXT", nullable: true),
                    ValueELGR = table.Column<string>(type: "TEXT", nullable: true),
                    ValueFI = table.Column<string>(type: "TEXT", nullable: true),
                    ValueFRFR = table.Column<string>(type: "TEXT", nullable: true),
                    ValueHE = table.Column<string>(type: "TEXT", nullable: true),
                    ValueHRHR = table.Column<string>(type: "TEXT", nullable: true),
                    ValueHU = table.Column<string>(type: "TEXT", nullable: true),
                    ValueITIT = table.Column<string>(type: "TEXT", nullable: true),
                    ValueJAJP = table.Column<string>(type: "TEXT", nullable: true),
                    ValueKOKR = table.Column<string>(type: "TEXT", nullable: true),
                    ValueNL = table.Column<string>(type: "TEXT", nullable: true),
                    ValuePLPL = table.Column<string>(type: "TEXT", nullable: true),
                    ValuePT = table.Column<string>(type: "TEXT", nullable: true),
                    ValueSLSL = table.Column<string>(type: "TEXT", nullable: true),
                    ValueTRTR = table.Column<string>(type: "TEXT", nullable: true),
                    ValueSR = table.Column<string>(type: "TEXT", nullable: true),
                    ValueSV = table.Column<string>(type: "TEXT", nullable: true),
                    ValueUK = table.Column<string>(type: "TEXT", nullable: true),
                    ValueVI = table.Column<string>(type: "TEXT", nullable: true),
                    ValueZHCN = table.Column<string>(type: "TEXT", nullable: true),
                    ValueZHTW = table.Column<string>(type: "TEXT", nullable: true),
                    ResourceType = table.Column<int>(type: "INTEGER", nullable: true),
                    ResourceId = table.Column<long>(type: "INTEGER", nullable: true),
                    ResourceKey = table.Column<string>(type: "TEXT", nullable: true),
                    ResourceTag = table.Column<string>(type: "TEXT", nullable: true),
                    ResouceDesc = table.Column<string>(type: "TEXT", nullable: true),
                    ResouceGroupId = table.Column<int>(type: "INTEGER", nullable: true),
                    AddDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseI18Ns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormFieldInfos",
                columns: table => new
                {
                    FieldId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FieldName = table.Column<string>(type: "TEXT", nullable: true),
                    FieldValue = table.Column<string>(type: "TEXT", nullable: true),
                    FieldValueType = table.Column<int>(type: "INTEGER", nullable: true),
                    FormId = table.Column<long>(type: "INTEGER", nullable: true),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FieldEditDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FieldCode = table.Column<string>(type: "TEXT", nullable: true),
                    FieldUnit = table.Column<string>(type: "TEXT", nullable: true),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: true),
                    FieldStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    FieldI18nKey = table.Column<string>(type: "TEXT", nullable: true),
                    FieldValueDataSource = table.Column<string>(type: "TEXT", nullable: true),
                    FieldValueLocalDataSource = table.Column<string>(type: "TEXT", nullable: true),
                    FieldPattern = table.Column<string>(type: "TEXT", nullable: true),
                    FieldMaxLength = table.Column<int>(type: "INTEGER", nullable: true),
                    FieldValueTypeName = table.Column<string>(type: "TEXT", nullable: true),
                    FieldUIElement = table.Column<long>(type: "INTEGER", nullable: true),
                    FieldUIElementSchema = table.Column<string>(type: "TEXT", nullable: true),
                    FieldPocoTypeName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormFieldInfos", x => x.FieldId);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormFieldValueInfos",
                columns: table => new
                {
                    FieldValueId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FieldId = table.Column<long>(type: "INTEGER", nullable: true),
                    FieldName = table.Column<string>(type: "TEXT", nullable: true),
                    FieldValue = table.Column<string>(type: "TEXT", nullable: true),
                    FromId = table.Column<long>(type: "INTEGER", nullable: true),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FieldCode = table.Column<string>(type: "TEXT", nullable: true),
                    FieldUnit = table.Column<string>(type: "TEXT", nullable: true),
                    FieldValueType = table.Column<long>(type: "INTEGER", nullable: true),
                    BizId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormFieldValueInfos", x => x.FieldValueId);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormInfos",
                columns: table => new
                {
                    FormId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BizId = table.Column<long>(type: "INTEGER", nullable: true),
                    FormCreator = table.Column<long>(type: "INTEGER", nullable: true),
                    FormName = table.Column<string>(type: "TEXT", nullable: true),
                    FormDesc = table.Column<string>(type: "TEXT", nullable: true),
                    FormStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    FormSchame = table.Column<string>(type: "TEXT", nullable: true),
                    ModelClass = table.Column<string>(type: "TEXT", nullable: true),
                    Url = table.Column<string>(type: "TEXT", nullable: true),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromCreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FormLayout = table.Column<string>(type: "TEXT", nullable: true),
                    IsCompact = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormInfos", x => x.FormId);
                });

            migrationBuilder.CreateTable(
                name: "FlowOperations",
                columns: table => new
                {
                    OperationId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AddDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NodeStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    OperationDesc = table.Column<string>(type: "TEXT", nullable: true),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    BizId = table.Column<string>(type: "TEXT", nullable: true),
                    bpmnid = table.Column<string>(type: "TEXT", nullable: true),
                    FlowId = table.Column<long>(type: "INTEGER", nullable: false),
                    RuleId = table.Column<long>(type: "INTEGER", nullable: false),
                    EventId = table.Column<long>(type: "INTEGER", nullable: false),
                    Step = table.Column<int>(type: "INTEGER", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowOperations", x => x.OperationId);
                });

            migrationBuilder.CreateTable(
                name: "FlowRules",
                columns: table => new
                {
                    RuleId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RuleType = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Describes = table.Column<string>(type: "TEXT", nullable: true),
                    Runner = table.Column<string>(type: "TEXT", nullable: true),
                    ExecutableCode = table.Column<string>(type: "TEXT", nullable: true),
                    Creator = table.Column<string>(type: "TEXT", nullable: true),
                    RuleDesc = table.Column<string>(type: "TEXT", nullable: true),
                    RuleStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DefinitionsXml = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowRules", x => x.RuleId);
                });

            migrationBuilder.CreateTable(
                name: "Flows",
                columns: table => new
                {
                    FlowId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    bpmnid = table.Column<string>(type: "TEXT", nullable: true),
                    Flowname = table.Column<string>(type: "TEXT", nullable: true),
                    RuleId = table.Column<long>(type: "INTEGER", nullable: false),
                    Flowdesc = table.Column<string>(type: "TEXT", nullable: true),
                    ObjectId = table.Column<string>(type: "TEXT", nullable: true),
                    FlowType = table.Column<string>(type: "TEXT", nullable: true),
                    SourceId = table.Column<string>(type: "TEXT", nullable: true),
                    TargetId = table.Column<string>(type: "TEXT", nullable: true),
                    NodeProcessClass = table.Column<string>(type: "TEXT", nullable: true),
                    Conditionexpression = table.Column<string>(type: "TEXT", nullable: true),
                    NodeProcessMethod = table.Column<string>(type: "TEXT", nullable: true),
                    NodeProcessParams = table.Column<string>(type: "TEXT", nullable: true),
                    NodeProcessType = table.Column<string>(type: "TEXT", nullable: true),
                    NodeProcessScriptType = table.Column<string>(type: "TEXT", nullable: true),
                    NodeProcessScript = table.Column<string>(type: "TEXT", nullable: true),
                    Incoming = table.Column<string>(type: "TEXT", nullable: true),
                    Outgoing = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flows", x => x.FlowId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaseDictionaries");

            migrationBuilder.DropTable(
                name: "BaseDictionaryGroups");

            migrationBuilder.DropTable(
                name: "BaseEvents");

            migrationBuilder.DropTable(
                name: "BaseI18Ns");

            migrationBuilder.DropTable(
                name: "DynamicFormFieldInfos");

            migrationBuilder.DropTable(
                name: "DynamicFormFieldValueInfos");

            migrationBuilder.DropTable(
                name: "DynamicFormInfos");

            migrationBuilder.DropTable(
                name: "FlowOperations");

            migrationBuilder.DropTable(
                name: "FlowRules");

            migrationBuilder.DropTable(
                name: "Flows");

            migrationBuilder.AlterColumn<long>(
                name: "Value_Long",
                table: "TelemetryData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Value_Double",
                table: "TelemetryData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "TelemetryData",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "TelemetryData",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Value_Long",
                table: "DataStorage",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Value_Double",
                table: "DataStorage",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "DataStorage",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "DataStorage",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId_KeyName_DateTime",
                table: "TelemetryData",
                columns: new[] { "DeviceId", "KeyName", "DateTime" });
        }
    }
}
