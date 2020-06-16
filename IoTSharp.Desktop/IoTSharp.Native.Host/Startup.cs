using Microsoft.Extensions.DependencyInjection;
using IoTSharp.App.View;
using Skclusive.Material.Layout;
using WebWindows.Blazor;

namespace IoTSharp.Native.Host
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Responsive is disabled due to bug in WebWindow javascript calling dotnet fails when delayed
            services.TryAddDashboardViewServices
            (
                new DashboardViewConfigBuilder()
                .WithIsServer(false)
                .WithIsPreRendering(false)
                .WithResponsive(false)
                .Build()
            );
        }

        public void Configure(DesktopApplicationBuilder app)
        {
            app.AddComponent<DashboardView>("app");
        }
    }
}
