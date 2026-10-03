using IoTSharp.Data;
using IoTSharp.Data.MySQL;
using IoTSharp.Data.Oracle;
using IoTSharp.Data.PostgreSQL;
using IoTSharp.Data.SonnetDB;
using IoTSharp.Data.Sqlite;
using IoTSharp.Data.SqlServer;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test;

/// <summary>
/// 验证各 provider 的租约升级脚本可以生成；脚本生成不代表数据库执行或并发能力验收。
/// </summary>
public sealed class ConfigurationLeaseMigrationTests
{
    /// <summary>
    /// 生成本次升级及降级 SQL，确认租约字段、索引和状态并发版本可被增删。
    /// </summary>
    /// <param name="provider">需要检查脚本的数据库 provider。</param>
    [Theory(Timeout = 30000)]
    [InlineData("Sqlite")]
    [InlineData("PostgreSQL")]
    [InlineData("SqlServer")]
    [InlineData("MySQL")]
    [InlineData("Oracle")]
    [InlineData("SonnetDB")]
    public async Task LeaseMigration_GeneratesProviderUpgradeScript(string provider)
    {
        var scripts = await Task.Run(() =>
        {
            using var context = CreateContext(provider);
            var migrator = context.GetService<IMigrator>();
            return (
                Upgrade: migrator.GenerateScript("20260706140000_AddReleaseCenter", "20261004010000_AddConfigurationOperationLease"),
                Downgrade: migrator.GenerateScript("20261004010000_AddConfigurationOperationLease", "20260706140000_AddReleaseCenter"));
        }).WaitAsync(TimeSpan.FromSeconds(25));
        Assert.Contains("ConfigurationOperationId", scripts.Upgrade, StringComparison.Ordinal);
        Assert.Contains("ConfigurationOperationExpiresAt", scripts.Upgrade, StringComparison.Ordinal);
        Assert.Contains("ConfigurationOperationOwner", scripts.Upgrade, StringComparison.Ordinal);
        Assert.Contains("IX_EdgeNodes_ConfigLeaseExpires", scripts.Upgrade, StringComparison.Ordinal);
        Assert.Contains("StateRevision", scripts.Upgrade, StringComparison.Ordinal);
        Assert.Contains("EdgeNodes", scripts.Downgrade, StringComparison.Ordinal);
        Assert.Contains("EdgeTasks", scripts.Downgrade, StringComparison.Ordinal);
        Assert.Contains("20261004010000_AddConfigurationOperationLease", scripts.Downgrade, StringComparison.Ordinal);
    }

    /// <summary>
    /// 复用已有设计时工厂，不连接真实数据库或创建新生产配置。
    /// </summary>
    /// <param name="provider">已知 provider 名称。</param>
    /// <returns>调用方负责释放的脚本生成上下文。</returns>
    private static ApplicationDbContext CreateContext(string provider) => provider switch
    {
        "Sqlite" => new SqliteDesignTimeApplicationDbContextFactory().CreateDbContext([]),
        "PostgreSQL" => new NpgsqlDesignTimeApplicationDbContextFactory().CreateDbContext([]),
        "SqlServer" => new SqlServerDesignTimeApplicationDbContextFactory().CreateDbContext([]),
        "MySQL" => new MySqlDesignTimeApplicationDbContextFactory().CreateDbContext([]),
        "Oracle" => new OracleDesignTimeApplicationDbContextFactory().CreateDbContext([]),
        "SonnetDB" => new SonnetDbDesignTimeApplicationDbContextFactory().CreateDbContext([]),
        _ => throw new ArgumentOutOfRangeException(nameof(provider))
    };
}
