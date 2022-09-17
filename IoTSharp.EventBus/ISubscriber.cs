using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using System.Dynamic;

namespace IoTSharp.EventBus
{
    public interface ISubscriber
    {
        public Task StoreAttributeData(PlayloadData msg);
        public Task OccurredAlarm(CreateAlarmDto alarmDto);
        public Task DeviceStatusEvent(PlayloadData status);
        public Task StoreTelemetryData(PlayloadData msg);
        public Task DeleteDevice(Guid deviceId);
        public Task CreateDevice(Guid deviceId);
    }
}