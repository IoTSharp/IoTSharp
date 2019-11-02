using CrystalQuartz.AspNetCore;
using IoTSharp.Data;
using IoTSharp.Diagnostics;
using IoTSharp.Extensions;
using IoTSharp.Handlers;
using IoTSharp.MQTT;
using IoTSharp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SpaServices.VueCli;
using Microsoft.AspNetCore.SpaServices.Webpack;
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
using QuartzHostedService;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
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
            settings = Configuration.Get<AppSettings>();
            if (settings.MqttBroker == null) settings.MqttBroker = new MqttBrokerSetting();
            if (settings.MqttClient == null) settings.MqttClient = new MqttClientSetting();
            if (string.IsNullOrEmpty(settings.MqttClient.MqttBroker)) settings.MqttClient.MqttBroker = "built-in";
            if (string.IsNullOrEmpty(settings.MqttClient.Password)) settings.MqttClient.Password = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(settings.MqttClient.UserName)) settings.MqttClient.UserName = Guid.NewGuid().ToString();
            if (settings.MqttClient.Port == 0) settings.MqttClient.Port = 1883;
        }
        private AppSettings settings;
        public IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure((Action<AppSettings>)(setting =>
            {
                Configuration.Bind(setting);
                setting.MqttBroker = settings.MqttBroker;
                setting.MqttClient = settings.MqttClient;
            }));
            services.AddEntityFrameworkNpgsql();
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("IoTSharp")), ServiceLifetime.Transient);
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
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
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

            services.AddSwaggerDocument(configure =>
            {
                Assembly assembly = typeof(Startup).GetTypeInfo().Assembly;
                var description = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                configure.Title = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
                configure.Version = typeof(Startup).GetTypeInfo().Assembly.GetName().Version.ToString();
                configure.Description = description?.Description;
            });
            services.AddTransient<ApplicationDBInitializer>();
            services.AddIoTSharpMqttServer(settings.MqttBroker);
            services.AddMqttClient(settings.MqttClient);
      
            services.AddSingleton<DiagnosticsService>();
            services.AddSingleton<RetainedMessageHandler>();
            services.AddSingleton<RuntimeStatusHandler>();

            services.AddQuartzHostedService();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISchedulerFactory factory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwaggerUi3();
            app.UseOpenApi();
            app.UseCrystalQuartz(() => factory.GetScheduler().Result);
            app.UseIotSharpMqttServer();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseResponseCompression(); // No need if you use IIS, but really something good for Kestrel!

            // Idea: https://code.msdn.microsoft.com/How-to-fix-the-routing-225ac90f
            // This avoid having a real mvc view. You have other way of doing, but this one works
            // properly.
            app.UseSpaStaticFiles();
 
           
            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";
                spa.Options.StartupTimeout = TimeSpan.FromSeconds(80);
                if (env.IsDevelopment())
                {
                    spa.UseVueCliServer(npmScript: "dev");
                }
            });
            app.UseIotSharpSelfCollecting();
        }
    }
}