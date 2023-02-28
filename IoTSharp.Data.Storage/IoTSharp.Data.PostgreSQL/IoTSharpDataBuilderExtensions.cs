
using IoTSharp;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using ShardingCore.Core.ShardingConfigurations;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
      

        public static void ConfigureNpgsql(this IServiceCollection services, string connectionString, int poolSize, IHealthChecksBuilder checksBuilder, HealthChecksUIBuilder healthChecksUI)
        {
            services.AddSingleton<IDataBaseModelBuilderOptions>( c=> new NpgsqlModelBuilderOptions());
            services.AddNpgsql<ApplicationDbContext>(connectionString, s =>
                            s.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                               .MigrationsAssembly("IoTSharp.Data.PostgreSQL"));
            checksBuilder.AddNpgSql(connectionString, name: "IoTSharp.Data.PostgreSQL");
            healthChecksUI.AddPostgreSqlStorage(connectionString, opt => opt.ConfigureWarnings(w => w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning)));

        }

        public static void UseNpgsqlToSharding(this ShardingConfigOptions options)
        {
            options.UseShardingQuery((conStr, builder) =>
            {
                builder.UseNpgsql(conStr, opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });
            options.UseShardingTransaction((conn, builder) =>
            {
                builder.UseNpgsql(conn, opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });
        }
    }
}
