using CoAP.Server;
using IoTSharp.Services.Coap;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Services
{

    public class CoAPService : IHostedService
    {
        private readonly ILogger _logger;
        private CoapServer server;

        public CoAPService(ILogger<CoAPService> logger, ICoapBusinessDispatcher dispatcher)
        {
            server = new CoapServer();
            _logger = logger;
            server.Add(CoapPlatformRouteEndpoints.Create(dispatcher).ToArray());
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            // server.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {

            //server.Stop();
            return Task.CompletedTask;
        }
    }
}
