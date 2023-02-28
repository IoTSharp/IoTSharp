
using IoTSharp;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using ShardingCore.Core.ShardingConfigurations;
using Microsoft.Data.Sqlite;
using System.IO;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
      

        public static void ConfigureSqlite(this IServiceCollection services, string connectionString, int poolSize, IHealthChecksBuilder checksBuilder, HealthChecksUIBuilder healthChecksUI)
        {
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder(connectionString);
            var fi = new FileInfo(builder.DataSource);
            if (!fi.Directory.Exists) fi.Directory.Create();

            services.AddSingleton<IDataBaseModelBuilderOptions>(c => new SqliteModelBuilderOptions());
            services.AddSqlite<ApplicationDbContext>(connectionString, opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                                   .MigrationsAssembly("IoTSharp.Data.Sqlite"));
            checksBuilder.AddSqlite(connectionString, name: "IoTSharp.Data.Sqlite");
            healthChecksUI.AddSqliteStorage($"Data Source={fi.DirectoryName}{Path.DirectorySeparatorChar}health_checks.db",opt => opt.ConfigureWarnings(w => w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning)));

        }

        public static void UseSQLiteToSharding(this ShardingConfigOptions options)
        {
            options.UseShardingQuery((conStr, builder) =>
            {
                builder.UseSqlite(conStr, opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });
            options.UseShardingTransaction((conn, builder) =>
            {
                builder.UseSqlite(conn,opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });
        }

        public static void SetCaseInsensitiveSearchesForSQLite(this ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("NOCASE");

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(string)))
            {
                property.SetCollation("NOCASE");
            }
        }
    }
}
