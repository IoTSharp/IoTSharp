using IoTSharp.Controllers;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Test
{
    [TestClass]
    public class InstallerTest
    {
        private TestServer _server;
        private HttpClient _client;
        private InstallerController _installer;
        private InstallDto _installdto;

        [TestInitialize()]
        public async void TestServerInitialize()
        {
            _server = new TestServer((IWebHostBuilder)Program.CreateHostBuilder(null));
            _client = _server.CreateClient();
            _installer = _server.Services.GetService<InstallerController>();
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
            if (!_installer.Instance().Data.Installed)
            {
                await _installer.Install(_installdto);
            }
        }

        [TestMethod]
        public  void  IsInstalled()
        {
            var installer = _server.Services.GetService<InstallerController>();
            var status = installer.Instance();
            Assert.IsTrue(status.Data.Installed);
        }

        [TestMethod]
        public async Task AccountLogin()
        {
            var account = _server.Services.GetService<AccountController>();
            var status =await account.Login(new Dtos.LoginDto() { UserName = _installdto.Email, Password = _installdto.Password });
            Assert.IsTrue(status.Data.Succeeded);
            Assert.IsNotNull(status.Data.Token.access_token);
        }
    }
}
