using DotNetCore.CAP;
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

        public void DeviceStatusEvent(PlayloadData status);
        public Task StoreTelemetryData(PlayloadData msg);
    
    }
}