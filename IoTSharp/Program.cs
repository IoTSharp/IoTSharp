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
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using LettuceEncrypt;
using LettuceEncrypt.Dns.Ali;
using Figgle.Fonts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System.Net;

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
                .ConfigureLogging(logging =>
                {
                    // Keep EventLog available for real Windows Service hosting, but
                    // avoid local debug startup failures when the current user cannot
                    // write to the Windows Event Log source.
                    if (!HostExtension.ShouldUseWindowsService())
                    {
                        logging.AddFilter<EventLogLoggerProvider>(level => false);
                    }
                })
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
                             ConfigureAcmeEndpoints(options, appServices);
                          }
                      });
 
                 });

        private static void ConfigureAcmeEndpoints(KestrelServerOptions options, IServiceProvider appServices)
        {
            var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");

            if (!string.IsNullOrWhiteSpace(urls))
            {
                foreach (var url in urls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    {
                        ConfigureListenEndpoint(options, uri, appServices);
                    }
                }

                return;
            }

            foreach (var port in GetConfiguredPorts("ASPNETCORE_HTTP_PORTS"))
            {
                options.ListenAnyIP(port);
            }

            foreach (var port in GetConfiguredPorts("ASPNETCORE_HTTPS_PORTS"))
            {
                options.ListenAnyIP(port, listenOptions =>
                {
                    listenOptions.UseHttps(httpsOptions =>
                    {
                        httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                    });
                    listenOptions.UseLettuceEncrypt(appServices);
                });
            }
        }

        private static IEnumerable<int> GetConfiguredPorts(string environmentVariableName)
        {
            var value = Environment.GetEnvironmentVariable(environmentVariableName);

            if (string.IsNullOrWhiteSpace(value))
            {
                return Enumerable.Empty<int>();
            }

            return value.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(segment => int.TryParse(segment, out var port) ? port : (int?)null)
                .Where(port => port.HasValue)
                .Select(port => port!.Value);
        }

        private static void ConfigureListenEndpoint(KestrelServerOptions options, Uri uri, IServiceProvider appServices)
        {
            var listen = CreateListenAction(uri, appServices);

            if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                options.ListenLocalhost(uri.Port, listen);
                return;
            }

            if (IPAddress.TryParse(uri.Host, out var address))
            {
                options.Listen(address, uri.Port, listen);
                return;
            }

            options.ListenAnyIP(uri.Port, listen);
        }

        private static Action<ListenOptions> CreateListenAction(Uri uri, IServiceProvider appServices) =>
            string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                ? listenOptions =>
                {
                    listenOptions.UseHttps(httpsOptions =>
                    {
                        httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                    });
                    listenOptions.UseLettuceEncrypt(appServices);
                }
                : _ => { };

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
