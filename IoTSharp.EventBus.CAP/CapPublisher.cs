using DotNetCore.CAP;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using IoTSharp.Contracts;
using IoTSharp.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.EventBus
{
    public class CapPublisher : IPublisher
    {
        private readonly ICapPublisher _queue;
        private readonly DotNetCore.CAP.Persistence.IDataStorage _storage;

        public CapPublisher(ICapPublisher queue, DotNetCore.CAP.Persistence.IDataStorage storage)
        {
            _queue = queue;
            _storage = storage;
        }
        public Task<EventBusMetrics> GetMetrics()
        {
            var _api = _storage.GetMonitoringApi();
            var ps = _api.HourlySucceededJobs(MessageType.Publish);
            var pf = _api.HourlyFailedJobs(MessageType.Publish);
            var ss = _api.HourlySucceededJobs(MessageType.Subscribe);
            var sf = _api.HourlyFailedJobs(MessageType.Subscribe);
            var dayHour = ps.Keys.Select(x => x.ToString("MM-dd HH:00")).ToList();
            var s = _api.GetStatistics();
            var result = new EventBusMetrics(
dayHour,
ps.Values.ToList(),
pf.Values.ToList(),
ss.Values.ToList(),
sf.Values.ToList()
            )
            {
                Servers = s.Servers,
                Subscribers = s.Subscribers,
                PublishedSucceeded = s.PublishedSucceeded,
                ReceivedSucceeded = s.ReceivedSucceeded,
                PublishedFailed = s.PublishedFailed,
                ReceivedFailed = s.ReceivedFailed
            };
            return Task.FromResult(result);
        }
        public async Task PublishAttributeData(PlayloadData msg)
        {
            await _queue.PublishAsync("iotsharp.services.datastream.attributedata", msg);
        }

        public  async Task PublishTelemetryData( PlayloadData msg)
        {
            await _queue.PublishAsync("iotsharp.services.datastream.telemetrydata", msg);
        }
 

        public  async Task PublishDeviceAlarm( CreateAlarmDto alarmDto)
        {
            await _queue.PublishAsync("iotsharp.services.datastream.alarm", alarmDto);
        }

        public  async Task PublishCreateDevice(Guid  devid)
        {
            await _queue.PublishAsync("iotsharp.services.platform.createdevice", devid);
        }

        public  async Task PublishDeleteDevice(Guid devid)
        {
            await _queue.PublishAsync("iotsharp.services.platform.deleteDevice", devid);
        }

        public async Task PublishConnect(Guid devid, ConnectStatus devicestatus)
        {
            await _queue.PublishAsync("iotsharp.services.platform.connect", new DeviceConnectStatus(devid, devicestatus));
        }

        public async Task PublishActive(Guid devid, ActivityStatus activity)
        {
            await _queue.PublishAsync("iotsharp.services.platform.active", new DeviceActivityStatus(devid, activity));
        }
    }
}
