using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class RetainedMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RetainedMessage",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Payload = table.Column<byte[]>(nullable: true),
                    QualityOfServiceLevel = table.Column<int>(nullable: false),
                    Topic = table.Column<string>(nullable: true),
                    Retain = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetainedMessage", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RetainedMessage");
        }
    }
}
