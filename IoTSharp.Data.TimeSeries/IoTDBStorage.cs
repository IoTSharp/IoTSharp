using Apache.IoTDB.Data;
using Apache.IoTDB.DataStructure;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Storage
{
    public class IoTDBStorage : IStorage
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly  Apache.IoTDB.SessionPool   _session;
        private readonly IoTDBConnection _ioTDB;
        private string _StorageGroupName;
        private bool dbisok = false;

        public IoTDBStorage(ILogger<IoTDBStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options, Apache.IoTDB.Data.IoTDBConnection ioTDB
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            _session = ioTDB.SessionPool;
            _ioTDB = ioTDB;
            CheckDataBase();
            var str = options.Value.ConnectionStrings["TelemetryStorage"];
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            str.Split(';', StringSplitOptions.RemoveEmptyEntries).ForEach(f =>
            {
                var kv = f.Split('=');
                pairs.TryAdd(key: kv[0], value: kv[1]);
            });
            _StorageGroupName = pairs.GetValueOrDefault("DefaultGroupName") ?? "iotsharp";
            var groupName = $"root.{_StorageGroupName}";
            using var query =  _session.ExecuteQueryStatementAsync($"show storage group {groupName}");//判断存储组是否已经存在
            if(query.Result.HasNext())
            {
                //存储组已经存在，无需处理
            }
            else
            {
                _session.SetStorageGroup(groupName);
            }
        }

        private bool CheckDataBase()
        {
            if (!_session.IsOpen())
            {
                dbisok = Retry.RetryOnAny(10, f =>
                {
                    _session.Open().Wait(TimeSpan.FromMilliseconds(100));
                    return true;
                }, ef =>
                {
                    _logger.LogError(ef.ex, $" open iotdb error。{ef.current}次失败{ef.ex.Message} {ef.ex.InnerException?.Message} ");
                });
            }
            return dbisok;
        }

        private async Task<List<(string key, string type)>> GetIotDbMeasurementPointInfor(string device,string measureKeys="*")
        {
            //show timeseries root.iotsharp.8984003f7016487db7f26528b246198f.**\
            var keylist = measureKeys.Split(';', ',').ToList();
            var sql = $"show timeseries  {device}.**";
            List<(string key, string type)> dt = new List<(string key, string type)>();
              var query = await _session.ExecuteQueryStatementAsync(sql);
            while (query.HasNext())
            {
                var next = query.Next();
                var values = next.Values;
                var measurePointName = values[0].ToString().Replace(@$"{device}.", "");
                if(measureKeys=="*"|| keylist.Contains(measurePointName))
                    dt.Add((measurePointName, Convert.ToString( values[3])));
            }
            return dt;
        }
        private dynamic GetIotSharpValue(object v,string iotDataType)
        {
            dynamic result=null;
            if (v.ToString().ToUpper() == "NULL")
                result= null;
            switch (iotDataType.ToUpper())
            {
                case "DOUBLE":
                    result= double.Parse(v.ToString());
                    break;
                case "FLOAT":
                    result= float.Parse(v.ToString());
                    break;
                case "TEXT":
                    result= v.ToString();
                    break;
                case "INT64":
                    result= Int64.Parse(v.ToString());
                    break;
                case "IN32":
                    result= Int32.Parse(v.ToString());
                    break;
                case "BOOLEAN":
                    result= bool.Parse(v.ToString());
                    break;
                default:
                    throw new Exception($"不支持的IotDB数据类型：{iotDataType}");
            }
            return result;
   
        }
        private  DataType GetIoTSharpDataType(string iotDataType)
        {
            if (iotDataType.ToUpper() == "DOUBLE")
                return  DataType.Double;
            else if (iotDataType.ToUpper() == "FLOAT")
            {
                return  DataType.Double;
            }
            else if (iotDataType.ToUpper() == "TEXT")
            {
                return  DataType.String;
            }
            else if (iotDataType.ToUpper() == "INT64")
            {
                return  DataType.Long;
            }
            else if (iotDataType.ToUpper() == "INT32")
            {
                return  DataType.Long;
            }
            else if (iotDataType.ToUpper() == "BOOLEAN")
            {
                return  DataType.Boolean;
            }
            else
                throw new Exception($"不支持的IotDB数据类型：{iotDataType}");
        }



        public async Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {
            return await GetTelemetryLatest(deviceId,"*");
        }



        /// <summary>
        /// 将IotSharp中的聚合转换为IotDB中的聚合函数名称
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        private string SharpAggregate2IotDBAggregate(Aggregate aggregate)
        {
            var result = "";
            switch (aggregate)
            {
                case Aggregate.Mean:
                    result= "AVG";
                    break;
                case Aggregate.Last:
                    result= "LAST_VALUE";
                    break;
                case Aggregate.First:
                    result= "FIRST_VALUE";
                    break;
                case Aggregate.Max:
                    result= "MAX_VALUE";
                    break;
                case Aggregate.Min:
                    result= "MIN_VALUE";
                    break;
                case Aggregate.Sum:
                    result= "SUM";
                    break;
                default:
                    result = "";
                    break;
            }
            return result;
        }
        public async Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            //select count(status), max_value(temperature) from root.ln.wf01.wt01 group by ([2017-11-01T00:00:00, 2017-11-07T23:00:00),1d);
            //select avg(v2) from root.iotsharp.8984003f7016487db7f26528b246198f group by ([2022-06-10 00:00:00, 2022-06-10 23:00:00),1000ms);
            //2022-06-08T00:56:04.452+08:00
            List<TelemetryDataDto> dt = new List<TelemetryDataDto>();
            var aggStr = SharpAggregate2IotDBAggregate(aggregate);
            if (!string.IsNullOrEmpty(aggStr))//
            {
                if (string.IsNullOrEmpty(keys))
                    keys = "*";

                string device = $"root.{_StorageGroupName}.{deviceId:N}";
                var sb = new StringBuilder();
                var MeasurementPointList = await this.GetIotDbMeasurementPointInfor(device, keys);
                var selectItemStr = string.Join(",", from m in MeasurementPointList select $"{aggStr}({m.key})");
                sb.AppendLine($@"select {selectItemStr} from {device} where Time >= {begin:yyyy-MM-ddTHH:mm:ss.fff+00:00} and Time < {end:yyyy-MM-ddTHH:mm:ss.fff+00:00}  ");

                if (every > TimeSpan.Zero)
                {
                    sb.AppendLine($@" group by ([{begin:yyyy-MM-ddTHH:mm:ss.fff+00:00}, {end:yyyy-MM-ddTHH:mm:ss.fff+00:00}),{(long)every.TotalMilliseconds}ms)");
                }

                _logger.LogInformation(sb.ToString());
                 var query = await _session.ExecuteQueryStatementAsync(sb.ToString());
                while (query.HasNext())
                {
                    var next = query.Next();
                    var values = next.Values;
                    var time = next.GetDateTime();
                    for (int i = 0; i < MeasurementPointList.Count; i++)
                    {
                        TelemetryDataDto telemetry = new TelemetryDataDto()
                        {
                            DateTime = time, //传入的时间为UTC+0,推给web，转为UTC+8
                            KeyName = MeasurementPointList[i].key,
                            Value = GetIotSharpValue(values[i], MeasurementPointList[i].type),
                            DataType = GetIoTSharpDataType(MeasurementPointList[i].type),
                        };
                        dt.Add(telemetry);
                    }
                }

            }


            return dt;
        }



        public async Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {

            string device = $"root.{_StorageGroupName}.{deviceId:N}";
            if (string.IsNullOrEmpty(keys))
                keys = "*";
            var selectItemStr = string.Join(",", from m in keys.Split(',') select $"last({m})");

            var sql = $"select {selectItemStr} from {device}";
            
            List<TelemetryDataDto> dt = new List<TelemetryDataDto>();
             var query = await _session.ExecuteQueryStatementAsync(sql);

            while (query.HasNext())
            {

                var next = query.Next();
                var values = next.Values;
                var time = next.GetDateTime();
                if (values != null && values.Count>=3)
                {
                    TelemetryDataDto telemetry = new TelemetryDataDto()
                    {
                        DateTime = next.GetDateTime(),
                        KeyName = values[0].ToString().Replace(@$"{device}.", ""),
                        Value = GetIotSharpValue(values[1], values[2].ToString()),
                        DataType = GetIoTSharpDataType(values[2].ToString()),
                    };
                    dt.Add(telemetry);
                }
         
            }
            return dt;

        }

        public async Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(PlayloadData msg)
        {

            bool result = false;
            List<TelemetryData> telemetries = new List<TelemetryData>();
            try
            {
                CheckDataBase();
                string device = $"root.{_StorageGroupName}.{msg.DeviceId:N}";
                List<object> values = new List<object>();
                msg.MsgBody.ToList().ForEach(kp =>
                {
                    if (kp.Value != null)
                    {
                        TelemetryData tdata = new TelemetryData() { DateTime = msg.ts, DeviceId = msg.DeviceId, KeyName = kp.Key, Value_DateTime = new DateTime(1970, 1, 1) };
                        tdata.FillKVToMe(kp);
                        object _value = null;
                        bool _hasvalue = true; switch (tdata.Type)
                        {
                            case  DataType.Boolean:
                                _value = tdata.Value_Boolean;
                                _hasvalue = tdata.Value_Boolean.HasValue;
                                break;
                            case  DataType.String:
                                _value = tdata.Value_String;
                                break;
                            case DataType.Long:
                                _value = tdata.Value_Long;
                                _hasvalue = tdata.Value_Long.HasValue;
                                break;
                            case DataType.Double:
                                _value = tdata.Value_Double;
                                _hasvalue = tdata.Value_Double.HasValue;
                                break;
                            case DataType.DateTime:
                                _value = tdata.Value_DateTime;
                                _hasvalue = tdata.Value_DateTime.HasValue;
                                break;
                            default:
                                break;
                        }
                        if (_hasvalue)
                        {
                            values.Add(_value);
                            telemetries.Add(tdata);
                        }
                    }
                });
                var record = new RowRecord(msg.ts, values, msg.MsgBody.Keys.ToList());
                var okCount = await _session.InsertRecordAsync(device, record);
                _logger.LogInformation($"数据入库完成，准备写入{values.Count}条数据，实际写入{okCount}条");
                result = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
            }
            return (result, telemetries);
        }
    }
}
