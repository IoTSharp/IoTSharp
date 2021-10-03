using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Data.SqlServer.Migrations
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
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<double>(
                name: "Value_Double",
                table: "TelemetryData",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "TelemetryData",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "TelemetryData",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

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
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "DataStorage",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "DataStorage",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateTable(
                name: "BaseDictionaries",
                columns: table => new
                {
                    DictionaryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DictionaryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DictionaryValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dictionary18NKeyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DictionaryStatus = table.Column<int>(type: "int", nullable: true),
                    DictionaryValueType = table.Column<int>(type: "int", nullable: true),
                    DictionaryValueTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DictionaryGroupId = table.Column<long>(type: "bigint", nullable: true),
                    DictionaryPattern = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DictionaryDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DictionaryColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DictionaryIcon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DictionaryTag = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DictionaryGroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DictionaryGroupKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DictionaryGroupValueType = table.Column<int>(type: "int", nullable: true),
                    DictionaryGroupStatus = table.Column<int>(type: "int", nullable: true),
                    DictionaryGroupValueTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DictionaryGroupDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DictionaryGroup18NKeyName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseDictionaryGroups", x => x.DictionaryGroupId);
                });

            migrationBuilder.CreateTable(
                name: "BaseEvents",
                columns: table => new
                {
                    EventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventStaus = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    MataData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleId = table.Column<long>(type: "bigint", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Bizid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreaterDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseEvents", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "BaseI18Ns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    KeyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueBG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueCS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueDA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueDEDE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueESES = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueENUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueENGR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueELGR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueFI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueFRFR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueHE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueHRHR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueHU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueITIT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueJAJP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueKOKR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueNL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValuePLPL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValuePT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueSLSL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueTRTR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueSR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueSV = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueUK = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueVI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueZHCN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueZHTW = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceType = table.Column<int>(type: "int", nullable: true),
                    ResourceId = table.Column<long>(type: "bigint", nullable: true),
                    ResourceKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResouceDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResouceGroupId = table.Column<int>(type: "int", nullable: true),
                    AddDate = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldValueType = table.Column<int>(type: "int", nullable: true),
                    FormId = table.Column<long>(type: "bigint", nullable: true),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FieldEditDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FieldCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: true),
                    FieldStatus = table.Column<int>(type: "int", nullable: true),
                    FieldI18nKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldValueDataSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldValueLocalDataSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldPattern = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldMaxLength = table.Column<int>(type: "int", nullable: true),
                    FieldValueTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldUIElement = table.Column<long>(type: "bigint", nullable: true),
                    FieldUIElementSchema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldPocoTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldId = table.Column<long>(type: "bigint", nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromId = table.Column<long>(type: "bigint", nullable: true),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FieldCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BizId = table.Column<long>(type: "bigint", nullable: true),
                    FormCreator = table.Column<long>(type: "bigint", nullable: true),
                    FormName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormStatus = table.Column<int>(type: "int", nullable: true),
                    FormSchame = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromCreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FormLayout = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCompact = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormInfos", x => x.FormId);
                });

            migrationBuilder.CreateTable(
                name: "FlowOperations",
                columns: table => new
                {
                    OperationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NodeStatus = table.Column<int>(type: "int", nullable: true),
                    OperationDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BizId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bpmnid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlowId = table.Column<long>(type: "bigint", nullable: false),
                    RuleId = table.Column<long>(type: "bigint", nullable: false),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    Step = table.Column<int>(type: "int", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowOperations", x => x.OperationId);
                });

            migrationBuilder.CreateTable(
                name: "FlowRules",
                columns: table => new
                {
                    RuleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Describes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Runner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutableCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleStatus = table.Column<int>(type: "int", nullable: true),
                    CreatTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DefinitionsXml = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowRules", x => x.RuleId);
                });

            migrationBuilder.CreateTable(
                name: "Flows",
                columns: table => new
                {
                    FlowId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    bpmnid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Flowname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleId = table.Column<long>(type: "bigint", nullable: false),
                    Flowdesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObjectId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlowType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NodeProcessClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Conditionexpression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NodeProcessMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NodeProcessParams = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NodeProcessType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NodeProcessScriptType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NodeProcessScript = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Incoming = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Outgoing = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Value_Double",
                table: "TelemetryData",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "TelemetryData",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "TelemetryData",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
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
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "DataStorage",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Value_Boolean",
                table: "DataStorage",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId_KeyName_DateTime",
                table: "TelemetryData",
                columns: new[] { "DeviceId", "KeyName", "DateTime" });
        }
    }
}
