using System.Collections.Generic;
using System.Threading.Tasks;
using AntDesign.ProLayout;
using IoTSharp.ClientApp.Models;
using IoTSharp.ClientApp.Services;
using Microsoft.AspNetCore.Components;

namespace IoTSharp.ClientApp.Pages.Profile
{
    public partial class Advanced
    {
        private readonly IList<TabPaneItem> _tabList = new List<TabPaneItem>
        {
            new TabPaneItem {Key = "detail", Tab = "详情"},
            new TabPaneItem {Key = "rules", Tab = "规则"}
        };

        private AdvancedProfileData _data = new AdvancedProfileData();

        [Inject] protected IProfileService ProfileService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _data = await ProfileService.GetAdvancedAsync();
        }
    }
}