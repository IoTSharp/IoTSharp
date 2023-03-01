using IoTSharp.Contracts;
using IoTSharp.Data;
using Shashlik.EventBus;

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
            await _queue.PublishAsync(new AttributeDataEvent { Data = msg }, null);
        }

        public async Task PublishTelemetryData(PlayloadData msg)
        {
            await _queue.PublishAsync(new TelemetryDataEvent { Data = msg }, null);
        }


        public async Task PublishDeviceAlarm(CreateAlarmDto alarmDto)
        {
            await _queue.PublishAsync(new AlarmEvent { Data = alarmDto }, null);
        }

        public async Task PublishCreateDevice(Guid devid)
        {
            await _queue.PublishAsync(new CreateDeviceEvent() { DeviceId = devid }, null);
        }

        public async Task PublishDeleteDevice(Guid devid)
        {
            await _queue.PublishAsync(new DeleteDeviceEvent() { DeviceId = devid }, null);
        }

        public async Task PublishConnect(Guid devid, ConnectStatus devicestatus)
        {
            await _queue.PublishAsync(new DeviceConnectEvent { Data = new DeviceConnectStatus(devid, devicestatus) },
                null);
        }

        public async Task PublishActive(Guid devid, ActivityStatus activity)
        {
            await _queue.PublishAsync(new DeviceActivityEvent { Data = new DeviceActivityStatus(devid, activity) },
                null);
        }

        public async Task<EventBusMetrics> GetMetrics()
        {
            var stat = await _storage.GetReceivedMessageStatusCountAsync(CancellationToken.None);
            return new EventBusMetrics();
        }
    }
}