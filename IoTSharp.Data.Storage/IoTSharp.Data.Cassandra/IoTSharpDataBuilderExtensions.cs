
using IoTSharp.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

using IoTSharp.Data.Cassandra;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
        
        public static void ConfigureCassandra(this IServiceCollection services, string connectionString, int poolSize, IHealthChecksBuilder checksBuilder, HealthChecksUIBuilder healthChecksUI)
        {
            services.AddEntityFrameworkCassandra();
            services.AddSingleton<IDataBaseModelBuilderOptions>(c => new CassandraModelBuilderOptions());
            services.AddDbContextPool<ApplicationDbContext>(builder =>
            {
                builder.UseCassandra(connectionString, "", s => s.MigrationsAssembly("IoTSharp.Data.Cassandra").UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                builder.UseInternalServiceProvider(services.BuildServiceProvider());
            }
     , poolSize);
              checksBuilder.AddCassandra(connectionString, name: "IoTSharp.Data.Cassandra");
            //   healthChecksUI.AddSqliteStorage("Data Source=health_checks.db");

        }


    }
}
