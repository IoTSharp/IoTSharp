using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using System.Dynamic;

namespace IoTSharp.EventBus
{
    public interface IPublisher
    {
        public Task<EventBusMetrics> GetMetrics();
        public Task PublishCreateDevice(Guid devid);
        public Task PublishDeleteDevice(Guid devid);

        public Task PublishAttributeData(PlayloadData msg);
        public Task PublishTelemetryData(PlayloadData msg);
   
        public Task PublishConnect(Guid devid, ConnectStatus devicestatus);
        public Task PublishActive(Guid devid, ActivityStatus  activity);
        public Task PublishDeviceAlarm(CreateAlarmDto alarmDto);
    }
}