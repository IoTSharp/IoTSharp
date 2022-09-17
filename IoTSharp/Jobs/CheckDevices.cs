using IoTSharp.EventBus;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;
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
        private readonly IPublisher _queue;

        public CheckDevices(ILogger<CheckDevices> logger, IServiceScopeFactory scopeFactor, MqttServer serverEx
           , IOptions<AppSettings> options, IPublisher queue)
        {
            _mcsetting = options.Value.MqttClient;
            _logger = logger;
            _scopeFactor = scopeFactor;
            _serverEx = serverEx;
            _queue = queue;
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
                var onlinedev = from client in clientstatus select client.Session.Items[nameof(Device)] as Device;
                //把超时时间小于1的都设置为300秒
                var tfx = from d in _dbContext.Device where d.Timeout < 1 select d;
                tfx.ToList().ForEach(d => d.Timeout = 300);

                //所有在线且活跃时间超时的设备如果不在已连接客户端内， 则认为是离线。 
                //这里的自身我们认为是 有链接的设备， 而不是无连接的。 
                _dbContext.Device.Where(d =>  d.Owner==null   && d.Online && DateTime.Now.Subtract(d.LastActive).TotalSeconds > d.Timeout)
                    .Select(d => d.Id).ToList().ForEach(id =>
                    {
                        if (!onlinedev.Any(dev => dev.Id != id))
                        {
                            _queue.PublishDeviceStatus(id, DeviceStatus.Bad);
                        }
                    });
                    var saveresult = await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($"设备检查程序已经处理{saveresult}调数据");
                }
            });
        }
    }
}
