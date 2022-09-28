using DotNetCore.CAP;
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
        public async Task PublishAttributeData(PlayloadData msg)
        {
            await _queue.PublishAsync("iotsharp.services.datastream.attributedata", msg);
        }

        public  async Task PublishTelemetryData( PlayloadData msg)
        {
            await _queue.PublishAsync("iotsharp.services.datastream.telemetrydata", msg);
        }

        public  async Task PublishDeviceStatus( Guid devid, DeviceStatus devicestatus)
        {
            await _queue.PublishAsync("iotsharp.services.datastream.devicestatus", new PlayloadData { DeviceId = devid, DeviceStatus = devicestatus });
        }

        public  async Task PublishDeviceAlarm( CreateAlarmDto alarmDto)
        {
            await _queue.PublishAsync("iotsharp.services.datastream.alarm", alarmDto);
        }

        public  async Task PublishCreateDevice(Guid  devid)
        {
            await _queue.PublishAsync("iotsharp.services.platform.createdevice", devid);
        }

        public  async Task PublishDeleteDevice(Guid devid)
        {
            await _queue.PublishAsync("iotsharp.services.platform.deleteDevice", devid);
        }
 
    }
}
