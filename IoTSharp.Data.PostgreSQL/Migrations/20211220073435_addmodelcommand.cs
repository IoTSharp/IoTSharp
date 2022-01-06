using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class addmodelcommand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeviceModelId",
                table: "Device",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeviceModels",
                columns: table => new
                {
                    DeviceModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: true),
                    ModelDesc = table.Column<string>(type: "text", nullable: true),
                    ModelStatus = table.Column<int>(type: "integer", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceModels", x => x.DeviceModelId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceModelCommands",
                columns: table => new
                {
                    CommandId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommandTitle = table.Column<string>(type: "text", nullable: true),
                    CommandI18N = table.Column<string>(type: "text", nullable: true),
                    CommandType = table.Column<int>(type: "integer", nullable: false),
                    CommandParams = table.Column<string>(type: "text", nullable: true),
                    CommandName = table.Column<string>(type: "text", nullable: true),
                    CommandTemplate = table.Column<string>(type: "text", nullable: true),
                    DeviceModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    CommandStatus = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Device_DeviceModelId",
                table: "Device",
                column: "DeviceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceModelCommands_DeviceModelId",
                table: "DeviceModelCommands",
                column: "DeviceModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_DeviceModels_DeviceModelId",
                table: "Device",
                column: "DeviceModelId",
                principalTable: "DeviceModels",
                principalColumn: "DeviceModelId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Device_DeviceModels_DeviceModelId",
                table: "Device");

            migrationBuilder.DropTable(
                name: "DeviceModelCommands");

            migrationBuilder.DropTable(
                name: "DeviceModels");

            migrationBuilder.DropIndex(
                name: "IX_Device_DeviceModelId",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "DeviceModelId",
                table: "Device");
        }
    }
}
