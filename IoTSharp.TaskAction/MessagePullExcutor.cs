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


            try
            {
                var config = JsonConvert.DeserializeObject<ModelExecutorConfig>(input.ExecutorConfig);
                JObject o = JsonConvert.DeserializeObject(input.Input) as JObject;
                var d = o.Property("temperature");
                d.Value = new JValue(d.Value.Value<double>() + 100);
                return new TaskActionOutput() { DynamicOutput = o.ToObject(typeof(ExpandoObject)) };
            }
            catch (Exception ex)
            {
                return new TaskActionOutput() {ExecutionInfo = ex.Message, ExecutionStatus = false};
            }

        }


        class MessagePullParam
        {
            public double temperature { get; set; }


        }

        class ModelExecutorConfig
        {
            public string PullUrl { get; set; }
            public string Mothod { get; set; }
        
        }
    }
}
