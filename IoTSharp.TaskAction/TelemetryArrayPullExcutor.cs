using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace IoTSharp.TaskAction
{
    [DisplayName("用于遥测数组推送的执行器")]
    public class TelemetryArrayPullExcutor : ITaskAction
    {
        public TelemetryArrayPullExcutor()
        {
        }

        public override Task<TaskActionOutput> ExecuteAsync(TaskActionInput input)
        {
            try
            {
                return SendData(input);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new TaskActionOutput() { ExecutionInfo = ex.Message, ExecutionStatus = false });
            }
        }

        private async Task<TaskActionOutput> SendData(TaskActionInput input)
        {
            try
            {
                var config = JsonConvert.DeserializeObject<ModelExecutorConfig>(input.ExecutorConfig);
                var restclient = new RestClient(config.BaseUrl);
                restclient.AddDefaultHeader(KnownHeaders.Accept, "*/*");
                var request =
                    new RestRequest(config.Url + (input.DeviceId == Guid.Empty ? "" : "/" + input.DeviceId));
                request.AddHeader("X-Access-Token",
                    config.Token);
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(JsonConvert.DeserializeObject<List<TelemetryData>>(input.Input, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }) ??new object() );
                 var response = await restclient.ExecutePostAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<MessagePullResult>(response.Content);
                    if (result is { success: true })
                    {
                        return new TaskActionOutput()
                        {
                            ExecutionInfo = response.Content,
                            ExecutionStatus = result.success,
                            DynamicOutput = input.DynamicInput
                        };
                    }
                    else
                    {
                        return new TaskActionOutput() { ExecutionInfo = response.Content, ExecutionStatus = false };
                    }
                }
                else
                {
                    return new TaskActionOutput()
                    {
                        ExecutionInfo =
                            $"StatusCode:{response.StatusCode} StatusDescription:{response.StatusDescription}  {response.ErrorMessage}",
                        ExecutionStatus = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new TaskActionOutput() { ExecutionInfo = ex.Message, ExecutionStatus = false };
                ;
            }
        }





        public class TelemetryData
        {
            [JsonProperty("keyName")]
            public string keyName { get; set; }
            [JsonProperty("dateTime")]
            public DateTime dateTime { get; set; }
            [JsonProperty("dataType")]
            public Data.DataType dataType { get; set; }
            [JsonProperty("value")]
            public object value { get; set; }

        }

        private class MessagePullResult
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string code { get; set; }

            public long timestamp { get; set; }

            public dynamic result { get; set; }
        }

        private class ModelExecutorConfig
        {
            public string Url { get; set; }
            public string BaseUrl { get; set; }
            public string Method { get; set; }
            public string Password { get; set; }
            public string UserName { get; set; }
            public string Token { get; set; }
        }
    }
}