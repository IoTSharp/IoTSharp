using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoTSharp.FlowRuleEngine.TaskExcutor
{
    [DisplayName("用于消息推送的执行器")]
    public class MessagePullExcutor: ITaskExcutor
    {

        private readonly ApplicationDbContext _context;
        public MessagePullExcutor()
        {
            

        }

        public TaskExcutorResult Excute(TaskExcutorParam param)
        {

            JObject o =   JsonConvert.DeserializeObject(param.Param) as JObject;
          var d=  o.First as JProperty;
          d.Value = new JValue(888);

   
         
            return new TaskExcutorResult() {Result = o.ToObject(typeof(ExpandoObject)) };
        }
    }
}
