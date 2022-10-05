using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using System.Dynamic;

namespace IoTSharp.EventBus
{
    public interface IPublisher
    {

        public Task PublishCreateDevice(Guid devid);
        public Task PublishDeleteDevice(Guid devid);

        public Task PublishAttributeData(PlayloadData msg);
        public Task PublishTelemetryData(PlayloadData msg);
        [Obsolete]
        public Task PublishDeviceStatus(Guid devid, DeviceStatus devicestatus);
        public Task PublishConnect(Guid devid, ConnectStatus devicestatus);
        public Task PublishActive(Guid devid, ActivityStatus  activity);
        public Task PublishDeviceAlarm(CreateAlarmDto alarmDto);
    }
}