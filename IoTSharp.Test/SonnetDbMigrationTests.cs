#nullable enable

using IoTSharp.Data.SonnetDB;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using SonnetDB.Data;
using System;
using System.Data;
using System.Linq;
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

    /// <summary>
    /// 当前业务日期列必须与最新迁移快照一致，避免 Provider 升级后仍向旧日期列发送新类型参数。
    /// </summary>
    [Fact]
    public void CurrentTemporalColumns_MatchLatestMigrationSnapshot()
    {
        using var context = new SonnetDbDesignTimeApplicationDbContextFactory()
            .CreateDbContext(new[] { $"Data Source=.data/model-test-{Guid.NewGuid():N}" });
        var model = context.GetService<IDesignTimeModel>().Model;
        var snapshot = Assert.IsAssignableFrom<IModel>(context.GetService<IMigrationsAssembly>().ModelSnapshot?.Model);
        var temporalProperties = model.GetEntityTypes()
            .SelectMany(entity => entity.GetDeclaredProperties())
            .Where(property =>
            {
                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                return type == typeof(DateTime) || type == typeof(DateTimeOffset);
            })
            .ToArray();

        Assert.NotEmpty(temporalProperties);
        Assert.All(temporalProperties, property =>
        {
            var entity = Assert.IsAssignableFrom<IEntityType>(snapshot.FindEntityType(property.DeclaringType.Name));
            var snapshotProperty = Assert.IsAssignableFrom<IProperty>(entity.FindProperty(property.Name));
            Assert.Equal(property.GetColumnType(), snapshotProperty.GetColumnType());
            Assert.Equal(property.IsNullable, snapshotProperty.IsNullable);
        });
    }

    /// <summary>
    /// 刷新令牌时间参数使用原生 DATETIME2，防止兼容旧列时静默截断业务时钟的 UTC ticks。
    /// </summary>
    [Theory]
    [InlineData(nameof(RefreshToken.AddedDate))]
    [InlineData(nameof(RefreshToken.ExpiryDate))]
    public void RefreshTokenDateParameters_PreserveSubMillisecondUtcTicks(string propertyName)
    {
        using var context = new SonnetDbDesignTimeApplicationDbContextFactory()
            .CreateDbContext(new[] { $"Data Source=.data/parameter-test-{Guid.NewGuid():N}" });
        var property = context.Model.FindEntityType(typeof(RefreshToken))!.FindProperty(propertyName)!;
        var preciseUtc = new DateTime(2026, 11, 12, 1, 2, 3, DateTimeKind.Utc).AddTicks(1_234_567);
        using var command = new SndbCommand();
        var parameter = property.GetRelationalTypeMapping().CreateParameter(command, "date", preciseUtc);

        Assert.Equal("DATETIME2", property.GetColumnType());
        Assert.Equal(DbType.DateTime2, parameter.DbType);
        var parameterValue = Assert.IsType<DateTime>(parameter.Value);
        Assert.Equal(preciseUtc.Ticks, parameterValue.Ticks);
        Assert.Equal(DateTimeKind.Utc, parameterValue.Kind);
    }
}
