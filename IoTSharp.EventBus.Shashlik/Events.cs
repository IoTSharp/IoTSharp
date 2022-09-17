using IoTSharp.Contracts;
using IoTSharp.Data;
using Shashlik.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.EventBus.Shashlik
{
    public class EventAttributeData : PlayloadData, IEvent
    {
    }
    public class TelemetryDataEvent : PlayloadData, IEvent
    {
    }
    public class DeviceStatusEvent : IEvent
    {
        public Guid DeviceId { get; set; }
        public DeviceStatus DeviceStatus { get; set; }
    }
    public class CreateDeviceEvent : IEvent
    {
        public Guid DeviceId { get; set; }
    }
    public class DeleteDeviceEvent : IEvent
    {
        public Guid DeviceId { get; set; }
    }
    public class AlarmEvent : CreateAlarmDto, IEvent
    {
    }
}
