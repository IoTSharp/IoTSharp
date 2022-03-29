using Ductus.FluentDocker;
using Ductus.FluentDocker.Extensions;
using Ductus.FluentDocker.Model.Common;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;
using Ductus.FluentDocker.Services.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ductus.FluentDocker.Builders;
namespace IoTSharp.Test
{

    [TestClass]
    public class TestIoTSharp
    {

        string fullPath = "";
      [TestInitialize]
        public void Initialize()
        {
            fullPath = System.IO.Path.GetFullPath((TemplateString)@"${PWD}../../../../../");
        }

        [TestMethod]
        public void Test()

        {
         
            using (var services = new Builder()

              .DefineImage("test/iotsharp").ReuseIfAlreadyExists()
                    .FromFile($"{fullPath}IoTSharp/Dockerfile").ExposePorts(2927).WorkingFolder(fullPath).Builder()
               .UseContainer().WithName("pgsql").UseImage("postgres").ExposePort(5432).WaitForProcess("postgres", 30000 /*30s*/)
                    .WithEnvironment("POSTGRES_USER=postgres", "POSTGRES_DB=IoTSharp", "POSTGRES_PASSWORD=future", "TZ=Asia/Shanghai").Builder()
               .UseContainer().WithName("influx").UseImage("quay.io/influxdb/influxdb:v2.0.4").ExposePort(8086)
                    .Command("influxd run --bolt-path /var/lib/influxdb2/influxd.bolt --engine-path /var/lib/influxdb2/engine --store bolt")
                    .WaitForProcess("influxd", 30000 /*30s*/)
                    .WithEnvironment("TZ=Asia/Shanghai").Builder()
                .UseContainer().WithName("influxdb_cli").UseImage("quay.io/influxdb/influxdb:v2.0.4").Link("influx")
                    .Command("influx setup --bucket iotsharp-bucket -t koMHcRQ9PJVPIIb6zFyLt-06EkM_oy1jut08bH7f0BwC85LUO6zxgihKZUayHzyetapJEkxrlO0KwJ278dKpnA== -o iotsharp --username=root --password=1-q2-w3-e4-r5-t --host=http://influx:8086 -f")
                    .WithEnvironment("TZ=Asia/Shanghai").Builder()
            .UseContainer().WithName("iotsharp").UseImage("test/iotsharp").Link("pgsql", "influx")
            .ExposePort(2927,80)
            .Builder()
              .Build().Start())
            {
                Assert.AreEqual(3, services.Containers.Count);
                var ep = services.Containers.First(x => x.Name == "iotsharp").ToHostExposedEndpoint("80/tcp");
                Assert.IsNotNull(ep);

                var round1 = $"http://{ep.Address}:{ep.Port}".Wget();
                Assert.AreEqual("This page has been viewed 1 times!", round1);

                var round2 = $"http://{ep.Address}:{ep.Port}".Wget();
                Assert.AreEqual("This page has been viewed 2 times!", round2);
            }


        }
    }
}
