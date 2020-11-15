using System.Collections.Generic;
using AntDesign.Pro.Layout;
using Microsoft.AspNetCore.Components;

namespace AntDesign.Pro.Template.Pages.List
{
    public partial class SearchList
    {
        private readonly IList<TabPaneItem> _tabList = new List<TabPaneItem>
        {
            new TabPaneItem {Key = "articles", Tab = "文章"},
            new TabPaneItem {Key = "projects", Tab = "项目"},
            new TabPaneItem {Key = "applications", Tab = "应用"}
        };

        [Inject] protected NavigationManager NavigationManager { get; set; }

        private string GetTabKey()
        {
            var url = NavigationManager.Uri.TrimEnd('/');
            var key = url.Substring(url.LastIndexOf('/') + 1);
            return key;
        }

        private void HandleTabChange(string key)
        {
            var url = NavigationManager.Uri.TrimEnd('/');
            url = url.Substring(0, url.LastIndexOf('/'));
            NavigationManager.NavigateTo($"{url}/{key}");
        }
    }
}