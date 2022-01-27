using EasyCaching.Core;
using IoTSharp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Server;
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
        private readonly MqttServer _serverEx;

        public CheckDevices(ILogger<CheckDevices> logger, IServiceScopeFactory scopeFactor, MqttServer serverEx
           , IOptions<AppSettings> options)
        {
            _mcsetting = options.Value.MqttClient;
            _logger = logger;
            _scopeFactor = scopeFactor;
            _serverEx = serverEx;
        }
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(async () =>
            {
                //如果中断在mqtt服务器列表中， 则取得最后一次收到消息的时间戳， 
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var clientstatus = await _serverEx.GetClientsAsync();
                    clientstatus.ToList().ForEach(cs =>
                   {
                       try
                       {
                           var _device = cs.Session.Items[ nameof(Device)] as Device;
                           if (_device != null)
                           {
                               var d = _dbContext.Device.FirstOrDefault(d => d.Id == _device.Id);
                               if (d != null)
                               {
                                   if (!d.Online && DateTime.Now.Subtract(d.LastActive).TotalSeconds > d.Timeout)
                                   {
                                       Task.Run(cs.DisconnectAsync);
                                   }
                               }
                           }
                       }
                       catch (Exception ex)
                       {
                           _logger.LogInformation($"检查设备{cs.Id}-{cs.Endpoint}) 时遇到异常{ex.Message}{ex.InnerException?.Message}  发送消息:{cs.SentApplicationMessagesCount}({cs.BytesSent}kb)  收到{cs.ReceivedApplicationMessagesCount}({cs.BytesReceived / 1024}KB )  ");

                       }
                   });
                    var tfx = from d in _dbContext.Device where d.Timeout < 1 select d;
                    tfx.ToList().ForEach(d => d.Timeout = 300);
                    //当前时间减去最后活跃时间如果小于超时时间， 则为在线， 否则就是离线
                    _dbContext.Device.ToList().ForEach(d =>
                    {
                         if  (d.Online  &&   DateTime.Now.Subtract(d.LastActive).TotalSeconds > d.Timeout)
                        {
                            d.Online = false;
                        }
                    });
                    var saveresult = await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($"设备检查程序已经处理{saveresult}调数据");
                }
            });
        }
    }
}
