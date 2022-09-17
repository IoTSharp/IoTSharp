using DotNetCore.CAP;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using System.Dynamic;

namespace IoTSharp.EventBus
{
    public interface IPublisher
    {

        public void PublishCreateDevice(Guid devid);
        public void PublishDeleteDevice(Guid devid);

        public void PublishAttributeData(PlayloadData msg);
        public void PublishTelemetryData(PlayloadData msg);
        public void PublishDeviceStatus(Guid devid, DeviceStatus devicestatus);
        public void PublishDeviceAlarm(CreateAlarmDto alarmDto);
    }
}