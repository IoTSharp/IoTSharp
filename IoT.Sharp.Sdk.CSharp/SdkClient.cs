using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Sharp.Sdk.CSharp
{
    public static class SdkClient
    {
        public static string BaseURL { get; set; } = "http://localhost:51498";
        public static HttpClient HttpClient { get; set; } = new HttpClient();
        public static T Create<T>() where T : class
        {
            return Activator.CreateInstance(typeof(T), HttpClient) as T;
        }
    }
}
