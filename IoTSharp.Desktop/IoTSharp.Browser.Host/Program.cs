using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using IoTSharp.App.View;
using Skclusive.Material.Layout;
using System.Net.Http;

namespace IoTSharp.Browser.Host
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<DashboardView>("app");

            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.TryAddDashboardViewServices
            (
                new DashboardViewConfigBuilder()
                .WithIsServer(false)
                .WithIsPreRendering(false)
                .WithResponsive(true)
                .Build()
            );

            await builder.Build().RunAsync();
        }
    }
}
