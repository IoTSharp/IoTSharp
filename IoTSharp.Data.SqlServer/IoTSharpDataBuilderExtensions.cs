
using EFCore.Sharding;
using IoTSharp.Data;
using IoTSharp.Data.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

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
                builder.UseSqlServer(connectionString, s =>  s.MigrationsAssembly("IoTSharp.Data.SqlServer"));
                builder.UseInternalServiceProvider(services.BuildServiceProvider());
            }
     , poolSize);
            checksBuilder.AddSqlServer(connectionString);
            healthChecksUI.AddSqlServerStorage(connectionString);

        }

        public static void UseSqlServerToSharding(this IShardingBuilder builder, string connectionString, ExpandByDateMode expandBy)
        {
            builder.AddDataSource(connectionString, ReadWriteType.Read | ReadWriteType.Write, DatabaseType.SqlServer);
            builder.SetDateSharding<TelemetryData>(nameof(TelemetryData.DateTime), expandBy, DateTime.Now);
        }
    }
}
