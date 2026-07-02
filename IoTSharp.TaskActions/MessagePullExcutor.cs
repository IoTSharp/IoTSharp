using IoTSharp.Extensions;
using RestSharp;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace IoTSharp.TaskActions
{
    [DisplayName("用于消息推送的执行器")]
    public class MessagePullExecutor : TaskAction
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
                var unwrapSerializedJsonElement = input.Input.Contains("ValueKind");
                var values = JsonNodeParser.ParseObject(input.Input);
                var config = JsonObjectSerializer.Deserialize<ModelExecutorConfig>(input.ExecutorConfig);
                var dd = values.Select(c => new ParamObject
                {
                    keyName = c.Key.ToLower(),
                    value = ToParamValue(c.Value, unwrapSerializedJsonElement)
                }).ToList();
                var restclient = new RestClient(config.BaseUrl);
                restclient.AddDefaultHeader(KnownHeaders.Accept, "*/*");
                var request = new RestRequest(config.Url + (input.DeviceId == Guid.Empty ? "" : "/" + input.DeviceId));
                request.AddHeader("X-Access-Token",
                    config.Token);
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(dd);

                var response = await restclient.ExecutePostAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonObjectSerializer.Deserialize<MessagePullResult>(response.Content);
                    if (result is { success: true })
                    {
                        return new TaskActionOutput() { ExecutionInfo = response.Content, ExecutionStatus = result.success, DynamicOutput = input.DynamicInput };
                    }
                    else
                    {
                        return new TaskActionOutput() { ExecutionInfo = response.Content, ExecutionStatus = false };
                    }
                }
                else
                {
                    return new TaskActionOutput() { ExecutionInfo = $"StatusCode:{response.StatusCode} StatusDescription:{response.StatusDescription}  {response.ErrorMessage}", ExecutionStatus = false };
                }
            }
            catch (Exception ex)
            {
                return new TaskActionOutput() { ExecutionInfo = ex.Message, ExecutionStatus = false };
            }
        }

        private static object ToParamValue(JsonNode value, bool unwrapSerializedJsonElement)
        {
            if (unwrapSerializedJsonElement && value is JsonObject obj && obj.Count > 0)
            {
                return obj.First().Value.ToClrObject();
            }

            return value.ToClrObject();
        }

        private class MessagePullResult
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string code { get; set; }

            public long timestamp { get; set; }

            public dynamic result { get; set; }
        }

        public class ParamObject
        {
            public string keyName { get; set; }
            public dynamic value { get; set; }
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
