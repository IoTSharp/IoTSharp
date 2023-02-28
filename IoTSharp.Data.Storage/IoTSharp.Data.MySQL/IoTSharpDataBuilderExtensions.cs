
using IoTSharp;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.MySQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using ShardingCore.Core.ShardingConfigurations;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
        public static void  ConfigureMySql(this IServiceCollection services,   string connectionString,int  poolSize , IHealthChecksBuilder checksBuilder, HealthChecksUIBuilder healthChecksUI)
        {
            services.AddSingleton<IDataBaseModelBuilderOptions>(c => new MySqlModelBuilderOptions());
            ServerVersion serverVersion=null;
            try
            {
                serverVersion = ServerVersion.AutoDetect(connectionString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Can't detect MySql server's version ,  {ex.Message} ", ex);
            }
            services.AddSingleton(serverVersion);
            services.AddMySql<ApplicationDbContext>(connectionString, serverVersion, s =>
                       s.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                          .MigrationsAssembly("IoTSharp.Data.MySQL"));
   
           
            checksBuilder.AddMySql(connectionString, "IoTSharp.Data.MySQL");
            healthChecksUI.AddMySqlStorage(connectionString, opt => opt.ConfigureWarnings(w => w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning)));
      
        }
        public static void UseMySqlToSharding(this ShardingConfigOptions options)
        {
            options.UseShardingQuery((conStr, builder) =>
            {
                builder.UseMySql(conStr, new MySqlServerVersion(new Version()),opt=> opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });
            options.UseShardingTransaction((conn, builder) =>
            {
                builder.UseMySql(conn, new MySqlServerVersion(new Version()), opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });
        }
    }
}
