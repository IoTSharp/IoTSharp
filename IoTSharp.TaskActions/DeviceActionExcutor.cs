using Newtonsoft.Json;
using RestSharp;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace IoTSharp.TaskActions
{
    [DisplayName("用于设备行为推送的执行器")]
    public class DeviceActionExcutor : TaskAction
    {
        public override async Task<TaskActionOutput> ExecuteAsync(TaskActionInput input)
        {
            var config = JsonConvert.DeserializeObject<ModelExecutorConfig>(input.ExecutorConfig);
            string contentType = "application/json";
            var restclient = new RestClient(config.BaseUrl);
            restclient.AddDefaultHeader(KnownHeaders.Accept, "*/*");
            var request = new RestRequest(config.Url + (input.DeviceId == Guid.Empty ? "" : "/" + input.DeviceId));
            request.AddHeader("X-Access-Token",
                config.Token);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Content-Type", contentType);
            request.AddHeader("cache-control", "no-cache");
            request.AddJsonBody(new { sosType = "1", sosContent = input.Input, usingUserId = "" });
            var response = await restclient.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = JsonConvert.DeserializeObject<DeviceActionResult>(response.Content);
                if (result is { success: true })
                {
                    return new TaskActionOutput() { ExecutionInfo = response.Content, ExecutionStatus = result.success, DynamicOutput = input.DynamicInput }; ;
                }
                else
                {
                    return new TaskActionOutput() { ExecutionInfo = response.Content, ExecutionStatus = false }; ;
                }
            }
            else
            {
                return new TaskActionOutput() { ExecutionInfo = $"StatusCode:{response.StatusCode} StatusDescription:{response.StatusDescription}  {response.ErrorMessage}", ExecutionStatus = false }; ;
            }
        }

        private class DeviceActionResult
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