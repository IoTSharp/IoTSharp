using IoTSharp.Contracts;
using IoTSharp.Data.Shardings;
using IoTSharp.Storage;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.TableExists.Abstractions;
using ShardingCore.TableExists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore;
using IoTSharp.Data.Shardings.Routes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using ShardingCore.Core.ShardingConfigurations;
using IoTSharp.Data.Taos;
using InfluxDB.Client;
using PinusDB.Data;

namespace IoTSharp.Data.TimeSeries
{
    public static class DependencyInjection
    {
        public  static void AddTelemetryStorage(this IServiceCollection services, AppSettings settings, IHealthChecksBuilder healthChecks, Action<ShardingConfigOptions> shardingConfigure)
        {
            string _hc_telemetryStorage = $"{nameof(TelemetryStorage)}-{Enum.GetName(settings.TelemetryStorage)}";
            var _connectionString = settings.ConnectionStrings["TelemetryStorage"];
            switch (settings.TelemetryStorage)
            {
                case TelemetryStorage.Sharding:
                    ShardingByDateMode settingsShardingByDateMode = settings.ShardingByDateMode;
                    var _sharding = services.AddShardingDbContext<ShardingDbContext>();
                    _sharding.UseRouteConfig(o =>
                    {
                        switch (settingsShardingByDateMode)
                        {
                            case ShardingByDateMode.PerMinute:
                                o.AddShardingTableRoute<TelemetryDataMinuteRoute>();
                                break;
                            case ShardingByDateMode.PerHour:
                                o.AddShardingTableRoute<TelemetryDataHourRoute>();
                                break;
                            case ShardingByDateMode.PerDay:
                                o.AddShardingTableRoute<TelemetryDataDayRoute>();
                                break;
                            case ShardingByDateMode.PerMonth:
                                o.AddShardingTableRoute<TelemetryDataMonthRoute>();
                                break;
                            case ShardingByDateMode.PerYear:
                                o.AddShardingTableRoute<TelemetryDataYearRoute>();
                                break;
                            default: throw new InvalidOperationException($"unknown sharding mode:{settingsShardingByDateMode}");
                        }
                    });
                    _sharding.UseConfig(o =>
                    {
                        o.ThrowIfQueryRouteNotMatch = false;
                        o.UseShellDbContextConfigure(builder => builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                        o.AddDefaultDataSource("ds0", _connectionString);
                        shardingConfigure?.Invoke(o);
                       
                    });
                    _sharding.AddShardingCore();
                    services.AddSingleton<IStorage, ShardingStorage>();
                    break;

                case TelemetryStorage.Taos:
                    services.AddSingleton<IStorage, TaosStorage>();
                    healthChecks.AddTDengine(new TaosConnectionStringBuilder( _connectionString).UseRESTful().ConnectionString, name: _hc_telemetryStorage);
                    break;
                case TelemetryStorage.InfluxDB:
                    //https://github.com/julian-fh/influxdb-setup
                    services.AddSingleton<IStorage, InfluxDBStorage>();
                    //"TelemetryStorage": "http://localhost:8086/?org=iotsharp&bucket=iotsharp-bucket&token=iotsharp-token"
                    services.AddObjectPool(()=>new InfluxDBClient( InfluxDBClientOptions.Builder.CreateNew().ConnectionString(_connectionString).Build()));
                    healthChecks.AddInfluxDB(_connectionString, name: _hc_telemetryStorage);
                    break;

                case TelemetryStorage.PinusDB:
                    services.AddSingleton<IStorage, PinusDBStorage>();
                    services.AddObjectPool(() =>
                    {
                        var cnt = new PinusConnection(_connectionString);
                        cnt.Open();
                        return cnt;
                    });
                    healthChecks.AddPinusDB(_connectionString, name: _hc_telemetryStorage);
                    break;

                case TelemetryStorage.TimescaleDB:
                    services.AddSingleton<IStorage, TimescaleDBStorage>();
                    break;
                case TelemetryStorage.IoTDB:
                    var str = _connectionString;
                    services.AddSingleton<IStorage, IoTDBStorage>();
                    services.AddSingleton(s =>
                    {
                        return new Apache.IoTDB.Data.IoTDBConnection(str);
                    });
                    healthChecks.AddIoTDB(str);
                    break;
                case TelemetryStorage.SingleTable:
                default:
                    services.AddSingleton<IStorage, EFStorage>();
                    break;
            }
        }
    }
}
