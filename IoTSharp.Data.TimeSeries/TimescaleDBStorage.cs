using Castle.Core.Logging;
using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Storage
{
    public class TimescaleDBStorage : IStorage
    {

        private readonly IServiceScopeFactory _scopeFactor;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        public TimescaleDBStorage(ILogger<TimescaleDBStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options)
        {
            _scopeFactor = scopeFactor;
            _logger = logger;
        }
        public Task<bool> CheckTelemetryStorage()
        {
            using var scope = _scopeFactor.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var have = context.Database.ExecuteScalar<long>("SELECT  count(0) FROM _timescaledb_catalog.hypertable where table_name='TelemetryData';");
            if (have == 0)
            {
                //创建表
                context.Database.ExecuteNonQuery("SELECT create_hypertable('\"TelemetryData\"', 'DateTime', 'DeviceId', 2, create_default_indexes=>FALSE);");
                context.Database.ExecuteNonQuery("CREATE INDEX ON \"TelemetryData\" (\"KeyName\", \"DateTime\" DESC);");
                context.Database.ExecuteNonQuery("CREATE INDEX ON \"TelemetryData\" (\"DataSide\", \"DateTime\" DESC);");
                context.Database.ExecuteNonQuery("CREATE INDEX ON \"TelemetryData\" (\"Type\", \"DateTime\" DESC);");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public async Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            var results = new List<TelemetryDataDto>();
            var keyList = keys.Split(',');
            // 使用 Task.WhenAll 并行处理多个 key
            var tasks = keyList.Select(key => AggregateTelemetryAsync(deviceId, key, begin, end, every, aggregate));
            // 等待所有任务完成
            var dataResults = await Task.WhenAll(tasks);
            // 将结果合并
            results.AddRange(dataResults.SelectMany(data => data));
            return results;
        }

        public async Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {
            using var scope = _scopeFactor.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var devid = from t in context?.TelemetryLatest
                        where t.DeviceId == deviceId
                        select new TelemetryDataDto() { DataType=t.Type,  DateTime = t.DateTime, KeyName = t.KeyName, Value = t.ToObject() };
            var temp = await devid.AsNoTracking().ToListAsync();
            return temp;
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            using var scope = _scopeFactor.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var keyarys = keys.Split(',', ' ', ';');
            var devid = from t in context?.TelemetryLatest
                        where t.DeviceId == deviceId && keyarys.Contains(t.KeyName)
                        select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, Value = t.ToObject() };
            var temp = devid.AsNoTracking().ToListAsync();
            return temp;
        }

        public async Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(PlayloadData msg)
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
                                var tdata = new TelemetryData() { DateTime = msg.ts, DeviceId = msg.DeviceId, KeyName = kp.Key };
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


        private async Task<List<TelemetryDataDto>> AggregateTelemetryAsync(Guid deviceId, string key, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            using var scope = _scopeFactor.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>() ?? throw new ArgumentNullException("ApplicationDbContext");
            var dataType = await context.TelemetryData
                .Where(t => t.DeviceId == deviceId && t.KeyName == key)
                .Select(t => t.Type)
                .FirstOrDefaultAsync();
            if (aggregate.Equals(Aggregate.None))
            {
                return await GetAllData(context!, deviceId, key, begin, end);
            }
            switch (dataType)
            {
                case DataType.Double:
                case DataType.Long:
                    return await AggregateAsync(context!, deviceId, key, dataType, begin, end, every, aggregate);
                default:
                    return await GetAllData(context!, deviceId, key, begin, end);
            }

        }

        private static async Task<List<TelemetryDataDto>> GetAllData(ApplicationDbContext context, Guid deviceId, string key, DateTime begin, DateTime end)
        {
            var kv = from t in context.TelemetryData
                     where t.DeviceId == deviceId && t.KeyName == key && t.DateTime >= begin && t.DateTime < end
                     select new TelemetryDataDto() { DateTime = t.DateTime, KeyName = t.KeyName, DataType = t.Type, Value = t.ToObject() };
            var temp = await kv.AsNoTracking().ToListAsync();
            return temp;
        }

        private async Task<List<TelemetryDataDto>> AggregateAsync(ApplicationDbContext context, Guid deviceId, string key, DataType dataType, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            var sql = new StringBuilder();
            sql.AppendLine($"SELECT time_bucket('{every.TotalMilliseconds} milliseconds', \"DateTime\") AS DateTime, \"KeyName\",");
            // 根据聚合类型生成 SQL 聚合函数
            sql.AppendLine(aggregate switch
            {
                Aggregate.Mean => $"avg(\"{GetFiledName(dataType)}\") AS Value",
                Aggregate.Max => $"max(\"{GetFiledName(dataType)}\") AS Value",
                Aggregate.Min => $"min(\"{GetFiledName(dataType)}\") AS Value",
                Aggregate.Sum => $"sum(\"{GetFiledName(dataType)}\") AS Value",
                Aggregate.First => $"first(\"{GetFiledName(dataType)}\",\"DateTime\") AS Value",
                Aggregate.Last => $"last(\"{GetFiledName(dataType)}\",\"DateTime\") AS Value",
                Aggregate.Median => $"PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY \"{GetFiledName(dataType)}\") AS Value",
                Aggregate.None => "",
                _ => throw new ArgumentOutOfRangeException(nameof(aggregate), aggregate, null)
            });
            sql.AppendLine($"FROM \"TelemetryData\" WHERE \"DeviceId\" = '{deviceId}' AND \"KeyName\" = '{key}' AND \"DateTime\" >= '{begin}' AND \"DateTime\" <= '{end}'");
            sql.AppendLine("GROUP BY DateTime, \"KeyName\" ORDER BY DateTime ASC");
            var result = await context.Database.SqlQuery<TelemetryDataDto>(sql.ToString()).ToListAsync();
            result.ToList().ForEach(e =>
            {
                e.DataType = dataType;
            });
            return result;
        }


        private string GetFiledName(DataType dataType)
        {
            return dataType switch
            {
                DataType.Double => "Value_Double",
                DataType.Long => "Value_Long",
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
            };
        }
    }
}