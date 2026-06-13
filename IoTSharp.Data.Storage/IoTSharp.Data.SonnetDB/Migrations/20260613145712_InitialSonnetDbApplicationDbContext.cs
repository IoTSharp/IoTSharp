using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SonnetDB.Migrations
{
    /// <inheritdoc />
    public partial class InitialSonnetDbApplicationDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AISettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    Name = table.Column<string>(type: "STRING", nullable: true),
                    MCP_API_KEY = table.Column<string>(type: "STRING", nullable: true),
                    Role = table.Column<int>(type: "INT", nullable: false),
                    Enable = table.Column<bool>(type: "BOOL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AISettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "STRING", nullable: false),
                    Name = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "STRING", nullable: false),
                    UserName = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "STRING", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "BOOL", nullable: false),
                    PasswordHash = table.Column<string>(type: "STRING", nullable: true),
                    SecurityStamp = table.Column<string>(type: "STRING", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "STRING", nullable: true),
                    PhoneNumber = table.Column<string>(type: "STRING", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "BOOL", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "BOOL", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "DATETIME", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "BOOL", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaseDictionaries",
                columns: table => new
                {
                    DictionaryId = table.Column<long>(type: "INT", nullable: false),
                    DictionaryName = table.Column<string>(type: "STRING", nullable: true),
                    DictionaryValue = table.Column<string>(type: "STRING", nullable: true),
                    Dictionary18NKeyName = table.Column<string>(type: "STRING", nullable: true),
                    DictionaryStatus = table.Column<int>(type: "INT", nullable: true),
                    DictionaryValueType = table.Column<int>(type: "INT", nullable: true),
                    DictionaryValueTypeName = table.Column<string>(type: "STRING", nullable: true),
                    DictionaryGroupId = table.Column<long>(type: "INT", nullable: true),
                    DictionaryPattern = table.Column<string>(type: "STRING", nullable: true),
                    DictionaryDesc = table.Column<string>(type: "STRING", nullable: true),
                    DictionaryColor = table.Column<string>(type: "STRING", nullable: true),
                    DictionaryIcon = table.Column<string>(type: "STRING", nullable: true),
                    DictionaryTag = table.Column<string>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseDictionaries", x => x.DictionaryId);
                });

            migrationBuilder.CreateTable(
                name: "BaseDictionaryGroups",
                columns: table => new
                {
                    DictionaryGroupId = table.Column<long>(type: "INT", nullable: false),
                    DictionaryGroupName = table.Column<string>(type: "STRING", nullable: true),
                    DictionaryGroupKey = table.Column<string>(type: "STRING", nullable: true),
                    DictionaryGroupValueType = table.Column<int>(type: "INT", nullable: true),
                    DictionaryGroupStatus = table.Column<int>(type: "INT", nullable: true),
                    DictionaryGroupValueTypeName = table.Column<string>(type: "STRING", nullable: true),
                    DictionaryGroupDesc = table.Column<string>(type: "STRING", nullable: true),
                    DictionaryGroup18NKeyName = table.Column<string>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseDictionaryGroups", x => x.DictionaryGroupId);
                });

            migrationBuilder.CreateTable(
                name: "BaseI18Ns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INT", nullable: false),
                    UserId = table.Column<Guid>(type: "STRING", nullable: false),
                    Status = table.Column<int>(type: "INT", nullable: false),
                    KeyName = table.Column<string>(type: "STRING", nullable: true),
                    ValueBG = table.Column<string>(type: "STRING", nullable: true),
                    ValueCS = table.Column<string>(type: "STRING", nullable: true),
                    ValueDA = table.Column<string>(type: "STRING", nullable: true),
                    ValueDEDE = table.Column<string>(type: "STRING", nullable: true),
                    ValueESES = table.Column<string>(type: "STRING", nullable: true),
                    ValueENUS = table.Column<string>(type: "STRING", nullable: true),
                    ValueENGR = table.Column<string>(type: "STRING", nullable: true),
                    ValueELGR = table.Column<string>(type: "STRING", nullable: true),
                    ValueFI = table.Column<string>(type: "STRING", nullable: true),
                    ValueFRFR = table.Column<string>(type: "STRING", nullable: true),
                    ValueHE = table.Column<string>(type: "STRING", nullable: true),
                    ValueHRHR = table.Column<string>(type: "STRING", nullable: true),
                    ValueHU = table.Column<string>(type: "STRING", nullable: true),
                    ValueITIT = table.Column<string>(type: "STRING", nullable: true),
                    ValueJAJP = table.Column<string>(type: "STRING", nullable: true),
                    ValueKOKR = table.Column<string>(type: "STRING", nullable: true),
                    ValueNL = table.Column<string>(type: "STRING", nullable: true),
                    ValuePLPL = table.Column<string>(type: "STRING", nullable: true),
                    ValuePT = table.Column<string>(type: "STRING", nullable: true),
                    ValueSLSL = table.Column<string>(type: "STRING", nullable: true),
                    ValueTRTR = table.Column<string>(type: "STRING", nullable: true),
                    ValueSR = table.Column<string>(type: "STRING", nullable: true),
                    ValueSV = table.Column<string>(type: "STRING", nullable: true),
                    ValueUK = table.Column<string>(type: "STRING", nullable: true),
                    ValueVI = table.Column<string>(type: "STRING", nullable: true),
                    ValueZHCN = table.Column<string>(type: "STRING", nullable: true),
                    ValueZHTW = table.Column<string>(type: "STRING", nullable: true),
                    ResourceType = table.Column<int>(type: "INT", nullable: true),
                    ResourceId = table.Column<long>(type: "INT", nullable: true),
                    ResourceKey = table.Column<string>(type: "STRING", nullable: true),
                    ResourceTag = table.Column<string>(type: "STRING", nullable: true),
                    ResouceDesc = table.Column<string>(type: "STRING", nullable: true),
                    ResouceGroupId = table.Column<int>(type: "INT", nullable: true),
                    AddDate = table.Column<DateTime>(type: "DATETIME", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseI18Ns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceModels",
                columns: table => new
                {
                    DeviceModelId = table.Column<Guid>(type: "STRING", nullable: false),
                    ModelName = table.Column<string>(type: "STRING", nullable: true),
                    ModelDesc = table.Column<string>(type: "STRING", nullable: true),
                    ModelStatus = table.Column<int>(type: "INT", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceModels", x => x.DeviceModelId);
                });

            migrationBuilder.CreateTable(
                name: "DevicePortMappings",
                columns: table => new
                {
                    MappingId = table.Column<Guid>(type: "STRING", nullable: false),
                    SourceId = table.Column<string>(type: "STRING", nullable: true),
                    TargeId = table.Column<string>(type: "STRING", nullable: true),
                    SourceElementId = table.Column<string>(type: "STRING", nullable: true),
                    TargetElementId = table.Column<string>(type: "STRING", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    MappingStatus = table.Column<int>(type: "INT", nullable: false),
                    MappingIndex = table.Column<int>(type: "INT", nullable: false),
                    SourceDeviceId = table.Column<Guid>(type: "STRING", nullable: false),
                    TargetDeviceId = table.Column<Guid>(type: "STRING", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePortMappings", x => x.MappingId);
                });

            migrationBuilder.CreateTable(
                name: "DevicePorts",
                columns: table => new
                {
                    PortId = table.Column<Guid>(type: "STRING", nullable: false),
                    PortName = table.Column<string>(type: "STRING", nullable: true),
                    PortDesc = table.Column<string>(type: "STRING", nullable: true),
                    PortPic = table.Column<string>(type: "STRING", nullable: true),
                    PortType = table.Column<int>(type: "INT", nullable: false),
                    PortPhyType = table.Column<int>(type: "INT", nullable: false),
                    PortStatus = table.Column<int>(type: "INT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "STRING", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    Creator = table.Column<long>(type: "INT", nullable: false),
                    PortElementId = table.Column<string>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePorts", x => x.PortId);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryData",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "STRING", nullable: false),
                    KeyName = table.Column<string>(type: "STRING", nullable: false),
                    DateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    DataSide = table.Column<int>(type: "INT", nullable: false),
                    Type = table.Column<int>(type: "INT", nullable: false),
                    Value_Boolean = table.Column<bool>(type: "BOOL", nullable: true),
                    Value_String = table.Column<string>(type: "STRING", nullable: true),
                    Value_Long = table.Column<long>(type: "INT", nullable: true),
                    Value_DateTime = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    Value_Double = table.Column<double>(type: "FLOAT", nullable: true),
                    Value_Json = table.Column<string>(type: "STRING", nullable: true),
                    Value_XML = table.Column<string>(type: "STRING", nullable: true),
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    Name = table.Column<string>(type: "STRING", nullable: true),
                    Email = table.Column<string>(type: "STRING", nullable: true),
                    Phone = table.Column<string>(type: "STRING", nullable: true),
                    Country = table.Column<string>(type: "STRING", nullable: true),
                    Province = table.Column<string>(type: "STRING", nullable: true),
                    City = table.Column<string>(type: "STRING", nullable: true),
                    Street = table.Column<string>(type: "STRING", nullable: true),
                    Address = table.Column<string>(type: "STRING", nullable: true),
                    ZipCode = table.Column<int>(type: "INT", nullable: false),
                    AISettingsId = table.Column<Guid>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenant_AISettings_AISettingsId",
                        column: x => x.AISettingsId,
                        principalTable: "AISettings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INT", nullable: false),
                    RoleId = table.Column<string>(type: "STRING", nullable: false),
                    ClaimType = table.Column<string>(type: "STRING", nullable: true),
                    ClaimValue = table.Column<string>(type: "STRING", nullable: true)
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
                    Id = table.Column<int>(type: "INT", nullable: false),
                    UserId = table.Column<string>(type: "STRING", nullable: false),
                    ClaimType = table.Column<string>(type: "STRING", nullable: true),
                    ClaimValue = table.Column<string>(type: "STRING", nullable: true)
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
                    LoginProvider = table.Column<string>(type: "STRING", nullable: false),
                    ProviderKey = table.Column<string>(type: "STRING", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "STRING", nullable: true),
                    UserId = table.Column<string>(type: "STRING", nullable: false)
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
                    UserId = table.Column<string>(type: "STRING", nullable: false),
                    RoleId = table.Column<string>(type: "STRING", nullable: false)
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
                    UserId = table.Column<string>(type: "STRING", nullable: false),
                    LoginProvider = table.Column<string>(type: "STRING", nullable: false),
                    Name = table.Column<string>(type: "STRING", nullable: false),
                    Value = table.Column<string>(type: "STRING", nullable: true)
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    UserId = table.Column<string>(type: "STRING", nullable: true),
                    Token = table.Column<string>(type: "STRING", nullable: true),
                    JwtId = table.Column<string>(type: "STRING", nullable: true),
                    IsUsed = table.Column<bool>(type: "BOOL", nullable: false),
                    IsRevorked = table.Column<bool>(type: "BOOL", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "DATETIME", nullable: false)
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
                    CommandId = table.Column<Guid>(type: "STRING", nullable: false),
                    CommandTitle = table.Column<string>(type: "STRING", nullable: true),
                    CommandI18N = table.Column<string>(type: "STRING", nullable: true),
                    CommandType = table.Column<int>(type: "INT", nullable: false),
                    CommandParams = table.Column<string>(type: "STRING", nullable: true),
                    CommandName = table.Column<string>(type: "STRING", nullable: true),
                    CommandTemplate = table.Column<string>(type: "STRING", nullable: true),
                    DeviceModelId = table.Column<Guid>(type: "STRING", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    CommandStatus = table.Column<int>(type: "INT", nullable: false)
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    Name = table.Column<string>(type: "STRING", nullable: true),
                    Email = table.Column<string>(type: "STRING", nullable: true),
                    Phone = table.Column<string>(type: "STRING", nullable: true),
                    Country = table.Column<string>(type: "STRING", nullable: true),
                    Province = table.Column<string>(type: "STRING", nullable: true),
                    City = table.Column<string>(type: "STRING", nullable: true),
                    Street = table.Column<string>(type: "STRING", nullable: true),
                    Address = table.Column<string>(type: "STRING", nullable: true),
                    ZipCode = table.Column<int>(type: "INT", nullable: false),
                    AISettingsId = table.Column<Guid>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customer_AISettings_AISettingsId",
                        column: x => x.AISettingsId,
                        principalTable: "AISettings",
                        principalColumn: "Id");
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    AlarmType = table.Column<string>(type: "STRING", nullable: true),
                    AlarmDetail = table.Column<string>(type: "STRING", nullable: true),
                    AckDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    ClearDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    AlarmStatus = table.Column<int>(type: "INT", nullable: false),
                    Serverity = table.Column<int>(type: "INT", nullable: false),
                    Propagate = table.Column<bool>(type: "BOOL", nullable: false),
                    OriginatorId = table.Column<Guid>(type: "STRING", nullable: false),
                    OriginatorType = table.Column<int>(type: "INT", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    Name = table.Column<string>(type: "STRING", nullable: true),
                    Description = table.Column<string>(type: "STRING", nullable: true),
                    AssetType = table.Column<string>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false)
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true),
                    UserId = table.Column<string>(type: "STRING", nullable: true),
                    UserName = table.Column<string>(type: "STRING", nullable: true),
                    ObjectID = table.Column<Guid>(type: "STRING", nullable: false),
                    ObjectName = table.Column<string>(type: "STRING", nullable: true),
                    ObjectType = table.Column<int>(type: "INT", nullable: false),
                    ActionName = table.Column<string>(type: "STRING", nullable: true),
                    ActionData = table.Column<string>(type: "STRING", nullable: true),
                    ActionResult = table.Column<string>(type: "STRING", nullable: true),
                    ActiveDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false)
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    Name = table.Column<string>(type: "STRING", nullable: true),
                    AuthToken = table.Column<string>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false)
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
                    DiagramId = table.Column<Guid>(type: "STRING", nullable: false),
                    DiagramName = table.Column<string>(type: "STRING", nullable: true),
                    DiagramDesc = table.Column<string>(type: "STRING", nullable: true),
                    DiagramStatus = table.Column<int>(type: "INT", nullable: false),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "DATETIME", nullable: true),
                    DiagramImage = table.Column<string>(type: "STRING", nullable: true),
                    IsDefault = table.Column<bool>(type: "BOOL", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                    ToolBoxId = table.Column<Guid>(type: "STRING", nullable: false),
                    ToolBoxName = table.Column<string>(type: "STRING", nullable: true),
                    ToolBoxIcon = table.Column<string>(type: "STRING", nullable: true),
                    ToolBoxStatus = table.Column<int>(type: "INT", nullable: false),
                    ToolBoxRequestUri = table.Column<string>(type: "STRING", nullable: true),
                    ToolBoxType = table.Column<string>(type: "STRING", nullable: true),
                    DeviceId = table.Column<long>(type: "INT", nullable: false),
                    ToolBoxOffsetX = table.Column<int>(type: "INT", nullable: false),
                    ToolBoxOffsetY = table.Column<int>(type: "INT", nullable: false),
                    ToolBoxOffsetTopPer = table.Column<int>(type: "INT", nullable: false),
                    ToolBoxOffsetLeftPer = table.Column<int>(type: "INT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    CommondParam = table.Column<string>(type: "STRING", nullable: true),
                    CommondType = table.Column<string>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                    FieldId = table.Column<long>(type: "INT", nullable: false),
                    FieldName = table.Column<string>(type: "STRING", nullable: true),
                    FieldValue = table.Column<string>(type: "STRING", nullable: true),
                    FieldValueType = table.Column<int>(type: "INT", nullable: false),
                    FormId = table.Column<long>(type: "INT", nullable: false),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    FieldEditDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    FieldCode = table.Column<string>(type: "STRING", nullable: true),
                    FieldUnit = table.Column<string>(type: "STRING", nullable: true),
                    IsRequired = table.Column<bool>(type: "BOOL", nullable: false),
                    IsEnabled = table.Column<bool>(type: "BOOL", nullable: false),
                    FieldStatus = table.Column<int>(type: "INT", nullable: false),
                    FieldI18nKey = table.Column<string>(type: "STRING", nullable: true),
                    FieldValueDataSource = table.Column<string>(type: "STRING", nullable: true),
                    FieldValueLocalDataSource = table.Column<string>(type: "STRING", nullable: true),
                    FieldPattern = table.Column<string>(type: "STRING", nullable: true),
                    FieldMaxLength = table.Column<int>(type: "INT", nullable: false),
                    FieldValueTypeName = table.Column<string>(type: "STRING", nullable: true),
                    FieldUIElement = table.Column<long>(type: "INT", nullable: false),
                    FieldUIElementSchema = table.Column<string>(type: "STRING", nullable: true),
                    FieldPocoTypeName = table.Column<string>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                    FieldValueId = table.Column<long>(type: "INT", nullable: false),
                    FieldId = table.Column<long>(type: "INT", nullable: false),
                    FieldName = table.Column<string>(type: "STRING", nullable: true),
                    FieldValue = table.Column<string>(type: "STRING", nullable: true),
                    FromId = table.Column<long>(type: "INT", nullable: false),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    FieldCreateDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    FieldCode = table.Column<string>(type: "STRING", nullable: true),
                    FieldUnit = table.Column<string>(type: "STRING", nullable: true),
                    FieldValueType = table.Column<long>(type: "INT", nullable: false),
                    BizId = table.Column<long>(type: "INT", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                    FormId = table.Column<long>(type: "INT", nullable: false),
                    BizId = table.Column<long>(type: "INT", nullable: false),
                    FormCreator = table.Column<long>(type: "INT", nullable: false),
                    FormName = table.Column<string>(type: "STRING", nullable: true),
                    FormDesc = table.Column<string>(type: "STRING", nullable: true),
                    FormStatus = table.Column<int>(type: "INT", nullable: false),
                    FormSchame = table.Column<string>(type: "STRING", nullable: true),
                    ModelClass = table.Column<string>(type: "STRING", nullable: true),
                    Url = table.Column<string>(type: "STRING", nullable: true),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    FromCreateDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    FormLayout = table.Column<string>(type: "STRING", nullable: true),
                    IsCompact = table.Column<bool>(type: "BOOL", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                    RuleId = table.Column<Guid>(type: "STRING", nullable: false),
                    RuleType = table.Column<int>(type: "INT", nullable: false),
                    Name = table.Column<string>(type: "STRING", nullable: false),
                    Describes = table.Column<string>(type: "STRING", nullable: true),
                    Runner = table.Column<string>(type: "STRING", nullable: true),
                    ExecutableCode = table.Column<string>(type: "STRING", nullable: true),
                    Creator = table.Column<string>(type: "STRING", nullable: true),
                    RuleDesc = table.Column<string>(type: "STRING", nullable: true),
                    RuleStatus = table.Column<int>(type: "INT", nullable: true),
                    CreatTime = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    DefinitionsXml = table.Column<string>(type: "STRING", nullable: true),
                    ParentRuleId = table.Column<Guid>(type: "STRING", nullable: false),
                    SubVersion = table.Column<double>(type: "FLOAT", nullable: false),
                    Version = table.Column<double>(type: "FLOAT", nullable: false),
                    CreateId = table.Column<Guid>(type: "STRING", nullable: false),
                    MountType = table.Column<int>(type: "INT", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                name: "Produces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    Name = table.Column<string>(type: "STRING", nullable: true),
                    Icon = table.Column<string>(type: "STRING", nullable: true),
                    GatewayType = table.Column<int>(type: "INT", nullable: false),
                    GatewayConfiguration = table.Column<string>(type: "STRING", nullable: true),
                    DefaultTimeout = table.Column<int>(type: "INT", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true),
                    DefaultIdentityType = table.Column<int>(type: "INT", nullable: false),
                    Description = table.Column<string>(type: "STRING", nullable: true),
                    DefaultDeviceType = table.Column<int>(type: "INT", nullable: false),
                    ProduceToken = table.Column<string>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produces_Customer_CustomerId",
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
                name: "Relationship",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    IdentityUserId = table.Column<string>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                    ExecutorId = table.Column<Guid>(type: "STRING", nullable: false),
                    ExecutorName = table.Column<string>(type: "STRING", nullable: true),
                    ExecutorDesc = table.Column<string>(type: "STRING", nullable: true),
                    Path = table.Column<string>(type: "STRING", nullable: true),
                    TypeName = table.Column<string>(type: "STRING", nullable: true),
                    DefaultConfig = table.Column<string>(type: "STRING", nullable: true),
                    MataData = table.Column<string>(type: "STRING", nullable: true),
                    Tag = table.Column<string>(type: "STRING", nullable: true),
                    ExecutorStatus = table.Column<int>(type: "INT", nullable: false),
                    AddDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    TestStatus = table.Column<int>(type: "INT", nullable: false),
                    Tester = table.Column<Guid>(type: "STRING", nullable: false),
                    TesterDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                    EventId = table.Column<Guid>(type: "STRING", nullable: false),
                    EventName = table.Column<string>(type: "STRING", nullable: true),
                    EventDesc = table.Column<string>(type: "STRING", nullable: true),
                    EventNameSpace = table.Column<string>(type: "STRING", nullable: true),
                    EventStatus = table.Column<int>(type: "INT", nullable: false),
                    Type = table.Column<int>(type: "INT", nullable: false),
                    EventParam = table.Column<string>(type: "STRING", nullable: true),
                    EventTag = table.Column<string>(type: "STRING", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    Name = table.Column<string>(type: "STRING", nullable: true),
                    Description = table.Column<string>(type: "STRING", nullable: true),
                    DeviceId = table.Column<Guid>(type: "STRING", nullable: false),
                    DataCatalog = table.Column<int>(type: "INT", nullable: false),
                    KeyName = table.Column<string>(type: "STRING", nullable: true),
                    AssetId = table.Column<Guid>(type: "STRING", nullable: true)
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
                name: "DeviceGraphs",
                columns: table => new
                {
                    GraphId = table.Column<Guid>(type: "STRING", nullable: false),
                    DeviceId = table.Column<Guid>(type: "STRING", nullable: false),
                    GraphShape = table.Column<string>(type: "STRING", nullable: true),
                    GraphWidth = table.Column<int>(type: "INT", nullable: false),
                    GraphHeight = table.Column<int>(type: "INT", nullable: false),
                    GraphPostionX = table.Column<int>(type: "INT", nullable: false),
                    GraphPostionY = table.Column<int>(type: "INT", nullable: false),
                    GraphElementId = table.Column<string>(type: "STRING", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    DeviceDiagramDiagramId = table.Column<Guid>(type: "STRING", nullable: true),
                    GraphFill = table.Column<string>(type: "STRING", nullable: true),
                    GraphStroke = table.Column<string>(type: "STRING", nullable: true),
                    GraphStrokeWidth = table.Column<int>(type: "INT", nullable: false),
                    GraphTextFill = table.Column<string>(type: "STRING", nullable: true),
                    GraphTextFontSize = table.Column<int>(type: "INT", nullable: false),
                    GraphTextRefX = table.Column<int>(type: "INT", nullable: false),
                    GraphTextAnchor = table.Column<string>(type: "STRING", nullable: true),
                    GraphTextVerticalAnchor = table.Column<string>(type: "STRING", nullable: true),
                    GraphTextFontFamily = table.Column<string>(type: "STRING", nullable: true),
                    GraphTextRefY = table.Column<int>(type: "INT", nullable: false),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                    EventId = table.Column<Guid>(type: "STRING", nullable: false),
                    EventName = table.Column<string>(type: "STRING", nullable: true),
                    EventDesc = table.Column<string>(type: "STRING", nullable: true),
                    EventStaus = table.Column<int>(type: "INT", nullable: false),
                    Type = table.Column<int>(type: "INT", nullable: false),
                    MataData = table.Column<string>(type: "STRING", nullable: true),
                    Creator = table.Column<Guid>(type: "STRING", nullable: false),
                    FlowRuleRuleId = table.Column<Guid>(type: "STRING", nullable: true),
                    Bizid = table.Column<string>(type: "STRING", nullable: true),
                    CreaterDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    BizData = table.Column<string>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true)
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
                name: "DataStorage",
                columns: table => new
                {
                    Catalog = table.Column<int>(type: "INT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "STRING", nullable: false),
                    KeyName = table.Column<string>(type: "STRING", nullable: false),
                    DateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    DataSide = table.Column<int>(type: "INT", nullable: false),
                    Type = table.Column<int>(type: "INT", nullable: false),
                    Value_Boolean = table.Column<bool>(type: "BOOL", nullable: true),
                    Value_String = table.Column<string>(type: "STRING", nullable: true),
                    Value_Long = table.Column<long>(type: "INT", nullable: true),
                    Value_DateTime = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    Value_Double = table.Column<double>(type: "FLOAT", nullable: true),
                    Value_Json = table.Column<string>(type: "STRING", nullable: true),
                    Value_XML = table.Column<string>(type: "STRING", nullable: true),
                    Value_Binary = table.Column<byte[]>(type: "BLOB", nullable: true),
                    OwnerId = table.Column<Guid>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataStorage", x => new { x.Catalog, x.DeviceId, x.KeyName });
                    table.ForeignKey(
                        name: "FK_DataStorage_Produces_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Produces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Device",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    Name = table.Column<string>(type: "STRING", nullable: true),
                    DeviceType = table.Column<int>(type: "INT", nullable: false),
                    Timeout = table.Column<int>(type: "INT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true),
                    DeviceModelId = table.Column<Guid>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    AuthorizedKeyId = table.Column<Guid>(type: "STRING", nullable: true),
                    ProduceId = table.Column<Guid>(type: "STRING", nullable: true)
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
                        name: "FK_Device_Produces_ProduceId",
                        column: x => x.ProduceId,
                        principalTable: "Produces",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Device_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProduceDataMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    ProduceId = table.Column<Guid>(type: "STRING", nullable: true),
                    ProduceKeyName = table.Column<string>(type: "STRING", nullable: true),
                    DataCatalog = table.Column<int>(type: "INT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "STRING", nullable: false),
                    DeviceKeyName = table.Column<string>(type: "STRING", nullable: true),
                    Description = table.Column<string>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduceDataMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProduceDataMappings_Produces_ProduceId",
                        column: x => x.ProduceId,
                        principalTable: "Produces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProduceDictionaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    KeyName = table.Column<string>(type: "STRING", nullable: true),
                    DisplayName = table.Column<string>(type: "STRING", nullable: true),
                    Unit = table.Column<string>(type: "STRING", nullable: true),
                    UnitExpression = table.Column<string>(type: "STRING", nullable: true),
                    UnitConvert = table.Column<bool>(type: "BOOL", nullable: false),
                    KeyDesc = table.Column<string>(type: "STRING", nullable: true),
                    DefaultValue = table.Column<string>(type: "STRING", nullable: true),
                    Display = table.Column<bool>(type: "BOOL", nullable: false),
                    Place0 = table.Column<string>(type: "STRING", nullable: true),
                    PlaceOrder0 = table.Column<string>(type: "STRING", nullable: true),
                    Place1 = table.Column<string>(type: "STRING", nullable: true),
                    PlaceOrder1 = table.Column<string>(type: "STRING", nullable: true),
                    Place2 = table.Column<string>(type: "STRING", nullable: true),
                    PlaceOrder2 = table.Column<string>(type: "STRING", nullable: true),
                    Place3 = table.Column<string>(type: "STRING", nullable: true),
                    PlaceOrder3 = table.Column<string>(type: "STRING", nullable: true),
                    Place4 = table.Column<string>(type: "STRING", nullable: true),
                    PlaceOrder4 = table.Column<string>(type: "STRING", nullable: true),
                    Place5 = table.Column<string>(type: "STRING", nullable: true),
                    PlaceOrder5 = table.Column<string>(type: "STRING", nullable: true),
                    DataType = table.Column<int>(type: "INT", nullable: false),
                    Tag = table.Column<string>(type: "STRING", nullable: true),
                    Customer = table.Column<Guid>(type: "STRING", nullable: true),
                    Deleted = table.Column<bool>(type: "BOOL", nullable: false),
                    ProduceId = table.Column<Guid>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduceDictionaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProduceDictionaries_Produces_ProduceId",
                        column: x => x.ProduceId,
                        principalTable: "Produces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Flows",
                columns: table => new
                {
                    FlowId = table.Column<Guid>(type: "STRING", nullable: false),
                    bpmnid = table.Column<string>(type: "STRING", nullable: true),
                    Flowname = table.Column<string>(type: "STRING", nullable: true),
                    FlowRuleRuleId = table.Column<Guid>(type: "STRING", nullable: true),
                    Flowdesc = table.Column<string>(type: "STRING", nullable: true),
                    ObjectId = table.Column<string>(type: "STRING", nullable: true),
                    FlowType = table.Column<string>(type: "STRING", nullable: true),
                    SourceId = table.Column<string>(type: "STRING", nullable: true),
                    TargetId = table.Column<string>(type: "STRING", nullable: true),
                    NodeProcessClass = table.Column<string>(type: "STRING", nullable: true),
                    Conditionexpression = table.Column<string>(type: "STRING", nullable: true),
                    NodeProcessMethod = table.Column<string>(type: "STRING", nullable: true),
                    NodeProcessParams = table.Column<string>(type: "STRING", nullable: true),
                    NodeProcessType = table.Column<string>(type: "STRING", nullable: true),
                    NodeProcessScriptType = table.Column<string>(type: "STRING", nullable: true),
                    NodeProcessScript = table.Column<string>(type: "STRING", nullable: true),
                    Incoming = table.Column<string>(type: "STRING", nullable: true),
                    Outgoing = table.Column<string>(type: "STRING", nullable: true),
                    FlowStatus = table.Column<int>(type: "INT", nullable: false),
                    TestStatus = table.Column<int>(type: "INT", nullable: false),
                    Tester = table.Column<Guid>(type: "STRING", nullable: false),
                    TesterDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    CreateId = table.Column<Guid>(type: "STRING", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Createor = table.Column<Guid>(type: "STRING", nullable: false),
                    ExecutorId = table.Column<Guid>(type: "STRING", nullable: true),
                    TenantId = table.Column<Guid>(type: "STRING", nullable: true),
                    CustomerId = table.Column<Guid>(type: "STRING", nullable: true),
                    Top = table.Column<string>(type: "STRING", nullable: true),
                    Left = table.Column<string>(type: "STRING", nullable: true),
                    FlowClass = table.Column<string>(type: "STRING", nullable: true),
                    FlowNameSpace = table.Column<string>(type: "STRING", nullable: true),
                    FlowIcon = table.Column<string>(type: "STRING", nullable: true),
                    FlowTag = table.Column<string>(type: "STRING", nullable: true),
                    FlowShapeType = table.Column<string>(type: "STRING", nullable: true)
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
                    BindId = table.Column<Guid>(type: "STRING", nullable: false),
                    EventId = table.Column<Guid>(type: "STRING", nullable: false),
                    SubscriptionEventId = table.Column<Guid>(type: "STRING", nullable: true),
                    RuleTaskExecutorExecutorId = table.Column<Guid>(type: "STRING", nullable: true),
                    Status = table.Column<int>(type: "INT", nullable: false),
                    TaskConfig = table.Column<string>(type: "STRING", nullable: true)
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
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    IdentityType = table.Column<int>(type: "INT", nullable: false),
                    IdentityId = table.Column<string>(type: "STRING", nullable: false),
                    IdentityValue = table.Column<string>(type: "STRING", nullable: true),
                    DeviceId = table.Column<Guid>(type: "STRING", nullable: false)
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
                    DeviceRuleId = table.Column<Guid>(type: "STRING", nullable: false),
                    DeviceId = table.Column<Guid>(type: "STRING", nullable: true),
                    FlowRuleRuleId = table.Column<Guid>(type: "STRING", nullable: true),
                    ConfigUser = table.Column<Guid>(type: "STRING", nullable: false),
                    ConfigDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    EnableTrace = table.Column<int>(type: "INT", nullable: false)
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
                    OperationId = table.Column<Guid>(type: "STRING", nullable: false),
                    AddDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    NodeStatus = table.Column<int>(type: "INT", nullable: false),
                    OperationDesc = table.Column<string>(type: "STRING", nullable: true),
                    Data = table.Column<string>(type: "STRING", nullable: true),
                    BizId = table.Column<string>(type: "STRING", nullable: true),
                    bpmnid = table.Column<string>(type: "STRING", nullable: true),
                    FlowId = table.Column<Guid>(type: "STRING", nullable: true),
                    FlowRuleRuleId = table.Column<Guid>(type: "STRING", nullable: true),
                    BaseEventEventId = table.Column<Guid>(type: "STRING", nullable: true),
                    Step = table.Column<int>(type: "INT", nullable: false),
                    Tag = table.Column<string>(type: "STRING", nullable: true)
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
                name: "IX_Customer_AISettingsId",
                table: "Customer",
                column: "AISettingsId");

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
                name: "IX_DataStorage_OwnerId",
                table: "DataStorage",
                column: "OwnerId");

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
                name: "IX_Device_ProduceId",
                table: "Device",
                column: "ProduceId");

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
                name: "IX_ProduceDataMappings_ProduceId",
                table: "ProduceDataMappings",
                column: "ProduceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProduceDictionaries_ProduceId",
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

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_AISettingsId",
                table: "Tenant",
                column: "AISettingsId");
        }

        /// <inheritdoc />
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
                name: "ProduceDataMappings");

            migrationBuilder.DropTable(
                name: "ProduceDictionaries");

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
                name: "DeviceModels");

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
                name: "Produces");

            migrationBuilder.DropTable(
                name: "FlowRules");

            migrationBuilder.DropTable(
                name: "RuleTaskExecutors");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "Tenant");

            migrationBuilder.DropTable(
                name: "AISettings");
        }
    }
}
