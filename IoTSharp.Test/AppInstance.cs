using Alba;
using DotNet.Testcontainers.Containers;
using IoTSharp.Contracts;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.Http;
using IoTSharp.Data;
using System.Net.Http;
namespace IoTSharp.Test
{
    public class AppInstance
    {
        internal IAlbaHost _host;
        internal InstallDto _installdto;
        internal static TestContext _context;
        internal CancellationToken _ct;
        [ClassInitialize]
        internal static void  FixtureSetup(TestContext context)
        {
            _context = context;
        }
        public async Task AppInitialize(string db_main, string db_telemetry, DataBaseType db_type) =>
                    await AppInitialize(db_main, db_telemetry, db_type, TelemetryStorage.Sharding, EventBusStore.InMemory);
        public async Task AppInitialize(string db_main, string db_telemetry, DataBaseType db_type, TelemetryStorage telemetry ) =>
                 await AppInitialize(db_main, db_telemetry, db_type,   telemetry, EventBusStore.InMemory);

        public async Task AppInitialize( string db_main,string db_telemetry, DataBaseType db_type, TelemetryStorage telemetry, EventBusStore eventBus)
        {
            _ct = _context.CancellationTokenSource.Token;
            _host = await AlbaHost.For<IoTSharp.Program>(x =>
            {
                x.UseEnvironment("Test");
                x.UseSetting("DataBase", Enum.GetName( db_type))
                .UseSetting("EventBusStore", Enum.GetName(eventBus))
                .UseSetting("TelemetryStorage", Enum.GetName(telemetry))
                .UseSetting("EventBus", "Shashlik")
                .UseSetting("ConnectionStrings:IoTSharp", db_main)
                .UseSetting("ConnectionStrings:TelemetryStorage", db_telemetry)
                .ConfigureLogging(cfg => cfg.AddMXLogger(_context.WriteLine));

            });
            var _client = _host.GetTestClient();
            _installdto = new Dtos.InstallDto()
            {
                CustomerEMail = "customer@iotsharp.net",
                CustomerName = "customer_test",
                Email = "admin@iotsharp.net",
                Password = "P@ssw0rd",
                PhoneNumber = "18900000000",
                TenantEMail = "tenant@iotsharp.net",
                TenantName = "tenant_test"
            };
            var result = await _client.PostAsJsonAsync("/api/Installer/Install", _installdto, _ct);
            Assert.IsTrue(result.IsSuccessStatusCode);
            var v = await result.Content.ReadFromJsonAsync<ApiResult<InstanceDto>>(_ct);
            Assert.IsTrue(v.Data.Installed);
        }

        [TestMethod]
        public async Task AppIsInstalled()
        {
            using var _client = _host.GetTestClient();
            var result = await _client.GetFromJsonAsync<ApiResult<InstanceDto>>("/api/Installer/Instance", _ct);
            Assert.AreEqual(result.Code, (int)ApiCode.Success);
            Assert.IsTrue(result.Data.Installed);
        }
       
        [TestMethod]
        public async Task AppAccountLogin()
        {
            using var _client = _host.GetTestClient();
            var rdto = await _Login(_client);
            Assert.IsTrue(rdto.Data.Succeeded);
            Assert.AreEqual(rdto.Data.UserName, _installdto.Email);
        }
        ApiResult<LoginResult> loginResult = null;
        private async Task<ApiResult<LoginResult>> _Login(HttpClient _client)
        {
            if (loginResult == null)
            {
                var dto = new Dtos.LoginDto() { UserName = _installdto.Email, Password = _installdto.Password };
                var result = await _client.PostAsJsonAsync("/api/Account/Login", dto, _ct);
                Assert.IsTrue(result.IsSuccessStatusCode);
                var rdto = await result.Content.ReadFromJsonAsync<ApiResult<LoginResult>>(_ct);
                Assert.IsTrue(rdto?.Data?.Succeeded);
                Assert.AreEqual(rdto?.Data?.UserName, _installdto.Email);
                Assert.IsNotNull(rdto?.Data?.Token?.access_token);
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {rdto.Data.Token.access_token}");
                loginResult = rdto;
            }
            return loginResult;
        }

        [TestMethod]
        public async Task AppDevicesCreate()
        {
            using var _client = _host.GetTestClient();
            var rdto = await _Login(_client);
            var dev = new DevicePostDto() { DeviceType = DeviceType.Device, Name = "test", Timeout = 30 };
            var  devresult = await _client.PostAsJsonAsync("/api/Devices", dev, _ct);
            var devx = await devresult.Content.ReadFromJsonAsync<ApiResult<Device>>(_ct);
            Assert.AreEqual(devx.Data.Name, "test");
            Assert.AreEqual(devx.Code, (int)ApiCode.Success);
       
        }
        public async Task AppCleanup()
        {
            await _host.StopAsync(_ct);
        }
    
    }
}
