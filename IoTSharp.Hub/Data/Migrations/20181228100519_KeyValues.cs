using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Hub.Data.Migrations
{
    public partial class KeyValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientSideAttribute");

            migrationBuilder.DropTable(
                name: "ServerSideAttribute");

            migrationBuilder.DropTable(
                name: "SharedSideAttribute");

            migrationBuilder.RenameColumn(
                name: "UseId",
                table: "Relationship",
                newName: "Id");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "Relationship",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Relationship",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<string>(
                name: "IdentityId",
                table: "Relationship",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SharedSide",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    KeyName = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Value_Boolean = table.Column<bool>(nullable: false),
                    Value_String = table.Column<string>(nullable: true),
                    Value_Long = table.Column<long>(nullable: false),
                    Value_Double = table.Column<double>(nullable: false),
                    Value_Json = table.Column<string>(type: "jsonb", nullable: true),
                    Value_XML = table.Column<string>(type: "xml", nullable: true),
                    Value_Binary = table.Column<byte[]>(nullable: true),
                    DeviceId1 = table.Column<Guid>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    DeviceId = table.Column<Guid>(nullable: true),
                    Scope = table.Column<int>(nullable: true),
                    KeyValueDeviceLatest_DeviceId = table.Column<Guid>(nullable: true),
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
                    KeyName = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Value_Boolean = table.Column<bool>(nullable: false),
                    Value_String = table.Column<string>(nullable: true),
                    Value_Long = table.Column<long>(nullable: false),
                    Value_Double = table.Column<double>(nullable: false),
                    Value_Json = table.Column<string>(nullable: true),
                    Value_XML = table.Column<string>(nullable: true),
                    Value_Binary = table.Column<byte[]>(nullable: true),
                    DeviceId = table.Column<Guid>(nullable: false)
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
                name: "IX_Relationship_CustomerId",
                table: "Relationship",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationship_IdentityId",
                table: "Relationship",
                column: "IdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationship_TenantId",
                table: "Relationship",
                column: "TenantId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Relationship_Customer_CustomerId",
                table: "Relationship",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Relationship_AspNetUsers_IdentityId",
                table: "Relationship",
                column: "IdentityId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Relationship_Tenant_TenantId",
                table: "Relationship",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relationship_Customer_CustomerId",
                table: "Relationship");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationship_AspNetUsers_IdentityId",
                table: "Relationship");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationship_Tenant_TenantId",
                table: "Relationship");

            migrationBuilder.DropTable(
                name: "SharedSide");

            migrationBuilder.DropTable(
                name: "TelemetryData");

            migrationBuilder.DropIndex(
                name: "IX_Relationship_CustomerId",
                table: "Relationship");

            migrationBuilder.DropIndex(
                name: "IX_Relationship_IdentityId",
                table: "Relationship");

            migrationBuilder.DropIndex(
                name: "IX_Relationship_TenantId",
                table: "Relationship");

            migrationBuilder.DropColumn(
                name: "IdentityId",
                table: "Relationship");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Relationship",
                newName: "UseId");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "Relationship",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Relationship",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ClientSideAttribute",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Value_Binary = table.Column<byte[]>(nullable: true),
                    Value_Boolean = table.Column<bool>(nullable: false),
                    Value_Double = table.Column<double>(nullable: false),
                    Value_Json = table.Column<string>(type: "jsonb", nullable: true),
                    Value_Long = table.Column<long>(nullable: false),
                    Value_String = table.Column<string>(nullable: true),
                    Value_XML = table.Column<string>(type: "xml", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSideAttribute", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerSideAttribute",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Value_Binary = table.Column<byte[]>(nullable: true),
                    Value_Boolean = table.Column<bool>(nullable: false),
                    Value_Double = table.Column<double>(nullable: false),
                    Value_Json = table.Column<string>(type: "jsonb", nullable: true),
                    Value_Long = table.Column<long>(nullable: false),
                    Value_String = table.Column<string>(nullable: true),
                    Value_XML = table.Column<string>(type: "xml", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerSideAttribute", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SharedSideAttribute",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Value_Binary = table.Column<byte[]>(nullable: true),
                    Value_Boolean = table.Column<bool>(nullable: false),
                    Value_Double = table.Column<double>(nullable: false),
                    Value_Json = table.Column<string>(type: "jsonb", nullable: true),
                    Value_Long = table.Column<long>(nullable: false),
                    Value_String = table.Column<string>(nullable: true),
                    Value_XML = table.Column<string>(type: "xml", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedSideAttribute", x => x.Id);
                });
        }
    }
}
