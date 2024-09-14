using Alba;
using IoTSharp.Contracts;
using IoTSharp.Controllers;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace IoTSharp.Test
{
    [TestClass]
    public class InstallerTest
    {
        IAlbaHost _host;
        PostgreSqlContainer _postgreSqlContainer;
        private InstallDto _installdto;
        private static TestContext _context;
        private CancellationToken _ct;

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            _context = context;

        }
        [TestInitialize()]
        public async Task TestServerInitialize()
        {
            _ct = _context.CancellationTokenSource.Token;
            _postgreSqlContainer = new PostgreSqlBuilder().Build();
            await _postgreSqlContainer.StartAsync(_ct);
            _host = await AlbaHost.For<IoTSharp.Program>(x =>
            {
                x.UseEnvironment("Test");
                x.UseSetting("DataBase", nameof(DataBaseType.PostgreSql))
                .UseSetting("EventBusStore", nameof(EventBusStore.InMemory))
                .UseSetting("TelemetryStorage", nameof(TelemetryStorage.Sharding))
                .UseSetting("EventBus", "Shashlik")
                .UseSetting("ConnectionStrings:IoTSharp", _postgreSqlContainer.GetConnectionString())
                .UseSetting("ConnectionStrings:TelemetryStorage", _postgreSqlContainer.GetConnectionString())
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
        public async Task IsInstalled()
        {
            using var _client = _host.GetTestClient();
            var result = await _client.GetFromJsonAsync<ApiResult<InstanceDto>>("/api/Installer/Instance", _ct);
            Assert.AreEqual(result.Code, (int)ApiCode.Success);
            Assert.IsTrue(result.Data.Installed);
        }

        [TestMethod]
        public async Task AccountLogin()
        {
            using var _client = _host.GetTestClient();


            var dto = new Dtos.LoginDto() { UserName = _installdto.Email, Password = _installdto.Password };
            var result = await _client.PostAsJsonAsync("/api/Account/Login", dto, _ct);
            Assert.IsTrue(result.IsSuccessStatusCode);
            var rdto = await result.Content.ReadFromJsonAsync<ApiResult<LoginResult>>(_ct);
            Assert.IsTrue(rdto.Data.Succeeded);
            Assert.AreEqual(rdto.Data.UserName, _installdto.Email);
        }
        [TestCleanup]
        public async Task Cleanup()
        {
            await _postgreSqlContainer.DisposeAsync();
            await _host.StopAsync(_ct);
        }
    }
}
