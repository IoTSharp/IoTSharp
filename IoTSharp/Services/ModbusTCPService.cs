using CoAP.Server;
using IoTSharp.Data;
using IoTSharp.Handlers;
using Kimbus;
using Kimbus.Slave;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly AppSettings _settings;
        public ModbusTCPService(ILogger<ModbusTCPService> logger, IServiceScopeFactory scopeFactor, IOptions<AppSettings> options)
        {
            _settings = options.Value;
            _slave = new MbTcpSlave("*", _settings.ModBusServer.Port, _settings.ModBusServer.TimeOut);
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
            _slave.OnWriteCoils = (_, start, bools) =>
           {
               Console.WriteLine("Write coils at {0}, len: {1}", start, bools.Length);

               for (var i = 0; i < bools.Length; ++i)
               {
                   Console.WriteLine("{0}: {1}", i + start, bools[i]);
               }

               return MbExceptionCode.Ok;
           };

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

            _slave.OnReadInputRegisters = _slave.OnReadHoldingRegisters;


            _slave.OnReadCoils = (_, start, count) =>
            {
                var buffer = new bool[count];

                for (var i = 0; i < count; ++i)
                {
                    buffer[i] = (start + i) % 3 == 0;
                }

                return (buffer, MbExceptionCode.Ok);
            };
            _slave.OnReadDiscretes = _slave.OnReadCoils;
            return _slave.Listen();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}