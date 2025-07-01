using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Figgle;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using LettuceEncrypt;
using LettuceEncrypt.Dns.Ali;
using Figgle.Fonts;

namespace IoTSharp
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Console.WriteLine(FiggleFonts.Doom.Render("IoTSharp"));
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWindowsServices()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    webBuilder.UseKestrel(options =>
                     {
                         var appServices = options.ApplicationServices;
                         if (Environment.GetEnvironmentVariable("IOTSHARP_ACME") == "true")
                         {
                             options.ConfigureHttpsDefaults(h =>
                               {
                                  h.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                                  h.UseLettuceEncrypt(appServices);
                              });
                         }
                     });

                });

    }
}
