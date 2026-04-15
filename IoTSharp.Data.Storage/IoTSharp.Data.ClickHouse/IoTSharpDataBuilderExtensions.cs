using ClickHouse.Driver.ADO;
using ClickHouse.EntityFrameworkCore.Extensions;
using IoTSharp.Data;
using IoTSharp.Data.ClickHouse;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Net.Http;

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
                builder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
                builder.UseInternalServiceProvider(services.BuildServiceProvider());
            }, poolSize);
            services.AddHttpClient("ClickHouseClient");
            checksBuilder.AddClickHouse( sp => {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new ClickHouseConnection(connectionString, httpClientFactory, "ClickHouseClient");
            });

             
        }
    }
}
