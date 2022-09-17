using DotNetCore.CAP;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using IoTSharp.Extensions;
using IoTSharp.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.EventBus.CAP
{

    public class CapSubscriber : EventBusSubscriber, ISubscriber, ICapSubscribe
    {
        public CapSubscriber(ILogger<EventBusSubscriber> logger, IServiceScopeFactory scopeFactor
           , IStorage storage, IEasyCachingProviderFactory factory, EventBusOption eventBusOption
            ) : base(logger, scopeFactor, storage, factory, eventBusOption)
        {

        }

        [CapSubscribe("iotsharp.services.datastream.attributedata")]
        public async Task attributedata(PlayloadData msg)
        {
            await StoreAttributeData(msg);
        }


        [CapSubscribe("iotsharp.services.datastream.alarm")]
        public async Task alarm(CreateAlarmDto alarmDto)
        {
            await OccurredAlarm(alarmDto);
        }


        [CapSubscribe("iotsharp.services.datastream.devicestatus")]
        public void devicestatus(PlayloadData status)
        {
            DeviceStatusEvent(status);
        }

        [CapSubscribe("iotsharp.services.datastream.telemetrydata")]
        public async Task telemetrydata(PlayloadData msg)
        {
            await StoreTelemetryData(msg);
        }

    }
}