using HealthChecks.UI.Client;
using IoTSharp.Hub.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IoTSharp.Hub
{
    public static class IoTSharpExtension
    {
        public static void AddIoTSharpHub(this IServiceCollection services, IConfiguration configuration)
        {
            var _DataBase = configuration["DataBase"] ?? "sqlite";
            var _ConnectionString = Environment.ExpandEnvironmentVariables(configuration.GetConnectionString(_DataBase) ?? "Data Source=%APPDATA%\\IoTSharp.Hub\\MQTTChat.db;Pooling=true;");
            switch (_DataBase)
            {
                case "mssql":
                    services.AddEntityFrameworkSqlServer();
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(_ConnectionString), ServiceLifetime.Transient);
                    services.AddHealthChecks().AddSqlServer(_ConnectionString, name: "database").AddMQTTChatHealthChecks();
                    break;

                case "npgsql":
                    services.AddEntityFrameworkNpgsql();
                    services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(_ConnectionString), ServiceLifetime.Transient);
                    services.AddHealthChecks().AddNpgSql(_ConnectionString, name: "database").AddMQTTChatHealthChecks();
                    break;

                case "memory":
                    services.AddEntityFrameworkInMemoryDatabase();
                    services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(ApplicationDbContext)), ServiceLifetime.Transient);
                    services.AddHealthChecks().AddMQTTChatHealthChecks();
                    break;

                case "sqlite":
                default:
                    services.AddEntityFrameworkSqlite();
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_ConnectionString), ServiceLifetime.Transient);
                    services.AddHealthChecks().AddSqlite(_ConnectionString, name: "database").AddMQTTChatHealthChecks();
                    break;
            }
            services.AddHealthChecksUI();
        }
        internal static void UseIotSharpHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecksUI();
            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }
        private static void AddMQTTChatHealthChecks(this IHealthChecksBuilder builder)
        {
            builder.AddPrivateMemoryHealthCheck(1024 * 1024 * 1024, "privatememory")
             .AddDiskStorageHealthCheck(setup =>
             {
                 DriveInfo.GetDrives().ToList().ForEach(di =>
                 {
                     setup.AddDrive(di.Name, 1024);
                 });
             });
        }

        private static string GetFullPathName(string filename)
        {
            FileInfo fi = new FileInfo(System.IO.Path.Combine(
              Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create)
              , MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Name, filename));
            if (!fi.Directory.Exists) fi.Directory.Create();
            return fi.FullName;
        }

        public static void UseMqttLogger(this IApplicationBuilder app)
        {
            var mqttNetLogger = app.ApplicationServices.GetService<IMqttNetLogger>();
            var _loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
            var logger = _loggerFactory.CreateLogger<IMqttNetLogger>();
            mqttNetLogger.LogMessagePublished += (object sender, MqttNetLogMessagePublishedEventArgs e) =>
            {
                var message = $"ID:{e.TraceMessage.LogId},ThreadId:{e.TraceMessage.ThreadId},Source:{e.TraceMessage.Source},Timestamp:{e.TraceMessage.Timestamp},Message:{e.TraceMessage.Message}";
                switch (e.TraceMessage.Level)
                {
                    case MqttNetLogLevel.Verbose:
                        logger.LogTrace(e.TraceMessage.Exception, message);
                        break;

                    case MqttNetLogLevel.Info:
                        logger.LogInformation(e.TraceMessage.Exception, message);
                        break;

                    case MqttNetLogLevel.Warning:
                        logger.LogWarning(e.TraceMessage.Exception, message);
                        break;

                    case MqttNetLogLevel.Error:
                        logger.LogError(e.TraceMessage.Exception, message);
                        break;

                    default:
                        break;
                }
            };
        }
    }
}