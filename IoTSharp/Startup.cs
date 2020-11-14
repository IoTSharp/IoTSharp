using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.Handlers;
using IoTSharp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MQTTnet.AspNetCore;
using MQTTnet.AspNetCoreEx;
using MQTTnet.Client;
using NSwag.AspNetCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Server;
using System.Runtime.InteropServices.ComTypes;
using SshNet.Security.Cryptography;
using SilkierQuartz;
using HealthChecks.UI.Client;
using NSwag;
using NSwag.Generation.Processors.Security;
using IoTSharp.Queue;
using Npgsql;
using EFCore.Sharding;
using IoTSharp.Storage;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using Savorboard.CAP.InMemoryMessageQueue;
using System.Diagnostics;
using EasyCaching.Core.Configurations;
using Silkier.Extensions;
using Maikebing.Data.Taos;
using Dynamitey;
using DotNetCore.CAP;
using RabbitMQ.Client;
using InfluxDB.Client;
using System.Security.Policy;
using Microsoft.CodeAnalysis.FlowAnalysis;

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

           var settings=  Configuration.Get<AppSettings>();
            services.Configure((Action<AppSettings>)(setting =>
            {
                Configuration.Bind(setting);
            }));
            services.AddDbContextPool<ApplicationDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("IoTSharp")), poolSize: settings.DbContextPoolSize);
            services.AddIdentity<IdentityUser, IdentityRole>()
                  .AddRoles<IdentityRole>()
                  .AddRoleManager<RoleManager<IdentityRole>>()
                 .AddDefaultTokenProviders()
                  .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddControllersWithViews();

            services.AddCors();

            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["JwtIssuer"],
                    ValidAudience = Configuration["JwtAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtKey"]))
                };
            });
     
       
          
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
          
            // Enable the Gzip compression especially for Kestrel
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression(options =>
                {
                    options.EnableForHttps = true;
                });



            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            services.AddOpenApiDocument(configure =>
            {
                Assembly assembly = typeof(Startup).GetTypeInfo().Assembly;
                var description = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                configure.Title = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
                configure.Version = typeof(Startup).GetTypeInfo().Assembly.GetName().Version.ToString();
                configure.Description = description?.Description;
                configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}." 
                });

                configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });
            services.AddTransient<ApplicationDBInitializer>();
            services.AddIoTSharpMqttServer(settings.MqttBroker);
            services.AddMqttClient(settings.MqttClient);
            services.AddSingleton<RetainedMessageHandler>();
           var healthChecks =  services.AddHealthChecks()
                 .AddNpgSql(Configuration.GetConnectionString("IoTSharp"), name: "PostgreSQL")
                 .AddDiskStorageHealthCheck(dso =>
                 {
                     System.IO.DriveInfo.GetDrives().Select(f=>f.Name).Distinct().ToList().ForEach(f => dso.AddDrive(f, 1024));

                 }, name: "Disk Storage");
            services.AddHealthChecksUI(cfg=>cfg.AddHealthCheckEndpoint("IoTSharp", "http://127.0.0.1/healthz"))
                    .AddPostgreSqlStorage(Configuration.GetConnectionString("IoTSharp"));
            services.AddSilkierQuartz(opt=>
            {
                //opt.Add("quartz.serializer.type", "json");
                //opt.Add("quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz");
                //opt.Add("quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz");
                //opt.Add("quartz.jobStore.tablePrefix", "qrtz_");
                //opt.Add("quartz.jobStore.dataSource", "myDS");
                //opt.Add("quartz.dataSource.myDS.provider", "Npgsql");
                //opt.Add("quartz.dataSource.myDS.connectionString", Configuration.GetConnectionString("IoTSharp"));
                opt.Add("quartz.plugin.recentHistory.type", "Quartz.Plugins.RecentHistory.ExecutionHistoryPlugin, Quartz.Plugins.RecentHistory");
                opt.Add("quartz.plugin.recentHistory.storeType", "Quartz.Plugins.RecentHistory.Impl.InProcExecutionHistoryStore, Quartz.Plugins.RecentHistory");
            });
            services.AddMemoryCache();
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
                                config.DBConfig.Endpoints.Add(new ServerEndPoint(hx[0],int.Parse(hx[1] )));
                            });
                        }, "iotsharp");
                        break;
                    case CachingUseIn.LiteDB:
                        options.UseLiteDB(cfg=>cfg.DBConfig=new EasyCaching.LiteDB.LiteDBDBOptions() {  } );
                        break;
                   case CachingUseIn.InMemory:
                    default:
                        options.UseInMemory("iotsharp");
                        break;
                }
              
            });
                switch (settings.TelemetryStorage)
            {
                case TelemetryStorage.Sharding:
                    services.AddEFCoreSharding(config =>
                    {
                        config.AddDataSource(Configuration.GetConnectionString("TelemetryStorage"), ReadWriteType.Read | ReadWriteType.Write, settings.Sharding.DatabaseType);
                        config.SetDateSharding<TelemetryData>(nameof(TelemetryData.DateTime),  settings.Sharding.ExpandByDateMode, DateTime.Now);
                    });
                    services.AddSingleton<IStorage, ShardingStorage>();
                    break;
                case TelemetryStorage.SingleTable:
                    services.AddSingleton<IStorage, EFStorage>();
                    break;
                case TelemetryStorage.Taos:
                    services.AddSingleton<IStorage, TaosStorage>();
                    services.AddObjectPool(()=>  new TaosConnection(settings.ConnectionStrings["TelemetryStorage"]));
                    healthChecks.AddTDengine(Configuration.GetConnectionString("TelemetryStorage"));
                    break;
                case TelemetryStorage.InfluxDB:
                    //https://github.com/julian-fh/influxdb-setup

                    services.AddSingleton<IStorage, InfluxDBStorage>();
                    //"TelemetryStorage": "http://localhost:8086/?org=iotsharp&bucket=iotsharp-bucket&token=iotsharp-token"
                    services.AddObjectPool(() => InfluxDBClientFactory.Create(Configuration.GetConnectionString("TelemetryStorage")));
                    break;
                case TelemetryStorage.InfluxDBV1:
                    //docker run -d -p 8083:8083 -p8086:8086 --expose 8090 --expose 8099 --name influxsrv tutum/influxdb
                    services.AddSingleton<IStorage, InfluxDBV1Storage>();
                    services.AddObjectPool(() => InfluxDBClientFactory.Create(Configuration.GetConnectionString("TelemetryStorage")));
                    break;
                default:
                    break;
            }
           
            services.AddTransient<IEventBusHandler,  EventBusHandler>();
            if (settings.ZMQOption!=null)
            {
                services.AddHostedZeroMQ(cfg=>cfg= settings.ZMQOption);
            }
            services.AddCap(x =>
            {
                x.ConsumerThreadCount = settings.ConsumerThreadCount  ;
                switch (settings.EventBusStore)
                {
                    case EventBusStore.PostgreSql:
                        x.UsePostgreSql(Configuration.GetConnectionString("EventBusStore"));
                        healthChecks.AddNpgSql(Configuration.GetConnectionString("EventBusStore"),name: "EventBusStore");
                        break;
                    case EventBusStore.MongoDB:
                        x.UseMongoDB(Configuration.GetConnectionString("EventBusStore"));  //注意，仅支持MongoDB 4.0+集群
                        healthChecks.AddMongoDb(Configuration.GetConnectionString("EventBusStore"),name: "EventBusStore");
                        break;
                    case EventBusStore.LiteDB:
                        x.UseLiteDBStorage(Configuration.GetConnectionString("EventBusStore"));
                        break;
                    case EventBusStore.InMemory:
                    default:
                        x.UseInMemoryStorage();
                        break;
                }
                switch (settings.EventBusMQ)
                {
                  
                    case EventBusMQ.RabbitMQ:
                        x.UseRabbitMQ(new Uri(  Configuration.GetConnectionString("EventBusMQ")));
                        //amqp://guest:guest@localhost:5672
                        healthChecks.AddRabbitMQ(Configuration.GetConnectionString("EventBusMQ"), name: "EventBusMQ");
                        break;
                    case EventBusMQ.Kafka:
                        //BootstrapServers
                        x.UseKafka( Configuration.GetConnectionString("EventBusMQ"));
                        healthChecks.AddKafka( cfg=>
                        {
                            cfg.BootstrapServers = Configuration.GetConnectionString("EventBusMQ");
                        } , name: "EventBusMQ");
                        break;
                    case EventBusMQ.ZeroMQ:
                        x.UseZeroMQ(cfg =>
                        {
                            cfg.HostName = Configuration.GetConnectionString("EventBusMQ")??"127.0.0.1";
                            cfg.Pattern = MaiKeBing.CAP.NetMQPattern.PushPull;
                        });
                        break;
                    case EventBusMQ.InMemory:
                    default:
                        x.UseInMemoryMessageQueue();
                        break;
                }
                // Register Dashboard
                x.UseDashboard();
                // Register to Consul
                if (settings.Discovery!=null)
                {
                    x.UseDiscovery(cfg => cfg=settings.Discovery);
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISchedulerFactory factory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors(option => option
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSilkierQuartz(new  SilkierQuartzOptions()
            {
                Scheduler = factory.GetScheduler().Result,
                VirtualPathRoot = "/quartzmin",
                ProductName = "IoTSharp",
                DefaultDateFormat = "yyyy-MM-dd",
                DefaultTimeFormat = "HH:mm:ss",
                UseLocalTime = true
            });
            app.UseIotSharpMqttServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMqtt("/mqtt");
            });
            app.UseSwaggerUi3();
            app.UseOpenApi();

            app.UseHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = (httpContext, report) =>
                {
                    return UIResponseWriter.WriteHealthCheckUIResponse(httpContext, report);
                }
            });
            app.UseHealthChecksUI();


            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseResponseCompression(); // No need if you use IIS, but really something good for Kestrel!
        }
    }
}