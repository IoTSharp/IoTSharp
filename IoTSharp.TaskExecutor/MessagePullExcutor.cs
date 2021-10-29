using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Data;
using IoTSharp.TaskAction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoTSharp.TaskAction
{
    [DisplayName("用于消息推送的执行器")]
    public class MessagePullExecutor : ITaskAction
    {

        public MessagePullExecutor()
        {


        }

        public TaskActionOutput Execute(TaskActionInput input)
        {

            JObject o = JsonConvert.DeserializeObject(input.Input) as JObject;
            var d = o.Property("temperature");
            d.Value = new JValue(d.Value.Value<double>() + 100);
            return new TaskActionOutput() { DynamicOutput = o.ToObject(typeof(ExpandoObject)) };
            var p= JsonConvert.DeserializeObject<MessagePullParam>(input.Input);
           p.temperature += 120;
           return new TaskActionOutput() { DynamicOutput =p };
        }


        class MessagePullParam
        {

            public double temperature { get; set; }


        }
    }
}
