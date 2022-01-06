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
    [DisplayName("属性发布器")]
    [Description("发布属性至队列")]
    public class PublishAttributeDataTask : ITaskAction
    {
        private readonly ICapPublisher _queue;

        public PublishAttributeDataTask(ICapPublisher queue)
        {
            _queue = queue;
        }

        public override  Task<TaskActionOutput> ExecuteAsync(TaskActionInput param)
        {
            var result = new TaskActionOutput() { DynamicOutput = new { code = ApiCode.Success, msg = "OK" } };
            try
            {
                var msg = new RawMsg() { MsgType = MsgType.CoAP, MsgBody = JToken.Parse(param.Input)?.JsonToDictionary(), DataCatalog = DataCatalog.AttributeData, DataSide = DataSide.ClientSide, DeviceId = param.DeviceId };
                if (msg.DeviceId != Guid.Empty && msg.MsgBody?.Count > 0)
                {
                    _queue.PublishAttributeData(msg);
                }
            }
            catch (Exception ex)
            {
                result.DynamicOutput = new { code = ApiCode.Exception, msg = ex.Message };
            }
            return  Task.FromResult( result);
        }
    }
}
