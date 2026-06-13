using System;
using System.Linq;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using SonnetDB.EntityFrameworkCore.Extensions;

namespace IoTSharp.Data.SonnetDB;

public sealed class SonnetDbDesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.FirstOrDefault(static arg => arg.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            ?? "Data Source=.data/iotsharp-sonnetdb-design";

        var services = new ServiceCollection();
        services.AddSingleton<IDataBaseModelBuilderOptions>(static _ => new SonnetDbModelBuilderOptions());
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSonnetDB(connectionString, static options =>
                options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .MigrationsAssembly("IoTSharp.Data.SonnetDB")));

        return services.BuildServiceProvider(validateScopes: true)
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
    }
}
