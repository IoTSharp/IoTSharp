
using IoTSharp.Contracts;
using IoTSharp.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.EventBus.NServiceBus
{
    public class NSBusPublisher : IPublisher
    {
        public Task<EventBusMetrics> GetMetrics()
        {
            throw new NotImplementedException();
        }

        public Task PublishActive(Guid devid, ActivityStatus activity)
        {
            throw new NotImplementedException();
        }

        public Task PublishAttributeData(PlayloadData msg)
        {
            throw new NotImplementedException();
        }

        public Task PublishConnect(Guid devid, ConnectStatus devicestatus)
        {
            throw new NotImplementedException();
        }

        public Task PublishCreateDevice(Guid devid)
        {
            throw new NotImplementedException();
        }

        public Task PublishDeleteDevice(Guid devid)
        {
            throw new NotImplementedException();
        }

        public Task PublishDeviceAlarm(CreateAlarmDto alarmDto)
        {
            throw new NotImplementedException();
        }

        public Task PublishTelemetryData(PlayloadData msg)
        {
            throw new NotImplementedException();
        }
    }
}
