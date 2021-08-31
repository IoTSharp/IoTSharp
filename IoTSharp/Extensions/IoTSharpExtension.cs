using DotNetCore.CAP;
using IoTSharp.Data;
using IoTSharp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics;
using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors.Security;
using Silkier.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace IoTSharp
{
    public static class IoTSharpExtension
    {

        public static void JustFill<T>(this ApplicationDbContext _context, ControllerBase controller,   T ak) where T : class, IJustMy
        {
            var cid = controller.User.Claims.First(c => c.Type == IoTSharpClaimTypes.Customer);
            var tid = controller. User.Claims.First(c => c.Type == IoTSharpClaimTypes.Tenant);
            ak.Tenant = _context.Tenant.Find(new Guid(tid.Value));
            ak.Customer = _context.Customer.Find(new Guid(cid.Value));
        }

        public static IQueryable<T> JustCustomer<T>(this DbSet<T> ts, ControllerBase controller) where T : class, IJustMy 
            => JustCustomer(ts, GetCustomerId(controller));
        public static IQueryable<T> JustCustomer<T>(this DbSet<T> ts, string _customerId) where T : class, IJustMy
        {
            return ts.Include(ak => ak.Customer).Where(ak => ak.Customer.Id.ToString() == _customerId);
        }

        public static IQueryable<T> JustTenant<T>(this DbSet<T> ts, ControllerBase controller) where T : class, IJustMy 
            => JustTenant(ts, GetTenantId(controller));

        public static IQueryable<T> JustTenant<T>(this DbSet<T> ts, string _tenantId) where T : class, IJustMy
        {
            return ts.Include(ak => ak.Tenant).Where(ak => ak.Tenant.Id.ToString() == _tenantId);
        }
        public static Customer GetCustomer(this ApplicationDbContext context, string custid) 
            => context.Customer.Include(c => c.Tenant).FirstOrDefault(c => c.Id  == Guid.Parse( custid));

        public static string GetCustomerId(this ControllerBase controller)
        {
            string custid = controller.User?.FindFirstValue(IoTSharpClaimTypes.Customer);
            return custid;
        }
        public static string GetTenantId(this ControllerBase controller)
        {
            string custid = controller.User.FindFirstValue(IoTSharpClaimTypes.Tenant);
            return custid;
        }
        public static IHostBuilder ConfigureIoTSharpHost(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                services.AddHostedService<CoAPService>();
            });
            return hostBuilder;
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
            app.UseOpenApi(config => config.PostProcess = (document, request) =>
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
     
        /// <summary>
        /// //如果上次活动时间距离当前时间超过10秒 或者 设备离线状态， 则更新状态。
        /// </summary>
        /// <param name="device"></param>
        internal static void CheckOrUpdateDevStatus(this Device device)
        {
            if (DateTime.Now.Subtract(device.LastActive).TotalSeconds > 10 || device.Online == false)
            {
                device.Online = true;
                device.LastActive = DateTime.Now;
            }
        }

        internal static HealthChecks.UI.Configuration.Settings AddIoTSharpHealthCheckEndpoint(this HealthChecks.UI.Configuration.Settings setup)
        {
            var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(';');
            var uris = urls?.Select(url => Regex.Replace(url, @"^(?<scheme>https?):\/\/((\+)|(\*)|(0.0.0.0))(?=[\:\/]|$)", "${scheme}://localhost"))
                            .Select(uri => new Uri(uri, UriKind.Absolute)).ToArray();
            var httpEndpoint = uris?.FirstOrDefault(uri => uri.Scheme == "http");
            var httpsEndpoint = uris?.FirstOrDefault(uri => uri.Scheme == "https");
            if (httpEndpoint != null) // Create an HTTP healthcheck endpoint
            {
                setup.AddHealthCheckEndpoint("IoTSharp", new UriBuilder(httpEndpoint.Scheme, httpEndpoint.Host, httpEndpoint.Port, "/healthz").ToString());
            }
            else if (httpsEndpoint != null) // Create an HTTPS healthcheck endpoint
            {
                setup.AddHealthCheckEndpoint("IoTSharp", new UriBuilder(httpsEndpoint.Scheme, httpsEndpoint.Host, httpsEndpoint.Port, "/healthz").ToString());
            }
            else
            {
                //One endpoint is configured in appsettings, let's add another one programatically
                setup.AddHealthCheckEndpoint("IoTSharp", "/healthz");
            }
            return setup;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configure"></param>
        internal static void AddJWTSecurity(this AspNetCoreOpenApiDocumentGeneratorSettings configure)
        {
            configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        }
    }
}