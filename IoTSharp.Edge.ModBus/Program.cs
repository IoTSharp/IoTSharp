using IoTSharp.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace IoT.Things.ModBus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().RunAsEnv();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
                        WebHost.CreateDefaultBuilder(args)
                        .UseContentRootAsEnv()
                        .UseStartup<Startup>();

       
    }
}