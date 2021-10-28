using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Data;
using IoTSharp.TaskExecutor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoTSharp.TaskExecutor
{
    [DisplayName("Air720S解析器")]
    public class Air720Sx : ITaskExecutor
    {

        public Air720Sx()
        {

        }

        public TaskExecutorResult Execute(TaskExecutorParam param)
        {
            var msg = JsonConvert.DeserializeObject<(string Topic, string Payload, string ClientId)>(param.Param);
            var playload = Convert.FromBase64String(msg.Payload);
            return new TaskExecutorResult();
        }
    }
}
