using DotNetCore.CAP;
using Dynamitey;
using IoTSharp.Contracts;
using IoTSharp.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.EventBus
{
    public class CapPublisher : IPublisher
    {
        private readonly ICapPublisher _queue;

        public CapPublisher(ICapPublisher queue)
        {
            _queue = queue;
        }
        public void PublishAttributeData(PlayloadData msg)
        {
            _queue.Publish("iotsharp.services.datastream.attributedata", msg);
        }


        public void PublishTelemetryData( PlayloadData msg)
        {
            _queue.Publish("iotsharp.services.datastream.telemetrydata", msg);
        }

        public void PublishDeviceStatus( Guid devid, DeviceStatus devicestatus)
        {
            _queue.Publish("iotsharp.services.datastream.devicestatus", new PlayloadData { DeviceId = devid, DeviceStatus = devicestatus });
        }

        public void PublishDeviceAlarm( CreateAlarmDto alarmDto)
        {
            _queue.Publish("iotsharp.services.datastream.alarm", alarmDto);
        }

      
    }
}
