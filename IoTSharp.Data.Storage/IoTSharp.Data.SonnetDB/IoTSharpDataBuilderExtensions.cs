using IoTSharp.Data;
using IoTSharp.Data.SonnetDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SonnetDB.EntityFrameworkCore.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTSharpDataBuilderExtensions
    {
        public static void ConfigureSonnetDB(
            this IServiceCollection services,
            string connectionString,
            int poolSize,
            IHealthChecksBuilder checksBuilder,
            HealthChecksUIBuilder healthChecksUI)
        {
            services.AddSingleton<IDataBaseModelBuilderOptions>(static _ => new SonnetDbModelBuilderOptions());
            services.AddDbContextPool<ApplicationDbContext>(options =>
            {
                options.UseSonnetDB(connectionString, opt =>
                    opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                       .MigrationsAssembly("IoTSharp.Data.SonnetDB"));
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }, poolSize);

            checksBuilder.AddCheck<SonnetDbHealthCheck>("IoTSharp.Data.SonnetDB");
            healthChecksUI.AddInMemoryStorage(opt => opt.ConfigureWarnings(w => w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning)));
        }
    }
}
