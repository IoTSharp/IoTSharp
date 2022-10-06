using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
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
    public class PrimeWebDefaultRequestShould
    {
        private   TestServer _server;
        private   HttpClient _client;
        [TestInitialize()]
        public void  TestServerInitialize()
        {
            _server = new TestServer((IWebHostBuilder)Program.CreateHostBuilder(null));
            _client = _server.CreateClient();
        }

        [TestMethod]
        public async Task ReturnHelloWorld()
        {
            // Act
            var response = await _client.GetAsync("/");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            // Assert
            Assert.AreEqual("Hello World!", responseString);
        }
    }
}
