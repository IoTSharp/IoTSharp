using EasyCaching.Core;
using IoTSharp.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IoTSharp.EventBus.SonnetMQ;

public sealed class SonnetMqSubscriber : EventBusSubscriber, ISubscriber
{
    public SonnetMqSubscriber(
        ILogger<EventBusSubscriber> logger,
        IServiceScopeFactory scopeFactor,
        IStorage storage,
        IEasyCachingProviderFactory factory,
        EventBusOption eventBusOption)
        : base(logger, scopeFactor, storage, factory, eventBusOption)
    {
    }
}
