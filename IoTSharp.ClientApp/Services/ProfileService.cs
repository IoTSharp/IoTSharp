using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AntDesign.Pro.Template.Models;

namespace AntDesign.Pro.Template.Services
{
    public interface IProfileService
    {
        Task<BasicProfileDataType> GetBasicAsync();
        Task<AdvancedProfileData> GetAdvancedAsync();
    }

    public class ProfileService : IProfileService
    {
        private readonly HttpClient _httpClient;

        public ProfileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<BasicProfileDataType> GetBasicAsync()
        {
            return await _httpClient.GetFromJsonAsync<BasicProfileDataType>("data/basic.json");
        }

        public async Task<AdvancedProfileData> GetAdvancedAsync()
        {
            return await _httpClient.GetFromJsonAsync<AdvancedProfileData>("data/advanced.json");
        }
    }
}