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
            await _subscriber.StoreAttributeData((PlayloadData)@event);
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
            await _subscriber.StoreTelemetryData((PlayloadData)@event);
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
            await _subscriber.OccurredAlarm((CreateAlarmDto)@event);
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
}