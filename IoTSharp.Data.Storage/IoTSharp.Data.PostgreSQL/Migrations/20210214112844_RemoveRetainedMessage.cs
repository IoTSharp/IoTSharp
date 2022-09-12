using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class RemoveRetainedMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceIdentities_Device_DeviceId",
                table: "DeviceIdentities");

            migrationBuilder.DropTable(
                name: "RetainedMessage");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceIdentities",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceIdentities_Device_DeviceId",
                table: "DeviceIdentities",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceIdentities_Device_DeviceId",
                table: "DeviceIdentities");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceIdentities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "RetainedMessage",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<byte[]>(type: "bytea", nullable: true),
                    QualityOfServiceLevel = table.Column<int>(type: "integer", nullable: false),
                    Retain = table.Column<bool>(type: "boolean", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetainedMessage", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceIdentities_Device_DeviceId",
                table: "DeviceIdentities",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
