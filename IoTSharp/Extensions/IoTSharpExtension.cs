using IoTSharp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IoTSharp
{
    public static class IoTSharpExtension
    {
        public static void AddIoTSharpHub(this IServiceCollection services, IConfiguration configuration)
        {
            var _DataBase = configuration["DataBase"] ?? "sqlite";
            var _ConnectionString = Environment.ExpandEnvironmentVariables(configuration.GetConnectionString(_DataBase) ?? "Data Source=%APPDATA%\\IoTSharp\\MQTTChat.db;Pooling=true;");
            switch (_DataBase)
            {
                case "mssql":
                    services.AddEntityFrameworkSqlServer();
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(_ConnectionString), ServiceLifetime.Singleton);
                    break;
                case "npgsql":
                    services.AddEntityFrameworkNpgsql();
                    services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(_ConnectionString), ServiceLifetime.Singleton);
                    break;
                case "memory":
                    services.AddEntityFrameworkInMemoryDatabase();
                    services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(ApplicationDbContext)), ServiceLifetime.Singleton);
                    break;
                case "sqlite":
                default:
                    services.AddEntityFrameworkSqlite();
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_ConnectionString), ServiceLifetime.Singleton);
                    break;
            }
        }

        private static string GetFullPathName(string filename)
        {
            FileInfo fi = new FileInfo(System.IO.Path.Combine(
              Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create)
              , MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Name, filename));
            if (!fi.Directory.Exists) fi.Directory.Create();
            return fi.FullName;
        }

        internal static void UseSwagger(this IApplicationBuilder app)
        {
            app.UseSwaggerUi3();
            app.UseSwagger(config => config.PostProcess = (document, request) =>
            {
                if (request.Headers.ContainsKey("X-External-Host"))
                {
                    // Change document server settings to public
                    document.Host = request.Headers["X-External-Host"].First();
                    document.BasePath = request.Headers["X-External-Path"].First();
                }
            });
            app.UseSwaggerUi3(config => config.TransformToExternalPath = (internalUiRoute, request) =>
            {
                // The header X-External-Path is set in the nginx.conf file
                var externalPath = request.Headers.ContainsKey("X-External-Path") ? request.Headers["X-External-Path"].First() : "";
                return externalPath + internalUiRoute;
            });
        }
    }
}