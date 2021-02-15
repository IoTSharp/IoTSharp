
using EFCore.Sharding;
using IoTSharp.Data;
using IoTSharp.Data.Oracle;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

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
                builder.UseOracle(connectionString, s =>   s.MigrationsAssembly("IoTSharp.Data.Oracle"));
                builder.UseInternalServiceProvider(services.BuildServiceProvider());
            }
     , poolSize);
            checksBuilder.AddOracle(connectionString);
            healthChecksUI.AddInMemoryStorage();

        }

        public static void UseOracleToSharding(this IShardingBuilder builder, string connectionString, ExpandByDateMode expandBy)
        {
            builder.AddDataSource(connectionString, ReadWriteType.Read | ReadWriteType.Write, DatabaseType.Oracle);
            builder.SetDateSharding<TelemetryData>(nameof(TelemetryData.DateTime), expandBy, DateTime.Now);
        }
    }
}
