using IoTSharp.EventBus;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.TaskActions;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IoTSharp.TaskActions
{
    [DisplayName("告警数据发布器")]
    [Description("告警数据发布器")]
    public class PublishAlarmDataTask : TaskAction
    {
        private readonly IPublisher _queue;

        public PublishAlarmDataTask(IPublisher queue)
        {
            _queue = queue;
        }

        public override Task<TaskActionOutput> ExecuteAsync(TaskActionInput param)
        {
            var result = new TaskActionOutput() { DynamicOutput = param.DynamicInput, ExecutionStatus = true, ExecutionInfo = ""};
            try
            {
                var dto = JsonConvert.DeserializeObject<CreateAlarmDto>(param.Input);
                _queue.PublishDeviceAlarm(dto);
            }
            catch (Exception ex)
            {
                result.DynamicOutput = new { DynamicOutput = param.DynamicInput, ExecutionStatus = false, ExecutionInfo = ex.Message };
            }
            return Task.FromResult(result);
        }
    }
}
