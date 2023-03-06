using hyjiacan.py4n;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Taos;
using IoTSharp.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

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

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            if (_taos.State != System.Data.ConnectionState.Open) _taos.Open();
            string sql = string.Empty;

            if (!string.IsNullOrEmpty(keys))
            {
                IEnumerable<string> kvs = from k in keys.Split(';', ',')
                                          select $" keyname = '{k}' ";
                sql = $"select * from telemetrydata where ts >='{begin:yyyy-MM-dd HH:mm:ss.fff}' and ts <='{end:yyyy-MM-dd HH:mm:ss.fff}' and deviceid='{deviceId:N}'  and ({string.Join("or", kvs)})  ";
            }
            else
            {
                sql = $"select  * from telemetrydata where ts >='{begin:yyyy-MM-dd HH:mm:ss.fff}' and ts <='{end:yyyy-MM-dd HH:mm:ss.fff}' and deviceid='{deviceId:N}'  ";
            }
            List<TelemetryDataDto> dtx = SqlToTDD(_taos, sql, "*");
            return Task.FromResult(dtx);
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
                                string vals = $"device_{tdata.DeviceId:N}_{Pinyin4Net.GetPinyin(tdata.KeyName, PinyinFormat.WITHOUT_TONE).Replace(" ", string.Empty).Replace("@", string.Empty)} USING telemetrydata TAGS('{tdata.DeviceId:N}','{tdata.KeyName}')  (ts,value_type,{_type}) values (now,{(int)tdata.Type},{_value})";
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
    }
}