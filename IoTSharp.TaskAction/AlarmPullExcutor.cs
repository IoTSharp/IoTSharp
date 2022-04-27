using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;

namespace IoTSharp.TaskAction
{
    [DisplayName("用于告警推送的执行器")]
    public class AlarmPullExcutor : ITaskAction
    {
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
                var config = JsonConvert.DeserializeObject<AlarmPullExcutor.ModelExecutorConfig>(input.ExecutorConfig);
                var restclient = new RestClient(config.BaseUrl);
                restclient.AddDefaultHeader(KnownHeaders.Accept, "*/*");
                var request =
                    new RestRequest(config.Url + (input.DeviceId == Guid.Empty ? "" : "/" + input.DeviceId));
                request.AddHeader("X-Access-Token",
                    config.Token);
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(JsonConvert.DeserializeObject<Alarm>(input.Input, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }) ?? new object());
                var response = await restclient.ExecutePostAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<AlarmPullExcutor.MessagePullResult>(response.Content);
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
