using DotNetCore.CAP;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.TaskAction;
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
    public class PublishAttributeDataTask : ITaskAction
    {
        private readonly ICapPublisher _queue;

        public PublishAttributeDataTask(ICapPublisher queue)
        {
            _queue = queue;
        }

        public TaskActionOutput Execute(TaskInput param)
        {
            var msg = JsonConvert.DeserializeObject<RawMsg>(param.Intput);
            if (msg.DeviceId != Guid.Empty && msg.MsgBody?.Count > 0)
            {
                _queue.PublishAttributeData(msg);
            }
            return new TaskActionOutput();
        }
    }
}
