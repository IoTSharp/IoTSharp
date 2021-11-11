using hyjiacan.py4n;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
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

    public class PinusDBStorage : IStorage
    {
        private readonly ILogger _logger;
        private readonly ObjectPool<PinusConnection> _pinuspool;
        public PinusDBStorage(ILogger<PinusDBStorage> logger, ObjectPool<PinusConnection> pinuspool
            )
        {
            _logger = logger;
            _pinuspool = pinuspool;
        }


        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {
            PinusConnection _pinus = _pinuspool.Get();
            string sql = $"select  devid,devname,expand  from sys_dev  where tabname='telemetrydata_{deviceId:N}'";
            List<TelemetryDataDto> dt = SqlToTDD(_pinus, sql, deviceId);
            _pinuspool.Return(_pinus);
            return Task.FromResult(dt);


        }
        /// <summary>
        /// 转换获取到的值
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="devid"></param>
        /// <returns></returns>
        private List<TelemetryDataDto> SqlToTDD(PinusConnection db, string sql, Guid devid)
        {
            List<TelemetryDataDto> dt = new List<TelemetryDataDto>();
            var kts = db.CreateCommand(sql).ExecuteReader().ToList<(long keyid, string keyname, string datatype)>();
            kts.ForEach(kf =>
            {
                TelemetryDataDto telemetry = new TelemetryDataDto() { KeyName = kf.keyname };
                try
                {
                    DataType datatype = (DataType)Convert.ToInt32(kf.datatype);
                    var queryvalue = db.CreateCommand($"SELECT {datatype.ToFieldName()},tstamp from {GetDevTableName(devid)}   where  devid ={kf.keyid} order by tstamp desc  limit 1 ")
                                        .ExecuteReader().ToList<(object obj, DateTime dt)>();
                    telemetry.AttachValue(datatype, queryvalue.FirstOrDefault().obj);
                    telemetry.DateTime = queryvalue.FirstOrDefault().dt;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{telemetry.KeyName}遇到{ex.Message}, sql:{sql}");
                }
                if (!string.IsNullOrEmpty(telemetry.KeyName))
                {
                    dt.Add(telemetry);
                }
            });
            return dt;
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            PinusConnection _pinus = _pinuspool.Get();
            List<TelemetryDataDto> dt=null;
            try
            {
                string sql = $"select  devid,devname,expand  from sys_dev  where tabname='telemetrydata_{deviceId:N}' and   in ('{ string.Join("','", keys.Split(';', ','))}')";
                dt = SqlToTDD(_pinus, sql, deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"LoadTelemetryAsync({deviceId},{keys}){ex.Message}");
            }
            finally
            {
                _pinuspool.Return(_pinus);
            }
            _pinuspool.Return(_pinus);
            return Task.FromResult(dt);

        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin)
        {
            return LoadTelemetryAsync(deviceId, keys, begin, DateTime.Now);
        }


        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end)
        {
            PinusConnection _pinus = _pinuspool.Get();
            List<TelemetryDataDto> dt = null;
            try
            {
                string sql = $"select  devid,devname,expand  from sys_dev  where tabname='telemetrydata_{deviceId:N}' and   in ('{ string.Join("','", keys.Split(';', ','))}')";
                dt = SQLToDTByDate(begin, end, _pinus, sql, deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"LoadTelemetryAsync({deviceId},{keys},{begin},{end}){ex.Message}");
            }
            finally
            {
                _pinuspool.Return(_pinus);
            }
          
            return Task.FromResult(dt);

        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, DateTime begin)
        {
            return LoadTelemetryAsync(deviceId, begin, DateTime.Now);
        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, DateTime begin, DateTime end)
        {
            PinusConnection _pinus = _pinuspool.Get();
            List<TelemetryDataDto> dt = null;
            try
            {
                string sql = $"select  tbname,keyname  from telemetrydata where deviceid='{deviceId:N}'";
                dt = SQLToDTByDate(begin, end, _pinus, sql, deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"LoadTelemetryAsync({deviceId}, {begin},{end}){ex.Message}");
            }
            finally
            {
                _pinuspool.Return(_pinus);
            }
            return Task.FromResult(dt);
        }
        private List<TelemetryDataDto> SQLToDTByDate(DateTime begin, DateTime end, PinusConnection db, string sql, Guid devid)
        {
            List<TelemetryDataDto> dt = new List<TelemetryDataDto>();
            var kts = db.CreateCommand(sql).ExecuteReader().ToList<(long keyid, string keyname, string datatype)>();
            kts.ForEach(kf =>
            {
                TelemetryDataDto telemetry = new TelemetryDataDto() { KeyName = kf.keyname };
                try
                {
                    DataType datatype = (DataType)Convert.ToInt32(kf.datatype);
                    var scmd = db.CreateCommand($"SELECT {datatype.ToFieldName()},tstamp from {GetDevTableName(devid)}   where  devid ={kf.keyid} and tstamp>=@begin and tstamp <@end order by tstamp desc  ");
                    scmd.Parameters.AddWithValue("@begin", begin);
                    scmd.Parameters.AddWithValue("@end", end);
                    var queryvalue = scmd.ExecuteReader().ToList<(object obj, DateTime dt)>();
                    telemetry.AttachValue(datatype, queryvalue.FirstOrDefault().obj);
                    telemetry.DateTime = queryvalue.FirstOrDefault().dt;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{telemetry.KeyName}遇到{ex.Message}, sql:{sql}");
                }
                if (!string.IsNullOrEmpty(telemetry.KeyName))
                {
                    dt.Add(telemetry);
                }
            });
            return dt;
        }
        public Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(RawMsg msg)
        {
            bool result = false;
            List<TelemetryData> telemetries = new List<TelemetryData>();
            PinusConnection _pinus = _pinuspool.Get();
            try
            {
                string tablename = GetDevTableName(msg);
                var _havedev = _pinus.CreateCommand($"select count(*) from sys_table where tabname =  '{tablename}'").ExecuteScalar() as long?;
                if ((long)_havedev == 0)
                {
                    _pinus.CreateCommand($"CREATE TABLE {tablename} (devid bigint,tstamp datetime,value_type  tinyint,value_boolean bool, value_string string, value_long bigint,value_datetime datetime,value_double double)").ExecuteNonQuery();
                }

                msg.MsgBody.ToList().ForEach(kp =>
                {
                    if (kp.Value != null)
                    {
                        List<PinusParameter> parameters = new List<PinusParameter>();
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
                        long? _keyid = GetKeyId(msg.DeviceId, kp.Key, _pinus);
                        if (!_keyid.HasValue)
                        {
                            long? maxid = _pinus.CreateCommand($"select  max(devid)  from sys_dev  where tabname='{tablename}'").ExecuteScalar() as long?;
                            long currdev = maxid.GetValueOrDefault() + 1;
                            _pinus.CreateCommand($"INSERT INTO sys_dev(tabname, devname,devid,expand) VALUES('{tablename}','{kp.Key}',{currdev},'{(int)tdata.Type}')").ExecuteNonQuery();
                            _keyid = GetKeyId(msg.DeviceId, kp.Key, _pinus);
                        }
                        cmd.CommandText = $"INSERT INTO {tablename} (devid,tstamp,value_type,{_type}) VALUES({_keyid}, now(), {(int)tdata.Type}, @{_type})";
                        _logger.LogInformation(cmd.CommandText);
                        try
                        {
                            int dt = cmd.ExecuteNonQuery();
                            result = dt > 0;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message}");
                        }
                        telemetries.Add(tdata);
                    }
                  
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
            }
            finally
            {
                _pinuspool.Return(_pinus);
            }
            return Task.FromResult((result, telemetries));
        }

        private static string GetDevTableName(RawMsg msg) => $"telemetrydata_{msg.DeviceId:N}";
        private static string GetDevTableName(Guid devid) => $"telemetrydata_{devid:N}";

        private long? GetKeyId(Guid devid, string keyname, PinusConnection _pinus)
        {
            return _pinus.CreateCommand($"select devid from sys_dev where tabname='{GetDevTableName(devid)}' and devname='{keyname}'").ExecuteScalar() as long?;
        }
    }
}