
using EFCore.Sharding;
using IoTSharp.Data;
using IoTSharp.Data.MySQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
        public static void  ConfigureMySql(this IServiceCollection services,   string connectionString,int  poolSize , IHealthChecksBuilder checksBuilder, HealthChecksUIBuilder healthChecksUI)
        {
            services.AddEntityFrameworkMySql();
            services.AddSingleton<IDataBaseModelBuilderOptions>(c => new MySqlModelBuilderOptions());
            services.AddDbContextPool<ApplicationDbContext>(builder =>
            {
                builder.UseInternalServiceProvider(services.BuildServiceProvider());
                builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), s => s.MigrationsAssembly("IoTSharp.Data.MySQL"));
            }
          , poolSize );
           
            checksBuilder.AddMySql(connectionString, "IoTSharp.Data.MySQL");
            healthChecksUI.AddMySqlStorage(connectionString);
      
        }
        public static void UseMySqlToSharding(this IShardingBuilder builder, string connectionString, ExpandByDateMode expandBy)
        {
            builder.AddDataSource(connectionString, ReadWriteType.Read | ReadWriteType.Write, DatabaseType.MySql);
            builder.SetDateSharding<TelemetryData>(nameof(TelemetryData.DateTime), expandBy, DateTime.Now);
        }
    }
}
