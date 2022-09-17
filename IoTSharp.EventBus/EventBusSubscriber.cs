using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using IoTSharp.Extensions;
using IoTSharp.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.EventBus
{

    public class EventBusSubscriber
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IStorage _storage;
        private readonly IEasyCachingProvider _caching;
        private readonly EventBusOption _eventBusOption;

        public EventBusSubscriber(ILogger logger, IServiceScopeFactory scopeFactor
           , IStorage storage, IEasyCachingProviderFactory factory, EventBusOption eventBusOption
            )
        {
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(eventBusOption.AppSettings.CachingUseIn)}";
            _logger = logger;
            _scopeFactor = scopeFactor;
            _storage = storage;
            _caching = factory.GetCachingProvider(_hc_Caching);
            _eventBusOption = eventBusOption;
        }

        public async Task StoreAttributeData(PlayloadData msg)
        {
            try
            {
                using (var _scope = _scopeFactor.CreateScope())
                {
                    using (var _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        var device = _dbContext.Device.FirstOrDefault(d => d.Id == msg.DeviceId);
                        if (device != null)
                        {
                            var dc = msg.ToDictionary();
                            var result2 = await _dbContext.SaveAsync<AttributeLatest>(dc, device.Id, msg.DataSide);
                            result2.exceptions?.ToList().ForEach(ex =>
                            {
                                _logger.LogError($"{ex.Key} {ex.Value} {Newtonsoft.Json.JsonConvert.SerializeObject(msg.MsgBody[ex.Key])}");
                            });
                            _logger.LogInformation($"更新{device.Name}({device.Id})属性数据结果{result2.ret}");
                            await RunRules(msg.DeviceId, dc.ToDynamic(), MountType.Attribute);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "StoreAttributeData:" + ex.Message);
            }
        }


        private async Task RunRules(Guid deviceId, object obj, MountType attribute)
        {
            await _eventBusOption.RunRules(deviceId, obj, attribute);
        }

        public async Task OccurredAlarm(CreateAlarmDto alarmDto)
        {
            try
            {
                using (var _scope = _scopeFactor.CreateScope())
                {
                    using (var _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        var alm = await _dbContext.OccurredAlarm(alarmDto);
                        if (alm.Code == (int)ApiCode.Success)
                        {
                            alarmDto.warnDataId = alm.Data.Id;
                            alarmDto.CreateDateTime = alm.Data.AckDateTime;
                            if (alm.Data.Propagate)
                            {
                                await RunRules(alm.Data.OriginatorId, alarmDto, MountType.Alarm);
                            }
                        }
                        else
                        {
                            //如果设备通过网关创建， 当警告先来， 设备创建再后会出现此问题。
                            _logger.LogWarning($"处理{alarmDto.OriginatorName} 的告警{alarmDto.AlarmType} 错误:{alm.Code}-{alm.Msg}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理{alarmDto.OriginatorName} 的告警{alarmDto.AlarmType} 时遇到异常:{ex.Message}");

            }
        }


        public async Task DeviceStatusEvent(PlayloadData status)
        {
            try
            {
                using (var _scope = _scopeFactor.CreateScope())
                {
                    using (var _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        var dev = _dbContext.Device.FirstOrDefault(d => d.Id == status.DeviceId);
                        if (dev != null)
                        {
                            if (dev.Online == true && status.DeviceStatus != DeviceStatus.Good)
                            {
                                dev.Online = false;
                                dev.LastActive = DateTime.Now;
                              await RunRules(dev.Id, status, MountType.Offline);
                                //真正离线
                            }
                            else if (dev.Online == false && status.DeviceStatus == DeviceStatus.Good)
                            {
                                dev.Online = true;
                                dev.LastActive = DateTime.Now;
                             await  RunRules(dev.Id, status, MountType.Online);
                                //真正掉线

                            }
                            _dbContext.SaveChanges();
                        }
                        else
                        {
                            _logger.LogWarning($"未找到设备{status.DeviceId} ，因此无法处理设备状态");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理{status.DeviceId} 的状态{status.DeviceStatus} 时遇到异常:{ex.Message}");

            }
        }

        public async Task StoreTelemetryData(PlayloadData msg)
        {
            var result = await _storage.StoreTelemetryAsync(msg);
            var data = from t in result.telemetries
                       select new TelemetryDataDto() { DateTime = t.DateTime, DataType = t.Type, KeyName = t.KeyName, Value = t.ToObject() };
            var array = data.ToList();
            ExpandoObject exps = array.ToDynamic();
            await RunRules(msg.DeviceId, (dynamic)exps, MountType.Telemetry);
            await RunRules(msg.DeviceId, array, MountType.TelemetryArray);
        }

        public async Task DeleteDevice(Guid deviceId)
        {
            await RunRules(deviceId, new object(), MountType.DeleteDevice);
        }
        public async Task CreateDevice(Guid deviceId)
        {
            await RunRules(deviceId, new object(), MountType.CreateDevice);
        }

    }
}