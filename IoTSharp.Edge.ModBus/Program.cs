using IoT.Things.ModBus.Services;
using IoTSharp.Extensions;
using IoTSharp.Extensions.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuartzHostedService;

namespace IoT.Things.ModBus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWindowsServices()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.AllowSynchronousIO = true;
                    });
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureQuartzHost()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<ModBusService>();
                 
                }); 


    }
}