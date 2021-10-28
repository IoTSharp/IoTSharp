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
    [DisplayName("用于消息推送的执行器")]
    public class MessagePullExecutor : ITaskExecutor
    {

        public MessagePullExecutor()
        {


        }

        public TaskExecutorResult Execute(TaskExecutorParam param)
        {

            JObject o = JsonConvert.DeserializeObject(param.Param) as JObject;
            var d = o.Property("temperature");
            d.Value = new JValue(d.Value.Value<double>() + 100);
            return new TaskExecutorResult() { Result = o.ToObject(typeof(ExpandoObject)) };
            var p= JsonConvert.DeserializeObject<MessagePullParam>(param.Param);
           p.temperature += 120;
           return new TaskExecutorResult() { Result =p };
        }


        class MessagePullParam
        {

            public double temperature { get; set; }


        }
    }
}
