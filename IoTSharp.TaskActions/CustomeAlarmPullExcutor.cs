using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.EventBus;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;

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

        private TaskActionOutput SendData(TaskActionInput input)
        {
            try
            {
                var values = JsonNodeParser.ParseObject(input.Input);
                var config = JsonObjectSerializer.Deserialize<CustomeAlarmPullExcutor.ModelExecutorConfig>(input.ExecutorConfig);
                var dd = values.Select(c => new MessagePullExecutor.ParamObject { keyName = c.Key.ToLower(), value = c.Value.ToClrObject() }).ToList();
                if (input.DeviceId == Guid.Empty)
                {
                    input.DeviceId = config.DeviceId;
                }
                var alarmdto = new CreateAlarmDto();
                alarmdto.OriginatorType = OriginatorType.Device;
                alarmdto.OriginatorName = input.DeviceId.ToString();
                alarmdto.CreateDateTime = DateTime.UtcNow;
                alarmdto.Serverity = (ServerityLevel)config.serverity;
                alarmdto.AlarmType = config.alarmType;
                alarmdto.AlarmDetail = config.AlarmDetail ?? JsonObjectSerializer.Serialize(dd);
                return new TaskActionOutput() { ExecutionInfo = JsonObjectSerializer.Serialize(alarmdto), ExecutionStatus = true, DynamicOutput = alarmdto };
            }
            catch (Exception ex)
            {
                return new TaskActionOutput() { ExecutionInfo = ex.Message, ExecutionStatus = false };
            }
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
