using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Sqlite.Migrations
{
    public partial class AddAlarm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlarmType = table.Column<string>(type: "TEXT", nullable: true),
                    AlarmDetail = table.Column<string>(type: "TEXT", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_CustomerId",
                table: "Alarms",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_TenantId",
                table: "Alarms",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alarms");
        }
    }
}
