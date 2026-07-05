using System;
using System.Linq;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace IoTSharp.Data.MySQL;

public sealed class MySqlDesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.FirstOrDefault(static arg => arg.Contains("server=", StringComparison.OrdinalIgnoreCase))
            ?? "server=localhost;user=root;password=pass;database=IoTSharpDesign";
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

        var services = new ServiceCollection();
        services.AddSingleton<ServerVersion>(serverVersion);
        services.AddSingleton<IDataBaseModelBuilderOptions>(static _ => new MySqlModelBuilderOptions());
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, serverVersion, static options =>
                options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .MigrationsAssembly("IoTSharp.Data.MySQL")));

        return services.BuildServiceProvider(validateScopes: true)
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
    }
}
