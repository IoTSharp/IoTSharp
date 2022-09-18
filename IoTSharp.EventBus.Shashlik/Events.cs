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
    public class AttributeDataEvent : IEvent
    {
        public PlayloadData Data { get; set; }
        public static implicit operator AttributeDataEvent(PlayloadData v)
        {
            return new AttributeDataEvent() { Data = v };
        }

        public static explicit operator PlayloadData(AttributeDataEvent v)
        {
            return v.Data;
        }
    }
    public class TelemetryDataEvent : IEvent
    {
        public PlayloadData Data { get; set; }

        public static implicit operator TelemetryDataEvent(PlayloadData v)
        {
            return new TelemetryDataEvent() { Data = v };
        }

        public static explicit operator PlayloadData(TelemetryDataEvent v)
        {
            return v.Data;
        }
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
    public class AlarmEvent : IEvent
    {
        public CreateAlarmDto Data { get; set; }
        public static implicit operator AlarmEvent(CreateAlarmDto v)
        {
            return new AlarmEvent() { Data = v };
        }

        public static explicit operator CreateAlarmDto(AlarmEvent v)
        {
            return v.Data;
        }
    }
}
