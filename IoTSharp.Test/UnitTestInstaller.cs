using IoTSharp;
using IoTSharp.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IoTSharp.Controllers.InstallerController;
using Microsoft.Extensions.DependencyInjection;

namespace IoTSharp.Test
{
    [TestClass]
    public class UnitTestInstaller
    {
        private InstallerController _installerController;

        [TestInitialize]
        public void TestInit()
        {
            _installerController = Program.CreateWebHostBuilder(new string[] { }).Build().Services.GetService<InstallerController>();
        }

        [TestMethod]
        public void TestInstall()
        {
            var fr = _installerController.Install(new InstallDto()
            {
                CustomerEMail = "iotmaster@iotsharp.net",
                CustomerName = "iotsharp",
                TenantName = "iotsharp",
                TenantEMail = "iotmaster@iotsharp.net",
                PhoneNumber = "400000000",
                Email = "iotmaster@iotsharp.net",
                Password = "P@ssw0rd"
            }).GetAwaiter().GetResult().Value;
            Assert.IsTrue(fr.Installed);
        }
    }
}