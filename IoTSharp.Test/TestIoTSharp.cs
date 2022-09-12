using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace IoTSharp.Test
{

    [TestClass]
    public class TestIoTSharp
    {

  
      [TestInitialize]
        public void Initialize()
        {
         
        }

        [TestMethod]
        public async Task TestAsync()
        {
            await using var application = new WebApplicationFactory<IoTSharp.Startup>();
            using var client = application.CreateClient();

            var response = await client.GetAsync("/weatherforecast");

        }
    }
}
