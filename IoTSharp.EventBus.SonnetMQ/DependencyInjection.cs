using IoTSharp.Contracts;
using Microsoft.Extensions.DependencyInjection;
using SonnetDB.Data.Mq;

namespace IoTSharp.EventBus.SonnetMQ;

public static class DependencyInjection
{
    public static void UseSonnetMQ(this EventBusOption opt)
    {
        var settings = opt.AppSettings;
        var services = opt.services;
        var connectionString = ResolveConnectionString(opt.EventBusMQ);

        services.Configure<SonnetMqEventBusOptions>(options =>
        {
            options.ConnectionString = connectionString;
            options.PullBatchSize = Math.Max(1, settings.ConsumerThreadCount) * 64;
        });

        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SonnetMqEventBusOptions>>().Value;
            return new SndbMqClient(options.ConnectionString);
        });
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
}
