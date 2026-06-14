using IoTSharp.Contracts;
using Microsoft.Extensions.DependencyInjection;
using SonnetMQ;

namespace IoTSharp.EventBus.SonnetMQ;

public static class DependencyInjection
{
    public static void UseSonnetMQ(this EventBusOption opt)
    {
        var settings = opt.AppSettings;
        var services = opt.services;
        var queuePath = ResolveQueuePath(opt.EventBusMQ);

        services.Configure<SonnetMqEventBusOptions>(options =>
        {
            options.Path = queuePath;
            options.PullBatchSize = Math.Max(1, settings.ConsumerThreadCount) * 64;
        });

        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SonnetMqEventBusOptions>>().Value;
            return SonnetMqStore.Open(new SonnetMqOptions
            {
                Path = options.Path,
                FlushOnPublish = options.FlushOnPublish,
                SyncOnPublish = options.SyncOnPublish,
                OffsetIndexStride = options.OffsetIndexStride
            });
        });
        services.AddTransient<ISubscriber, SonnetMqSubscriber>();
        services.AddTransient<IPublisher, SonnetMqPublisher>();
        services.AddHostedService<SonnetMqEventBusWorker>();
    }

    private static string ResolveQueuePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new SonnetMqEventBusOptions().Path;

        if (value.StartsWith("sonnetmq://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(value);
            string localPath = Uri.UnescapeDataString(uri.LocalPath);
            if (OperatingSystem.IsWindows() && uri.Host.Length == 1 && !string.IsNullOrWhiteSpace(localPath))
                return $"{uri.Host}:{localPath}";

            if (!string.IsNullOrWhiteSpace(localPath) && localPath != "/")
            {
                if (OperatingSystem.IsWindows() && localPath.Length >= 3 && localPath[0] == '/' && localPath[2] == ':')
                    return localPath[1..];

                return localPath;
            }
        }

        return value;
    }
}
