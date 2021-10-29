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
    [DisplayName("Air720S解析器")]
    public class Air720Sx : ITaskAction
    {

        public Air720Sx()
        {

        }

        public TaskActionOutput Execute(TaskInput param)
        {
            var msg = JsonConvert.DeserializeObject<(string Topic, string Payload, string ClientId)>(param.Intput);
            var playload = Convert.FromBase64String(msg.Payload);
            return new TaskActionOutput();
        }
    }
}
