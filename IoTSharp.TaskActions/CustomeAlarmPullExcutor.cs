using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IoTSharp.EventBus;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.TaskActions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace IoTSharp.TaskActions
{

    [DisplayName("用于自定义的告警推送的执行器")]
    public class CustomeAlarmPullExcutor : TaskAction
    {

        private readonly IPublisher _queue;
        private readonly ApplicationDbContext _context;
        public CustomeAlarmPullExcutor(ApplicationDbContext context, IPublisher queue)
        {
            this._context = context; _queue = queue;
        }


        public override Task<TaskActionOutput> ExecuteAsync(TaskActionInput input)
        {
            try
            {
                return Task.FromResult(SendData(input));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new TaskActionOutput() { ExecutionInfo = ex.Message, ExecutionStatus = false });
            }
        }

        private  TaskActionOutput SendData(TaskActionInput input)
        {
            try
            {
                JObject o = JsonConvert.DeserializeObject(input.Input) as JObject;
                var config = JsonConvert.DeserializeObject<CustomeAlarmPullExcutor.ModelExecutorConfig>(input.ExecutorConfig);
                var dd = o.Properties().Select(c => new MessagePullExecutor.ParamObject { keyName = c.Name.ToLower(), value = JPropertyToObject(c) }).ToList();
                if (input.DeviceId == Guid.Empty)
                {
                    input.DeviceId = config.DeviceId;
                }
                var alarmdto = new CreateAlarmDto();
                alarmdto.OriginatorType = OriginatorType.Device;
                alarmdto.OriginatorName = input.DeviceId.ToString();
                alarmdto.CreateDateTime = DateTime.Now;
                alarmdto.Serverity = (ServerityLevel)config.serverity;
                alarmdto.AlarmType = config.alarmType;
                alarmdto.AlarmDetail = config.AlarmDetail ?? JsonConvert.SerializeObject(dd) ;
                return new TaskActionOutput() { ExecutionInfo = JsonConvert.SerializeObject(alarmdto), ExecutionStatus = true, DynamicOutput = alarmdto };
             
            }
            catch (Exception ex)
            {
                return new TaskActionOutput() { ExecutionInfo = ex.Message, ExecutionStatus = false };
            }
        }

        public static object JPropertyToObject(JProperty property)
        {
            object obj = null;
            switch (property.Value.Type)
            {
                case JTokenType.Integer:
                    obj = property.Value.ToObject<int>();
                    break;

                case JTokenType.Float:
                    obj = property.Value.ToObject<float>();
                    break;

                case JTokenType.String:
                    obj = property.Value.ToObject<string>();
                    break;

                case JTokenType.Boolean:
                    obj = property.Value.ToObject<bool>();
                    break;

                case JTokenType.Date:
                    obj = property.Value.ToObject<DateTime>();
                    break;

                case JTokenType.Bytes:
                    obj = property.Value.ToObject<byte[]>();
                    break;

                case JTokenType.Guid:
                    obj = property.Value.ToObject<Guid>();
                    break;

                case JTokenType.Uri:
                    obj = property.Value.ToObject<Uri>();
                    break;

                case JTokenType.TimeSpan:
                    obj = property.Value.ToObject<TimeSpan>();
                    break;

                default:
                    obj = property.Value;
                    break;
            }
            return obj;
        }

        private class Alarm
        {

            public string originatorType { get; set; }


            public string originatorName { get; set; }

            public string alarmType { get; set; }

            public string alarmDetail { get; set; }

            public string serverity { get; set; }

            public string warnDataId { get; set; }
            public DateTime createDateTime { get; set; }

        }



        private class ModelExecutorConfig
        {
            public string Url { get; set; }
            public string BaseUrl { get; set; }
            public string Method { get; set; }
            public string Password { get; set; }
            public string UserName { get; set; }
            public string Token { get; set; }
            public string AlarmDetail { get; set; }
            public string originatorType { get; set; }
            public int serverity { get; set; }
            public string originatorName { get; set; }
            public string alarmType { get; set; }


            public Guid DeviceId { get; set; }
        }




        private class MessagePullResult
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string code { get; set; }

            public long timestamp { get; set; }

            public dynamic result { get; set; }
        }
    }
}
