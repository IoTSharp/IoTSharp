﻿using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using  Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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

        public  virtual Task<bool>  CheckTelemetryStorage()
        {
           return Task.FromResult( true );
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {
            using var scope = _scopeFactor.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var devid = from t in context?.TelemetryLatest
                        where t.DeviceId == deviceId
                        select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, DataType = t.Type, Value = t.ToObject() };
            return devid.AsNoTracking().ToListAsync();
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            using var scope = _scopeFactor.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var keyary = keys.Split(',', ' ', ';');
             var devid = from t in context?.TelemetryLatest
                        where t.DeviceId == deviceId && keyary.Contains(t.KeyName)
                        select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, DataType = t.Type, Value = t.ToObject() };
            return devid.AsNoTracking().ToListAsync();
        }

        public async Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            using var scope = _scopeFactor.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var lst = new List<TelemetryDataDto>();
            var kv = from t in context?.TelemetryData
                     where t.DeviceId == deviceId &&  t.DateTime >= begin && t.DateTime < end
                     select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, DataType=t.Type,  Value = t.ToObject() };
            if (!string.IsNullOrEmpty(keys) )
            {
                var keyarys = keys.Split(',', ' ', ';');
                var kfk = from t in kv where keyarys.Contains(t.KeyName) select t;
                lst=await kfk.AsNoTracking().ToListAsync();
            }
            else
            {
                lst = await kv.AsNoTracking().ToListAsync();
            }
           return AggregateDataHelpers.AggregateData(lst, begin, end, every, aggregate);
        }

        public virtual async Task<(bool result, List<TelemetryData> telemetries)>  StoreTelemetryAsync(PlayloadData msg)
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
                                var tdata = new TelemetryData() { DateTime = msg.ts,  DeviceId = msg.DeviceId, KeyName = kp.Key };
                                tdata.FillKVToMe(kp);
                                _dbContext.Set<TelemetryData>().Add(tdata);
                                telemetries.Add(tdata);
                            }
                        });
                        var result1 = await _dbContext.SaveAsync<TelemetryLatest>(msg.MsgBody, msg.DeviceId, msg.DataSide);
                        result1.exceptions?.ToList().ForEach(ex =>
                        {
                            _logger.LogError($"{ex.Key} {ex.Value} {Newtonsoft.Json.JsonConvert.SerializeObject(msg.MsgBody[ex.Key])}");
                        });
                        _logger.LogInformation($"新增({msg.DeviceId})遥测数据更新最新信息{result1.ret}");
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