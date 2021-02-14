
using EFCore.Sharding;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
        public static void ConfigureNpgsql(this IServiceCollection services, string connectionString, int poolSize, IHealthChecksBuilder checksBuilder, HealthChecksUIBuilder healthChecksUI)
        {
            services.AddDbContextPool<ApplicationDbContext>(builder =>
            {
                builder.UseNpgsql(connectionString, s => s.MigrationsAssembly("IoTSharp.Data.PostgreSQL"));
            }
     , poolSize);
            checksBuilder.AddNpgSql(connectionString);
            healthChecksUI.AddPostgreSqlStorage(connectionString);

        }

        public static void UseNpgsqlToSharding(this IShardingBuilder builder, string connectionString, ExpandByDateMode expandBy)
        {
            builder.AddDataSource(connectionString, ReadWriteType.Read | ReadWriteType.Write, DatabaseType.PostgreSql);
            builder.SetDateSharding<TelemetryData>(nameof(TelemetryData.DateTime), expandBy, DateTime.Now);
        }
    }
}
