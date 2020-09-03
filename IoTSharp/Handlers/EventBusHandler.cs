using DotNetCore.CAP;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.Queue;
using IoTSharp.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Handlers
{

    public interface IEventBusHandler
    {


        public void StoreAttributeData(RawMsg msg);

        public void StoreTelemetryData(RawMsg msg);
    }

    /// <summary>
    /// iotsharp.services.datastream
    /// </summary>
    ///<remarks>Note: The injection of services needs before of `services.AddCap()`</remarks>
    ///
    public class EventBusHandler : IEventBusHandler, ICapSubscribe
    {
        private readonly AppSettings _appSettings;
        readonly ILogger _logger;
        readonly IServiceScope scope;
        private readonly IStorage _storage;

        public EventBusHandler(ILogger<EventBusHandler> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options, IStorage storage
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            scope = scopeFactor.CreateScope();
            _storage = storage;
        }
        [CapSubscribe("iotsharp.services.datastream.attributedata")]
        public void StoreAttributeData(RawMsg msg)
        {
            Task.Run(async () =>
            {
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var device = _dbContext.Device.FirstOrDefault(d => d.Id == msg.DeviceId);
                    if (device != null)
                    {
                        var result2 = await _dbContext.SaveAsync<AttributeLatest>(msg.MsgBody, device.Id, msg.DataSide);
                        result2.exceptions?.ToList().ForEach(ex =>
                        {
                            _logger.LogError($"{ex.Key} {ex.Value} {Newtonsoft.Json.JsonConvert.SerializeObject(msg.MsgBody[ex.Key])}");
                        });
                        _logger.LogInformation($"更新{device.Name}({device.Id})属性数据结果{result2.ret}");
                    }
                }
            });
        }
        [CapSubscribe("iotsharp.services.datastream.telemetrydata")]
        public void StoreTelemetryData(RawMsg msg) => Task.Run(async () => await _storage.StoreTelemetryAsync(msg));
    }
}
