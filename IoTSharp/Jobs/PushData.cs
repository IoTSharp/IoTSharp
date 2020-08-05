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
using System.Threading.Tasks;

namespace IoTSharp.Jobs
{

    [SilkierQuartz(1, "PushData", "Push Iot Message Data to DataBase ", TriggerGroup = "Data")]
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
               var msg = _queue.Dequeue();
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
                                   var result2 = await _dbContext.SaveAsync<AttributeLatest>(msg.MsgBody, device, msg.DataSide);
                                   if (result2.exceptions?.Count > 0)
                                   {
                                       _logger.LogError(Newtonsoft.Json.JsonConvert.SerializeObject(msg.MsgBody));
                                   }
                                   else
                                   {
                                       _logger.LogInformation(Newtonsoft.Json.JsonConvert.SerializeObject(result2));
                                   }
                                   break;

                               case DataCatalog.TelemetryData:
                                   await _storage.StoreTelemetryAsync(msg);
                                   var result1 = await _dbContext.SaveAsync<TelemetryLatest>(msg.MsgBody, device, msg.DataSide);
                                   if (result1.exceptions?.Count > 0)
                                   {
                                       _logger.LogError(Newtonsoft.Json.JsonConvert.SerializeObject(msg.MsgBody));
                                   }
                                   else
                                   {
                                       _logger.LogInformation(Newtonsoft.Json.JsonConvert.SerializeObject(result1));
                                   }
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
