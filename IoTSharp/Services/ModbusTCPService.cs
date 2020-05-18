using CoAP.Server;
using IoTSharp.Data;
using IoTSharp.Handlers;
using Kimbus;
using Kimbus.Slave;
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
    public class ModbusTCPService : IHostedService
    {
        private readonly ILogger _logger;
        private MbTcpSlave _slave;
        private ApplicationDbContext _dbContext;
        private IServiceScope _serviceScope;
        public ModbusTCPService(ILogger<ModbusTCPService> logger, IServiceScopeFactory scopeFactor)
        {
            _slave = new MbTcpSlave("*", 502);
            _logger = logger;
            _serviceScope = scopeFactor.CreateScope();
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
          
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _slave.OnWriteHoldingRegisters = (_, start, hrs) =>
            {
                Console.WriteLine("Write holding registers at {0}, len: {1}", start, hrs.Length);

                for (var i = 0; i < hrs.Length; ++i)
                {
                    Console.WriteLine("{0}: {1}", i + start, hrs[i]);
                }

                return MbExceptionCode.Ok;
            };
            _slave.OnReadHoldingRegisters = (_, start, count) =>
            {
                var buffer = new ushort[count];

                for (var i = 0; i < count; ++i)
                {
                    buffer[i] = (ushort)(i + start);
                }

                return (buffer, MbExceptionCode.Ok);
            };
            return _slave.Listen();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}