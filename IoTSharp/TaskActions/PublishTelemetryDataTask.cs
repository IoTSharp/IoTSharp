using IoTSharp.EventBus;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.TaskActions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.TaskActions
{
    [DisplayName("遥测数据发布器")]
    [Description("遥测数据发布器")]
    public class PublishTelemetryDataTask : TaskAction
    {
        private readonly IPublisher _queue;

        public PublishTelemetryDataTask(IPublisher queue)
        {
            _queue = queue;
        }

        public override Task<TaskActionOutput> ExecuteAsync(TaskActionInput param)
        {
            var result = new TaskActionOutput() { DynamicOutput = new { code =  ApiCode.Success,msg="OK" } };
            try
            {
                var msg = new PlayloadData() {  MsgBody = JToken.Parse(param.Input)?.JsonToDictionary(), DataCatalog = DataCatalog.TelemetryData, DataSide = DataSide.ClientSide, DeviceId = param.DeviceId };
                if (msg.DeviceId != Guid.Empty && msg.MsgBody?.Count > 0)
                {
                    _queue.PublishTelemetryData(msg);
                }
            }
            catch (Exception ex)
            {
                result.DynamicOutput = new { code = ApiCode.Exception, msg = ex.Message };
            }
            return Task.FromResult(result);
        }
    }
}
