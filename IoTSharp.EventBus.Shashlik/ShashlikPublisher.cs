using IoTSharp.Contracts;
using IoTSharp.Data;
using RabbitMQ.Client;
using Shashlik.EventBus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.EventBus.Shashlik
{
   
    public class ShashlikPublisher : IPublisher
    {
        private IEventPublisher _queue;

        public ShashlikPublisher(IEventPublisher queue)
        {
            _queue = queue;
        }
        public async Task PublishAttributeData(PlayloadData msg)
        {
          await  _queue.PublishAsync((AttributeDataEvent)msg   ,null);
        }

        public async Task PublishTelemetryData( PlayloadData msg)
        {
            await _queue.PublishAsync((TelemetryDataEvent)msg, null);
            msg = (PlayloadData)(TelemetryDataEvent)msg;
        }

        public async Task PublishDeviceStatus( Guid devid, DeviceStatus devicestatus)
        {
            await _queue.PublishAsync(new DeviceStatusEvent() { DeviceId = devid, DeviceStatus = devicestatus },null);
        }

        public async Task PublishDeviceAlarm( CreateAlarmDto alarmDto)
        {
            await _queue.PublishAsync((AlarmEvent)alarmDto, null);
        }

        public async Task PublishCreateDevice(Guid  devid)
        {
            await _queue.PublishAsync(new CreateDeviceEvent() { DeviceId = devid }, null);
        }

        public async Task PublishDeleteDevice(Guid devid)
        {
            await _queue.PublishAsync(new DeleteDeviceEvent() { DeviceId = devid }, null);
        }
    }
}
