using DotNetCore.CAP;
using IoTSharp.Data;
using IoTSharp.Services;
using IoTSharp.X509Extensions;
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

        public static void JustFill<T>(this ApplicationDbContext _context, ControllerBase controller,   T ak) where T : class, IJustMy
        {
            var cid = controller.User.Claims.First(c => c.Type == IoTSharpClaimTypes.Customer);
            var tid = controller. User.Claims.First(c => c.Type == IoTSharpClaimTypes.Tenant);
            ak.Tenant = _context.Tenant.Find(new Guid(tid.Value));
            ak.Customer = _context.Customer.Find(new Guid(cid.Value));
        }

        public static IQueryable<T> JustCustomer<T>(this DbSet<T> ts, ControllerBase controller) where T : class, IJustMy 
            => JustCustomer(ts, GetNowUserCustomerId(controller));
        public static IQueryable<T> JustCustomer<T>(this DbSet<T> ts, string _customerId) where T : class, IJustMy
        {
            return ts.Include(ak => ak.Customer).Where(ak => ak.Customer.Id.ToString() == _customerId);
        }

        public static IQueryable<T> JustTenant<T>(this DbSet<T> ts, ControllerBase controller) where T : class, IJustMy 
            => JustTenant(ts, GetNowUserTenantId(controller));

        public static IQueryable<T> JustTenant<T>(this DbSet<T> ts, string _tenantId) where T : class, IJustMy
        {
            return ts.Include(ak => ak.Tenant).Where(ak => ak.Tenant.Id.ToString() == _tenantId);
        }
        public static Customer GetCustomer(this ApplicationDbContext context, string custid) 
            => context.Customer.Include(c => c.Tenant).FirstOrDefault(c => c.Id  == Guid.Parse( custid));
        public static Tenant GetTenant(this ApplicationDbContext context, string custid)
           => context.Tenant.FirstOrDefault(c => c.Id == Guid.Parse(custid));

        public static string GetNowUserCustomerId(this ControllerBase controller)
        {
            string custid = controller.User?.FindFirstValue(IoTSharpClaimTypes.Customer);
            return custid;
        }
       
        public static string GetNowUserTenantId(this ControllerBase controller)
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

    }
}