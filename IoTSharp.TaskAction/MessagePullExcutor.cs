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




                // new TaskActionOutput() { DynamicOutput = input.DynamicInput, ExecutionStatus = true };



                //JObject o = JsonConvert.DeserializeObject(input.Input) as JObject;
                //var d = o.Property("temperature");
                //d.Value = new JValue(d.Value.Value<double>() + 100);


                var param = JsonConvert.DeserializeObject<MessagePullParam>(input.Input);
                string contentType = "application/json";
                var restclient = new RestClient(config.BaseUrl);
                var request = new RestRequest(config.Url, Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("cache-control", "no-cache");
                request.AddJsonBody(input.Input);
        
                var response = restclient.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)

                    return new TaskActionOutput() { DynamicOutput = param };
              

             return new TaskActionOutput() { DynamicOutput = param};
            }
            catch (Exception ex)
            {
                return new TaskActionOutput() {ExecutionInfo = ex.Message, ExecutionStatus = false};
            }

        }


        class MessagePullParam
        {

       
            public double param1 { get; set; }
            public double param2 { get; set; }
            public double param3 { get; set; }
            public int param4 { get; set; }

            public long param5 { get; set; }


            public ParamObject param6 { get; set; }

            public string param9 { get; set; }
        }

        public class ParamObject
        {

            public bool param7 { get; set; }
            public DateTime param8 { get; set; }
        }

        class ModelExecutorConfig
        {
            public string Url { get; set; }
            public string BaseUrl { get; set; }
            public string Method { get; set; }
        
        }
    }
}
