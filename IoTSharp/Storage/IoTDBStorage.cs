using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Salvini.IoTDB;
using Salvini.IoTDB.Data;
using Silkier;
using Silkier.Extensions;
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
        private readonly Session _session;
        private string _StorageGroupName;
        private bool dbisok = false;

        public IoTDBStorage(ILogger<IoTDBStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options, Salvini.IoTDB.Session session
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            _session = session;
            CheckDataBase();
            var str = options.Value.ConnectionStrings["TelemetryStorage"];
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            str.Split(';', StringSplitOptions.RemoveEmptyEntries).ForEach(f =>
            {
                var kv = f.Split('=');
                pairs.TryAdd(key: kv[0], value: kv[1]);
            });
            _StorageGroupName = pairs.GetValueOrDefault("DefaultGroupName") ?? "iotsharp";
            _session.CreateStorageGroup($"root.{_StorageGroupName}");
        }

        private bool CheckDataBase()
        {
            if (!_session.IsOpen)
            {
                dbisok = Retry.RetryOnAny(10, f =>
                {
                    _session.OpenAsync().Wait(TimeSpan.FromMilliseconds(100));
                    return true;
                }, ef =>
                {
                    _logger.LogError(ef.ex, $" open iotdb error。{ef.current}次失败{ef.ex.Message} {ef.ex.InnerException?.Message} ");
                });
            }
            return dbisok;
        }

        private async Task<List<(string key, string type)>> GetIotDbMeasurementPointInfor(string device)
        {
            //show timeseries root.iotsharp.8984003f7016487db7f26528b246198f.**\

            var sql = $"show timeseries  {device}.**";
            List<(string key, string type)> dt = new List<(string key, string type)>();
            using var query = await _session.ExecuteQueryStatementAsync(sql);
            while (query.HasNext())
            {
                var next = query.Next();
                var values = next.Values;

                dt.Add((values[0].ToString().Replace(@$"{device}.", ""), values[3]));
            }
            return dt;
        }

        private IoTSharp.Data.DataType GetIoTSharpDataType(string iotDataType)
        {
            if (iotDataType.ToUpper() == "DOUBLE")
                return Data.DataType.Double;
            else if (iotDataType.ToUpper() == "FLOAT")
            {
                return Data.DataType.Double;
            }
            else if (iotDataType.ToUpper() == "TEXT")
            {
                return Data.DataType.String;
            }
            else if (iotDataType.ToUpper() == "INT64")
            {
                return Data.DataType.Long;
            }
            else if (iotDataType.ToUpper() == "INT32")
            {
                return Data.DataType.Long;
            }
            else if (iotDataType.ToUpper() == "BOOLEAN")
            {
                return Data.DataType.Boolean;
            }
            else
                throw new Exception($"不支持的IotDB数据类型：{iotDataType}");
        }



        public async Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {

            string device = $"root.{_StorageGroupName}.{deviceId:N}";

            var sql = $"select last * from {device}";

            List<TelemetryDataDto> dt = new List<TelemetryDataDto>();
            using var query = await _session.ExecuteQueryStatementAsync(sql);

            while (query.HasNext())
            {

                var next = query.Next();
                var values = next.Values;
                var time = next.GetDateTime();

                TelemetryDataDto telemetry = new TelemetryDataDto()
                {
                    DateTime = next.GetDateTime(),
                    KeyName = values[0].ToString().Replace(@$"{device}.", ""),
                    Value = values[1].ToString(),
                    DataType = GetIoTSharpDataType(values[2].ToString()),
                };
                dt.Add(telemetry);
            }
            return dt;
        }



        /// <summary>
        /// 将IotSharp中的聚合转换为IotDB中的聚合函数名称
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        private string SharpAggregate2IotDBAggregate(Aggregate aggregate)
        {
            switch (aggregate)
            {
                case Aggregate.Mean:
                    return "AVG";
                    break;
                case Aggregate.Last:
                    return "LAST_VALUE";
                    break;
                case Aggregate.First:
                    return "FIRST_VALUE";
                    break;
                case Aggregate.Max:
                    return "MAX_VALUE";
                    break;
                case Aggregate.Min:
                    return "MIN_VALUE";
                    break;
                case Aggregate.Sum:
                    return "SUM";
                    break;
                default:
                    break;
            }
            return "";
        }
        public async Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            //select count(status), max_value(temperature) from root.ln.wf01.wt01 group by ([2017-11-01T00:00:00, 2017-11-07T23:00:00),1d);
            //select avg(v2) from root.iotsharp.8984003f7016487db7f26528b246198f group by ([2022-06-10 00:00:00, 2022-06-10 23:00:00),1000ms);

            List<TelemetryDataDto> dt = new List<TelemetryDataDto>();
            var aggStr = SharpAggregate2IotDBAggregate(aggregate);
            if (!string.IsNullOrEmpty(aggStr))//
            {

                string device = $"root.{_StorageGroupName}.{deviceId:N}";
                var sb = new StringBuilder();
                var MeasurementPointList = await this.GetIotDbMeasurementPointInfor(device);

                var selectItemStr = string.Join(",", from m in MeasurementPointList select $"{aggStr}({m.key})");

                if (!string.IsNullOrEmpty(keys))
                {
                    //待测试！！
                    IEnumerable<string> kvs = from k in keys.Split(';', ',') select $" keyname = '{k}' ";
                    sb.AppendLine($@"select {selectItemStr} from {device} where Time >={begin:yyyy-MM-dd HH:mm:ss.fff} and Time <={end:yyyy-MM-dd HH:mm:ss.fff}  and ({string.Join("or", kvs)})  ");


                }
                else
                {
                    sb.AppendLine($@"select {selectItemStr} from {device} where Time >= {begin:yyyy-MM-dd HH:mm:ss.fff} and Time < {end:yyyy-MM-dd HH:mm:ss.fff}  ");

                }
                if (every > TimeSpan.Zero)
                {
                    sb.AppendLine($@" group by ([{begin:yyyy-MM-dd HH:mm:ss.fff}, {end:yyyy-MM-dd HH:mm:ss.fff}),{(long)every.TotalMilliseconds}ms)");
                }

                _logger.LogInformation(sb.ToString());
                using var query = await _session.ExecuteQueryStatementAsync(sb.ToString());
                while (query.HasNext())
                {
                    var next = query.Next();
                    var values = next.Values;
                    var time = next.GetDateTime();
                    for (int i = 0; i < MeasurementPointList.Count; i++)
                    {
                        TelemetryDataDto telemetry = new TelemetryDataDto()
                        {
                            DateTime = time,
                            KeyName = MeasurementPointList[i].key,
                            Value = values[i],
                            DataType = GetIoTSharpDataType(MeasurementPointList[i].type),
                        };
                        dt.Add(telemetry);
                    }
                }

            }


            return dt;
        }



        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            //keys目前不确定格式，先直接返回GetTelemetryLatest；
            return GetTelemetryLatest(deviceId);
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
                        string _type = "";
                        object _value = null;
                        bool _hasvalue = true; switch (tdata.Type)
                        {
                            case IoTSharp.Data.DataType.Boolean:
                                _type = "value_boolean";
                                _value = tdata.Value_Boolean;
                                _hasvalue = tdata.Value_Boolean.HasValue;
                                break;
                            case IoTSharp.Data.DataType.String:
                                _type = "value_string";
                                _value = tdata.Value_String;
                                break;
                            case IoTSharp.Data.DataType.Long:
                                _type = "value_long";
                                _value = tdata.Value_Long;
                                _hasvalue = tdata.Value_Long.HasValue;
                                break;
                            case IoTSharp.Data.DataType.Double:
                                _type = "value_double";
                                _value = tdata.Value_Double;
                                _hasvalue = tdata.Value_Double.HasValue;
                                break;
                            case IoTSharp.Data.DataType.DateTime:
                                _type = "value_datetime";
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
                var okCount = await _session.InsertRecordAsync(device, record, false);
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
