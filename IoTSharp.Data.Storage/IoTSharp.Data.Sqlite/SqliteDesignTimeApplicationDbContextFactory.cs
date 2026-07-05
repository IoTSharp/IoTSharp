using System;
using System.Linq;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace IoTSharp.Data.Sqlite;

public sealed class SqliteDesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.FirstOrDefault(static arg => arg.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            ?? "Data Source=.data/iotsharp-sqlite-design.db";

        var services = new ServiceCollection();
        services.AddSingleton<IDataBaseModelBuilderOptions>(static _ => new SqliteModelBuilderOptions());
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString, static options =>
                options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .MigrationsAssembly("IoTSharp.Data.Sqlite")));

        return services.BuildServiceProvider(validateScopes: true)
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
    }
}
