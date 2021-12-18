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

        public override Task<TaskActionOutput> ExecuteAsync(TaskActionInput _input)
        {
            var msg = _input.DynamicInput;
            var playload = System.Text.Encoding.Default.GetString( Convert.FromBase64String(msg.Payload));
            return  Task.FromResult( new TaskActionOutput() {   Output=playload, DeviceId=_input.DeviceId});
        }
    }
}
