using CoAP.Server;
using IoTSharp.EventBus;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Services.CoApResources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
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
        private readonly IPublisher _capBus;
        private IServiceScope _serviceScope;
        private CoapServer server;
        private readonly AppSettings _settings;
        public CoAPService(ILogger<CoAPService> logger, IServiceScopeFactory scopeFactor, IOptions<AppSettings> options, IPublisher capBus)
        {
            _settings = options.Value;
            server = new CoapServer();
            _logger = logger;
            _serviceScope = scopeFactor.CreateScope();
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _capBus = capBus;
            Enum.GetNames(typeof(CoApRes)).ToList().ForEach(n => server.Add(new CoApResource(n, _serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>(), _logger, _capBus)));
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