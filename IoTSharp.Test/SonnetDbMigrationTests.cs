#nullable enable

using IoTSharp.Data.SonnetDB;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using Xunit;

namespace IoTSharp.Test;

/// <summary>
/// 验证 SonnetDB 历史迁移生成的关键建表语义。
/// </summary>
public sealed class SonnetDbMigrationTests
{
    private const string InitialMigration = "20260613145712_InitialSonnetDbApplicationDbContext";

    /// <summary>
    /// 验证首个迁移保留全部数据库生成的整型主键，避免新安装写入默认数据时产生 NULL 主键。
    /// </summary>
    [Fact]
    public void InitialMigration_IntegerGeneratedKeys_AreAutoIncrement()
    {
        using var context = new SonnetDbDesignTimeApplicationDbContextFactory()
            .CreateDbContext(new[] { $"Data Source=.data/migration-test-{Guid.NewGuid():N}" });
        var sql = context.GetService<IMigrator>()
            .GenerateScript(Migration.InitialDatabase, InitialMigration);

        Assert.Equal(8, sql.Split("AUTO_INCREMENT", StringSplitOptions.None).Length - 1);
        Assert.Contains("\"DictionaryId\" INT AUTO_INCREMENT NOT NULL", sql, StringComparison.Ordinal);
        Assert.Contains("\"DictionaryGroupId\" INT AUTO_INCREMENT NOT NULL", sql, StringComparison.Ordinal);
        Assert.Contains("\"FieldId\" INT AUTO_INCREMENT NOT NULL", sql, StringComparison.Ordinal);
        Assert.Contains("\"FieldValueId\" INT AUTO_INCREMENT NOT NULL", sql, StringComparison.Ordinal);
        Assert.Contains("\"FormId\" INT AUTO_INCREMENT NOT NULL", sql, StringComparison.Ordinal);
    }
}
