using EasyCaching.Core.Configurations;
using HealthChecks.UI.Client;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using IoTSharp.Data.SonnetDB;
using IoTSharp.Data.TimeSeries;
using IoTSharp.EventBus;
using IoTSharp.EventBus.CAP;
using IoTSharp.EventBus.SonnetMQ;
using IoTSharp.FlowRuleEngine;
using IoTSharp.Gateways;
using IoTSharp.Interpreter;
using IoTSharp.McpTools;
using IoTSharp.Services;
using IoTSharp.Services.Coap;
using IoTSharp.TaskActions;
using Jdenticon.AspNetCore;
using Jdenticon.Rendering;
using LettuceEncrypt;
using LettuceEncrypt.Dns.Ali;
using MaiKeBing.HostedService.ZeroMQ;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.Server;
using MQTTnet.AspNetCore;
using Quartz;
using Quartz.AspNetCore;
using Storage.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var settings = new AppSettings();
            Configuration.Bind(settings);
            var hostOptions = new IoTSharpHostOptions();
            Configuration.Bind(hostOptions);
            services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });
            services.Configure<AppSettings>(setting => Configuration.Bind(setting));
            var healthChecksUI = services.AddHealthChecksUI(setup =>
            {
                setup.SetHeaderText("IoTSharp HealthChecks");
                //Maximum history entries by endpoint
                setup.MaximumHistoryEntriesPerEndpoint(50);
                setup.AddIoTSharpHealthCheckEndpoint(hostOptions);
            });

            var healthChecks = services.AddHealthChecks()
                .AddDiskStorageHealthCheck(dso =>
                {
                    System.IO.DriveInfo.GetDrives()
                        .Where(d => d.DriveType == System.IO.DriveType.Fixed && d.DriveFormat != "overlay" && !d.Name.StartsWith("/sys"))
                        .Select(f => f.Name).Distinct().ToList()
                        .ForEach(f => dso.AddDrive(f));
                }, name: "Disk Storage");

            switch (settings.DataBase)
            {
                case DataBaseType.MySql:
                    services.ConfigureMySql(GetConnectionString(settings, "IoTSharp"), settings.DbContextPoolSize, healthChecks, healthChecksUI);
                    break;

                case DataBaseType.SqlServer:
                    services.ConfigureSqlServer(GetConnectionString(settings, "IoTSharp"), settings.DbContextPoolSize, healthChecks, healthChecksUI);
                    break;

                case DataBaseType.Oracle:
                    services.ConfigureOracle(GetConnectionString(settings, "IoTSharp"), settings.DbContextPoolSize, healthChecks, healthChecksUI);
                    break;

                case DataBaseType.Sqlite:
                    services.ConfigureSqlite(GetConnectionString(settings, "IoTSharp"), settings.DbContextPoolSize, healthChecks, healthChecksUI);
                    break;
                case DataBaseType.InMemory:
                    services.ConfigureInMemory(settings.DbContextPoolSize, healthChecksUI);
                    settings.TelemetryStorage = TelemetryStorage.SingleTable;
                    break;
                case DataBaseType.ClickHouse:
                    services.ConfigureClickHouse(GetConnectionString(settings, "IoTSharp"), settings.DbContextPoolSize, healthChecks, healthChecksUI);
                    settings.TelemetryStorage = TelemetryStorage.SingleTable;
                    break;
                case DataBaseType.SonnetDB:
                    services.ConfigureSonnetDB(GetConnectionString(settings, "IoTSharp"), settings.DbContextPoolSize, healthChecks, healthChecksUI);
                    break;
                case DataBaseType.PostgreSql:
                default:
                    services.ConfigureNpgsql(GetConnectionString(settings, "IoTSharp"), settings.DbContextPoolSize, healthChecks, healthChecksUI);
                    break;
            }
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddRoles<IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();





            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = RequireSetting(settings.JwtIssuer, nameof(AppSettings.JwtIssuer)),
                    ValidAudience = RequireSetting(settings.JwtAudience, nameof(AppSettings.JwtAudience)),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(RequireSetting(settings.JwtKey, nameof(AppSettings.JwtKey)))),
                    //     ClockSkew=TimeSpan.Zero //JWT的缓冲时间默认5分钟，token实际过期时间为 appsettings.json 当中JwtExpireHours配置的时间（小时）加上这个时间。
                }; ;
            });

            services.AddCors();
            services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddRinLogger();
                    loggingBuilder.AddPrettyConsole();
                }
            );
            services.AddRin();
            services.AddOpenApiDocument(configure =>
            {
                Assembly assembly = typeof(Startup).GetTypeInfo().Assembly;
                var description = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                configure.Title = assembly.GetName().Name;
                configure.Version = assembly.GetName().Version.ToString();
                configure.Description = description?.Description;
                configure.AddJWTSecurity();
            });

            services.AddTransient<ApplicationDBInitializer>();
            services.AddIoTSharpMqttServer(settings.MqttBroker);
            services.AddMqttClient(settings.MqttClient);
            services.AddQuartz(q =>
            {
                q.DiscoverJobs();
            });

            // ASP.NET Core hosting
            services.AddQuartzServer(options =>
            {
                options.StartDelay = TimeSpan.FromSeconds(10);
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });
            services.AddResponseCompression();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
            });

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
                        options.UseLiteDB(cfg => cfg.DBConfig = new EasyCaching.LiteDB.LiteDBDBOptions() { }, name: _hc_Caching);
                        break;

                    case CachingUseIn.SonnetDB:
                        options.UseSonnetDB(config =>
                        {
                            config.ConnectionString = !string.IsNullOrWhiteSpace(settings.CachingUseSonnetDBConnectionString)
                                ? settings.CachingUseSonnetDBConnectionString
                                : GetConnectionString(settings, "TelemetryStorage") ?? string.Empty;
                            config.Keyspace = settings.CachingUseSonnetDBKeyspace;
                            config.Namespace = settings.CachingUseSonnetDBNamespace;
                        }, _hc_Caching);
                        break;

                    case CachingUseIn.InMemory:
                    default:
                        options.UseInMemory(_hc_Caching);
                        break;
                }
            });
            services.AddTelemetryStorage(settings, healthChecks);
            var zmqSection = Configuration.GetSection(nameof(ZMQOption));
            if (zmqSection.Exists())
            {
                services.AddHostedZeroMQ(options => zmqSection.Bind(options));
            }
            services.AddEventBus(opt =>
            {
                opt.AppSettings = settings;
                opt.EventBusStore = GetConnectionString(settings, "EventBusStore");
                opt.EventBusMQ = GetConnectionString(settings, "EventBusMQ");
                opt.HealthChecks = healthChecks;
                switch (settings.EventBus)
                {
                    case EventBusFramework.Shashlik:
                        throw new NotSupportedException(" EventBusFramework.Shashlik is not supported yet");
                    case EventBusFramework.SonnetMQ:
                        opt.UseSonnetMQ();
                        break;
                    case EventBusFramework.CAP:
                    default:
                        opt.UserCAP();
                        break;
                }
            });
            services.AddCoapServer(Configuration.GetSection("CoapServer"));
            services.AddIoTSharpCoapResources();
            services.AddTransient(_ =>
            {
                var blobStorage = GetConnectionString(settings, "BlobStorage");
                if (!string.IsNullOrWhiteSpace(blobStorage)
                    && blobStorage.StartsWith("sonnetdb://", StringComparison.OrdinalIgnoreCase))
                {
                    var parsed = SonnetDbBlobStorage.ParseConnectionString(blobStorage);
                    return new SonnetDbBlobStorage(parsed.ConnectionString, parsed.Bucket);
                }

                if (string.IsNullOrWhiteSpace(blobStorage))
                {
                    blobStorage = $"disk://path={Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.Create)}/IoTSharp/";
                }

                return StorageFactory.Blobs.FromConnectionString(blobStorage);
            });

            services.AddRazorPages();

            services.AddScriptEngines(Configuration.GetSection("EngineSetting"));
            services.AddTransient<FlowRuleProcessor>();
            services.AddTransient<CustomeAlarmPullExcutor>();
            services.AddSingleton<TaskExecutorHelper>();
            services.AddTransient<PublishAttributeDataTask>();
            services.AddTransient<PublishTelemetryDataTask>();
            services.AddTransient<PublishAlarmDataTask>();
            services.AddTransient<RawDataGateway>();
            services.AddTransient<KepServerEx>();

            if (hostOptions.IOTSHARP_ACME)
            {
                services.AddLettuceEncrypt()
                        .PersistDataToDirectory(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "security")), "kissme")
                        .Services.AddAliDnsChallengeProvider();
                services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(60);
                });
            }

            services.AddMcpServer()
             .WithHttpTransport(options =>
             {
                 options.ConfigureSessionOptions += async (context, serverOptions, token) =>
                 {
                     var api_key = context.Request.RouteValues["api_key"]?.ToString()?.ToLower() ?? "none";
                     serverOptions.InitializationTimeout = TimeSpan.FromSeconds(600);
                     serverOptions.Capabilities!.Experimental = new Dictionary<string, object>();
                     serverOptions.Capabilities.Experimental.Add("API_KEY", api_key);
                     await Task.CompletedTask;
                 };

                 options.Stateless = true;
             })
             .WithPromptsFromAssembly()
             .WithResourcesFromAssembly()
                .WithToolsFromAssembly();
        }

        private static string RequireSetting(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"{name} 未配置。");
            }

            return value;
        }

        private static string GetConnectionString(AppSettings settings, string name)
        {
            if (settings.ConnectionStrings == null)
            {
                return null;
            }

            if (settings.ConnectionStrings.TryGetValue(name, out var connectionString))
            {
                return connectionString;
            }

            return settings.ConnectionStrings
                .FirstOrDefault(item => string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
                .Value;
        }





        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if ((env.IsDevelopment() || !env.IsEnvironment("Production")) && !env.IsEnvironment("Test"))
            {
                app.UseRin();
                app.UseRinMvcSupport();
                app.UseDeveloperExceptionPage();
                app.UseRinDiagnosticsHandler();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.Map("/healthz", healthz =>
            {
                healthz.Run(async context =>
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"status\":\"Healthy\",\"totalDuration\":\"00:00:00\",\"entries\":{}}");
                });
            });

            app.CheckApplicationDBMigrations();
            //添加定时任务创建表

            app.UseRouting();
            app.UseCors(option => option
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseResponseCompression();
            app.UseIotSharpMqttServer();
            app.UseCoapServer();
            app.UseSwaggerUi();
            app.UseHealthChecksUI();
            app.UseOpenApi();
            app.UseEventBus(opt =>
            {
                var frp = app.ApplicationServices.GetService<FlowRuleProcessor>();
                return frp.RunRules;
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMqtt("/mqtt");
                endpoints.MapHealthChecks("/readyz", new HealthCheckOptions()
                {
                    Predicate = check => check.Tags.Contains("ready"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI();
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
                endpoints.MapMcp("/mcp/{api_key}");
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
            app.UseTelemetryStorage();

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".fbx"] = "application/octet-stream";
            provider.Mappings[".glb"] = "application/octet-stream";
            app.Use(async (context, next) =>
            {
                if (ShouldServeSpaFallback(context.Request))
                {
                    context.Request.Path = "/index.html";
                }

                await next();
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider,
            });
        }

        /// <summary>
        /// 判断请求是否应回退到 SPA 入口页；平台 API、管理端点和静态资源不参与前端路由回退。
        /// </summary>
        /// <param name="request">当前 HTTP 请求。</param>
        /// <returns>需要返回前端入口页时为 true。</returns>
        private static bool ShouldServeSpaFallback(HttpRequest request)
        {
            if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            {
                return false;
            }

            var path = request.Path;
            var pathValue = path.Value ?? string.Empty;
            if (Path.HasExtension(pathValue))
            {
                return false;
            }

            return !path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
                   && !path.StartsWithSegments("/cap", StringComparison.OrdinalIgnoreCase)
                   && !path.StartsWithSegments("/healthz", StringComparison.OrdinalIgnoreCase)
                   && !path.StartsWithSegments("/readyz", StringComparison.OrdinalIgnoreCase)
                   && !path.StartsWithSegments("/mcp", StringComparison.OrdinalIgnoreCase)
                   && !path.StartsWithSegments("/mqtt", StringComparison.OrdinalIgnoreCase)
                   && !path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase)
                   && !path.StartsWithSegments("/healthchecks-ui", StringComparison.OrdinalIgnoreCase);
        }

    }
}
