
using IoTSharp;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using ShardingCore.Core.ShardingConfigurations;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
      

        public static void ConfigureNpgsql(this IServiceCollection services, string connectionString, int poolSize, IHealthChecksBuilder checksBuilder, HealthChecksUIBuilder healthChecksUI)
        {
            services.AddEntityFrameworkNpgsql();
            services.AddSingleton<IDataBaseModelBuilderOptions>( c=> new NpgsqlModelBuilderOptions());
            services.AddDbContextPool<ApplicationDbContext>(builder =>
            {
                builder.UseNpgsql(connectionString, s =>  s.MigrationsAssembly("IoTSharp.Data.PostgreSQL").UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                builder.UseInternalServiceProvider(services.BuildServiceProvider());
            }
     , poolSize);
            checksBuilder.AddNpgSql(connectionString, name: "IoTSharp.Data.PostgreSQL");
            healthChecksUI.AddPostgreSqlStorage(connectionString);

        }

        public static void UseNpgsqlToSharding(this ShardingConfigOptions options)
        {
            options.UseShardingQuery((conStr, builder) =>
            {
                builder.UseNpgsql(conStr);
            });
            options.UseShardingTransaction((conn, builder) =>
            {
                builder.UseNpgsql(conn);
            });
        }
    }
}
