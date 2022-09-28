
using IoTSharp;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using ShardingCore.Core.ShardingConfigurations;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
      

        public static void ConfigureSqlServer(this IServiceCollection services, string connectionString, int poolSize, IHealthChecksBuilder checksBuilder, HealthChecksUIBuilder healthChecksUI)
        {
            services.AddEntityFrameworkSqlServer();
            services.AddSingleton<IDataBaseModelBuilderOptions>( c=> new MsSqlModelBuilderOptions());
            services.AddDbContextPool<ApplicationDbContext>(builder =>
            {
                builder.UseSqlServer(connectionString, s =>  s.MigrationsAssembly("IoTSharp.Data.SqlServer").UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                builder.UseInternalServiceProvider(services.BuildServiceProvider());
            }
     , poolSize);
            checksBuilder.AddSqlServer(connectionString,name: "IoTSharp.Data.SqlServer");
            healthChecksUI.AddSqlServerStorage(connectionString);

        }

        public static void UseSqlServerToSharding(this ShardingConfigOptions options)
        {
            options.UseShardingQuery((conStr, builder) =>
            {
                builder.UseSqlServer(conStr);
            });
            options.UseShardingTransaction((conn, builder) =>
            {
                builder.UseSqlServer(conn);
            });
        }
    }
}
