
using IoTSharp;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Oracle;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using ShardingCore.Core.ShardingConfigurations;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
      

        public static void ConfigureOracle(this IServiceCollection services, string connectionString, int poolSize, IHealthChecksBuilder checksBuilder, HealthChecksUIBuilder healthChecksUI)
        {
            services.AddSingleton<IDataBaseModelBuilderOptions>( c=> new OracleModelBuilderOptions());
            services.AddOracle<ApplicationDbContext>(connectionString, s =>
                         s.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                            .MigrationsAssembly("IoTSharp.Data.Oracle"));
            checksBuilder.AddOracle(connectionString,name: "IoTSharp.Data.Oracle");
            healthChecksUI.AddInMemoryStorage(opt => opt.ConfigureWarnings(w => w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning)));

        }

        public static void UseOracleToSharding(this ShardingConfigOptions options)
        {
            options.UseShardingQuery((conStr, builder) =>
            {
                builder.UseOracle(conStr, opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });
            options.UseShardingTransaction((conn, builder) =>
            {
                builder.UseOracle(conn, opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });
        }
    }
}
