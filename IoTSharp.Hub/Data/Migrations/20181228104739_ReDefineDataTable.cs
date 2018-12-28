using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Hub.Data.Migrations
{
    public partial class ReDefineDataTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedSide");

            migrationBuilder.DropTable(
                name: "TelemetryData");

            migrationBuilder.CreateTable(
                name: "AttributeData",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    KeyName = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Value_Boolean = table.Column<bool>(nullable: false),
                    Value_String = table.Column<string>(nullable: true),
                    Value_Long = table.Column<long>(nullable: false),
                    Value_Double = table.Column<double>(nullable: false),
                    Value_Json = table.Column<string>(type: "jsonb", nullable: true),
                    Value_XML = table.Column<string>(type: "xml", nullable: true),
                    Value_Binary = table.Column<byte[]>(nullable: true),
                    DeviceId = table.Column<Guid>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    Scope = table.Column<int>(nullable: true),
                    DeviceId1 = table.Column<Guid>(nullable: true),
                    TelemetryData_DeviceId1 = table.Column<Guid>(nullable: true),
                    TelemetryLatest_DeviceId1 = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeData_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttributeData_Device_DeviceId1",
                        column: x => x.DeviceId1,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttributeData_Device_TelemetryData_DeviceId1",
                        column: x => x.TelemetryData_DeviceId1,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttributeData_Device_TelemetryLatest_DeviceId1",
                        column: x => x.TelemetryLatest_DeviceId1,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttributeData_DeviceId",
                table: "AttributeData",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeData_DeviceId1",
                table: "AttributeData",
                column: "DeviceId1");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeData_TelemetryData_DeviceId1",
                table: "AttributeData",
                column: "TelemetryData_DeviceId1");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeData_TelemetryLatest_DeviceId1",
                table: "AttributeData",
                column: "TelemetryLatest_DeviceId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttributeData");

            migrationBuilder.CreateTable(
                name: "SharedSide",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DeviceId1 = table.Column<Guid>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    KeyName = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Value_Binary = table.Column<byte[]>(nullable: true),
                    Value_Boolean = table.Column<bool>(nullable: false),
                    Value_Double = table.Column<double>(nullable: false),
                    Value_Json = table.Column<string>(type: "jsonb", nullable: true),
                    Value_Long = table.Column<long>(nullable: false),
                    Value_String = table.Column<string>(nullable: true),
                    Value_XML = table.Column<string>(type: "xml", nullable: true),
                    DeviceId = table.Column<Guid>(nullable: true),
                    KeyValueDeviceLatest_DeviceId = table.Column<Guid>(nullable: true),
                    Scope = table.Column<int>(nullable: true),
                    KeyValueServerSide_DeviceId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedSide", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedSide_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedSide_Device_KeyValueDeviceLatest_DeviceId",
                        column: x => x.KeyValueDeviceLatest_DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedSide_Device_KeyValueServerSide_DeviceId",
                        column: x => x.KeyValueServerSide_DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedSide_Device_DeviceId1",
                        column: x => x.DeviceId1,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryData",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DeviceId = table.Column<Guid>(nullable: false),
                    KeyName = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Value_Binary = table.Column<byte[]>(nullable: true),
                    Value_Boolean = table.Column<bool>(nullable: false),
                    Value_Double = table.Column<double>(nullable: false),
                    Value_Json = table.Column<string>(nullable: true),
                    Value_Long = table.Column<long>(nullable: false),
                    Value_String = table.Column<string>(nullable: true),
                    Value_XML = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelemetryData_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedSide_DeviceId",
                table: "SharedSide",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedSide_KeyValueDeviceLatest_DeviceId",
                table: "SharedSide",
                column: "KeyValueDeviceLatest_DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedSide_KeyValueServerSide_DeviceId",
                table: "SharedSide",
                column: "KeyValueServerSide_DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedSide_DeviceId1",
                table: "SharedSide",
                column: "DeviceId1");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryData_DeviceId",
                table: "TelemetryData",
                column: "DeviceId");
        }
    }
}
