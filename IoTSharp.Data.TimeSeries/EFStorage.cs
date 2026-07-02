using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IoTSharp.Storage
{
    public class EFStorage : IStorage
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;

        public EFStorage(ILogger<EFStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            _scopeFactor = scopeFactor;
        }

        public virtual Task<bool> CheckTelemetryStorage()
        {
            return Task.FromResult(true);
        }

        public virtual Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {
            using var scope = _scopeFactor.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var devid = from t in context?.TelemetryLatest
                        where t.DeviceId == deviceId
                        select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, DataType = t.Type, Value = t.ToObject() };
            return devid.AsNoTracking().ToListAsync();
        }

        public virtual Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            using var scope = _scopeFactor.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var keyary = keys.Split(',', ' ', ';');
            var devid = from t in context?.TelemetryLatest
                        where t.DeviceId == deviceId && keyary.Contains(t.KeyName)
                        select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, DataType = t.Type, Value = t.ToObject() };
            return devid.AsNoTracking().ToListAsync();
        }

        public virtual async Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            using var scope = _scopeFactor.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var lst = new List<TelemetryDataDto>();
            var kv = from t in context?.TelemetryData
                     where t.DeviceId == deviceId && t.DateTime >= begin && t.DateTime < end
                     select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, DataType = t.Type, Value = t.ToObject() };
            if (!string.IsNullOrEmpty(keys))
            {
                var keyarys = keys.Split(',', ' ', ';');
                var kfk = from t in kv where keyarys.Contains(t.KeyName) select t;
                lst = await kfk.AsNoTracking().ToListAsync();
            }
            else
            {
                lst = await kv.AsNoTracking().ToListAsync();
            }
            return AggregateDataHelpers.AggregateData(lst, begin, end, every, aggregate);
        }

        public virtual async Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(PlayloadData msg)
        {
            bool result = false;
            List<TelemetryData> telemetries = new List<TelemetryData>();
            try
            {
                using (var scope = _scopeFactor.CreateScope())
                {
                    using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        msg.MsgBody.ToList().ForEach(kp =>
                        {
                            if (kp.Value != null)
                            {
                                var tdata = new TelemetryData() { DateTime = msg.ts, DeviceId = msg.DeviceId, KeyName = kp.Key, DataSide = msg.DataSide };
                                tdata.FillKVToMe(kp);
                                _dbContext.Set<TelemetryData>().Add(tdata);
                                telemetries.Add(tdata);
                            }
                        });
                        var exceptions = _dbContext.PreparingData<TelemetryLatest>(msg.MsgBody, msg.DeviceId, msg.DataSide);
                        var ret = await _dbContext.SaveChangesAsync();
                        exceptions?.ToList().ForEach(ex =>
                        {
                            _logger.LogError($"{ex.Key} {ex.Value} {JsonObjectSerializer.Serialize(msg.MsgBody[ex.Key])}");
                        });
                        _logger.LogInformation($"新增({msg.DeviceId})遥测数据更新最新信息{ret}");
                        result = ret > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
            }
            return (result, telemetries);
        }

        public virtual async Task<TelemetryBatchStoreResult> StoreTelemetryBatchAsync(IReadOnlyCollection<PlayloadData> messages)
        {
            var telemetries = new List<TelemetryData>();
            if (messages.Count == 0)
            {
                return new TelemetryBatchStoreResult(true, telemetries, 0);
            }

            try
            {
                using var scope = _scopeFactor.CreateScope();
                using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var latestValues = new Dictionary<(Guid DeviceId, string KeyName), (object Value, DataSide DataSide)>();
                foreach (var msg in messages)
                {
                    foreach (var kp in msg.MsgBody)
                    {
                        if (kp.Value == null)
                        {
                            continue;
                        }

                        var tdata = new TelemetryData() { DateTime = msg.ts, DeviceId = msg.DeviceId, KeyName = kp.Key, DataSide = msg.DataSide };
                        tdata.FillKVToMe(kp);
                        dbContext.Set<TelemetryData>().Add(tdata);
                        telemetries.Add(tdata);
                        latestValues[(msg.DeviceId, kp.Key)] = (kp.Value, msg.DataSide);
                    }
                }

                foreach (var deviceGroup in latestValues.GroupBy(x => x.Key.DeviceId))
                {
                    foreach (var sideGroup in deviceGroup.GroupBy(x => x.Value.DataSide))
                    {
                        var latest = sideGroup.ToDictionary(x => x.Key.KeyName, x => x.Value.Value);
                        var exceptions = dbContext.PreparingData<TelemetryLatest>(latest, deviceGroup.Key, sideGroup.Key);
                        exceptions?.ToList().ForEach(ex =>
                        {
                            _logger.LogError($"{ex.Key} {ex.Value} {JsonObjectSerializer.Serialize(latest[ex.Key])}");
                        });
                    }
                }

                var ret = await dbContext.SaveChangesAsync();
                _logger.LogInformation($"批量新增遥测数据完成, 消息{messages.Count}条, 数据{telemetries.Count}条, 更新{ret}条");
                return new TelemetryBatchStoreResult(ret > 0 || telemetries.Count == 0, telemetries, messages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批量遥测数据处理失败{ex.Message} {ex.InnerException?.Message} ");
                return new TelemetryBatchStoreResult(false, telemetries, messages.Count);
            }
        }


    }
}
