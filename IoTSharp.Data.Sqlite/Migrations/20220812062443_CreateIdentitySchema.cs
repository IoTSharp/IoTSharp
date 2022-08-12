using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
{
    public partial class CreateIdentitySchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaseDictionaries",
                columns: table => new
                {
                    DictionaryId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DictionaryName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DictionaryValue = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Dictionary18NKeyName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DictionaryStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    DictionaryValueType = table.Column<int>(type: "INTEGER", nullable: true),
                    DictionaryValueTypeName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DictionaryGroupId = table.Column<long>(type: "INTEGER", nullable: true),
                    DictionaryPattern = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DictionaryDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DictionaryColor = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DictionaryIcon = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DictionaryTag = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
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
                    DictionaryGroupName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DictionaryGroupKey = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DictionaryGroupValueType = table.Column<int>(type: "INTEGER", nullable: true),
                    DictionaryGroupStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    DictionaryGroupValueTypeName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DictionaryGroupDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DictionaryGroup18NKeyName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseDictionaryGroups", x => x.DictionaryGroupId);
                });

            migrationBuilder.CreateTable(
                name: "BaseI18Ns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    KeyName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueBG = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueCS = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueDA = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueDEDE = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueESES = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueENUS = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueENGR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueELGR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueFI = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueFRFR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueHE = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueHRHR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueHU = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueITIT = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueJAJP = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueKOKR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueNL = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValuePLPL = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValuePT = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueSLSL = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueTRTR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueSR = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueSV = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueUK = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueVI = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueZHCN = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ValueZHTW = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ResourceType = table.Column<int>(type: "INTEGER", nullable: true),
                    ResourceId = table.Column<long>(type: "INTEGER", nullable: true),
                    ResourceKey = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ResourceTag = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ResouceDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ResouceGroupId = table.Column<int>(type: "INTEGER", nullable: true),
                    AddDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseI18Ns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataStorage",
                columns: table => new
                {
                    Catalog = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KeyName = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataSide = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Value_Boolean = table.Column<bool>(type: "INTEGER", nullable: true),
                    Value_String = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Value_Long = table.Column<long>(type: "INTEGER", nullable: true),
                    Value_DateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Value_Double = table.Column<double>(type: "REAL", nullable: true),
                    Value_Json = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Value_XML = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Value_Binary = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataStorage", x => new { x.Catalog, x.DeviceId, x.KeyName });
                });

            migrationBuilder.CreateTable(
                name: "DeviceModels",
                columns: table => new
                {
                    DeviceModelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModelName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ModelDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ModelStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceModels", x => x.DeviceModelId);
                });

            migrationBuilder.CreateTable(
                name: "DevicePortMappings",
                columns: table => new
                {
                    MappingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    TargeId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    SourceElementId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    TargetElementId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    MappingStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    MappingIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceDeviceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetDeviceId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePortMappings", x => x.MappingId);
                });

            migrationBuilder.CreateTable(
                name: "DevicePorts",
                columns: table => new
                {
                    PortId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PortName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    PortDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    PortPic = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    PortType = table.Column<int>(type: "INTEGER", nullable: false),
                    PortPhyType = table.Column<int>(type: "INTEGER", nullable: false),
                    PortStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Creator = table.Column<long>(type: "INTEGER", nullable: false),
                    PortElementId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePorts", x => x.PortId);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryData",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KeyName = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataSide = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Value_Boolean = table.Column<bool>(type: "INTEGER", nullable: true),
                    Value_String = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Value_Long = table.Column<long>(type: "INTEGER", nullable: true),
                    Value_DateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Value_Double = table.Column<double>(type: "REAL", nullable: true),
                    Value_Json = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Value_XML = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Value_Binary = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryData", x => new { x.DeviceId, x.KeyName, x.DateTime });
                });

            migrationBuilder.CreateTable(
                name: "Tenant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    EMail = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Phone = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Country = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Province = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    City = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Street = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Address = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ZipCode = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    UserId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Name = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Value = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Token = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    JwtId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    IsUsed = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsRevorked = table.Column<bool>(type: "INTEGER", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "DeviceModelCommands",
                columns: table => new
                {
                    CommandId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CommandTitle = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CommandI18N = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CommandType = table.Column<int>(type: "INTEGER", nullable: false),
                    CommandParams = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CommandName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CommandTemplate = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DeviceModelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    CommandStatus = table.Column<int>(type: "INTEGER", nullable: false)
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
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Email = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Phone = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Country = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Province = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    City = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Street = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Address = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ZipCode = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customer_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Alarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlarmType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    AlarmDetail = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    AckDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClearDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AlarmStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Serverity = table.Column<int>(type: "INTEGER", nullable: false),
                    Propagate = table.Column<bool>(type: "INTEGER", nullable: false),
                    OriginatorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginatorType = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Description = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    AssetType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    UserName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ObjectID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ObjectName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ObjectType = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ActionData = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ActionResult = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ActiveDateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLog_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuthorizedKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    AuthToken = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizedKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizedKeys_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuthorizedKeys_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceDiagrams",
                columns: table => new
                {
                    DiagramId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiagramName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DiagramDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DiagramStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    DiagramImage = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    ToolBoxId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ToolBoxName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ToolBoxIcon = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ToolBoxStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ToolBoxRequestUri = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ToolBoxType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DeviceId = table.Column<long>(type: "INTEGER", nullable: false),
                    ToolBoxOffsetX = table.Column<int>(type: "INTEGER", nullable: false),
                    ToolBoxOffsetY = table.Column<int>(type: "INTEGER", nullable: false),
                    ToolBoxOffsetTopPer = table.Column<int>(type: "INTEGER", nullable: false),
                    ToolBoxOffsetLeftPer = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    CommondParam = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CommondType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                name: "DynamicFormFieldInfos",
                columns: table => new
                {
                    FieldId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FieldName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FieldValue = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FieldValueType = table.Column<int>(type: "INTEGER", nullable: false),
                    FormId = table.Column<long>(type: "INTEGER", nullable: false),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FieldEditDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FieldCode = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FieldUnit = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    FieldStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldI18nKey = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FieldValueDataSource = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FieldValueLocalDataSource = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FieldPattern = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FieldMaxLength = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldValueTypeName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FieldUIElement = table.Column<long>(type: "INTEGER", nullable: false),
                    FieldUIElementSchema = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FieldPocoTypeName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    FieldValueId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FieldId = table.Column<long>(type: "INTEGER", nullable: false),
                    FieldName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FieldValue = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FromId = table.Column<long>(type: "INTEGER", nullable: false),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FieldCode = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FieldUnit = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FieldValueType = table.Column<long>(type: "INTEGER", nullable: false),
                    BizId = table.Column<long>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    FormId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BizId = table.Column<long>(type: "INTEGER", nullable: false),
                    FormCreator = table.Column<long>(type: "INTEGER", nullable: false),
                    FormName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FormDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FormStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    FormSchame = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ModelClass = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Url = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromCreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FormLayout = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    IsCompact = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    RuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RuleType = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Describes = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Runner = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ExecutableCode = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Creator = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    RuleDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    RuleStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DefinitionsXml = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ParentRuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubVersion = table.Column<double>(type: "REAL", nullable: false),
                    Version = table.Column<double>(type: "REAL", nullable: false),
                    CreateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MountType = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                name: "Relationship",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IdentityUserId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relationship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Relationship_AspNetUsers_IdentityUserId",
                        column: x => x.IdentityUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Relationship_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Relationship_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RuleTaskExecutors",
                columns: table => new
                {
                    ExecutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExecutorName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ExecutorDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Path = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    TypeName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DefaultConfig = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    MataData = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Tag = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ExecutorStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    AddDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    TestStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Tester = table.Column<Guid>(type: "TEXT", nullable: false),
                    TesterDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    EventDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    EventNameSpace = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    EventStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    EventParam = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    EventTag = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CreateDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                name: "AssetRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Description = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DataCatalog = table.Column<int>(type: "INTEGER", nullable: false),
                    KeyName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    AssetId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetRelations_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Device",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DeviceType = table.Column<int>(type: "INTEGER", nullable: false),
                    Online = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastActive = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Timeout = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceModelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AuthorizedKeyId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Device_AuthorizedKeys_AuthorizedKeyId",
                        column: x => x.AuthorizedKeyId,
                        principalTable: "AuthorizedKeys",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Device_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Device_Device_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Device",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Device_DeviceModels_DeviceModelId",
                        column: x => x.DeviceModelId,
                        principalTable: "DeviceModels",
                        principalColumn: "DeviceModelId");
                    table.ForeignKey(
                        name: "FK_Device_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceGraphs",
                columns: table => new
                {
                    GraphId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GraphShape = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphHeight = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphPostionX = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphPostionY = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphElementId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceDiagramDiagramId = table.Column<Guid>(type: "TEXT", nullable: true),
                    GraphFill = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphStroke = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphStrokeWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphTextFill = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphTextFontSize = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphTextRefX = table.Column<int>(type: "INTEGER", nullable: false),
                    GraphTextAnchor = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphTextVerticalAnchor = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphTextFontFamily = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    GraphTextRefY = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                name: "BaseEvents",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    EventDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    EventStaus = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    MataData = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Creator = table.Column<Guid>(type: "TEXT", nullable: false),
                    FlowRuleRuleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Bizid = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    CreaterDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BizData = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                name: "Flows",
                columns: table => new
                {
                    FlowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    bpmnid = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Flowname = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FlowRuleRuleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Flowdesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    ObjectId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FlowType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    SourceId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    TargetId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    NodeProcessClass = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Conditionexpression = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    NodeProcessMethod = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    NodeProcessParams = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    NodeProcessType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    NodeProcessScriptType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    NodeProcessScript = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Incoming = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Outgoing = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FlowStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    TestStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Tester = table.Column<Guid>(type: "TEXT", nullable: false),
                    TesterDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Createor = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExecutorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    BindId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubscriptionEventId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RuleTaskExecutorExecutorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskConfig = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionTasks", x => x.BindId);
                    table.ForeignKey(
                        name: "FK_SubscriptionTasks_RuleTaskExecutors_RuleTaskExecutorExecutorId",
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
                name: "DeviceIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IdentityType = table.Column<int>(type: "INTEGER", nullable: false),
                    IdentityId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    IdentityValue = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceIdentities_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceRules",
                columns: table => new
                {
                    DeviceRuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FlowRuleRuleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ConfigUser = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EnableTrace = table.Column<int>(type: "INTEGER", nullable: false)
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
                name: "FlowOperations",
                columns: table => new
                {
                    OperationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AddDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NodeStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    OperationDesc = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    Data = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    BizId = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    bpmnid = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
                    FlowId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FlowRuleRuleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    BaseEventEventId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Step = table.Column<int>(type: "INTEGER", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
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
                name: "IX_Alarms_CustomerId",
                table: "Alarms",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_TenantId",
                table: "Alarms",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

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
                unique: true);

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
                name: "IX_Customer_TenantId",
                table: "Customer",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_Catalog",
                table: "DataStorage",
                column: "Catalog");

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_Catalog_DeviceId",
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
                name: "IX_Device_DeviceModelId",
                table: "Device",
                column: "DeviceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_OwnerId",
                table: "Device",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_TenantId",
                table: "Device",
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
                name: "IX_DeviceIdentities_DeviceId",
                table: "DeviceIdentities",
                column: "DeviceId",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId",
                table: "TelemetryData",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId_KeyName",
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
                name: "Alarms");

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
                name: "AssetRelations");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "BaseDictionaries");

            migrationBuilder.DropTable(
                name: "BaseDictionaryGroups");

            migrationBuilder.DropTable(
                name: "BaseI18Ns");

            migrationBuilder.DropTable(
                name: "DataStorage");

            migrationBuilder.DropTable(
                name: "DeviceGraphs");

            migrationBuilder.DropTable(
                name: "DeviceGraphToolBoxes");

            migrationBuilder.DropTable(
                name: "DeviceIdentities");

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
                name: "Relationship");

            migrationBuilder.DropTable(
                name: "SubscriptionTasks");

            migrationBuilder.DropTable(
                name: "TelemetryData");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "DeviceDiagrams");

            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "BaseEvents");

            migrationBuilder.DropTable(
                name: "Flows");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "SubscriptionEvents");

            migrationBuilder.DropTable(
                name: "AuthorizedKeys");

            migrationBuilder.DropTable(
                name: "DeviceModels");

            migrationBuilder.DropTable(
                name: "FlowRules");

            migrationBuilder.DropTable(
                name: "RuleTaskExecutors");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "Tenant");
        }
    }
}
