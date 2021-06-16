using System.Threading.Tasks;
using IoTSharp.ClientApp.Models;
using IoTSharp.ClientApp.Services;
using Microsoft.AspNetCore.Components;

namespace IoTSharp.ClientApp.Pages.Profile
{
    public partial class Basic
    {
        private BasicProfileDataType _data = new BasicProfileDataType();

        [Inject] protected IProfileService ProfileService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _data = await ProfileService.GetBasicAsync();
        }
    }
}