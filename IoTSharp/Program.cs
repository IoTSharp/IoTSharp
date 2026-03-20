using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
            InitializeProcessPaths();
            Console.WriteLine(FiggleFonts.Doom.Render("IoTSharp"));
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    var environmentName = hostingContext.HostingEnvironment.EnvironmentName;

                    // The Windows installer writes environment-specific overrides so the
                    // selected database template can stay intact while secrets live in a
                    // separate file that is easier to rotate or regenerate.
                    configuration.AddJsonFile("appsettings.Installer.json", optional: true, reloadOnChange: false);
                    configuration.AddJsonFile($"appsettings.{environmentName}.Installer.json", optional: true, reloadOnChange: false);
                })
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

        private static void InitializeProcessPaths()
        {
            // Services inherit a system working directory on Windows. Pinning it to the
            // executable location keeps relative SQLite, certificate, and log paths stable.
            var baseDirectory = AppContext.BaseDirectory;

            if (!string.IsNullOrWhiteSpace(baseDirectory))
            {
                Directory.SetCurrentDirectory(baseDirectory);
            }
        }

    }
}
