using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
namespace IoTSharp.Extensions
{
    public static class RestClientExtensions
    {


        public static T ReqResp<T>(this RestClient client, string resource, Method method = Method.GET, DataFormat dataFormat = DataFormat.Json, Action<int, string> __log = null, List<HttpCookie> cookies = null)
                      => client.ReqResp<T>(new RestRequest(resource, method, dataFormat), __log, cookies);

        public static T ReqResp<T>(this RestClient client, Uri resource, Method method = Method.GET, DataFormat dataFormat = DataFormat.Json, Action<int, string> __log = null, List<HttpCookie> cookies = null)
                        => client.ReqResp<T>(new RestRequest(resource, method, dataFormat), __log, cookies);

        public static T ReqResp<T>(this RestClient client, RestRequest rest, Action<int, string> __log = null, List<HttpCookie> cookies = null)
        {
            T result = default;
            try
            {
                if (cookies != null && cookies.Count > 0)
                {
                    cookies.ForEach(co =>
                    {
                        rest.AddCookie(co.Name, co.Value);
                    });
                }
                var response = client.Execute(rest);
                if (response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content)
                       && JToken.Parse(response.Content).ToObject<T>() is T jDResult)
                {
                    result = jDResult;
                    if (cookies != null)
                    {
                        cookies.Clear();
                        cookies.AddRange(response.Cookies.Select(s => s.HttpCookie));
                    }
                }
                else
                {
                    __log?.Invoke((int)response.StatusCode, response.Content);
                }
            }
            catch (Exception ex)
            {
                __log?.Invoke(-999, ex.Message);
            }
            return result;
        }
    }
}
