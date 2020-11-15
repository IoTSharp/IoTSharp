using System.Collections.Generic;
using AntDesign.Pro.Template.Models;
using Microsoft.AspNetCore.Components;

namespace AntDesign.Pro.Template.Pages.Account.Center
{
    public partial class Articles
    {
        [Parameter] public IList<ListItemDataType> List { get; set; }
    }
}