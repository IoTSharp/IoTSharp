using IoTSharp.Contracts;
using Microsoft.Extensions.DependencyInjection;
using SonnetDB.Data;
using SonnetDB.Data.Mq;

namespace IoTSharp.EventBus.SonnetMQ;

public static class DependencyInjection
{
    public static void UseSonnetMQ(this EventBusOption opt)
    {
        var settings = opt.AppSettings;
        var services = opt.services;
        var connectionString = ResolveConnectionString(opt.EventBusMQ);
        SndbResourceInitializer.EnsureDatabase(connectionString, "事件总线数据库");

        services.Configure<SonnetMqEventBusOptions>(options =>
        {
            options.ConnectionString = connectionString;
            options.PullBatchSize = Math.Max(1, settings.ConsumerThreadCount) * 64;
        });

        services.AddSingleton<SonnetMqActiveEventCoalescer>();
        services.AddSingleton(provider => CreateClient(
            provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SonnetMqEventBusOptions>>().Value.ConnectionString));
        services.AddTransient<ISubscriber, SonnetMqSubscriber>();
        services.AddTransient<IPublisher, SonnetMqPublisher>();
        services.AddHostedService<SonnetMqEventBusWorker>();
    }

    private static string ResolveConnectionString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("SonnetMQ EventBus requires ConnectionStrings:EventBusMQ.");

        return value;
    }

    private static SndbMqClient CreateClient(string connectionString)
    {
        var client = new SndbMqClient(connectionString);
        try
        {
            _ = client.GetStatsAsync("iotsharp.startup.probe").ConfigureAwait(false).GetAwaiter().GetResult();
            return client;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }
}
