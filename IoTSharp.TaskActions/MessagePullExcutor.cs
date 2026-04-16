using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
                if (input.Input.Contains("ValueKind"))
                {
                    JObject o = JsonConvert.DeserializeObject(input.Input) as JObject;
                    var config = JsonConvert.DeserializeObject<ModelExecutorConfig>(input.ExecutorConfig);
                    var dd = o.Properties().Select(c => new ParamObject { keyName = c.Name.ToLower(), value = JPropertyToObject(c.Value.First as JProperty) }).ToList();
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
                        var result = JsonConvert.DeserializeObject<MessagePullResult>(response.Content);
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
                        return new TaskActionOutput() { ExecutionInfo = response.ErrorMessage, ExecutionStatus = false }; ;
                    }
                }
                else
                {
                    JObject o = JsonConvert.DeserializeObject(input.Input) as JObject;
                    var config = JsonConvert.DeserializeObject<ModelExecutorConfig>(input.ExecutorConfig);
                    var dd = o.Properties().Select(c => new ParamObject { keyName = c.Name.ToLower(), value = JPropertyToObject(c) }).ToList();
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
                        var result = JsonConvert.DeserializeObject<MessagePullResult>(response.Content);
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
            }
            catch (Exception ex)
            {
                return new TaskActionOutput() { ExecutionInfo = ex.Message, ExecutionStatus = false }; ;
            }
        }

        public static object JPropertyToObject(JProperty property)
        {
            object obj = null;
            switch (property.Value.Type)
            {
                case JTokenType.Integer:
                    obj = property.Value.ToObject<int>();
                    break;

                case JTokenType.Float:
                    obj = property.Value.ToObject<float>();
                    break;

                case JTokenType.String:
                    obj = property.Value.ToObject<string>();
                    break;

                case JTokenType.Boolean:
                    obj = property.Value.ToObject<bool>();
                    break;

                case JTokenType.Date:
                    obj = property.Value.ToObject<DateTime>();
                    break;

                case JTokenType.Bytes:
                    obj = property.Value.ToObject<byte[]>();
                    break;

                case JTokenType.Guid:
                    obj = property.Value.ToObject<Guid>();
                    break;

                case JTokenType.Uri:
                    obj = property.Value.ToObject<Uri>();
                    break;

                case JTokenType.TimeSpan:
                    obj = property.Value.ToObject<TimeSpan>();
                    break;

                default:
                    obj = property.Value;
                    break;
            }
            return obj;
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