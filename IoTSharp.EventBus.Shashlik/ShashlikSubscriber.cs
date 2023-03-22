using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shashlik.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.EventBus.Shashlik
{
    public class ShashlikSubscriber : EventBusSubscriber, ISubscriber
    {
        public ShashlikSubscriber(ILogger<EventBusSubscriber> logger, IServiceScopeFactory scopeFactor
           , IStorage storage, IEasyCachingProviderFactory factory, EventBusOption eventBusOption
            ) : base(logger, scopeFactor, storage, factory, eventBusOption)
        {

        }

      
    }

    public class EventAttributeDataHandler : IEventHandler<AttributeDataEvent>
    {
        private readonly ISubscriber _subscriber;

        public EventAttributeDataHandler(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }
        public async Task Execute(AttributeDataEvent @event, IDictionary<string, string> items)
        {
            var data = @event.Data;
            if (data != null) await _subscriber.StoreAttributeData(data);
        }
    }

    public class TelemetryDataEventHandler : IEventHandler<TelemetryDataEvent>
    {
        private readonly ISubscriber _subscriber;

        public TelemetryDataEventHandler(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }
        public async Task Execute(TelemetryDataEvent @event, IDictionary<string, string> items)
        {
            var data = @event.Data;
            if (data != null) await _subscriber.StoreTelemetryData(data);
        }
    }

    public class AlarmEventHandler : IEventHandler<AlarmEvent>
    {
        private readonly ISubscriber _subscriber;

        public AlarmEventHandler(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }
        public async Task Execute(AlarmEvent @event, IDictionary<string, string> items)
        {
            var data = @event.Data;
            if (data != null) await _subscriber.OccurredAlarm(data);
        }
    }

   
    public class CreateDeviceEventHandler : IEventHandler<CreateDeviceEvent>
    {
        private readonly ISubscriber _subscriber;

        public CreateDeviceEventHandler(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }
        public async Task Execute(CreateDeviceEvent @event, IDictionary<string, string> items)
        {
            await _subscriber.CreateDevice(@event.DeviceId);
        }
    }


    public class DeleteDeviceEventHandler : IEventHandler<DeleteDeviceEvent>
    {
        private readonly ISubscriber _subscriber;

        public DeleteDeviceEventHandler(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }
        public async Task Execute(DeleteDeviceEvent @event, IDictionary<string, string> items)
        {
            await _subscriber.DeleteDevice(@event.DeviceId);
        }
    }
    public class DeviceConnectEventHandler : IEventHandler<DeviceConnectEvent>
    {
        private readonly ISubscriber _subscriber;

        public DeviceConnectEventHandler(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }
        public async Task Execute(DeviceConnectEvent @event, IDictionary<string, string> items)
        {
            var data = @event.Data;
            if (data != null)
                await _subscriber.Connect(data.DeviceId, data.ConnectStatus);
        }
    }
    public class DeviceActivityEventHandler : IEventHandler<DeviceActivityEvent>
    {
        private readonly ISubscriber _subscriber;

        public DeviceActivityEventHandler(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }
        public async Task Execute(DeviceActivityEvent @event, IDictionary<string, string> items)
        {
            var data = @event.Data;
            if (data != null)
                await _subscriber.Active(data.DeviceId, data.Activity);
        }
    }
}