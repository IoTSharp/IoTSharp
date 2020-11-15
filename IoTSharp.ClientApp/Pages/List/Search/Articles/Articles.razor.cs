using System.Collections.Generic;
using System.Threading.Tasks;
using AntDesign.Pro.Template.Models;
using AntDesign.Pro.Template.Services;
using Microsoft.AspNetCore.Components;

namespace AntDesign.Pro.Template.Pages.List
{
    public partial class Articles
    {
        private readonly string[] _defaultOwners = {"wzj", "wjh"};
        private readonly ListFormModel _model = new ListFormModel();

        private readonly Owner[] _owners =
        {
            new Owner {Id = "wzj", Name = "我自己"},
            new Owner {Id = "wjh", Name = "吴家豪"},
            new Owner {Id = "zxx", Name = "周星星"},
            new Owner {Id = "zly", Name = "赵丽颖"},
            new Owner {Id = "ym", Name = "姚明"}
        };

        private IList<ListItemDataType> _fakeList = new List<ListItemDataType>();

        [Inject] public IProjectService ProjectService { get; set; }

        private void SetOwner()
        {
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _fakeList = await ProjectService.GetFakeListAsync(8);
        }
    }
}