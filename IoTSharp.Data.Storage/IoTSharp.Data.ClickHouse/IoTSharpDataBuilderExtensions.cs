using System;
using ClickHouse.EntityFrameworkCore.Extensions;
using HealthChecks.Clickhouse.DependencyInjection;
using IoTSharp.Data;
using IoTSharp.Data.ClickHouse;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
        public static void ConfigureClickHouse(this IServiceCollection services, string connectionString, int poolSize, IHealthChecksBuilder checksBuilder, HealthChecksUIBuilder healthChecksUI)
        {
            services.AddEntityFrameworkClickHouse();
            services.AddSingleton<IDataBaseModelBuilderOptions>(c => new ClickHouseModelBuilderOptions());
            services.AddDbContextPool<ApplicationDbContext>(builder =>
            {
                builder.UseClickHouse(connectionString, s => s.MigrationsAssembly("IoTSharp.Data.ClickHouse").UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                builder.UseInternalServiceProvider(services.BuildServiceProvider());
            }, poolSize);
            checksBuilder.AddClickHouseHealthCheck(connectionString, name: "IoTSharp.Data.ClickHouse");
        }
    }
}
