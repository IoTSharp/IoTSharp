using EasyCaching.Core;
using IoTSharp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.AspNetCoreEx;
using Quartz;
using SilkierQuartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Jobs
{

    [SilkierQuartz(60)]
    public class CheckDevices : IJob
    {
        private readonly MqttClientSetting _mcsetting;
        private readonly ILogger<CheckDevices> _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IEasyCachingProviderFactory _factory;
        private readonly IMqttServerEx _serverEx;
        private readonly IEasyCachingProvider _device;

        public CheckDevices(ILogger<CheckDevices> logger, IServiceScopeFactory scopeFactor, IMqttServerEx serverEx
           , IOptions<AppSettings> options, IEasyCachingProviderFactory factory)
        {
            _mcsetting = options.Value.MqttClient;
            _logger = logger;
            _scopeFactor = scopeFactor;
            _factory = factory;
            _serverEx = serverEx;
            _device = _factory.GetCachingProvider("iotsharp");
        }
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(async () =>
            {

                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var clientstatus = await _serverEx.GetClientStatusAsync();
                    clientstatus.ToList().ForEach(cs =>
                    {
                        var dev = _device.Get<Device>(cs.ClientId);
                        if (dev.HasValue)
                        {
                            var d = _dbContext.Device.FirstOrDefault(d => d.Id == dev.Value.Id);
                            if (d != null)
                            {
                                d.LastActive = cs.LastPacketReceivedTimestamp;
                                d.Online = true;
                            }
                        }

                    });
                    var tfx = from d in _dbContext.Device where d.Timeout < 1 select d;
                    tfx.ToList().ForEach(d => d.Timeout = 180);
                    var to = _dbContext.Device.ToList().Where(d => DateTime.Now.Subtract(d.LastActive).TotalSeconds > d.Timeout).ToList();
                    to.ForEach(d => d.Online = false);
                    await _dbContext.SaveChangesAsync();
                }
            });
        }
    }
}
