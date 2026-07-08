using System;
using System.Collections.Generic;
using System.Data.Common;
using IoTSharp.Data;
using IoTSharp.Data.SonnetDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.TableExists.Abstractions;
using SonnetDB.Data;
using SonnetDB.EntityFrameworkCore.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
        public static void ConfigureSonnetDB(
            this IServiceCollection services,
            string connectionString,
            int poolSize,
            IHealthChecksBuilder checksBuilder,
            HealthChecksUIBuilder healthChecksUI)
        {
            SndbResourceInitializer.EnsureDatabase(connectionString, "主数据库");
            services.AddSingleton<IDataBaseModelBuilderOptions>(static _ => new SonnetDbModelBuilderOptions());
            services.AddDbContextPool<ApplicationDbContext>(options =>
            {
                options.UseSonnetDB(connectionString, opt =>
                    opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                       .MigrationsAssembly("IoTSharp.Data.SonnetDB"));
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }, poolSize);

            checksBuilder.AddCheck<SonnetDbHealthCheck>(
                "IoTSharp.Data.SonnetDB",
                tags: new[] { "ready" });
            healthChecksUI.AddInMemoryStorage(opt => opt.ConfigureWarnings(w => w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning)));
        }

        public static void UseSonnetDBToSharding(this ShardingConfigOptions options)
        {
            options.UseShardingQuery((conStr, builder) =>
                builder.UseSonnetDB(conStr, opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

            options.UseShardingTransaction((conn, builder) =>
                builder.UseSonnetDB(conn, opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
        }
    }
}

namespace IoTSharp.Data.SonnetDB
{
    public sealed class SonnetDbTableEnsureManager : AbstractTableEnsureManager
    {
        public SonnetDbTableEnsureManager(IRouteTailFactory routeTailFactory)
            : base(routeTailFactory)
        {
        }

        public override ISet<string> DoGetExistTables(DbConnection connection, string dataSourceName)
        {
            if (connection is not SndbConnection)
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using var command = connection.CreateCommand();
            command.CommandText = "SHOW TABLES";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }

            return tables;
        }
    }
}
