
using IoTSharp;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Oracle;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using ShardingCore.Core.ShardingConfigurations;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
      

        public static void ConfigureOracle(this IServiceCollection services, string connectionString, int poolSize, IHealthChecksBuilder checksBuilder, HealthChecksUIBuilder healthChecksUI)
        {
            services.AddEntityFrameworkOracle();
            services.AddSingleton<IDataBaseModelBuilderOptions>( c=> new OracleModelBuilderOptions());
            services.AddDbContextPool<ApplicationDbContext>(builder =>
            {
                builder.UseOracle(connectionString, s =>   s.MigrationsAssembly("IoTSharp.Data.Oracle").UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                builder.UseInternalServiceProvider(services.BuildServiceProvider());
            }
     , poolSize);
            checksBuilder.AddOracle(connectionString,name: "IoTSharp.Data.Oracle");
            healthChecksUI.AddInMemoryStorage();

        }

        public static void UseOracleToSharding(this ShardingConfigOptions options)
        {
            options.UseShardingQuery((conStr, builder) =>
            {
                builder.UseOracle(conStr);
            });
            options.UseShardingTransaction((conn, builder) =>
            {
                builder.UseOracle(conn);
            });
        }
    }
}
