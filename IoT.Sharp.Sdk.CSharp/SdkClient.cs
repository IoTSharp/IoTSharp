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
        public static LoginResult Session { get; set; }
        public static UserInfoDto MyInfo { get; set; }

        public static string BaseURL { get; set; } = "http://localhost:51498";
        public static HttpClient HttpClient { get; set; } = new HttpClient();

        public static T Create<T>() where T : class
        {
            return Activator.CreateInstance(typeof(T), HttpClient) as T;
        }

        public static async Task<LoginResult> LoginAsync(this AccountClient client, string username, string password)
        {
            Session = await client.LoginAsync(new LoginDto() { Email = username, Password = password });
            var token = Session.Token;
            HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.Access_token}");
            ApiResultOfUserInfoDto userInfoDto = await client.MyInfoAsync();
            MyInfo = userInfoDto.Data;
            return Session;
        }
    }
}