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
    public class AttributeDataEvent : ShashlikEvent<PlayloadData>
    {
    }
    public class TelemetryDataEvent : ShashlikEvent<PlayloadData>
    {

    }
 
    public class CreateDeviceEvent : IEvent
    {
        public Guid DeviceId { get; set; }
    }
    public class DeleteDeviceEvent : IEvent
    {
        public Guid DeviceId { get; set; }
    }
    public class AlarmEvent : ShashlikEvent<CreateAlarmDto>
    {
    }
    public class DeviceActivityEvent : ShashlikEvent<DeviceActivityStatus>
    {
    }
    public class DeviceConnectEvent : ShashlikEvent<DeviceConnectStatus>
    {

    }
}
