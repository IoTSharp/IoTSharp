using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.Oracle.Migrations
{
    /// <summary>建立个人资料和按收件人隔离的站内信表。</summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261127090000_AddPersonalCenter")]
    public sealed class AddPersonalCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalProfiles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: true),
                    Introduction = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    AvatarPng = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalProfiles", profile => profile.UserId);
                });

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Title = table.Column<string>(type: "NVARCHAR2(160)", maxLength: 160, nullable: false),
                    Body = table.Column<string>(type: "NVARCHAR2(1500)", maxLength: 1500, nullable: false),
                    Category = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    Sender = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", message => message.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inbox_Recipient_CreatedAt",
                table: "InboxMessages",
                columns: new[] { "UserId", "TenantId", "CustomerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Inbox_Recipient_ReadAt",
                table: "InboxMessages",
                columns: new[] { "UserId", "TenantId", "CustomerId", "ReadAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("InboxMessages");
            migrationBuilder.DropTable("PersonalProfiles");
        }
    }
}
