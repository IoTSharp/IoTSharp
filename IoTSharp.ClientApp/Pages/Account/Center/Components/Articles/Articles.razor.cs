using System.Collections.Generic;
using IoTSharp.ClientApp.Models;
using Microsoft.AspNetCore.Components;

namespace IoTSharp.ClientApp.Pages.Account.Center
{
    public partial class Articles
    {
        [Parameter] public IList<ListItemDataType> List { get; set; }
    }
}