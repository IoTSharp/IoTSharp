using hyjiacan.py4n;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Taos;
using IoTSharp.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Text;

namespace IoTSharp.Storage
{
    public class TaosStorage : IStorage
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly IServiceScope scope;
        private readonly TaosConnection _taos;

        public TaosStorage(ILogger<TaosStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            scope = scopeFactor.CreateScope();
            _taos = new TaosConnection(_appSettings.ConnectionStrings["TelemetryStorage"]);
        }

        public Task<bool> CheckTelemetryStorage()
        {
            var _taosBuilder = new TaosConnectionStringBuilder(_taos.ConnectionString);
            if (_taos.State != System.Data.ConnectionState.Open) _taos.Open();
            _taos.CreateCommand($"CREATE DATABASE IF NOT EXISTS {_taosBuilder.DataBase}").ExecuteNonQuery();
            _taos.ChangeDatabase(_taosBuilder.DataBase);
            _taos.CreateCommand($"USE {_taosBuilder.DataBase}").ExecuteNonQuery();
            _taos.CreateCommand($"CREATE TABLE IF NOT EXISTS telemetrydata  (ts timestamp,value_type  tinyint, value_boolean bool, value_string binary(10240), value_long bigint,value_datetime timestamp,value_double double)   TAGS (deviceid binary(32),keyname binary(64));")
               .ExecuteNonQuery();
            return Task.FromResult(true);
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {
            if (_taos.State != System.Data.ConnectionState.Open) _taos.Open();
            //https://github.com/taosdata/TDengine/issues/4269
            string sql = $"select last_row(*),keyname from telemetrydata where deviceid='{deviceId:N}' group by deviceid,keyname";
            List<TelemetryDataDto> dt = SqlToTDD(_taos, sql, "last_row(*)");
            return Task.FromResult(dt);
        }

        /// <summary>
        /// 转换获取到的值
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="keyname"></param>
        /// <returns></returns>
        /// <exception cref="https://github.com/taosdata/TDengine/issues/4269">务必注意此bug</exception>
        private List<TelemetryDataDto> SqlToTDD(TaosConnection db, string sql, string keyname)
        {
            List<TelemetryDataDto> dt = new List<TelemetryDataDto>();
            TaosDataReader dataReader = db.CreateCommand(sql).ExecuteReader();
            while (dataReader.Read())
            {
                TelemetryDataDto telemetry = new TelemetryDataDto();
                try
                {
                    int idx = dataReader.GetOrdinal(keyname.Replace("*", "value_type"));
                    byte datatype = dataReader.GetByte(idx);
                    telemetry.DataType = (DataType)datatype;
                    telemetry.KeyName = dataReader.GetString(dataReader.GetOrdinal("keyname"));
                    telemetry.DateTime = dataReader.GetDateTime(dataReader.GetOrdinal(keyname.Replace("*", "ts")));
                    switch ((DataType)datatype)
                    {
                        case DataType.Boolean:
                            telemetry.Value = dataReader.GetBoolean(dataReader.GetOrdinal(keyname.Replace("*", "value_boolean")));
                            break;

                        case DataType.String:
                            telemetry.Value = dataReader.GetString(dataReader.GetOrdinal(keyname.Replace("*", "value_string")));
                            break;

                        case DataType.Long:
                            telemetry.Value = dataReader.GetInt64(dataReader.GetOrdinal(keyname.Replace("*", "value_long")));
                            break;

                        case DataType.Double:
                            telemetry.Value = dataReader.GetDouble(dataReader.GetOrdinal(keyname.Replace("*", "value_double")));
                            break;

                        case DataType.Json:
                        case DataType.XML:
                        case DataType.Binary:
                            telemetry.Value = dataReader.GetString(dataReader.GetOrdinal(keyname.Replace("*", "value_string")));
                            break;

                        case DataType.DateTime:
                            telemetry.Value = dataReader.GetDateTime(dataReader.GetOrdinal(keyname.Replace("*", "value_datetime")));
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString(), $"{telemetry.KeyName}遇到{ex.Message}, sql:{sql}");
                }
                if (!string.IsNullOrEmpty(telemetry.KeyName))
                {
                    dt.Add(telemetry);
                }
            }
            return dt;
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            if (_taos.State != System.Data.ConnectionState.Open) _taos.Open();
            IEnumerable<string> kvs = from k in keys
                                      select $" keyname = '{k}' ";
            string sql = $"select last_row(*),keyname from telemetrydata where deviceid='{deviceId:N}' and ({string.Join("or", kvs)}) group by deviceid,keyname";
            List<TelemetryDataDto> dt = SqlToTDD(_taos, sql, "last_row(*)");
            return Task.FromResult(dt);
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

        public async Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(PlayloadData msg)
        {
            bool result = false;
            List<TelemetryData> telemetries = new List<TelemetryData>();

            try
            {
                List<string> lst = new List<string>();
                msg.MsgBody.ToList().ForEach(kp =>
                    {
                        if (kp.Value != null)
                        {
                            TelemetryData tdata = new TelemetryData() { DateTime = msg.ts, DeviceId = msg.DeviceId, KeyName = kp.Key, Value_DateTime = DateTime.UnixEpoch };
                            tdata.FillKVToMe(kp);
                            string _type = "";
                            string _value = "";
                            bool _hasvalue = true;
                            // value_boolean bool, value_string binary(4096), value_long bigint,value_datetime timestamp,value_double double,value_json binary(4096) ,value_xml binary
                            switch (tdata.Type)
                            {
                                case DataType.Boolean:
                                    _type = "value_boolean";
                                    _value = tdata.Value_Boolean.GetValueOrDefault().ToString().ToLower();
                                    _hasvalue = tdata.Value_Boolean.HasValue;
                                    break;

                                case DataType.String:
                                    _type = "value_string";
                                    _value = $"'{tdata.Value_String?.Replace("'", "\\'")}'";
                                    break;

                                case DataType.Long:
                                    _type = "value_long";
                                    _value = $"{tdata.Value_Long}";
                                    _hasvalue = tdata.Value_Long.HasValue;
                                    break;

                                case DataType.Double:
                                    _type = "value_double";
                                    _value = $"{tdata.Value_Double}";
                                    _hasvalue = tdata.Value_Double.HasValue;
                                    break;

                                case DataType.Json://td 一条记录16kb , 因此为了写更多数据， 我们json  xml binary 全部使用 string
                                    _type = "value_string";
                                    _value = $"'{tdata.Value_Json?.Replace("'", "\\'")}'";
                                    break;

                                case DataType.XML:
                                    _type = "value_string";
                                    _value = $"'{tdata.Value_XML?.Replace("'", "\\'")}'";
                                    break;

                                case DataType.Binary:
                                    _type = "value_string";
                                    _value = $"\"{Hex.BytesToHex(tdata.Value_Binary)}\"";
                                    break;

                                case DataType.DateTime:
                                    _type = "value_datetime";
                                    _value = $"{tdata.Value_DateTime?.Subtract(DateTime.UnixEpoch).TotalMilliseconds}";
                                    _hasvalue = tdata.Value_DateTime.HasValue;
                                    break;

                                default:
                                    break;
                            }
                            if (_hasvalue)
                            {
                                string vals = $"device_{tdata.DeviceId:N}_{Pinyin4Net.GetPinyin(tdata.KeyName, PinyinFormat.WITHOUT_TONE).Replace(" ", string.Empty).Replace("@", string.Empty).Replace(":", string.Empty)} USING telemetrydata TAGS('{tdata.DeviceId:N}','{tdata.KeyName}')  (ts,value_type,{_type}) values (now,{(int)tdata.Type},{_value})";
                                lst.Add(vals);
                                telemetries.Add(tdata);
                            }
                        }
                    });

                if (_taos.State != System.Data.ConnectionState.Open) _taos.Open();
                var cmd = _taos.CreateCommand($"INSERT INTO {string.Join("\r\n", lst)}");
                _logger.LogInformation(cmd.CommandText);
                int dt = await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation($"数据入库完成,共数据{lst.Count}条，写入{dt}条");
            }
            catch (TaosException ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.ErrorCode}-{ex.Message} {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
            }
            return (result, telemetries);
        }

        public async Task<DataType?> GetTelemetryDataType(Guid deviceId, string key)
        {
            if (_taos.State != System.Data.ConnectionState.Open) _taos.Open();
            string sql = $"select last_row(value_type) from telemetrydata where deviceid='{deviceId:N}' and keyname='{key}'";
            var temp =await _taos.CreateCommand(sql).ExecuteScalarAsync();
            if(temp==null || !int.TryParse(temp.ToString(),out int type)) return null; 
            return (DataType)type;
        }

        private async Task<List<TelemetryDataDto>> AggregateTelemetryAsync(Guid deviceId, string key, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            //获取最新的数据
            var lastTemp = await GetTelemetryDataType(deviceId, key);
            if (lastTemp == null) return [];
            if (aggregate.Equals(Aggregate.None))
            {
                return await GetAllData(deviceId, key, begin, end);
            }
            switch (lastTemp.Value)
            {
                case DataType.Double:
                case DataType.Long:
                    return await AggregateAsync(deviceId, key, lastTemp.Value, begin, end, every, aggregate);
                default:
                    return await GetAllData(deviceId, key, begin, end);
            }

        }

        private Task<List<TelemetryDataDto>> GetAllData(Guid deviceId, string key, DateTime begin, DateTime end)
        {
            if (_taos.State != System.Data.ConnectionState.Open) _taos.Open();
            string sql = $"select  * from telemetrydata where ts >='{begin:yyyy-MM-dd HH:mm:ss.fff}' and ts <='{end:yyyy-MM-dd HH:mm:ss.fff}' and deviceid='{deviceId:N}' and keyname='{key}' ";
            List<TelemetryDataDto> dtx = SqlToTDD(_taos, sql, "*");
            return Task.FromResult(dtx);
        }

        private async Task<List<TelemetryDataDto>> AggregateAsync(Guid deviceId, string key, DataType dataType, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            var sql = new StringBuilder();
            sql.AppendLine($"select _wend as Time, ");
            // 根据聚合类型生成 SQL 聚合函数
            sql.AppendLine(aggregate switch
            {
                Aggregate.Mean => $"avg({GetFiledName(dataType)}) ",
                Aggregate.Max => $"max({GetFiledName(dataType)}) ",
                Aggregate.Min => $"min({GetFiledName(dataType)}) ",
                Aggregate.Sum => $"sum({GetFiledName(dataType)}) ",
                Aggregate.First => $"first({GetFiledName(dataType)}) ",
                Aggregate.Last => $"last({GetFiledName(dataType)}) ",
                Aggregate.Median => $"APERCENTILE({GetFiledName(dataType)},50) ",
                Aggregate.None => "",
                _ => throw new ArgumentOutOfRangeException(nameof(aggregate), aggregate, null)
            });
            sql.AppendLine($" as v from iotsharp.telemetrydata where ts >='{begin:yyyy-MM-dd HH:mm:ss.fff}' and ts <='{end:yyyy-MM-dd HH:mm:ss.fff}' and deviceid='{deviceId:N}' and keyname='{key}'");
            sql.AppendLine($"interval ({every.TotalMilliseconds}a)");
            TaosDataReader dataReader = await _taos.CreateCommand(sql.ToString()).ExecuteReaderAsync();
            var results = new List<TelemetryDataDto>();
            while (dataReader.Read())
            {
                var time = dataReader.GetDateTime(dataReader.GetOrdinal("time"));
                var resultValue = dataReader.GetDouble(dataReader.GetOrdinal("v"));
                results.Add(new TelemetryDataDto
                {
                    DateTime = time,
                    KeyName = key,
                    DataType = dataType,
                    Value = resultValue,
                });
            }
            return results;
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
