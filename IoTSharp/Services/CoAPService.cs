using CoAP.Server;
using IoTSharp.Data;
using IoTSharp.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Services
{
    public class CoAPService : IHostedService
    {
        private readonly ILogger _logger;
 
        private ApplicationDbContext _dbContext;
        private IServiceScope _serviceScope;
        private CoapServer server;
        public CoAPService(ILogger<CoAPService> logger, IServiceScopeFactory scopeFactor)
        {

            server = new CoapServer();
            _logger = logger;
            _serviceScope = scopeFactor.CreateScope();
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Enum.GetNames(typeof(CoApRes)).ToList().ForEach(n => server.Add(new CoApResource(n, _serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>(), _logger)));
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                server.Start();
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                server.Stop();
            });
        }
    }
}