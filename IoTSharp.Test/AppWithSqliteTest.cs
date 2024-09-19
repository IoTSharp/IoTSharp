using Alba;
using DotNet.Testcontainers.Containers;
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

namespace IoTSharp.Test
{
    [TestClass]
    public class AppWithSqliteTest : AppInstance
    {
      
        [ClassInitialize]
        public static void TextFixtureSetup(TestContext context) => FixtureSetup(context);



        [TestInitialize()]
        public async Task TestServerInitialize()
        {
            _ct = _context.CancellationTokenSource.Token;
            await AppInitialize("Data Source=.data/IoTSharp.db", "Data Source=.data/TelemetryStorage.db", DataBaseType.Sqlite);
        }
        [TestCleanup]
        public async Task Cleanup()
        {
            await  AppCleanup();
        }
    }
}
