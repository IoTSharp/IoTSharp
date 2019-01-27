using IoT.Sharp.Sdk.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTSharp.Test
{
    [TestClass]
    public class UnitTestInstaller
    {
        [TestInitialize]
        public void TestInit()
        {
            SdkClient.BaseURL = "http://localhost:51498";
        }

        [TestMethod, Priority(1)]
        public void TestInstance()
        {
            var Client = SdkClient.Create<InstallerClient>();
            var fr = Client.InstallAsync(new InstallDto()
            {
                CustomerEMail = "iotmaster@iotsharp.net",
                CustomerName = "iotsharp",
                TenantName = "iotsharp",
                TenantEMail = "iotmaster@iotsharp.net",
                PhoneNumber = "400000000",
                Email = "iotmaster@iotsharp.net",
                Password = "P@ssw0rd"
            }).GetAwaiter().GetResult();
            Assert.IsTrue(fr.Installed);
        }
    }
}