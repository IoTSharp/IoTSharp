using DotNetCore.CAP;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.Services;
using IoTSharp.X509Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
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
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace IoTSharp
{
    public static class IoTSharpExtension
    {
        /// <summary>
        /// 根据用户信息填写表里面的内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_context"></param>
        /// <param name="controller"></param>
        /// <param name="ak"></param>
        public static void JustFill<T>(this ApplicationDbContext _context, ControllerBase controller,   T ak) where T : class, IJustMy
        {
            var cid = controller.User.Claims.First(c => c.Type == IoTSharpClaimTypes.Customer);
            var tid = controller. User.Claims.First(c => c.Type == IoTSharpClaimTypes.Tenant);
            ak.Tenant = _context.Tenant.Find(new Guid(tid.Value));
            ak.Customer = _context.Customer.Find(new Guid(cid.Value));
        }
        /// <summary>
        /// 查询当前客户的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static IQueryable<T> JustCustomer<T>(this DbSet<T> ts, ControllerBase controller) where T : class, IJustMy 
            => JustCustomer(ts,controller.User.GetCustomerId());
        /// <summary>
        /// 查询指定客户的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="_customerId"></param>
        /// <returns></returns>
        public static IQueryable<T> JustCustomer<T>(this DbSet<T> ts, Guid _customerId) where T : class, IJustMy
        {
            return ts.Include(ak => ak.Customer).Where(ak => ak.Customer.Id == _customerId);
        }
        /// <summary>
        /// 查询当前用户所在租户的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static IQueryable<T> JustTenant<T>(this DbSet<T> ts, ControllerBase controller) where T : class, IJustMy 
            => JustTenant(ts, controller.User.GetTenantId());
        /// <summary>
        /// 查询指定租户的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="_tenantId"></param>
        /// <returns></returns>
        public static IQueryable<T> JustTenant<T>(this DbSet<T> ts, Guid _tenantId) where T : class, IJustMy
        {
            return ts.Include(ak => ak.Tenant).Where(ak => ak.Tenant.Id == _tenantId);
        }
        /// <summary>
        /// 获取指定客户信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="custId"></param>
        /// <returns></returns>
        public static Customer GetCustomer(this ApplicationDbContext context, Guid custId) 
            => context.Customer.Include(c => c.Tenant).FirstOrDefault(c => c.Id  ==  custId);
      /// <summary>
      /// 获取指定的租户信息
      /// </summary>
      /// <param name="context"></param>
      /// <param name="tenId"></param>
      /// <returns></returns>
        public static Tenant GetTenant(this ApplicationDbContext context, Guid tenId)
           => context.Tenant.FirstOrDefault(c => c.Id == tenId);

       /// <summary>
       /// 获取当前用户的邮箱
       /// </summary>
       /// <param name="_user"></param>
       /// <returns></returns>
        public static string GetEmail(this ClaimsPrincipal _user) => _user.FindFirstValue(ClaimTypes.Email);
        /// <summary>
        /// 获取当前用户的ID
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static Guid  GetUserId(this ClaimsPrincipal _user)
        {
            return  Guid.Parse( _user.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        /// <summary>
        /// 获取当前用户的ID
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static Guid GetTenantId(this ClaimsPrincipal _user)
        {
            return  Guid.Parse( _user.FindFirstValue(IoTSharpClaimTypes.Tenant));
        }
        /// <summary>
        /// 获取当前用户的客户ID
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static Guid GetCustomerId(this ClaimsPrincipal _user)
        {
            return Guid.Parse(_user.FindFirstValue(IoTSharpClaimTypes.Customer));
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

        public static void CreateBrokerTlsCert(this X509Certificate2 CACertificate, string domainname, IPAddress iP, string pubfile, string pivfile,string email)
        {
            var build = new SubjectAlternativeNameBuilder();
            build.AddDnsName(domainname);
            build.AddIpAddress(iP);
            build.AddEmailAddress(email??"mysticboy@live.com");
            string name = $"C=CN,CN={iP.ToString()},ST=IoTSharp,O={Dns.GetHostName()},OU= CA_{MethodBase.GetCurrentMethod().Module.Assembly.GetName().Name}";
            var broker = CACertificate.CreateTlsClientRSA(name, build);
            broker.SavePem(pubfile, pivfile);
        }

        public static void LoadCAToRoot(this X509Certificate2 CACertificate)
        {
            //https://stackoverflow.com/questions/3625624/inserting-certificate-with-privatekey-in-root-localmachine-certificate-store?lq=1
            X509Store x509 = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            if (!x509.Certificates.Contains(CACertificate))
            {
                x509.Open(OpenFlags.MaxAllowed);
                x509.Add(CACertificate);
                x509.Close();
            }
        }


        public static X509Certificate2 CreateCA(this string Domain, string capubfile, string capivfile)
        {
            var ca = new X509Certificate2().CreateCA($"C=CN,CN={Domain},ST=IoTSharp,O={Dns.GetHostName()},OU=CA_{MethodBase.GetCurrentMethod().Module.Assembly.GetName().Name}");
            ca.SavePem(capubfile, capivfile);
            return ca;
        }
        public static X509Certificate2 CreateCA(this IPAddress ip, string capubfile, string capivfile)
        {
            var ca = new X509Certificate2().CreateCA($"C=CN,CN={ip},ST=IoTSharp,O={Dns.GetHostName()},OU=CA_{MethodBase.GetCurrentMethod().Module.Assembly.GetName().Name}");
            ca.SavePem(capubfile, capivfile);
            return ca;
        }

        public static string GetDefaultIPAddress()
        {
            var infos = GetDefaultAddressInfos();
            var add = from address in Dns.GetHostAddresses(Dns.GetHostName()) where infos.ContainsValue(address.ToString()) select address.ToString();
            return add.FirstOrDefault();
        }

        public static bool IPAddressInUse(this IPAddress _address)
        {
            var infos = GetDefaultAddressInfos();
            var add = from address in infos where address.Value == _address.ToString() select address;
            return add.Any();
        }

        public static string GetDefaultMacAddress()
        {
            var ipaddress = GetDefaultIPAddress();
            var infos = from mac in GetDefaultAddressInfos() where mac.Value == ipaddress select mac.Key;
            return infos.FirstOrDefault();
        }

        public static Dictionary<string, string> GetDefaultAddressInfos()
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            var ip = from nic in NetworkInterface.GetAllNetworkInterfaces()
                     let searchSub = from p in nic.GetIPProperties().UnicastAddresses
                                     where p.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(p.Address)
                                     select p
                     where
                     nic.OperationalStatus == OperationalStatus.Up
                     && (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet || nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                     && searchSub.Any() // && nic.GetIPProperties().GatewayAddresses.Count>0
                     select new { PhysicalAddress = nic.GetPhysicalAddress().ToString(), IPAddress = (searchSub).FirstOrDefault()?.Address.ToString() };
            ip.ToList().ForEach(ipx => pairs.Add(ipx.PhysicalAddress, ipx.IPAddress));
            return pairs;
        }
        internal static void BuildFlowOperation(this FlowOperation end, FlowOperation peroperation, Flow flow)
        {
            end.FlowRule = new FlowRule() { RuleId = peroperation.BaseEvent.FlowRule.RuleId };
            end.BaseEvent = new BaseEvent() { EventId = peroperation.BaseEvent.EventId };
            end.Flow = new Flow() { FlowId = flow.FlowId };
        }
        /// <summary>
        /// 创建网关的子设备。 
        /// </summary>
        /// <param name="device">父设备</param>
        /// <param name="devname">子设备名称</param>
        /// <param name="_scopeFactor"></param>
        /// <param name="_logger"></param>
        /// <returns></returns>
        internal static Device JudgeOrCreateNewDevice(this  Device device ,string devname, IServiceScopeFactory _scopeFactor, ILogger _logger)
        {
            Device devicedatato = null;
            using (var scope = _scopeFactor.CreateScope())
            using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
            {

                if (devname != "me" && device.DeviceType == DeviceType.Gateway)
                {
                    var ch = from g in _dbContext.Gateway.Include(g => g.Tenant).Include(g => g.Customer).Include(c => c.Children) where g.Id == device.Id select g;
                    var gw = ch.FirstOrDefault();
                    var subdev = from cd in gw.Children where cd.Name == devname select cd;
                    if (!subdev.Any())
                    {
                        devicedatato = new Device() { Id = Guid.NewGuid(), Name = devname, DeviceType = DeviceType.Device, Tenant = gw.Tenant, Customer = gw.Customer, Owner = gw, LastActive = DateTime.Now, Timeout = 300 };
                        gw.Children.Add(devicedatato);
                        _dbContext.AfterCreateDevice(devicedatato);
                        _logger.LogInformation($"网关 {gw.Id}-{gw.Name}在线.最后活动时间{gw.LastActive},添加了子设备{devicedatato.Name}");
                    }
                    else
                    {
                        devicedatato = subdev.FirstOrDefault();
                        _logger.LogInformation($"网关子设备 {devicedatato.Id}-{devicedatato.Name}在线.最后活动时间{devicedatato.LastActive}");
                    }
                }
                else
                {
                    devicedatato = _dbContext.Device.Find(device.Id);
                    _logger.LogInformation($"独立设备 {devicedatato.Id}-{devicedatato.Name}在线.最后活动时间{devicedatato.LastActive}");
                }
                _dbContext.SaveChanges();
            }
            return devicedatato;
        }
    }
}