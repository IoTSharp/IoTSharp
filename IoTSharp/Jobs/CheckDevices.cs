using IoTSharp.EventBus;
using IoTSharp.Contracts;
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
using Microsoft.EntityFrameworkCore;

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
        public async Task Execute(IJobExecutionContext context)
        {
            //如果中断在mqtt服务器列表中， 则取得最后一次收到消息的时间戳， 
            using (var scope = _scopeFactor.CreateScope())
            using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
            {
                try
                {
                    var sf = from d in _dbContext.AttributeLatest where (d.KeyName == Constants._Active && d.Value_Boolean == true) select d.DeviceId;
                    if (sf.Any())
                    {
                        var devids = await sf.ToListAsync();
                        foreach (var id in devids)
                        {
                            var dev = await _dbContext.Device.FirstOrDefaultAsync(d=>d.Id== id);
                            var ladt = from d in _dbContext.AttributeLatest where d.DeviceId == id && d.DataSide == DataSide.ServerSide && d.KeyName == Constants._LastActivityDateTime select d.Value_DateTime;
                            var __LastActivityDateTime = await ladt.FirstOrDefaultAsync();
                            if (dev != null && __LastActivityDateTime!=null)
                            {
                             
                                if (DateTime.UtcNow.Subtract(__LastActivityDateTime.GetValueOrDefault()).TotalSeconds > dev.Timeout)
                                {
                                    _logger.LogInformation($"设备{dev.Name}({dev.Id})现在置非活跃状态，上次活跃时间为{__LastActivityDateTime},超时时间{dev.Timeout}秒");
                                    await _queue.PublishActive(id, ActivityStatus.Inactivity);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, "检查设备在线状态错误。");
                }
            }
        }
    }
}
