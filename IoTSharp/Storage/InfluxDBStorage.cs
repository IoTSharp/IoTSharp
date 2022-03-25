using hyjiacan.py4n;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Writes;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Utilities.Encoders;
using Silkier;
using Silkier.EFCore;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IoTSharp.Storage
{

    public class InfluxDBStorage : IStorage
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly IServiceScope scope;
        private readonly ObjectPool<InfluxDBClient> _taospool;
        private readonly string _org;
        private readonly string _bucket;
        private readonly string _token;
        private readonly string _latest;

        public InfluxDBStorage(ILogger<InfluxDBStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options,   ObjectPool<InfluxDBClient> taospool
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            scope = scopeFactor.CreateScope();
            _taospool = taospool;
            Uri uri = new Uri(_appSettings.ConnectionStrings["TelemetryStorage"]);
            string leftPart = uri.GetLeftPart(UriPartial.Path);
            NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uri.Query);
              _org = nameValueCollection.Get("org");
              _bucket = nameValueCollection.Get("bucket");
              _token = nameValueCollection.Get("token");
            _latest= nameValueCollection.Get("latest");
            _latest ??= "-72h";
            //string logLevel = nameValueCollection.Get("logLevel");
            //string timeout = nameValueCollection.Get("timeout");
            //string readWriteTimeout = nameValueCollection.Get("readWriteTimeout");
        }
       

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {
            InfluxDBClient _taos = _taospool.Get();
            var query = _taos.GetQueryApi();
            var v = query.QueryAsync(@$"	
from(bucket: ""{_bucket}"")
|> range(start: {_latest})
  |> filter(fn: (r) => r[""_measurement""] == ""TelemetryData"")
  |> filter(fn: (r) => r[""DeviceId""] == ""{deviceId}"")
  |> last()");
           
            _taospool.Return(_taos);
            return FluxToDtoAsync(v);
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            InfluxDBClient _taos = _taospool.Get();
            var query = _taos.GetQueryApi();
            var kvs = from k in keys.Split(';', ',')
                      select $"r[\"_field\"] == \"{k}\"";
            var v = query.QueryAsync(@$"	
from(bucket: ""{_bucket}"")
|> range(start: {_latest})
|> filter(fn: (r) => r[""_measurement""] == ""TelemetryData"")
|> filter(fn: (r) => r[""DeviceId""] == ""{deviceId}"")
|> filter(fn: (r) => {string.Join(" or ", kvs)})
|> group(columns: [""_field""])
|> last()");
            _taospool.Return(_taos);
            return FluxToDtoAsync(v);

        }

         

        private async Task<List<TelemetryDataDto>> FluxToDtoAsync(Task< List<FluxTable>> v)
        {
            List<TelemetryDataDto> dt = new List<TelemetryDataDto>();
            (await v)?.ForEach(ft =>
            {
                ft.Records.ForEach(fr =>
                {
                    dt.Add(new TelemetryDataDto()
                    {
                        KeyName = fr.GetField(),
                        DateTime = fr.GetTimeInDateTime().GetValueOrDefault(DateTime.MinValue).ToLocalTime(),
                        Value = fr.GetValue() ,
                         DataType= InfluxTypeToIoTSharpType(ft.Columns.Find(fv=>fv.Label=="_value")?.DataType)
                    });
                });
            });
            return dt;
        }
        Data.DataType InfluxTypeToIoTSharpType(string _itype)
        {
            Data.DataType data = DataType.String;
            switch (_itype)
            {
                case "long":
                    data = DataType.Long;
                    break;
                case "double":
                    data = DataType.Double;
                    break;
                case "boolean":
                case "bool":
                    data = DataType.Boolean;
                    break;
                case "dateTime:RFC3339":
                    data = DataType.DateTime;
                    break;
                case "string":
                default:
                    data = DataType.String;
                    break;
            }
            return data;
        }

  
        /// <summary>
        /// 加载遥测数据
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <param name="keys">如果为空则全部 ， 否则都好分隔</param>
        /// <param name="begin">时间开始</param>
        /// <param name="end">结束</param>
        /// <param name="every">数据堆叠断面时间</param>
        /// <param name="aggregate">聚合方式</param>
        /// <returns></returns>
        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end,TimeSpan every, Aggregate aggregate)
        {
            InfluxDBClient _taos = _taospool.Get();
            var query = _taos.GetQueryApi();
            var sb = new StringBuilder();
            sb.AppendLine(@$"from(bucket: ""{_bucket}"")");
            sb.AppendLine($"|> range(start: {begin:o},stop:{end:o})");
            sb.AppendLine(@$"|> filter(fn: (r) => r[""_measurement""] == ""TelemetryData"")");
            sb.AppendLine(@$"|> filter(fn: (r) => r[""DeviceId""] == ""{deviceId}"")");
            if (!string.IsNullOrEmpty(keys))
            {
                var kvs = from k in keys.Split(';', ',')
                          select $"r[\"_field\"] == \"{k}\"";
                sb.AppendLine(@$"|> filter(fn: (r) => {string.Join(" or ", kvs)})");
                sb.AppendLine(@$"|> group(columns: [""_field""])");
            }
            if (every > TimeSpan.Zero)
            {
                sb.AppendLine($@"|> aggregateWindow(every: {(long)every.TotalMilliseconds}ms, fn: {Enum.GetName(aggregate).ToLower()}, createEmpty: false)");
                sb.AppendLine(@$"|> yield(name: ""{Enum.GetName( aggregate).ToLower()}"")");
            }
            else
            {
                sb.AppendLine(@$"|> yield()");
            }
           _logger.LogInformation(sb.ToString());
            var v = query.QueryAsync(sb.ToString());
            _taospool.Return(_taos);
            return FluxToDtoAsync(v);
        }

        public async Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(RawMsg msg  )
        {
            bool result = false;
            List<TelemetryData> telemetries = new List<TelemetryData>(); ;
            try
            {
                List<PointData> lst = new List<PointData>();
                msg.MsgBody.ToList().ForEach(kp =>
                    {
                        if (kp.Value != null)
                        {
                            TelemetryData tdata = new TelemetryData() { DateTime = msg.ts, DeviceId = msg.DeviceId, KeyName = kp.Key, Value_DateTime = new DateTime(1970, 1, 1) };
                            tdata.FillKVToMe(kp);
                            var point = PointData.Measurement(nameof(TelemetryData))
                .Tag("DeviceId", tdata.DeviceId.ToString());
                            switch (tdata.Type)
                            {
                                case DataType.Boolean:
                                    // point.Field("value_type", "value_boolean");
                                  if (tdata.Value_Boolean.HasValue)  point = point.Field(tdata.KeyName, tdata.Value_Boolean.Value);
                                    break;
                                case DataType.String:
                                    //point.Field("value_string", "value_boolean");
                                    point = point.Field(tdata.KeyName, tdata.Value_String);
                                    break;
                                case DataType.Long:
                                    if (tdata.Value_Long.HasValue) point = point.Field(tdata.KeyName, tdata.Value_Long.Value);
                                    break;
                                case DataType.Double:
                                    if (tdata.Value_Double.HasValue) point = point.Field(tdata.KeyName, tdata.Value_Double.Value);
                                    break;
                                case DataType.Json:
                                    point = point.Field(tdata.KeyName, tdata.Value_Json);
                                    break;
                                case DataType.XML:
                                    point = point.Field(tdata.KeyName, tdata.Value_XML);
                                    break;
                                case DataType.Binary:
                                    point = point.Field(tdata.KeyName, Hex.ToHexString(tdata.Value_Binary));
                                    break;
                                case DataType.DateTime:
                                    point = point.Field(tdata.KeyName, tdata.Value_DateTime.GetValueOrDefault().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);
                                    break;
                                default:
                                    break;
                            }
                            if (point.HasFields())
                            {
                                point = point.Timestamp(msg.ts.ToUniversalTime() , WritePrecision.Ns);
                                lst.Add(point);
                                telemetries.Add(tdata);
                            }
                        }
                    });

                InfluxDBClient _taos = _taospool.Get();
                var writeApi = _taos.GetWriteApiAsync();
                await writeApi.WritePointsAsync(lst);
                _taospool.Return(_taos);
                _logger.LogInformation($"数据入库完成,共数据{lst.Count}条");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
            }
            return (result, telemetries);
        }

      

        
    }
}