using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Skclusive.Material.Core;

namespace IoTSharp.App.View.Common
{
    public enum StatusSize
    {
        Small,

        Medium,

        Large
    }

    public enum StatusColor
    {
        Default,

        Neutral,

        Primary,

        Info,

        Success,

        Warning,

        Danger
    }

    public class StatusBulletComponent : MaterialComponent
    {
        public StatusBulletComponent() : base("StatusBullet")
        {
        }

        [Parameter]
        public StatusColor Color { set; get; } = StatusColor.Default;

        [Parameter]
        public StatusSize Size { set; get; } = StatusSize.Medium;

        protected override IEnumerable<string> Classes
        {
            get
            {
                foreach (var item in base.Classes)
                    yield return item;

                yield return $"{nameof(Color)}-{Color}";

                yield return $"{nameof(Size)}-{Size}";
            }
        }
    }
}
