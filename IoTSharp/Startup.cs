using DotNetCore.CAP;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using EasyCaching.Core.Configurations;
using EFCore.Sharding;
using HealthChecks.UI.Client;
using InfluxDB.Client;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Data.Sqlite;
using IoTSharp.FlowRuleEngine;
using IoTSharp.Handlers;
using IoTSharp.Interpreter;
using IoTSharp.Storage;
using IoTSharp.X509Extensions;
using Jdenticon.AspNetCore;
using Jdenticon.Rendering;
using Maikebing.Data.Taos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MQTTnet.AspNetCore;
using Newtonsoft.Json.Serialization;
using PinusDB.Data;
using Quartz;
using RabbitMQ.Client;
using Savorboard.CAP.InMemoryMessageQueue;
using Silkier.Extensions;
using SilkierQuartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Jdenticon;


namespace IoTSharp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var settings = Configuration.Get<AppSettings>();
            services.Configure((Action<AppSettings>)(setting =>
            {
                var option = setting.MqttBroker;
                Configuration.Bind(setting);
            }));
            var healthChecksUI = services.AddHealthChecksUI(setup =>
            {
                setup.SetHeaderText("IoTSharp HealthChecks");
                //Maximum history entries by endpoint
                setup.MaximumHistoryEntriesPerEndpoint(50);
                setup.AddIoTSharpHealthCheckEndpoint();
            });

            var healthChecks = services.AddHealthChecks()
                .AddDiskStorageHealthCheck(dso =>
                {
                    System.IO.DriveInfo.GetDrives()
                        .Where(d => d.DriveType != System.IO.DriveType.CDRom && d.DriveType != System.IO.DriveType.Ram)
                        .Select(f => f.Name).Distinct().ToList()
                        .ForEach(f => dso.AddDrive(f, 1024));
                }, name: "Disk Storage");

            switch (settings.DataBase)
            {
                case DataBaseType.MySql:
                    services.ConfigureMySql(Configuration.GetConnectionString("IoTSharp"), settings.DbContextPoolSize, healthChecks, healthChecksUI);
                    break;

                case DataBaseType.SqlServer:
                    services.ConfigureSqlServer(Configuration.GetConnectionString("IoTSharp"), settings.DbContextPoolSize, healthChecks, healthChecksUI);
                    break;

                case DataBaseType.Oracle:
                    services.ConfigureOracle(Configuration.GetConnectionString("IoTSharp"), settings.DbContextPoolSize, healthChecks, healthChecksUI);
                    break;

                case DataBaseType.Sqlite:
                    services.ConfigureSqlite(Configuration.GetConnectionString("IoTSharp"), settings.DbContextPoolSize, healthChecks, healthChecksUI);
                    break;
                case DataBaseType.InMemory:
                    services.ConfigureInMemory(settings.DbContextPoolSize,  healthChecksUI);

                    break;
                case DataBaseType.PostgreSql:
                default:
                    services.ConfigureNpgsql(Configuration.GetConnectionString("IoTSharp"), settings.DbContextPoolSize, healthChecks, healthChecksUI);
                    break;
            }

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddRoles<IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();



            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["JwtIssuer"],
                ValidAudience = Configuration["JwtAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtKey"])),
                //     ClockSkew=TimeSpan.Zero //JWT的缓冲时间默认5分钟，token实际过期时间为 appsettings.json 当中JwtExpireHours配置的时间（小时）加上这个时间。
            };

            services.AddSingleton(tokenValidationParams);

            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParams;
            });

            services.AddCors();
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddRinLogger();
            }
            );
            services.AddRin();
            services.AddOpenApiDocument(configure =>
            {
                Assembly assembly = typeof(Startup).GetTypeInfo().Assembly;
                var description = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                configure.Title = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
                configure.Version = typeof(Startup).GetTypeInfo().Assembly.GetName().Version.ToString();
                configure.Description = description?.Description;
                configure.AddJWTSecurity();
            });

            services.AddTransient<ApplicationDBInitializer>();
            services.AddIoTSharpMqttServer(settings.MqttBroker);
            services.AddMqttClient(settings.MqttClient);
            services.AddSilkierQuartz(options =>
            {
                options.VirtualPathRoot = "/quartz";
                options.UseLocalTime = true;
                options.DefaultDateFormat = "yyyy-MM-dd";
                options.DefaultTimeFormat = "HH:mm:ss";
                options.CronExpressionOptions = new CronExpressionDescriptor.Options()
                {
                    DayOfWeekStartIndexZero = false //Quartz uses 1-7 as the range
                };
            }, authenticationOptions =>
            {
                authenticationOptions.AuthScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                authenticationOptions.SilkierQuartzClaim = "Silkier";
                authenticationOptions.SilkierQuartzClaimValue = "Quartz";
                authenticationOptions.UserName = settings.SilkierUsername;
                authenticationOptions.UserPassword = settings.SilkierPassword;  
                authenticationOptions.AccessRequirement = SilkierQuartzAuthenticationOptions.SimpleAccessRequirement.AllowAnonymous;//登录认证有问题
            }, stdSchedulerFactoryOption =>
             {
                //opt.Add("quartz.serializer.type", "json");
                //opt.Add("quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz");
                //opt.Add("quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz");
                //opt.Add("quartz.jobStore.tablePrefix", "qrtz_");
                //opt.Add("quartz.jobStore.dataSource", "myDS");
                //opt.Add("quartz.dataSource.myDS.provider", "Npgsql");
                //opt.Add("quartz.dataSource.myDS.connectionString", Configuration.GetConnectionString("IoTSharp"));
                stdSchedulerFactoryOption.Add("quartz.plugin.recentHistory.type", "Quartz.Plugins.RecentHistory.ExecutionHistoryPlugin, Quartz.Plugins.RecentHistory");
                 stdSchedulerFactoryOption.Add("quartz.plugin.recentHistory.storeType", "Quartz.Plugins.RecentHistory.Impl.InProcExecutionHistoryStore, Quartz.Plugins.RecentHistory");
             }
        );
            services.AddControllers();

            services.AddMemoryCache();
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(settings.CachingUseIn)}";
            services.AddEasyCaching(options =>
            {
                switch (settings.CachingUseIn)
                {
                    case CachingUseIn.Redis:
                        options.UseRedis(config =>
                        {
                            settings.CachingUseRedisHosts?.Split(';').ToList().ForEach(h =>
                            {
                                var hx = h.Split(':');
                                config.DBConfig.Endpoints.Add(new ServerEndPoint(hx[0], int.Parse(hx[1])));
                            });
                        }, "iotsharp");
                        healthChecks.AddRedis(settings.CachingUseRedisHosts, name: _hc_Caching);
                        break;

                    case CachingUseIn.LiteDB:
                        options.UseLiteDB(cfg => cfg.DBConfig = new EasyCaching.LiteDB.LiteDBDBOptions() { }, name: "iotsharp");
                        break;

                    case CachingUseIn.InMemory:
                    default:
                        options.UseInMemory("iotsharp");
                        break;
                }
            });
            string _hc_telemetryStorage = $"{nameof(TelemetryStorage)}-{Enum.GetName(settings.TelemetryStorage)}";
            switch (settings.TelemetryStorage)
            {
                case TelemetryStorage.Sharding:
                    services.AddEFCoreSharding(config =>
                    {
                        switch (settings.DataBase)
                        {
                            case DataBaseType.MySql:
                                config.UseMySqlToSharding(Configuration.GetConnectionString("TelemetryStorage"), settings.Sharding.ExpandByDateMode);
                                break;

                            case DataBaseType.SqlServer:
                                config.UseSqlServerToSharding(Configuration.GetConnectionString("TelemetryStorage"), settings.Sharding.ExpandByDateMode);
                                break;

                            case DataBaseType.Oracle:
                                config.UseOracleToSharding(Configuration.GetConnectionString("TelemetryStorage"), settings.Sharding.ExpandByDateMode);
                                break;

                            case DataBaseType.Sqlite:
                                config.UseSQLiteToSharding(Configuration.GetConnectionString("TelemetryStorage"), settings.Sharding.ExpandByDateMode);
                                break;

                            case DataBaseType.PostgreSql:
                            default:
                                config.UseNpgsqlToSharding(Configuration.GetConnectionString("TelemetryStorage"), settings.Sharding.ExpandByDateMode);
                                break;
                        }
                        config.SetEntityAssemblies(new Assembly[] { typeof(TelemetryData).Assembly });
                    });
                    services.AddSingleton<IStorage, ShardingStorage>();
                    break;

                case TelemetryStorage.Taos:
                    services.AddSingleton<IStorage, TaosStorage>();
                    services.AddObjectPool(() => new TaosConnection(settings.ConnectionStrings["TelemetryStorage"]));
                    healthChecks.AddTDengine(Configuration.GetConnectionString("TelemetryStorage"), name: _hc_telemetryStorage);
                    break;

                case TelemetryStorage.InfluxDB:
                    //https://github.com/julian-fh/influxdb-setup
                    services.AddSingleton<IStorage, InfluxDBStorage>();
                    //"TelemetryStorage": "http://localhost:8086/?org=iotsharp&bucket=iotsharp-bucket&token=iotsharp-token"
                    services.AddObjectPool(() => InfluxDBClientFactory.Create(Configuration.GetConnectionString("TelemetryStorage")));
                    //healthChecks.AddInfluxDB(Configuration.GetConnectionString("TelemetryStorage"),name: _hc_telemetryStorage);
                    break;

                case TelemetryStorage.PinusDB:
                    services.AddSingleton<IStorage, PinusDBStorage>();
                    services.AddObjectPool(() =>
                    {
                        var cnt = new PinusConnection(settings.ConnectionStrings["TelemetryStorage"]);
                        cnt.Open();
                        return cnt;
                    });
                    healthChecks.AddPinusDB(Configuration.GetConnectionString("TelemetryStorage"), name: _hc_telemetryStorage);
                    break;

                case TelemetryStorage.TimescaleDB:
                    services.AddSingleton<IStorage, TimescaleDBStorage>();
                    break;

                case TelemetryStorage.SingleTable:
                default:
                    services.AddSingleton<IStorage, EFStorage>();
                    break;
            }

            services.AddTransient<IEventBusHandler, EventBusHandler>();
            if (settings.ZMQOption != null)
            {
                services.AddHostedZeroMQ(cfg => cfg = settings.ZMQOption);
            }
            services.AddCap(x =>
            {
                string _hc_EventBusStore = $"{nameof(EventBusStore)}-{Enum.GetName(settings.EventBusStore)}";
                x.SucceedMessageExpiredAfter = settings.SucceedMessageExpiredAfter;
                x.ConsumerThreadCount = settings.ConsumerThreadCount;
                switch (settings.EventBusStore)
                {
                    case EventBusStore.PostgreSql:
                        x.UsePostgreSql(Configuration.GetConnectionString("EventBusStore"));
                        healthChecks.AddNpgSql(Configuration.GetConnectionString("EventBusStore"), name: _hc_EventBusStore);
                        break;

                    case EventBusStore.MongoDB:
                        x.UseMongoDB(Configuration.GetConnectionString("EventBusStore"));  //注意，仅支持MongoDB 4.0+集群
                        healthChecks.AddMongoDb(Configuration.GetConnectionString("EventBusStore"), name: _hc_EventBusStore);
                        break;

                    case EventBusStore.LiteDB:
                        x.UseLiteDBStorage(Configuration.GetConnectionString("EventBusStore"));
                        break;

                    case EventBusStore.InMemory:
                    default:
                        x.UseInMemoryStorage();
                        break;
                }
                string _hc_EventBusMQ = $"{nameof(EventBusMQ)}-{Enum.GetName(settings.EventBusMQ)}";
                switch (settings.EventBusMQ)
                {
                    case EventBusMQ.RabbitMQ:
                        var url = new Uri(Configuration.GetConnectionString("EventBusMQ"));
                        x.UseRabbitMQ(cfg =>
                        {
                            cfg.ConnectionFactoryOptions = cf =>
                            {
                                cf.AutomaticRecoveryEnabled = true;
                                cf.Uri = new Uri(Configuration.GetConnectionString("EventBusMQ"));
                            };
                        });
                        //amqp://guest:guest@localhost:5672
                        healthChecks.AddRabbitMQ(connectionFactory =>
                       {
                           var factory = new ConnectionFactory()
                           {
                               Uri = new Uri(Configuration.GetConnectionString("EventBusMQ")),
                               AutomaticRecoveryEnabled = true
                           };
                           return factory.CreateConnection();
                       }, _hc_EventBusMQ);
                        break;

                    case EventBusMQ.Kafka:
                        x.UseKafka(Configuration.GetConnectionString("EventBusMQ"));
                        healthChecks.AddKafka(cfg =>
                       {
                           cfg.BootstrapServers = Configuration.GetConnectionString("EventBusMQ");
                       }, name: _hc_EventBusMQ);
                        break;

                    case EventBusMQ.ZeroMQ:
                        x.UseZeroMQ(cfg =>
                        {
                            cfg.HostName = Configuration.GetConnectionString("EventBusMQ") ?? "127.0.0.1";
                            cfg.Pattern = MaiKeBing.CAP.NetMQPattern.PushPull;
                        });
                        break;

                    case EventBusMQ.InMemory:
                    default:
                        x.UseInMemoryMessageQueue();
                        break;
                }
                x.UseDashboard();
                if (settings.Discovery != null)
                {
                    x.UseDiscovery(cfg => cfg = settings.Discovery);
                }
            });

            services.Configure<BaiduTranslateProfile>(Configuration.GetSection("BaiduTranslateProfile"));
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            services.AddRazorPages();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            services.AddScriptEngines(Configuration.GetSection("EngineSetting"));
            services.AddTransient<FlowRuleProcessor>();
            services.AddSingleton<TaskExecutorHelper>();
            services.AddTransient<PublishAttributeDataTask>();
            services.AddTransient<PublishTelemetryDataTask>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISchedulerFactory factory)
        {
            if (env.IsDevelopment() || !env.IsEnvironment("Production"))
            {
                    // Add: Enable request/response recording and serve a inspector frontend.
                    // Important: `UseRin` (Middlewares) must be top of the HTTP pipeline.
                    app.UseRin();

                    // Add(option): Enable ASP.NET Core MVC support if the project built with ASP.NET Core MVC
                    app.UseRinMvcSupport();

                    app.UseDeveloperExceptionPage();

                    // Add: Enable Exception recorder. this handler must be after `UseDeveloperExceptionPage`.
                    app.UseRinDiagnosticsHandler();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();
            app.UseCors(option => option
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseDefaultFiles();
            if (env.IsDevelopment() || !env.IsEnvironment("Production"))
            {
                app.UseStaticFiles();
            }
            else
            {
                app.UseSpaStaticFiles();
            }
      
            app.UseIotSharpMqttServer();

            app.UseSwaggerUi3();
            app.UseOpenApi();

            app.UseSilkierQuartz();//必须要在UseEndpoints之前调用
            app.UseCapDashboard();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMqtt("/mqtt");
                endpoints.MapHealthChecks("/healthz", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501
                spa.Options.SourcePath = "ClientApp";
                if (env.IsDevelopment() || !env.IsEnvironment("Production"))
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
            app.UseJdenticon(defaultStyle =>
            {
                // Custom identicon style
                // https://jdenticon.com/icon-designer.html?config=8644440010c4330a24461852
                defaultStyle.Hues = new HueCollection { { 196, HueUnit.Degrees } };
                defaultStyle.BackColor = Color.FromRgba(134, 68, 68, 0);
                defaultStyle.ColorLightness = Jdenticon.Range.Create(0.36f, 0.70f);
                defaultStyle.GrayscaleLightness = Jdenticon.Range.Create(0.24f, 0.82f);
                defaultStyle.ColorSaturation = 0.51f;
                defaultStyle.GrayscaleSaturation = 0.10f;
            });
        }
    }
}