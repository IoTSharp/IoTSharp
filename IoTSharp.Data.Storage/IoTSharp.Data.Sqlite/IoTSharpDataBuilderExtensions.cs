
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

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
      

        public static void ConfigureSqlite(this IServiceCollection services, string connectionString, int poolSize, IHealthChecksBuilder checksBuilder, HealthChecksUIBuilder healthChecksUI)
        {
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder(connectionString);
            var fi = new FileInfo(builder.DataSource);
            if (!fi.Directory.Exists) fi.Directory.Create();

            services.AddEntityFrameworkSqlite();
            services.AddSingleton<IDataBaseModelBuilderOptions>( c=> new SqliteModelBuilderOptions());
            services.AddDbContextPool<ApplicationDbContext>(builder =>
            {
              
                builder.UseSqlite(connectionString, s =>  s.MigrationsAssembly("IoTSharp.Data.Sqlite").UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                builder.UseInternalServiceProvider(services.BuildServiceProvider());
            }
     , poolSize);
            checksBuilder.AddSqlite(connectionString, name: "IoTSharp.Data.Sqlite");
            healthChecksUI.AddSqliteStorage($"Data Source={fi.DirectoryName}{Path.DirectorySeparatorChar}health_checks.db");

        }

        public static void UseSQLiteToSharding(this ShardingConfigOptions options)
        {
            options.UseShardingQuery((conStr, builder) =>
            {
                builder.UseSqlite(conStr);
            });
            options.UseShardingTransaction((conn, builder) =>
            {
                builder.UseSqlite(conn);
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
