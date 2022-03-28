using DotNetCore.CAP;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.TaskAction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Handlers
{
    [DisplayName("遥测数据发布器")]
    [Description("遥测数据发布器")]
    public class PublishTelemetryDataTask : ITaskAction
    {
        private readonly ICapPublisher _queue;

        public PublishTelemetryDataTask(ICapPublisher queue)
        {
            _queue = queue;
        }

        public override Task<TaskActionOutput> ExecuteAsync(TaskActionInput param)
        {
            var result = new TaskActionOutput() { DynamicOutput = new { code =  ApiCode.Success,msg="OK" } };
            try
            {
                var msg = new RawMsg() { MsgType = MsgType.HTTP, MsgBody = JToken.Parse(param.Input)?.JsonToDictionary(), DataCatalog = DataCatalog.TelemetryData, DataSide = DataSide.ClientSide, DeviceId = param.DeviceId };
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
