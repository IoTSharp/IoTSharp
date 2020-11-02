using hyjiacan.py4n;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
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

    public class InfluxDBV2Storage : IStorage
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly IServiceScope scope;
        private readonly ObjectPool<InfluxDBClient> _taospool;
        public InfluxDBV2Storage(ILogger<InfluxDBV2Storage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options,   ObjectPool<InfluxDBClient> taospool
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            scope = scopeFactor.CreateScope();
            _taospool = taospool;
        }
       

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {
            InfluxDBClient _taos = _taospool.Get();
            var query = _taos.GetQueryApi();
         var v=      query.QueryAsync( @$"	
from(bucket: ""iotsharp-bucket"")
|> range(start: -10h)
  |> filter(fn: (r) => r[""_measurement""] == ""TelemetryData"")
  |> filter(fn: (r) => r[""DeviceId""] == ""{deviceId}"")
  |> last()").GetAwaiter().GetResult();
            List<TelemetryDataDto> dt =  new List<TelemetryDataDto> ();
            v.ForEach(ft =>
            {
                ft.Records.ForEach(fr =>
                {
                    dt.Add(new TelemetryDataDto()
                    {
                        KeyName = fr.GetField(),
                        DateTime = fr.GetTimeInDateTime().GetValueOrDefault(),
                        Value = fr.GetValue()
                    });
                });
            });
            _taospool.Return(_taos);
            return Task.FromResult(dt);
        }
 
        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            InfluxDBClient _taos = _taospool.Get();
            IEnumerable<string> kvs = from k in keys
                                      select $" keyname = '{k}' ";
            string sql = $"select last_row(*) from telemetrydata where deviceid='{deviceId:N}' and ({string.Join("or", kvs) }) group by deviceid,keyname";
            List<TelemetryDataDto> dt = null;// SqlToTDD(_taos, sql, "last_row(", ")", string.Empty);
            _taospool.Return(_taos);
            return Task.FromResult(dt);

        }
    
        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin)
        {
            return LoadTelemetryAsync(deviceId, keys, begin, DateTime.Now);
        }


        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end)
        {
            InfluxDBClient _taos = _taospool.Get();
            IEnumerable<string> kvs = from k in keys
                                          select $" keyname = '{k}' ";
                string sql = $"select  tbname,keyname  from telemetrydata where deviceid='{deviceId:N}'  and ({string.Join("or", kvs) })  ";
            List<TelemetryDataDto> dt = null;// SQLToDTByDate(begin, end, _taos, sql);
            _taospool.Return(_taos);
            return Task.FromResult(dt);
            
        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, DateTime begin)
        {
            return LoadTelemetryAsync(deviceId, begin, DateTime.Now);
        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, DateTime begin, DateTime end)
        {
            InfluxDBClient _taos = _taospool.Get();
            string sql = $"select  tbname,keyname  from telemetrydata where deviceid='{deviceId:N}'";
            List<TelemetryDataDto> dt = null; //SQLToDTByDate(begin, end, _taos, sql);
            _taospool.Return(_taos);
            return Task.FromResult(dt);
        }

        public async Task<bool> StoreTelemetryAsync(RawMsg msg)
        {
            bool result = false;
            try
            {
               
                List<PointData> lst = new List<PointData>();
                msg.MsgBody.ToList().ForEach(kp =>
                    {
                        if (kp.Value != null)
                        {
                            TelemetryData tdata = new TelemetryData() { DateTime = DateTime.Now, DeviceId = msg.DeviceId, KeyName = kp.Key, Value_DateTime = new DateTime(1970, 1, 1) };
                            tdata.FillKVToMe(kp);
                            var point = PointData.Measurement(nameof(TelemetryData))
                .Tag("DeviceId", tdata.DeviceId.ToString());
                            switch (tdata.Type)
                            {
                                case DataType.Boolean:
                                    // point.Field("value_type", "value_boolean");
                                    point= point.Field(tdata.KeyName, tdata.Value_Boolean);
                                    break;
                                case DataType.String:
                                    //point.Field("value_string", "value_boolean");
                                    point = point.Field(tdata.KeyName, tdata.Value_String);
                                    break;
                                case DataType.Long:
                                    point = point.Field(tdata.KeyName, tdata.Value_Long);
                                    break;
                                case DataType.Double:
                                    point = point.Field(tdata.KeyName, tdata.Value_Double);
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
                                    point = point.Field(tdata.KeyName, tdata.Value_DateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);
                                    break;
                                default:
                                    break;
                            }
                            point = point.Timestamp(DateTime.UtcNow, WritePrecision.Ns);
                            lst.Add(point);
                        }
                    });

                InfluxDBClient _taos = _taospool.Get();
                var writeApi = _taos.GetWriteApiAsync();
                await writeApi.WritePointsAsync("iotsharp-bucket", "iotsharp", lst );
                _taospool.Return(_taos);
                _logger.LogInformation($"数据入库完成,共数据{lst.Count}条");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{msg.DeviceId}数据处理失败{ex.Message} {ex.InnerException?.Message} ");
            }
            return result;
        }
    }
}