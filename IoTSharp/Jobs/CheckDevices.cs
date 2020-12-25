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
                       try
                       {

                           var _device = cs.Session.Items?.FirstOrDefault(k => (string)k.Key == nameof(Device)).Value as Device;
                           var d = _dbContext.Device.FirstOrDefault(d => d.Id == _device.Id);
                           if (d != null)
                           {

                               d.LastActive = cs.LastPacketReceivedTimestamp;
                               d.Online = DateTime.Now.Subtract(d.LastActive).TotalSeconds < d.Timeout;
                               _logger.LogInformation($"设备{cs.ClientId}-{d.Name}({d.Id},{cs.Endpoint}) 最后活动时间{d.LastActive} 在线{d.Online} 发送消息:{cs.SentApplicationMessagesCount}({cs.BytesSent}kb)  收到{cs.ReceivedApplicationMessagesCount}({cs.BytesReceived / 1024}KB )  ");
                               if (!d.Online)
                               {
                                   Task.Run(cs.DisconnectAsync);
                               }
                           }
                       }
                       catch (Exception ex)
                       {
                           _logger.LogInformation($"检查设备{cs.ClientId}-{cs.Endpoint}) 时遇到异常{ex.Message}{ex.InnerException?.Message}  发送消息:{cs.SentApplicationMessagesCount}({cs.BytesSent}kb)  收到{cs.ReceivedApplicationMessagesCount}({cs.BytesReceived / 1024}KB )  ");

                       }
                   });
                    var saveresult = await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($"设备检查程序已经处理{saveresult}调数据");
                }
            });
        }
    }
}
