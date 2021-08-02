using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IoTSharp.Migrations
{
    public partial class adddicfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    DictionaryIcon = table.Column<string>(type: "text", nullable: true)
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
                name: "BaseEvents",
                columns: table => new
                {
                    EventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventName = table.Column<string>(type: "text", nullable: true),
                    EventDesc = table.Column<string>(type: "text", nullable: true),
                    EventStaus = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    MataData = table.Column<string>(type: "text", nullable: true),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    CreaterDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    FromCreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormInfos", x => x.FormId);
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
        }
    }
}
