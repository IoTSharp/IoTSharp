using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.Queue;
using IoTSharp.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.AspNetCoreEx;
using MQTTnet.Server;
using Quartz;
using SilkierQuartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Jobs
{
    [SilkierQuartz(0, "PushData", "Push Iot Message Data to DataBase ", TriggerGroup = "Data")]
    public class PushData : IJob
    {
        private readonly AppSettings _appSettings;
        readonly ILogger _logger;
        readonly IServiceScope scope;
        readonly IMsgQueue _queue;
        private readonly IStorage _storage;

        public PushData(ILogger<PushData> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options, IMsgQueue queue, IStorage storage
            )
        {
              _appSettings = options.Value;
            _logger = logger;
            scope = scopeFactor.CreateScope();
            _queue = queue;
            _storage = storage;
        }
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(async () =>
           {
               RawMsg msg = null;
               int sec = 0;
               do
               {
                   msg = _queue.Dequeue();
                   if (msg == null)
                   {
                       Thread.Sleep(TimeSpan.FromSeconds(1));
                       sec++;
                   }
               } while (msg == null && sec<60);
               if (msg != null)
               {
                   using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                   {
                       var device = _dbContext.Device.FirstOrDefault(d => d.Id == msg.DeviceId);

                       if (device != null)
                       {
                           switch (msg.DataCatalog)
                           {

                               case DataCatalog.AttributeData:
                                   var result2 = await _dbContext.SaveAsync<AttributeLatest>(msg.MsgBody, device.Id, msg.DataSide);
                                   result2.exceptions?.ToList().ForEach(ex =>
                                   {
                                       _logger.LogError($"{ex.Key} {ex.Value} {Newtonsoft.Json.JsonConvert.SerializeObject(msg.MsgBody[ex.Key])}");
                                   });
                                   _logger.LogInformation($"更新{device.Name}({device.Id})属性数据结果{result2.ret}");
                                   break;

                               case DataCatalog.TelemetryData:
                                   bool sta = await _storage.StoreTelemetryAsync(msg);
                                   _logger.LogInformation($"新增{device.Name}({device.Id})遥测数据{sta}");
                                   break;
                               default:
                                   break;
                           }
                       }
                   }
               }
           });
        }

    
    }

}
