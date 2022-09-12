using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace RestSharp
{
    public static class RestClientExtensions
    {

        /// <summary>
        /// 将<paramref name="body"/>作为 body发起针对<paramref name="resource"/>的请求
        /// </summary>
        /// <typeparam name="T">返回值</typeparam>
        /// <typeparam name="I">body的类型</typeparam>
        /// <param name="client"></param>
        /// <param name="resource"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static T ReqResp<T, I>(this RestClient client, string resource, I body) where I : class
        {
            return ReqResp<T, I>(client, resource, body, Method.Post, DataFormat.Json, null, null);
        }
        public static T ReqResp<T, I>(this RestClient client, string resource, I body, Method method = Method.Get, DataFormat dataFormat = DataFormat.Json
          , Action<int, string> __log = null
          , List<Cookie> cookies = null) where I : class
        {
            return ReqResp<T>(client, resource, method,  __log, cookies, req =>
                {
                    switch (dataFormat)
                    {
                        case DataFormat.Json:
                            req.AddJsonBody(body);
                            break;
                        case DataFormat.Xml:
                            req.AddXmlBody(body);
                            break;
                        case DataFormat.None:
                            break;
                        default:
                            break;
                    }
                    return req;
                });
        }
        public static T ReqResp<T>(this RestClient client, string resource, Method method = Method.Get
                , Action<int, string> __log = null
                , List<Cookie> cookies = null
                , Func<RestRequest, RestRequest> func = null)
        {
            var req = new RestRequest(resource, method);
            if (func != null)
            {
                req = func.Invoke(req);
            }
            return client.ReqResp<T>(req,DataFormat.Json, __log, cookies);
        }
        public static T ReqResp<T>(this RestClient client, string resource)
            => client.ReqResp<T>(new RestRequest(resource, Method.Get),  DataFormat.Json, null, null);

        public static T ReqResp<T>(this RestClient client, string resource, Method method = Method.Get, DataFormat dataFormat = DataFormat.Json)
                    => client.ReqResp<T>(new RestRequest(resource, method), dataFormat, null, null);

        public static T ReqResp<T>(this RestClient client, string resource, Method method = Method.Get, DataFormat dataFormat = DataFormat.Json, Action<int, string> __log = null, List<Cookie> cookies = null)
                      => client.ReqResp<T>(new RestRequest(resource, method),dataFormat, __log, cookies);

        public static T ReqResp<T>(this RestClient client, Uri resource, Method method = Method.Get, DataFormat dataFormat = DataFormat.Json, Action<int, string> __log = null, List<Cookie> cookies = null)
                        => client.ReqResp<T>(new RestRequest(resource, method), dataFormat, __log, cookies);

        public static T ReqResp<T>(this RestClient client, Uri resource, Method method = Method.Get, DataFormat dataFormat = DataFormat.Json, Func<RestRequest, RestRequest> func = null)
             => client.ReqResp<T>(new RestRequest(resource, method),dataFormat , null, null, func);

        public static T ReqResp<T>(this RestClient client, Uri resource, Method method = Method.Get, DataFormat dataFormat = DataFormat.Json, Action<int, string> __log = null, List<Cookie> cookies = null, Func<RestRequest, RestRequest> func = null)
                    => client.ReqResp<T>(new RestRequest(resource,method),dataFormat, __log, cookies, func);
 
        public static T ReqResp<T>(this RestClient client, RestRequest rest, DataFormat dataFormat = DataFormat.Json, Action<int, string> __log = null, List<Cookie> cookies = null, Func<RestRequest, RestRequest> func = null)
        {
            T result = default;
            try
            {
                if (cookies != null && cookies.Count > 0)
                {
                    cookies.ForEach(co =>
                    {
                        client.AddCookie(co.Name, co.Value,co.Port,co.Domain);
                    });
                }
                var response = client.Execute(func.Invoke(rest));
                if (response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content)
                       && JToken.Parse(response.Content).ToObject<T>() is T jDResult)
                {
                    result = jDResult;
                    if (cookies != null)
                    {
                        cookies.Clear();
                        cookies.AddRange(response.Cookies.ToArray());
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
