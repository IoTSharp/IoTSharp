using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Shardings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace IoTSharp.Storage
{
    public class ShardingStorage : IStorage
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;

        public ShardingStorage(ILogger<ShardingStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            _scopeFactor = scopeFactor;
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {
            try
            {
                using (var scope = _scopeFactor.CreateScope())
                {
                    using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        var devid = from t in context.TelemetryLatest
                                    where t.DeviceId == deviceId
                                    select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, Value = t.ToObject() };

                        return Task.FromResult(devid.AsNoTracking().ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{deviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
                throw;
            }
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            try
            {
                using (var scope = _scopeFactor.CreateScope())
                {
                    using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        var devid = from t in context.TelemetryLatest
                                    where t.DeviceId == deviceId && keys.Split(',', ' ', ';').Contains(t.KeyName)

                                    select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, Value = t.ToObject() };

                        return Task.FromResult(devid.AsNoTracking().ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{deviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
                throw;
            }
        }

        public async Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            List<TelemetryDataDto> result = new List<TelemetryDataDto>();
            try
            {
                using (var scope = _scopeFactor.CreateScope())
                {
                    using (var context = scope.ServiceProvider.GetRequiredService<ShardingDbContext>())
                    {
                        var lst = new List<TelemetryDataDto>();
                        var kv = await context.Set<TelemetryData>()
                            .Where(t => t.DeviceId == deviceId && t.DateTime >= begin && t.DateTime < end)
                            .Select(t => new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, Value = t.ToObject(), DataType = t.Type }).ToListAsync();
                        if (!string.IsNullOrEmpty(keys))
                        {
                            lst = kv.Where(t => keys.Split(',', ' ', ';').Contains(t.KeyName)).ToList();
                        }
                        else
                        {
                            lst = kv.ToList();
                        }
                        result= AggregateDataHelpers. AggregateData(lst,begin, end,  every, aggregate);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{deviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
                throw;
            }
            return result;
        }

        public async Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(PlayloadData msg)
        {
            bool result = false;
            List<TelemetryData> telemetries = new List<TelemetryData>();

            try
            {
                using var scope = _scopeFactor.CreateScope();

                using (var db = scope.ServiceProvider.GetRequiredService<ShardingDbContext>())
                {
              
                    var lst = new List<TelemetryData>();
                    msg.MsgBody.ToList().ForEach(kp =>
                                     {
                                         if (kp.Value != null)
                                         {
                                             var tdata = new TelemetryData() { DateTime = msg.ts, DeviceId = msg.DeviceId, KeyName = kp.Key };
                                             tdata.FillKVToMe(kp);
                                             lst.Add(tdata);
                                             telemetries.Add(tdata);
                                         }
                                     });
                    await db.Set<TelemetryData>().AddRangeAsync(lst);
                    await db.SaveChangesAsync();
                    _logger.LogInformation($"新增({msg.DeviceId})遥测数据1");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
            }

            try
            {
                using (var scope = _scopeFactor.CreateScope())
                {
                    using (var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        var result1 = await dbContext.SaveAsync<TelemetryLatest>(msg.MsgBody, msg.DeviceId, msg.DataSide);
                        result1.exceptions?.ToList().ForEach(ex =>
                        {
                            _logger.LogError(ex.Value, $"{ex.Key} {ex.Value.Message} {ex.Value.InnerException?.Message}");
                        });
                        _logger.LogInformation($"新增({msg.DeviceId})遥测数据更新最新信息{result1.ret}");
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
            }
            return (result, telemetries);
        }
    }
}