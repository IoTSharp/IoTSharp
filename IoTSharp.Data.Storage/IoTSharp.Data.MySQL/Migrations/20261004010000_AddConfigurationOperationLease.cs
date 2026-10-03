using System;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.MySQL.Migrations
{
    /// <summary>
    /// 增加配置操作事务锚点及正式任务状态并发版本。
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261004010000_AddConfigurationOperationLease")]
    public partial class AddConfigurationOperationLease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(name: "ConfigurationOperationId", table: "EdgeNodes", type: "char(36)", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "ConfigurationOperationExpiresAt", table: "EdgeNodes", type: "datetime(6)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "ConfigurationOperationOwner", table: "EdgeNodes", type: "varchar(256)", maxLength: 256, nullable: true);
            migrationBuilder.CreateIndex(name: "IX_EdgeNodes_ConfigLeaseExpires", table: "EdgeNodes", column: "ConfigurationOperationExpiresAt");
            migrationBuilder.AddColumn<long>(name: "StateRevision", table: "EdgeTasks", type: "bigint", nullable: false, defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "StateRevision", table: "EdgeTasks");
            migrationBuilder.DropIndex(name: "IX_EdgeNodes_ConfigLeaseExpires", table: "EdgeNodes");
            migrationBuilder.DropColumn(name: "ConfigurationOperationOwner", table: "EdgeNodes");
            migrationBuilder.DropColumn(name: "ConfigurationOperationExpiresAt", table: "EdgeNodes");
            migrationBuilder.DropColumn(name: "ConfigurationOperationId", table: "EdgeNodes");
        }
    }
}
