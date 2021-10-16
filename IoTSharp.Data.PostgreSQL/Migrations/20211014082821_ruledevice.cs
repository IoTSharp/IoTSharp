using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IoTSharp.Migrations
{
    public partial class ruledevice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceRules",
                columns: table => new
                {
                    DeviceRuleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FlowId = table.Column<long>(type: "bigint", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigUser = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceRules", x => x.DeviceRuleId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceRules");
        }
    }
}
