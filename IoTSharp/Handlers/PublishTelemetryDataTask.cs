using DotNetCore.CAP;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.TaskExecutor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Handlers
{
    [DisplayName("属性发布器")]
    [Description("发布属性至队列")]
    public class PublishTelemetryDataTask : ITaskExecutor
    {
        private readonly ICapPublisher _queue;

        public PublishTelemetryDataTask(ICapPublisher queue)
        {
            _queue = queue;
        }

        public TaskExecutorResult Execute(TaskExecutorParam param)
        {
            var msg = JsonConvert.DeserializeObject<RawMsg>(param.Param);
            if (msg.DeviceId != Guid.Empty && msg.MsgBody?.Count > 0)
            {
                _queue.PublishTelemetryData(msg);
            }
            return new TaskExecutorResult();
        }
    }
}
