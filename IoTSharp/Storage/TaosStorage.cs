using hyjiacan.py4n;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.Queue;
using Maikebing.Data.Taos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Utilities.Encoders;
using Silkier;
using Silkier.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace IoTSharp.Storage
{

    public class TaosStorage : IStorage
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly IServiceScope scope;
        private readonly TaosConnectionStringBuilder _taosBuilder;
        public TaosStorage(ILogger<TaosStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            scope = scopeFactor.CreateScope();
            _taosBuilder = new TaosConnectionStringBuilder(_appSettings.ConnectionStrings["Taos"]);
        }
        private bool dbisok = false;
        private bool CheckDataBase()
        {
            if (!dbisok)
            {
                dbisok = Retry.RetryOnAny(10, f =>
                           {
                               using (TaosConnection _taos = new TaosConnection(_taosBuilder.ConnectionString))
                               {
                                   _taos.Open();
                                   _taos.CreateCommand($"CREATE DATABASE IF NOT EXISTS {_taosBuilder.DataBase} KEEP 365 DAYS 10 BLOCKS 4;").ExecuteNonQuery();
                                   _taos.ChangeDatabase(_taosBuilder.DataBase);
                                   _taos.CreateCommand("CREATE TABLE IF NOT EXISTS telemetrydata  (ts timestamp,value_type  tinyint, value_boolean bool, value_string binary(10240), value_long bigint,value_datetime timestamp,value_double double)   TAGS (deviceid binary(32),keyname binary(64));")
                                      .ExecuteNonQuery();
                                   dbisok = true;
                                   _taos.Close();
                               }
                               return true;
                           }, ef =>
                           {
                               _logger.LogError(ef.ex, $"CheckDataBase第{ef.current}次失败{ef.ex.Message} {ef.ex.InnerException?.Message} ");
                           });
            }
            return dbisok;
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {
            using (TaosConnection db = new TaosConnection(_taosBuilder.ConnectionString))
            {
                string sql = $"select last_row(*) from telemetrydata where deviceid='{deviceId:N}' group by deviceid,keyname";
                List<TelemetryDataDto> dt = SqlToTDD(db, sql, "last_row(", ")", string.Empty);
                return Task.FromResult(dt);
            }
        }

        private List<TelemetryDataDto> SqlToTDD(TaosConnection db, string sql, string prefix, string suffix, string keyname)
        {
            List<TelemetryDataDto> dt = new List<TelemetryDataDto>();
            TaosDataReader dataReader = db.CreateCommand(sql).ExecuteReader();
            while (dataReader.Read())
            {
                TelemetryDataDto telemetry = new TelemetryDataDto();
                byte datatype = (byte)dataReader[dataReader.GetOrdinal($"{prefix}value_type{suffix}")];
                if (string.IsNullOrEmpty(keyname))
                {
                    telemetry.KeyName = dataReader.GetString(dataReader.GetOrdinal("keyname"));
                }
                else
                {
                    telemetry.KeyName = keyname;
                }
                telemetry.DateTime = dataReader.GetDateTime(dataReader.GetOrdinal($"{prefix}ts{suffix}"));
                switch ((DataType)datatype)
                {
                    case DataType.Boolean:
                        telemetry.Value = dataReader.GetBoolean(dataReader.GetOrdinal($"{prefix}value_boolean{suffix}"));
                        break;
                    case DataType.String:
                        telemetry.Value = dataReader.GetString(dataReader.GetOrdinal($"{prefix}value_string{suffix}"));
                        break;
                    case DataType.Long:
                        telemetry.Value = dataReader.GetInt64(dataReader.GetOrdinal($"{prefix}value_long{suffix}"));
                        break;
                    case DataType.Double:
                        telemetry.Value = dataReader.GetDouble(dataReader.GetOrdinal($"{prefix}value_double{suffix}"));
                        break;
                    case DataType.Json:
                    case DataType.XML:
                    case DataType.Binary:
                        telemetry.Value = dataReader.GetString(dataReader.GetOrdinal($"{prefix}value_string{suffix}"));
                        break;
                    case DataType.DateTime:
                        telemetry.Value = dataReader.GetDateTime(dataReader.GetOrdinal($"{prefix}value_datetime{suffix}"));
                        break;
                    default:
                        break;
                }
                dt.Add(telemetry);
            }
            return dt;
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            using (TaosConnection db = new TaosConnection(_taosBuilder.ConnectionString))
            {
                IEnumerable<string> kvs = from k in keys
                                          select $" keyname = '{k}' ";
                string sql = $"select last_row(*) from telemetrydata where deviceid='{deviceId:N}' and ({string.Join("or", kvs) }) group by deviceid,keyname";
                List<TelemetryDataDto> dt = SqlToTDD(db, sql, "last_row(", ")", string.Empty);
                return Task.FromResult(dt);
            }
        }
        private List<TelemetryDataDto> SQLToDTByDate(DateTime begin, DateTime end, TaosConnection db, string sql)
        {
            List<TelemetryDataDto> dt = new List<TelemetryDataDto>();
            List<(string tbname, string keyname)> list = db.CreateCommand(sql).ExecuteReader().ToList<(string tbname, string keyname)>();
            foreach ((string tbname, string keyname) item in list)
            {
                string susql = $" select * from {item.tbname} where ts >={begin:yyyy-MM-dd HH:mm:ss.fff} and ts <={end:yyyy-MM-dd HH:mm:ss.fff}";
                List<TelemetryDataDto> dtx = SqlToTDD(db, susql, "", "", item.keyname);
                dt.AddRange(dtx);
            }
            return dt;
        }
        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin)
        {
            return LoadTelemetryAsync(deviceId, keys, begin, DateTime.Now);
        }


        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end)
        {
            using (TaosConnection db = new TaosConnection(_taosBuilder.ConnectionString))
            {
                IEnumerable<string> kvs = from k in keys
                                          select $" keyname = '{k}' ";
                string sql = $"select  tbname,keyname  from telemetrydata where deviceid='{deviceId:N}'  and ({string.Join("or", kvs) })  ";
                List<TelemetryDataDto> dt = SQLToDTByDate(begin, end, db, sql);
                return Task.FromResult(dt);
            }
        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, DateTime begin)
        {
            return LoadTelemetryAsync(deviceId, begin, DateTime.Now);
        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, DateTime begin, DateTime end)
        {
            using (TaosConnection db = new TaosConnection(_taosBuilder.ConnectionString))
            {
                string sql = $"select  tbname,keyname  from telemetrydata where deviceid='{deviceId:N}'";
                List<TelemetryDataDto> dt = SQLToDTByDate(begin, end, db, sql);
                return Task.FromResult(dt);
            }
        }

        public async Task<bool> StoreTelemetryAsync(RawMsg msg)
        {
            bool result = false;
            try
            {
                CheckDataBase();
                using (TaosConnection db = new TaosConnection(_taosBuilder.ConnectionString))
                {
                    List<string> lst = new List<string>();
                    msg.MsgBody.ToList().ForEach(kp =>
                        {
                            if (kp.Value != null)
                            {
                                TelemetryData tdata = new TelemetryData() { DateTime = DateTime.Now, DeviceId = msg.DeviceId, KeyName = kp.Key, Value_DateTime = new DateTime(1970, 1, 1) };
                                tdata.FillKVToMe(kp);
                                string _type = "";
                                string _value = "";
                                // value_boolean bool, value_string binary(4096), value_long bigint,value_datetime timestamp,value_double double,value_json binary(4096) ,value_xml binary
                                switch (tdata.Type)
                                {
                                    case DataType.Boolean:
                                        _type = "value_boolean";
                                        _value = tdata.Value_Boolean.ToString().ToLower();
                                        break;
                                    case DataType.String:
                                        _type = "value_string";
                                        _value = $"'{tdata.Value_String?.Replace("'", "\\'")}'";
                                        break;
                                    case DataType.Long:
                                        _type = "value_long";
                                        _value = $"{tdata.Value_Long}";
                                        break;
                                    case DataType.Double:
                                        _type = "value_double";
                                        _value = $"{tdata.Value_Double}";
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
                                        _value = $"\"{Hex.ToHexString(tdata.Value_Binary)}\"";
                                        break;
                                    case DataType.DateTime:
                                        _type = "value_datetime";
                                        _value = $"{tdata.Value_DateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds}";
                                        break;
                                    default:
                                        break;
                                }
                                string vals = $"device_{tdata.DeviceId:N}_{ Pinyin4Net.GetPinyin( tdata.KeyName, PinyinFormat.WITHOUT_TONE ).Replace(" ",string.Empty).Replace("@",string.Empty)} USING telemetrydata TAGS('{tdata.DeviceId:N}','{tdata.KeyName}')  (ts,value_type,{_type}) values (now,{(int)tdata.Type},{_value})";
                                lst.Add(vals);
                            }
                        });
                    await Retry.RetryOnAny(10, async f =>
                    {
                        db.Open();
                        var cmd = db.CreateCommand($"INSERT INTO {string.Join("\r\n", lst)}");
                        _logger.LogInformation(cmd.CommandText);
                        int dt = await cmd.ExecuteNonQueryAsync();
                        db.Close();
                        _logger.LogInformation($"数据入库完成,共数据{lst.Count}条，写入{dt}条");
                    }, ef =>
                     {
                         _logger.LogError(ef.ex, $"{msg.DeviceId}数据处理第{ef.current}次失败{ef.ex.Message} {ef.ex.InnerException?.Message} ");
                     });
                }
            }
            catch (TaosException ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.ErrorCode}-{ex.Message} {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
            }
            return result;
        }
    }
}