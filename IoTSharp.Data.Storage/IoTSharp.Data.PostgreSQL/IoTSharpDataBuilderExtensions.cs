
using EFCore.Sharding;
using IoTSharp;
using IoTSharp.Data;
using IoTSharp.Data.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

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
                builder.UseNpgsql(connectionString, s =>  s.MigrationsAssembly("IoTSharp.Data.PostgreSQL"));
                builder.UseInternalServiceProvider(services.BuildServiceProvider());
            }
     , poolSize);
            checksBuilder.AddNpgSql(connectionString, name: "IoTSharp.Data.PostgreSQL");
            healthChecksUI.AddPostgreSqlStorage(connectionString);

        }

        public static void UseNpgsqlToSharding(this IShardingBuilder builder, string connectionString, ShardingByDateMode expandBy)
        {
            builder.AddDataSource(connectionString, ReadWriteType.Read | ReadWriteType.Write, DatabaseType.PostgreSql);
            builder.SetDateSharding<TelemetryData>(nameof(TelemetryData.DateTime),(ExpandByDateMode)(int)expandBy, DateTime.Now);
        }
    }
}
