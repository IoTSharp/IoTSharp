using hyjiacan.py4n;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Utilities.Encoders;
using PinusDB.Data;
using Silkier;
using Silkier.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace IoTSharp.Storage
{

    public class PinusStorage : IStorage
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly IServiceScope scope;
        private readonly ObjectPool<PinusConnection> _pinuspool;
        public PinusStorage(ILogger<PinusStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options,   ObjectPool<PinusConnection> pinuspool
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            scope = scopeFactor.CreateScope();
            _pinuspool = pinuspool;
        }
        private bool dbisok = false;
        private bool CheckDataBase()
        {
            if (!dbisok)
            {
                dbisok = Retry.RetryOnAny(10, f =>
                           {
                               PinusConnection _pinus = _pinuspool.Get();
                               {
                                   var _pinusBuilder = new PinusConnectionStringBuilder(_pinus.ConnectionString);
                                   if (_pinus.State != System.Data.ConnectionState.Open) _pinus.Open();
                                   if (_pinus.CreateCommand("select count(*) from sys_table where tabname ='telemetrydata'").ExecuteScalar() as long? == 1)
                                   {
                                       _pinus.CreateCommand($"DROP TABLE telemetrydata").ExecuteNonQuery();
                                       _pinus.CreateCommand($"DELETE FROM sys_dev WHERE tabname='telemetrydata'").ExecuteNonQuery();
                                   }
                                   _pinus.CreateCommand("CREATE TABLE telemetrydata (devid bigint,tstamp datetime,keyname string , value_type  tinyint,value_boolean bool, value_string string, value_long bigint,value_datetime datetime,value_double double)")
                                      .ExecuteNonQuery();
                                   dbisok = true;
                                   _pinuspool.Return(_pinus);
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
            PinusConnection _pinus = _pinuspool.Get();
            _pinus.ChangeDatabase(_pinus.Database);
            if (_pinus.State != System.Data.ConnectionState.Open) _pinus.Open();
            //https://github.com/Pinusdata/TDengine/issues/4269
            string sql = $"select last_row(*) from telemetrydata where deviceid='{deviceId:N}' group by deviceid,keyname";
            List<TelemetryDataDto> dt = SqlToTDD(_pinus, sql, "last_row(", ")", string.Empty);
            _pinuspool.Return(_pinus);
            return Task.FromResult(dt);


        }
        /// <summary>
        /// 转换获取到的值
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <param name="keyname"></param>
        /// <returns></returns>
        /// <exception cref="https://github.com/Pinusdata/TDengine/issues/4269">务必注意此bug</exception>
        private List<TelemetryDataDto> SqlToTDD(PinusConnection db, string sql, string prefix, string suffix, string keyname)
        {
            List<TelemetryDataDto> dt = new List<TelemetryDataDto>();
            PinusDataReader dataReader = db.CreateCommand(sql).ExecuteReader();
            while (dataReader.Read())
            {
                TelemetryDataDto telemetry = new TelemetryDataDto();
                try
                {
                    int idx = dataReader.GetOrdinal($"{prefix}value_type{suffix}");
                    byte  datatype;
                    if (dataReader.FieldCount > idx && idx >= 0)
                    {
                        datatype =dataReader.GetByte(idx);
                    }
                    else
                    {
                        throw new Exception($"字段{prefix}value_type{suffix}的Index={idx}小于0或者大于FieldCount{dataReader.FieldCount},更多信息请访问 HelpLink") { HelpLink= "https://github.com/Pinusdata/TDengine/issues/4269" };
                    }

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
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{telemetry.KeyName}遇到{ex.Message}, sql:{sql}");
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
           PinusConnection _pinus = _pinuspool.Get();
            if (_pinus.State != System.Data.ConnectionState.Open) _pinus.Open();
            IEnumerable<string> kvs = from k in keys
                                      select $" keyname = '{k}' ";
            string sql = $"select last_row(*) from telemetrydata where deviceid='{deviceId:N}' and ({string.Join("or", kvs) }) group by deviceid,keyname";
            List<TelemetryDataDto> dt = SqlToTDD(_pinus, sql, "last_row(", ")", string.Empty);
            _pinuspool.Return(_pinus);
            return Task.FromResult(dt);

        }
        private List<TelemetryDataDto> SQLToDTByDate(DateTime begin, DateTime end, PinusConnection db, string sql)
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
            PinusConnection _pinus = _pinuspool.Get();
            if (_pinus.State != System.Data.ConnectionState.Open) _pinus.Open();
            IEnumerable<string> kvs = from k in keys
                                          select $" keyname = '{k}' ";
                string sql = $"select  tbname,keyname  from telemetrydata where deviceid='{deviceId:N}'  and ({string.Join("or", kvs) })  ";
                List<TelemetryDataDto> dt = SQLToDTByDate(begin, end, _pinus, sql);
            _pinuspool.Return(_pinus);
            return Task.FromResult(dt);
            
        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, DateTime begin)
        {
            return LoadTelemetryAsync(deviceId, begin, DateTime.Now);
        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, DateTime begin, DateTime end)
        {
            PinusConnection _pinus = _pinuspool.Get();
            if (_pinus.State != System.Data.ConnectionState.Open) _pinus.Open();
            string sql = $"select  tbname,keyname  from telemetrydata where deviceid='{deviceId:N}'";
                List<TelemetryDataDto> dt = SQLToDTByDate(begin, end, _pinus, sql);
            _pinuspool.Return(_pinus);
            return Task.FromResult(dt);
        }

        public   Task<bool> StoreTelemetryAsync(RawMsg msg)
        {
            bool result = false;
            try
            {
                CheckDataBase();
                List<string> lst = new List<string>();
                List<PinusParameter> parameters = new List<PinusParameter>();
                PinusConnection _pinus = _pinuspool.Get();
                if (_pinus.State != System.Data.ConnectionState.Open) _pinus.Open();
                long? devid = GetDevid(msg, _pinus);
                if (!devid.HasValue)
                {
                    long? maxid = _pinus.CreateCommand("select  max(devid)  from sys_dev  where tabname='telemetrydata'").ExecuteScalar() as long?;
                    long currdev = (long)(maxid + 1);
                    _pinus.CreateCommand($"INSERT INTO sys_dev(tabname, devname,devid) VALUES('telemetrydata','{msg.DeviceId}',{maxid + 1})").ExecuteNonQuery();
                    devid = GetDevid(msg, _pinus);
                }

                msg.MsgBody.ToList().ForEach(kp =>
                {
                    if (kp.Value != null)
                    {
                        TelemetryData tdata = new TelemetryData() { DateTime = DateTime.Now, DeviceId = msg.DeviceId, KeyName = kp.Key, Value_DateTime = new DateTime(1970, 1, 1) };
                        tdata.FillKVToMe(kp);
                        string _type = "";
                        var cmd = _pinus.CreateCommand();
                        switch (tdata.Type)
                        {
                            case DataType.Boolean:
                                _type = "value_boolean";
                                cmd.Parameters.Add(new PinusParameter(_type, tdata.Value_Boolean));
                                break;
                            case DataType.String:
                                _type = "value_string";
                                cmd.Parameters.Add(new PinusParameter(_type, tdata.Value_String));
                                break;
                            case DataType.Long:
                                _type = "value_long";
                                cmd.Parameters.Add(new PinusParameter(_type, tdata.Value_Long));
                                break;
                            case DataType.Double:
                                _type = "value_double";
                                cmd.Parameters.Add(new PinusParameter(_type, tdata.Value_Double));
                                break;
                            case DataType.Json:
                                _type = "value_string";
                                cmd.Parameters.Add(new PinusParameter(_type, tdata.Value_String));
                                break;
                            case DataType.XML:
                                _type = "value_string";
                                cmd.Parameters.Add(new PinusParameter(_type, tdata.Value_XML));
                                break;
                            case DataType.Binary:
                                _type = "value_string";
                                cmd.Parameters.Add(new PinusParameter(_type, Hex.ToHexString(tdata.Value_Binary)));
                                break;
                            case DataType.DateTime:
                                _type = "value_datetime";
                                cmd.Parameters.Add(new PinusParameter(_type, tdata.Value_DateTime));
                                break;
                            default:
                                break;
                        }
                        cmd.CommandText = $"INSERT INTO telemetrydata(devid,tstamp,{_type}) VALUES({devid}, now(), @{_type})";
                        _logger.LogInformation(cmd.CommandText);
                        try
                        {
                            int dt = cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message}");
                        }
                    }
                });
                _pinuspool.Return(_pinus);
                //_logger.LogInformation($"数据入库完成,共数据{lst.Count}条，写入{dt}条");

            }
            catch (PinusException ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.ErrorCode}-{ex.Message} {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
            }
            return  Task.FromResult(result);
        }

        private static long? GetDevid(RawMsg msg, PinusConnection _pinus)
        {
            return _pinus.CreateCommand($"select devid from sys_dev where tabname='telemetrydata' and devname='{msg.DeviceId}'").ExecuteScalar() as long?;
        }
    }
}