using System.Collections.Generic;
using Skclusive.Material.Core;
using ChartJs.Blazor.Charts;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Handlers;
using ChartJs.Blazor.ChartJS.Common.Enums;
using ChartJs.Blazor.ChartJS.BarChart;
using ChartJs.Blazor.ChartJS.Common;
using ChartJs.Blazor.ChartJS.BarChart.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
using ChartJs.Blazor.ChartJS.Common.Wrappers;

namespace IoTSharp.App.View.Home
{
    public class LatestSalesComponent : MaterialComponent
    {
        public LatestSalesComponent() : base("dashboard-sales")
        {
        }

        protected BarConfig _config;

        protected ChartJsBarChart _barChartJs;

        protected override void OnInitialized()
        {
            _config = new BarConfig
            {
                Options = new BarOptions
                {
                    Legend = new Legend
                    {
                        Display = false
                    },

                    Responsive = true,

                    MaintainAspectRatio = false,

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
                    },

                    Scales = new BarScales
                    {
                        XAxes = new List<CartesianAxis>
                        {
                            new BarCategoryAxis
                            {
                                BarThickness = 12,

                                MaxBarThickness = 10,

                                BarPercentage = 0.5,

                                CategoryPercentage= 0.5,

                                // Ticks = new CategoryTicks
                                // {
                                //     FontColor = "rgba(0, 0, 0, 0.54)"
                                // },

                                GridLines = new GridLines
                                {
                                    Display = false,

                                    DrawBorder = false,

                                    OffsetGridLines = true
                                },

                                Offset = true,

                                OffsetGridLines = true
                            }
                        },

                        YAxes = new List<CartesianAxis>
                        {
                            new BarLinearCartesianAxis
                            {
                                BarThickness = 12,

                                MaxBarThickness = 10,

                                BarPercentage = 0.5,

                                CategoryPercentage= 0.5,

                                Ticks = new LinearCartesianTicks {
                                    BeginAtZero = true,

                                    Min = 0

                                    // FontColor = "rgba(0, 0, 0, 0.54)"
							    },

                                GridLines = new GridLines
                                {
                                    BorderDash = new double [] { 2 },

                                    DrawBorder = false,

                                    Color = "rgba(0, 0, 0, 0.12)",

                                    ZeroLineBorderDash = new int [] { 2 },

                                    ZeroLineBorderDashOffset = 2,

                                    ZeroLineColor = "rgba(0, 0, 0, 0.12)"
                                }
                            }
                        }
                    }
                }
            };

            _config.Data.Labels.AddRange(new[] { "1 Aug", "2 Aug", "3 Aug", "4 Aug", "5 Aug", "6 Aug" });

            var barSet1 = new BarDataset<DoubleWrapper>
            {
                Label = "This year",

                BackgroundColor = "#3f51b5"
            };
            barSet1.AddRange(new DoubleWrapper[] { 18, 5, 19, 27, 29, 19 });

            var barSet2 = new BarDataset<DoubleWrapper>
            {
                Label = "Last year",

                BackgroundColor = "#e5e5e5"
            };
            barSet2.AddRange(new DoubleWrapper[] { 11, 20, 12, 29, 30, 25 });

            _config.Data.Datasets.AddRange(new[] { barSet1, barSet2 });
        }
    }
}
