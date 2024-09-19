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
using Mono.Unix;
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
using Testcontainers.InfluxDb;
namespace IoTSharp.Test
{
    [TestClass]
    public class AppWithInfluxDBTest : AppInstance
    {

        [ClassInitialize]
        public static void TextFixtureSetup(TestContext context) => FixtureSetup(context);
        private const string _org = "iotsharp";
        private const string _bucket = "iotsharp-bucket";
        private const string _adminToken = "WUhUUUJB0JMIwavPQ4cl-euwnv-B1NC9vHe3fes9NaQ18-D5B37ngIWuTVYPqrIrnuNB2K6halXionzg6K1eyQ==";
        PostgreSqlContainer _db_container;
        InfluxDbContainer _tsdb_container;
        [TestInitialize()]
        public async Task TestServerInitialize()
        {
            _ct = _context.CancellationTokenSource.Token;
            _db_container = new PostgreSqlBuilder().Build();
            _tsdb_container = new InfluxDbBuilder()
                .WithOrganization(_org)
                .WithBucket(_bucket)
                .WithAdminToken(_adminToken)
                .Build();
            await _db_container.StartAsync(_ct);
            await _tsdb_container.StartAsync(_ct);
            await AppInitialize(_db_container.GetConnectionString(), $"http://{_tsdb_container.GetAddress()}/?org={_org}&bucket={_bucket}&token={_adminToken}", DataBaseType.PostgreSql, TelemetryStorage.InfluxDB);
        }
        [TestCleanup]
        public async Task Cleanup()
        {
            await AppCleanup();
            await _db_container.StopAsync(_ct);
            await _db_container.DisposeAsync();
            await _tsdb_container.StopAsync();
            await _tsdb_container.DisposeAsync();
        }
    }
}
