using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Sdk.Http
{
    public static class SdkClient
    {
        public static Session Session { get; set; }
        public static UserInfoDto MyInfo { get; set; }

        public static string BaseURL { get; set; } = "http://localhost:51498";
        public static HttpClient HttpClient { get; set; } = new HttpClient();

        public static T Create<T>() where T : class
        {
            T t = Activator.CreateInstance(typeof(T), HttpClient) as T;
            typeof(T).GetProperty("BaseUrl").SetValue(t, BaseURL);
            return t;
        }

        public static async Task<Session> LoginAsync(this AccountClient client, string username, string password)
        {
            Session =( await client.LoginAsync(new LoginDto() { UserName = username, Password = password })).Data;
            var token = Session.Token;
            HttpClient.DefaultRequestHeaders.Authorization =new System.Net.Http.Headers.AuthenticationHeaderValue ("Bearer",token.Access_token);
            ApiResultOfUserInfoDto userInfoDto = await client.MyInfoAsync();
            MyInfo = userInfoDto.Data;
            return Session;
        }
    }
}