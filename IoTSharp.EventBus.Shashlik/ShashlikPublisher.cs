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
        private IMessageStorage _storage;

        public ShashlikPublisher(IEventPublisher queue, IMessageStorage storage)
        {
            _queue = queue;
            _storage = storage;
        }
        public async Task PublishAttributeData(PlayloadData msg)
        {
          await  _queue.PublishAsync((AttributeDataEvent)msg   ,null);
        }

        public async Task PublishTelemetryData( PlayloadData msg)
        {
            await _queue.PublishAsync((TelemetryDataEvent)msg, null);
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

        public async Task PublishConnect(Guid devid, ConnectStatus devicestatus)
        {
            await _queue.PublishAsync((DeviceConnectEvent)new DeviceConnectStatus(devid, devicestatus), null);
        }

        public async Task PublishActive(Guid devid, ActivityStatus activity)
        {
            await _queue.PublishAsync((DeviceActivityEvent)new DeviceActivityStatus(  devid, activity),null);
        }

        public async Task<EventBusMetrics> GetMetrics()
        {
            var stat = await _storage.GetReceivedMessageStatusCountAsync(CancellationToken.None);
            return  new EventBusMetrics();
        }
    }
}
