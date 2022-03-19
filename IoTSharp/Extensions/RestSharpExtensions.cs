using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp
{
   
    public static class RestSharpExtensions
    {
        public static async Task<T> GetDataBy<T>(this Uri uri, params object[] objparam)
        {
            return await GetDataBy<T>(uri, request =>
            {
                if (objparam != null)
                {
                    objparam.GetType().GetProperties().ToList().ForEach(p =>
                    {
                        request.AddParameter(p.Name, p.GetValue(objparam), ParameterType.QueryString);
                    });
                }
            });

        }
        public static async Task<T> GetDataBy<T>(this Uri uri, Action<RestRequest> action)
        {
            return await GetDataBy<T>(uri, action, false);
        }
        public static async Task<T> GetDataBy<T>(this Uri uri, Action<RestRequest> action, bool checktype = false)
        {
            var result1 = default(T);
            var client = Create(uri);
            var request = new RestRequest();
            action?.Invoke(request);
            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (checktype)
                {
                    var ret = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content);
                    if (ret?.GetType() == typeof(string))
                    {
                        result1 = Newtonsoft.Json.JsonConvert.DeserializeObject<T>((string)ret);
                    }
                    else
                    {
                        result1 = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content);
                    }
                }
                else
                {
                    result1 = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content);
                }
            }
            return result1;
        }

        public static async Task<R> PostDataBy<T, R>(this Uri uri, T body) where T : class
        {
            return await PostDataBy<R>(uri, request =>
            {
                request.AddHeader("Content-Type", "application/json");
                if (body != null)
                {
                    request.AddJsonBody(body);
                }
            });
        }
        public static async Task<R> PostDataBy<R>(this Uri uri, Action<RestRequest> action)
        {
            var client = Create(uri);
            var request = new RestRequest();
            action?.Invoke(request);
            var response = await client.PostAsync(request);
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<R>(response.Content);
            return result;
        }


        private static RestClient Create(Uri uri)
        {
            var client = new RestClient(new RestClientOptions(uri) { Timeout = -1, FollowRedirects = false });
            client.AddDefaultHeader(KnownHeaders.Accept, "*/*");
            return client;
        }
    }
}
