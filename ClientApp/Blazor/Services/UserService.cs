using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IoTSharp.ClientApp.Models;
using IoTSharp.Sdk.Http;

namespace IoTSharp.ClientApp.Services
{
    public interface IUserService
    {
        Task<CurrentUser> GetCurrentUserAsync();
    }

    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly IoTSharpClient _client;

        public UserService(HttpClient httpClient, IoTSharpClient client)
        {
            _httpClient = httpClient;
            _client = client;
        }

        public async Task<CurrentUser> GetCurrentUserAsync()
        {
            var cu = await _httpClient.GetFromJsonAsync<CurrentUser>("data/current_user.json");
            var my = _client.MyInfo;
               cu.Name = my.Name;
            cu.Email = my.Email;
            cu.Avatar = my.Avatar;
            cu.Title= my.Introduction;
            cu.Group = $"{my.Tenant?.Name}-{my.Customer?.Name}";
            cu.Userid = _client.MyInfo.Name;
            return cu ;
        }
    }
}