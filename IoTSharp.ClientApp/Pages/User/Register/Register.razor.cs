using AntDesign;
using IoTSharp.ClientApp.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.ClientApp.Pages.User
{
    public class RegisterModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Captcha { get; set; }
        public string Customer { get;  set; }
    }

    public partial class Register
    {
        private readonly RegisterModel _user = new RegisterModel();

        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] public IAccountService AccountService { get; set; }

        [Inject] public MessageService Message { get; set; }
        public void Reg()
        {
            AccountService.RegisterAsync(_user);
        }
    }
}