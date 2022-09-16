using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SilkierQuartz;
using Rin;
using Figgle;

namespace IoTSharp
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Console.WriteLine(FiggleFonts.Ogre.Render($"IoTSharp {typeof(Startup).Assembly.GetName().Version}"));
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWindowsServices()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
               .ConfigureSilkierQuartzHost()
               .ConfigureIoTSharpHost();
        
        

    }
}
