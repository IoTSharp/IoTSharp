#nullable enable

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Data;
using IoTSharp.Data.SonnetDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using SonnetDB.Data;
using SonnetDB.EntityFrameworkCore.Extensions;
using SonnetDB.Tables;
using Testcontainers.SonnetDB;
using Xunit;
using Xunit.Abstractions;

namespace IoTSharp.Test;

/// <summary>用正式镜像检验带存量数据的真实升级；显式启用，避免把高成本验收混入普通单元测试。</summary>
public sealed class SonnetDbExistingDatabaseUpgradeTests(ITestOutputHelper output)
{
    private const string BeforeAlignment = "20261004010000_AddConfigurationOperationLease";
    private const string Alignment = "20261112061658_AlignSonnetDb5Model";

    /// <summary>分别重建带自增元数据的旧库和历史上缺少该元数据的旧库，验证升级后的数据与约束。</summary>
    /// <param name="legacyKeys">为 true 时复现修复历史迁移前缺失 AUTO_INCREMENT 的数据库。</param>
    [SonnetDbUpgradeAcceptance]
    [Trait("Category", "SonnetDbUpgradeAcceptance")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExistingRows_SurviveDateAlignmentAndRetainKeyConstraints(bool legacyKeys)
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var token = deadline.Token;
        await using var container = new SonnetDbBuilder("iotsharp/sonnetdb:5.0.0")
            .WithAdminToken("iotsharp_upgrade_acceptance_only")
            .WithDatabase("upgrade_acceptance")
            .Build();
        await container.StartAsync(token);
        var connectionString = container.GetConnectionString("upgrade_acceptance");
        var interceptor = new LegacyGeneratedKeyInterceptor { StripAutoIncrement = legacyKeys };
        var services = new ServiceCollection();
        services.AddSingleton<IDataBaseModelBuilderOptions>(new SonnetDbModelBuilderOptions());
        services.AddDbContext<ApplicationDbContext>(options => options
            .UseSonnetDB(connectionString, provider => provider.MigrationsAssembly("IoTSharp.Data.SonnetDB"))
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .AddInterceptors(interceptor));
        await using var provider = services.BuildServiceProvider(validateScopes: true);
        var assembly = typeof(SndbConnection).Assembly;
        output.WriteLine("SonnetDB.Data: {0}; sha256={1}",
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            Convert.ToHexStringLower(SHA256.HashData(await System.IO.File.ReadAllBytesAsync(assembly.Location, token))));

        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.GetService<IMigrator>().MigrateAsync(BeforeAlignment, token);
        }
        interceptor.StripAutoIncrement = false;
        Assert.Equal(legacyKeys ? 8 : 0, interceptor.RemovedClauses);
        var originalId = Guid.NewGuid();
        var oldTime = new DateTime(2026, 10, 14, 12, 34, 56, 123, DateTimeKind.Utc);
        await using (var connection = new SndbConnection(connectionString))
        {
            await connection.OpenAsync(token);
            await ExecuteAsync(connection,
                "INSERT INTO \"BaseDictionaryGroups\" (\"DictionaryGroupId\", \"DictionaryGroupName\") VALUES (41, 'upgrade-original')", token);
            await using var seed = connection.CreateCommand();
            seed.CommandText = "INSERT INTO \"RefreshTokens\" (\"Id\", \"IsUsed\", \"IsRevorked\", \"AddedDate\", \"ExpiryDate\") VALUES (@id, false, false, @added, @expires)";
            seed.Parameters.AddWithValue("id", originalId.ToString("D"));
            seed.Parameters.Add(new SndbParameter("added", oldTime) { DbType = DbType.DateTime });
            seed.Parameters.Add(new SndbParameter("expires", oldTime.AddHours(1)) { DbType = DbType.DateTime });
            await seed.ExecuteNonQueryAsync(token);
        }

        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.GetService<IMigrator>().MigrateAsync(Alignment, token);
            Assert.Contains(Alignment, await context.Database.GetAppliedMigrationsAsync(token));
            var existing = await context.Set<RefreshToken>().AsNoTracking().SingleAsync(item => item.Id == originalId, token);
            Assert.Equal(oldTime.Ticks, existing.AddedDate.Ticks);
            Assert.Equal(oldTime.AddHours(1).Ticks, existing.ExpiryDate.Ticks);
            var precise = oldTime.AddTicks(4567);
            var current = new RefreshToken { Id = Guid.NewGuid(), AddedDate = precise, ExpiryDate = precise.AddHours(1) };
            context.Add(current);
            await context.SaveChangesAsync(token);
            context.ChangeTracker.Clear();
            var reloaded = await context.Set<RefreshToken>().AsNoTracking().SingleAsync(item => item.Id == current.Id, token);
            Assert.Equal(precise.Ticks, reloaded.AddedDate.Ticks);
            Assert.Equal(DateTimeKind.Utc, reloaded.AddedDate.Kind);
            Assert.Equal(2, await context.Set<RefreshToken>().CountAsync(token));
            output.WriteLine("PASS: 存量日期、行数、旧主键与新 DATETIME2 UTC ticks 回读。");
        }

        await using (var connection = new SndbConnection(connectionString))
        {
            await connection.OpenAsync(token);
            var primaryKey = await Assert.ThrowsAsync<SndbServerException>(() => ExecuteAsync(connection,
                "INSERT INTO \"BaseDictionaryGroups\" (\"DictionaryGroupId\") VALUES (41)", token));
            Assert.True(primaryKey.Error == TableConstraintException.UniqueViolation
                || primaryKey.Message.Contains("主键", StringComparison.Ordinal)
                || primaryKey.Message.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase), primaryKey.Message);
            await ExecuteAsync(connection, "INSERT INTO \"AspNetRoles\" (\"Id\", \"NormalizedName\") VALUES ('upgrade-role-a', 'UPGRADE-ROLE')", token);
            var unique = await Assert.ThrowsAsync<SndbServerException>(() => ExecuteAsync(connection,
                "INSERT INTO \"AspNetRoles\" (\"Id\", \"NormalizedName\") VALUES ('upgrade-role-b', 'UPGRADE-ROLE')", token));
            Assert.Equal(TableConstraintException.UniqueViolation, unique.Error);
            await using var missingParent = connection.CreateCommand();
            missingParent.CommandText = "INSERT INTO \"RefreshTokens\" (\"Id\", \"UserId\", \"IsUsed\", \"IsRevorked\", \"AddedDate\", \"ExpiryDate\") VALUES (@id, 'missing-user', false, false, @added, @expires)";
            missingParent.Parameters.AddWithValue("id", Guid.NewGuid().ToString("D"));
            missingParent.Parameters.Add(new SndbParameter("added", oldTime) { DbType = DbType.DateTime2 });
            missingParent.Parameters.Add(new SndbParameter("expires", oldTime.AddHours(1)) { DbType = DbType.DateTime2 });
            var foreignKey = await Assert.ThrowsAsync<SndbServerException>(() => missingParent.ExecuteNonQueryAsync(token));
            output.WriteLine("约束诊断：PK={0}; UNIQUE={1}; FK={2}", primaryKey.Message, unique.Error, foreignKey.Error);
            Assert.Equal(TableConstraintException.ForeignKeyViolation, foreignKey.Error);
            output.WriteLine("PASS: 重复主键、唯一索引、外键均拒绝违规写入。");

            await using var insert = connection.CreateCommand();
            insert.CommandText = "INSERT INTO \"BaseDictionaryGroups\" (\"DictionaryGroupName\") VALUES ('upgrade-new') RETURNING \"DictionaryGroupId\"";
            var generated = Convert.ToInt64(await insert.ExecuteScalarAsync(token));
            Assert.True(generated > 41, $"Generated key must advance past the preserved key 41, got {generated}");
            output.WriteLine("PASS: 升级后数据库生成的主键 {0} 大于存量主键 41。", generated);
        }
    }

    /// <summary>仅在受控临时库中执行单条命令，异常必须由各项断言检查。</summary>
    private static async Task ExecuteAsync(SndbConnection connection, string sql, CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(token);
    }

    /// <summary>精确复现旧迁移遗漏的八个自增声明，保留其它历史 schema、索引和迁移记录。</summary>
    private sealed class LegacyGeneratedKeyInterceptor : DbCommandInterceptor
    {
        internal bool StripAutoIncrement { get; set; }
        internal int RemovedClauses { get; private set; }

        /// <inheritdoc />
        public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            Rewrite(command);
            return result;
        }

        /// <inheritdoc />
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Rewrite(command);
            return ValueTask.FromResult(result);
        }

        /// <summary>只在历史建表阶段移除固定字面量，升级阶段不改写任何 SQL。</summary>
        private void Rewrite(DbCommand command)
        {
            if (!StripAutoIncrement) return;
            var before = command.CommandText;
            command.CommandText = before.Replace(" AUTO_INCREMENT", "", StringComparison.Ordinal);
            RemovedClauses += (before.Length - command.CommandText.Length) / " AUTO_INCREMENT".Length;
        }
    }
}

/// <summary>以明确环境开关启用实际容器、历史数据和已知失败边界的专项验收。</summary>
public sealed class SonnetDbUpgradeAcceptanceAttribute : TheoryAttribute
{
    /// <summary>默认不启动专项数据库；显式运行时保留失败断言，不把失败降为跳过。</summary>
    public SonnetDbUpgradeAcceptanceAttribute()
    {
        Timeout = 150000;
        if (Environment.GetEnvironmentVariable("IOTSHARP_SONNETDB_UPGRADE_ACCEPTANCE") != "1")
            Skip = "Set IOTSHARP_SONNETDB_UPGRADE_ACCEPTANCE=1 to run the isolated existing-database upgrade acceptance";
    }
}
