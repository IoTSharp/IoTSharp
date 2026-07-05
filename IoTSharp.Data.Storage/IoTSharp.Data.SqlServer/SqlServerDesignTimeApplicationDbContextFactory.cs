using System;
using System.Linq;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace IoTSharp.Data.SqlServer;

public sealed class SqlServerDesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.FirstOrDefault(static arg => arg.Contains("Server=", StringComparison.OrdinalIgnoreCase)
            || arg.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
            ?? "Server=(localdb)\\mssqllocaldb;Database=IoTSharpDesign;Trusted_Connection=True;TrustServerCertificate=true";

        var services = new ServiceCollection();
        services.AddSingleton<IDataBaseModelBuilderOptions>(static _ => new MsSqlModelBuilderOptions());
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, static options =>
                options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .MigrationsAssembly("IoTSharp.Data.SqlServer")));

        return services.BuildServiceProvider(validateScopes: true)
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
    }
}
