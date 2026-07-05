using System;
using System.Linq;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace IoTSharp.Data.Oracle;

public sealed class OracleDesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.FirstOrDefault(static arg => arg.Contains("DATA SOURCE=", StringComparison.OrdinalIgnoreCase))
            ?? "DATA SOURCE=127.0.0.1:1521/xe;PASSWORD=oracle;PERSIST SECURITY INFO=True;USER ID=SYSTEM;";

        var services = new ServiceCollection();
        services.AddSingleton<IDataBaseModelBuilderOptions>(static _ => new OracleModelBuilderOptions());
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseOracle(connectionString, static options =>
                options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .MigrationsAssembly("IoTSharp.Data.Oracle")));

        return services.BuildServiceProvider(validateScopes: true)
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
    }
}
