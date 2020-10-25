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
                //如果中断在mqtt服务器列表中， 则取得最后一次收到消息的时间戳， 
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var clientstatus = await _serverEx.GetClientStatusAsync();
                    clientstatus.ToList().ForEach(cs =>
                    {
                        if (_device.Exists(cs.ClientId))
                        {
                            var dev = _device.Get<Device>(cs.ClientId);
                            var d = _dbContext.Device.FirstOrDefault(d => d.Id == dev.Value.Id);
                            if (d != null)
                            {
                                d.LastActive = cs.LastPacketReceivedTimestamp;
                            }
                        }
                    });
                    //如果超时小于1 就设置为默认300秒
                    var tfx = from d in _dbContext.Device where d.Timeout < 1 select d;
                    tfx.ToList().ForEach(d => d.Timeout = 300);
                    //当前时间减去最后活跃时间如果小于超时时间， 则为在线， 否则就是离线
                    _dbContext.Device.ToList().ForEach(d =>
                    {
                        d.Online = DateTime.Now.Subtract(d.LastActive).TotalSeconds < d.Timeout;
                    });
                    await _dbContext.SaveChangesAsync();
                }
            });
        }
    }
}
