using System;
using System.Linq;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace IoTSharp.Data.PostgreSQL;

public sealed class NpgsqlDesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.FirstOrDefault(static arg => arg.Contains("Host=", StringComparison.OrdinalIgnoreCase)
            || arg.Contains("Server=", StringComparison.OrdinalIgnoreCase))
            ?? "Host=localhost;Database=IoTSharpDesign;Username=postgres;Password=postgres;Include Error Detail=true";

        var services = new ServiceCollection();
        services.AddSingleton<IDataBaseModelBuilderOptions>(static _ => new NpgsqlModelBuilderOptions());
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, static options =>
                options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .MigrationsAssembly("IoTSharp.Data.PostgreSQL")));

        return services.BuildServiceProvider(validateScopes: true)
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
    }
}
