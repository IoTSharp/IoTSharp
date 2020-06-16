using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Skclusive.Material.Core;
using ChartJs.Blazor.Charts;
using ChartJs.Blazor.ChartJS.PieChart;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Handlers;
using ChartJs.Blazor.ChartJS.Common.Enums;
using Skclusive.Material.Icon;
using Microsoft.AspNetCore.Components.Rendering;

namespace IoTSharp.App.View.Home
{
    public class UsersByDeviceComponent : MaterialComponent
    {
        public UsersByDeviceComponent() : base("dashboard-userdivces")
        {
        }

        protected static List<(string title, int value, string color, RenderFragment icon)> Devices = new List<(string title, int value, string color, RenderFragment icon)>
        {
            (
                title: "Desktop",
                value: 63,
                color: "#3f51b5",
                icon: (RenderFragment)((RenderTreeBuilder builder) => {
                    builder.OpenComponent<LaptopMacIcon>(0);
                    builder.CloseComponent();
                })
            ),
            (
                title: "Tablet",
                value: 15,
                color: "#e53935",
                icon: (RenderFragment)((RenderTreeBuilder builder) => {
                    builder.OpenComponent<TabletMacIcon>(0);
                    builder.CloseComponent();
                })
            ),
            (
                title: "Mobile",
                value: 23,
                color: "#fb8c00",
                icon: (RenderFragment)((RenderTreeBuilder builder) => {
                    builder.OpenComponent<PhoneIphoneIcon>(0);
                    builder.CloseComponent();
                })
            )
        };

        protected PieConfig _config;

        protected ChartJsPieChart _doughnutChartJs;

        protected override void OnInitialized()
        {
            _config = new PieConfig
            {
                Options = new PieOptions(true)
                {
                    Legend = new Legend
                    {
                        Display = false
                    },

                    Responsive = true,

                    MaintainAspectRatio = false,

                    CutoutPercentage = 80,

                    Tooltips = new Tooltips
                    {
                        Enabled = true,

                        Mode = InteractionMode.Index,

                        Intersect = false,

                        BorderWidth = 1,

                        BorderColor = "rgba(0, 0, 0, 0.12)",

                        BackgroundColor = "#ffffff",

                        TitleFontColor = "rgba(0, 0, 0, 0.87)",

                        BodyFontColor = "rgba(0, 0, 0, 0.54)",

                        FooterFontColor = "rgba(0, 0, 0, 0.54)"
                    }
                }
            };

            _config.Data.Labels.AddRange(new[] { "Desktop", "Tablet", "Mobile" });

            var doughnutSet = new PieDataset
            {
                BackgroundColor = new[] { "#3f51b5", "#e53935", "#fb8c00" },

                BorderWidth = 8,

                HoverBorderColor = "#ffffff",

                BorderColor = "#ffffff"
            };

            doughnutSet.Data.AddRange(new double[] { 63, 15, 22 });

            _config.Data.Datasets.Add(doughnutSet);
        }
    }
}
