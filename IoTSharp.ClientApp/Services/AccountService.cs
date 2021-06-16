using System;
using System.Threading.Tasks;
using IoTSharp.ClientApp.Models;
using IoTSharp.ClientApp.Pages.User;
using IoTSharp.Sdk.Http;

namespace IoTSharp.ClientApp.Services
{
    public interface IAccountService
    {
        Task<bool> LoginAsync(LoginParamsType model);
        Task<bool> RegisterAsync(RegisterModel model);
        Task<string> GetCaptchaAsync(string modile);
    }

    public class AccountService : IAccountService
    {
        private readonly Random _random = new Random();
        private  readonly IoTSharpClient _client;

        public AccountService  ( IoTSharpClient client)
        {
            _client = client;
        }
        public async Task<bool> LoginAsync(LoginParamsType model)
        {
          return   await _client.LoginAsync(model.UserName, model.Password);
        }
        public async Task<bool> RegisterAsync(RegisterModel model)
        {
            return await _client.RegisterAsync(model.Customer,model.UserName, model.Password,model.Phone);
        }

        public Task<string> GetCaptchaAsync(string modile)
        {
            var captcha = _random.Next(0, 9999).ToString().PadLeft(4, '0');
            return Task.FromResult(captcha);
        }
    }
}