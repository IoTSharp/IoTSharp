using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IoTSharp.Data;
using IoTSharp.TaskAction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace IoTSharp.TaskAction
{
    [DisplayName("用于消息推送的执行器")]
    public class MessagePullExecutor : ITaskAction
    {

        public MessagePullExecutor()
        {


        }

        public TaskActionOutput Execute(TaskActionInput input)
        {


            try
            {
                var config = JsonConvert.DeserializeObject<ModelExecutorConfig>(input.ExecutorConfig);
                JObject o = JsonConvert.DeserializeObject(input.Input) as JObject;
                var d = o.Property("temperature");
                d.Value = new JValue(d.Value.Value<double>() + 100);
                var restclient = new RestClient(config.BaseUrl);
                var request = new RestRequest(config.Url, Method.POST);
                request.AddObject(d);
                var response = restclient.Execute(request);
              //  if (response.StatusCode == HttpStatusCode.OK)
              
                    return new TaskActionOutput() { DynamicOutput = o.ToObject(typeof(ExpandoObject)) };
              

              //  return new TaskActionOutput() { DynamicOutput = o.ToObject(typeof(ExpandoObject)) };
            }
            catch (Exception ex)
            {
                return new TaskActionOutput() {ExecutionInfo = ex.Message, ExecutionStatus = false};
            }

        }


        class MessagePullParam
        {
            public double temperature { get; set; }


        }

        class ModelExecutorConfig
        {
            public string Url { get; set; }
            public string BaseUrl { get; set; }
            public string Mothod { get; set; }
        
        }
    }
}
