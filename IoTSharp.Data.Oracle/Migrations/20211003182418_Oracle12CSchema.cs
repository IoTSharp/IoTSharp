using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Data.Oracle.Migrations
{
    public partial class Oracle12CSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    UserName = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaseDictionaries",
                columns: table => new
                {
                    DictionaryId = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "1, 1"),
                    DictionaryName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DictionaryValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Dictionary18NKeyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DictionaryStatus = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DictionaryValueType = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DictionaryValueTypeName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DictionaryGroupId = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    DictionaryPattern = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DictionaryDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DictionaryColor = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DictionaryIcon = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DictionaryTag = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseDictionaries", x => x.DictionaryId);
                });

            migrationBuilder.CreateTable(
                name: "BaseDictionaryGroups",
                columns: table => new
                {
                    DictionaryGroupId = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "1, 1"),
                    DictionaryGroupName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DictionaryGroupKey = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DictionaryGroupValueType = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DictionaryGroupStatus = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DictionaryGroupValueTypeName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DictionaryGroupDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DictionaryGroup18NKeyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseDictionaryGroups", x => x.DictionaryGroupId);
                });

            migrationBuilder.CreateTable(
                name: "BaseEvents",
                columns: table => new
                {
                    EventId = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "1, 1"),
                    EventName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EventDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EventStaus = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MataData = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RuleId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Bizid = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreaterDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseEvents", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "BaseI18Ns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    KeyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueBG = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueCS = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueDA = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueDEDE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueESES = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueENUS = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueENGR = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueELGR = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueFI = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueFRFR = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueHE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueHRHR = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueHU = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueITIT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueJAJP = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueKOKR = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueNL = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValuePLPL = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValuePT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueSLSL = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueTRTR = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueSR = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueSV = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueUK = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueVI = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueZHCN = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ValueZHTW = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ResourceType = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ResourceId = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    ResourceKey = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ResourceTag = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ResouceDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ResouceGroupId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    AddDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseI18Ns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataStorage",
                columns: table => new
                {
                    Catalog = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DeviceId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    KeyName = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataSide = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Value_Boolean = table.Column<bool>(type: "NUMBER(1)", nullable: true),
                    Value_String = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Value_Long = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    Value_DateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Value_Double = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    Value_Json = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Value_XML = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Value_Binary = table.Column<byte[]>(type: "RAW(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataStorage", x => new { x.Catalog, x.DeviceId, x.KeyName });
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormFieldInfos",
                columns: table => new
                {
                    FieldId = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "1, 1"),
                    FieldName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldValueType = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FormId = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FieldEditDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FieldCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldUnit = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsRequired = table.Column<bool>(type: "NUMBER(1)", nullable: true),
                    IsEnabled = table.Column<bool>(type: "NUMBER(1)", nullable: true),
                    FieldStatus = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FieldI18nKey = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldValueDataSource = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldValueLocalDataSource = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldPattern = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldMaxLength = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FieldValueTypeName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldUIElement = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    FieldUIElementSchema = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldPocoTypeName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormFieldInfos", x => x.FieldId);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormFieldValueInfos",
                columns: table => new
                {
                    FieldValueId = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "1, 1"),
                    FieldId = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    FieldName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FromId = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FieldCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldUnit = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldValueType = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    BizId = table.Column<long>(type: "NUMBER(19)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormFieldValueInfos", x => x.FieldValueId);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormInfos",
                columns: table => new
                {
                    FormId = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "1, 1"),
                    BizId = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    FormCreator = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    FormName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FormDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FormStatus = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FormSchame = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ModelClass = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Url = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Creator = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FromCreateDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FormLayout = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsCompact = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormInfos", x => x.FormId);
                });

            migrationBuilder.CreateTable(
                name: "FlowOperations",
                columns: table => new
                {
                    OperationId = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "1, 1"),
                    AddDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    NodeStatus = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    OperationDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Data = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    BizId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    bpmnid = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FlowId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    RuleId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    EventId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    Step = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Tag = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowOperations", x => x.OperationId);
                });

            migrationBuilder.CreateTable(
                name: "FlowRules",
                columns: table => new
                {
                    RuleId = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "1, 1"),
                    RuleType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Describes = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Runner = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ExecutableCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Creator = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RuleDesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RuleStatus = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CreatTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DefinitionsXml = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowRules", x => x.RuleId);
                });

            migrationBuilder.CreateTable(
                name: "Flows",
                columns: table => new
                {
                    FlowId = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "1, 1"),
                    bpmnid = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Flowname = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RuleId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    Flowdesc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ObjectId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FlowType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SourceId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TargetId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    NodeProcessClass = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Conditionexpression = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    NodeProcessMethod = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    NodeProcessParams = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    NodeProcessType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    NodeProcessScriptType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    NodeProcessScript = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Incoming = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Outgoing = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flows", x => x.FlowId);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryData",
                columns: table => new
                {
                    DateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DeviceId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    KeyName = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    DataSide = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Value_Boolean = table.Column<bool>(type: "NUMBER(1)", nullable: true),
                    Value_String = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Value_Long = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    Value_DateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Value_Double = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    Value_Json = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Value_XML = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Value_Binary = table.Column<byte[]>(type: "RAW(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryData", x => new { x.DeviceId, x.KeyName, x.DateTime });
                });

            migrationBuilder.CreateTable(
                name: "Tenant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EMail = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Phone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Country = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Province = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    City = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Street = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Address = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ZipCode = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ClaimValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRol~",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ClaimValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUse~",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    UserId = table.Column<string>(type: "NVARCHAR2(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUse~",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    RoleId = table.Column<string>(type: "NVARCHAR2(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRole~",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUser~",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Value = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUse~",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Email = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Phone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Country = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Province = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    City = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Street = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Address = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ZipCode = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customer_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    UserId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    UserName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ObjectID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ObjectName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ObjectType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ActionName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ActionData = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ActionResult = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ActiveDateTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_Customer_Customer~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLog_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizedKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    AuthToken = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizedKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizedKeys_Customer_Cu~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuthorizedKeys_Tenant_Tena~",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Relationship",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    IdentityUserId = table.Column<string>(type: "NVARCHAR2(450)", nullable: true),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relationship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Relationship_AspNetUsers_I~",
                        column: x => x.IdentityUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relationship_Customer_Cust~",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relationship_Tenant_Tenant~",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Device",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DeviceType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Online = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    LastActive = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Timeout = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    OwnerId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    AuthorizedKeyId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Device_AuthorizedKeys_Auth~",
                        column: x => x.AuthorizedKeyId,
                        principalTable: "AuthorizedKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Device_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Device_Device_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Device_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    IdentityType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IdentityId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IdentityValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DeviceId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceIdentities_Device_De~",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "\"NormalizedName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "\"NormalizedUserName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_CustomerId",
                table: "AuditLog",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_TenantId",
                table: "AuditLog",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedKeys_CustomerId",
                table: "AuthorizedKeys",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedKeys_TenantId",
                table: "AuthorizedKeys",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_TenantId",
                table: "Customer",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_Catalog",
                table: "DataStorage",
                column: "Catalog");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_Catalog_Device~",
                table: "DataStorage",
                columns: new[] { "Catalog", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Device_AuthorizedKeyId",
                table: "Device",
                column: "AuthorizedKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_CustomerId",
                table: "Device",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_OwnerId",
                table: "Device",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_TenantId",
                table: "Device",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIdentities_DeviceId",
                table: "DeviceIdentities",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationship_CustomerId",
                table: "Relationship",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationship_IdentityUserId",
                table: "Relationship",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationship_TenantId",
                table: "Relationship",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId",
                table: "TelemetryData",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId_Key~",
                table: "TelemetryData",
                columns: new[] { "DeviceId", "KeyName" });

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_KeyName",
                table: "TelemetryData",
                column: "KeyName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "BaseDictionaries");

            migrationBuilder.DropTable(
                name: "BaseDictionaryGroups");

            migrationBuilder.DropTable(
                name: "BaseEvents");

            migrationBuilder.DropTable(
                name: "BaseI18Ns");

            migrationBuilder.DropTable(
                name: "DataStorage");

            migrationBuilder.DropTable(
                name: "DeviceIdentities");

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

            migrationBuilder.DropTable(
                name: "Relationship");

            migrationBuilder.DropTable(
                name: "TelemetryData");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "AuthorizedKeys");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "Tenant");
        }
    }
}
