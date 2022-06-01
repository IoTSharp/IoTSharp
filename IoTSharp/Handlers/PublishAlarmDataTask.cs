using DotNetCore.CAP;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.TaskAction;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IoTSharp.Handlers
{
    [DisplayName("告警数据发布器")]
    [Description("告警数据发布器")]
    public class PublishAlarmDataTask : ITaskAction
    {
        private readonly ICapPublisher _queue;

        public PublishAlarmDataTask(ICapPublisher queue)
        {
            _queue = queue;
        }

        public override Task<TaskActionOutput> ExecuteAsync(TaskActionInput param)
        {
            var result = new TaskActionOutput() { DynamicOutput = param.DynamicInput, ExecutionStatus = true, ExecutionInfo = ""};
            try
            {
                var dto = JsonConvert.DeserializeObject<Dtos.CreateAlarmDto>(param.Input);
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
