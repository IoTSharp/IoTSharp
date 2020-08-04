using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.Queue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Storage
{
    public class EFStorage : IStorage
    {
        private readonly AppSettings _appSettings;
        readonly ILogger _logger;
        readonly IServiceScope scope;

        public EFStorage(ILogger<EFStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            scope = scopeFactor.CreateScope();
        }

   

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin)
        {
            using (var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
            {
                var kv = from t in _context.TelemetryData 
                         where t.DeviceId == deviceId && keys.Split(',', ' ', ';').Contains(t.KeyName) && t.DateTime >= begin 
                         select new TelemetryDataDto() { DateTime=t.DateTime, KeyName=t.KeyName, Value= t.ToObject() };
                return   kv.ToListAsync();
            }
        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end)
        {
            using (var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
            {
                var kv = from t in _context.TelemetryData 
                         where t.DeviceId == deviceId && keys.Split(',', ' ', ';').Contains(t.KeyName) && t.DateTime >= begin && t.DateTime < end
                         select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, Value = t.ToObject() };
                return  kv.ToListAsync();
            }
        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, DateTime begin)
        {
            using (var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
            {
                var kv = from t in _context.TelemetryData where t.DeviceId == deviceId  && t.DateTime >= begin
                         select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, Value = t.ToObject() };
                return kv.ToListAsync();
            }
        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, DateTime begin, DateTime end)
        {
            using (var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
            {
                var kv = from t in _context.TelemetryData 
                         where t.DeviceId == deviceId && t.DateTime >= begin && t.DateTime < end
                         select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, Value = t.ToObject() };
                return kv.ToListAsync();
            }
        }

        public async Task<bool> StoreTelemetryAsync(RawMsg msg)
        {
            bool result = false;
            try
            {
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    msg.MsgBody.ToList().ForEach(kp =>
                    {
                        var tdata = new TelemetryData() { DateTime = DateTime.Now, DeviceId = msg.DeviceId, KeyName = kp.Key, Value_DateTime = new DateTime(1970, 1, 1) };
                        tdata.FillKVToMe(kp);
                        _dbContext.Set<TelemetryData>().Add(tdata);
                    });
                    await _dbContext.SaveChangesAsync();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
            }
            return result;
        }

       
    }
}
