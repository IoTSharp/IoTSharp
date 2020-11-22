using System.Threading.Tasks;
using IoTSharp.ClientApp.Models;
using IoTSharp.ClientApp.Services;
using Microsoft.AspNetCore.Components;
using AntDesign;

namespace IoTSharp.ClientApp.Pages.User
{
    public partial class Login
    {
        private readonly LoginParamsType _model = new LoginParamsType();

        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] public IAccountService AccountService { get; set; }

        [Inject] public MessageService Message { get; set; }

        public async Task HandleSubmitAsync()
        {
            var ok= await  AccountService.LoginAsync(_model);
            if (ok)
            {
                NavigationManager.NavigateTo("/");
            }
            else
            {
                await Message.Error($"登录失败");
            }
        }

        public async Task GetCaptcha()
        {
            var captcha = await AccountService.GetCaptchaAsync(_model.Mobile);
            await Message.Success($"获取验证码成功！验证码为：{captcha}");
        }
    }
}