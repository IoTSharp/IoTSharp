using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using IoTSharp.Extensions;
using IoTSharp.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            await StoreAttributeData(msg, EventType.Attribute);
        }
        public async Task StoreAttributeData(PlayloadData msg, EventType _event)
        {
            bool orderedData = _event == EventType.Attribute && SonnetDbWriteRecovery.IsEnabled;
            bool storageCompleted = false;
            try
            {
                using (var _scope = _scopeFactor.CreateScope())
                {
                    using (var _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        DeviceAttributeTarget? device;
                        using (SonnetMqProcessingMetrics.Measure(_event == EventType.Attribute ? SonnetMqProcessingMetrics.AttributeDeviceLookup : null))
                        {
                            device = await _dbContext.Device.Where(d => d.Id == msg.DeviceId)
                                .Select(d => new DeviceAttributeTarget(d.Id, d.Name)).FirstOrDefaultAsync();
                        }
                        if (device != null)
                        {
                            var dc = msg.ToDictionary();
                            if (orderedData && dc.Count == 0)
                                return;

                            (int ret, Dictionary<string, Exception> exceptions) result2;
                            using (SonnetMqProcessingMetrics.BeginStorage(_event == EventType.Attribute ? SonnetMqProcessingMetrics.AttributeSaveChanges : null))
                            {
                                result2 = await _dbContext.SaveAsync<AttributeLatest>(
                                    dc, device.Id, msg.DataSide, orderedData ? msg.ts : null);
                            }
                            storageCompleted = true;
                            result2.exceptions?.ToList().ForEach(ex =>
                            {
                                _logger.LogError($"{ex.Key} {ex.Value} {JsonObjectSerializer.Serialize(msg.MsgBody[ex.Key])}");
                            });
                            _logger.LogInformation("更新{DeviceName}({DeviceId})属性数据结果{Rows}", device.Name, device.Id, result2.ret);
                            if (_event != EventType.None)
                            {
                                using var timing = SonnetMqProcessingMetrics.Measure(_event == EventType.Attribute ? SonnetMqProcessingMetrics.AttributeRules : null);
                                await RunRules(msg.DeviceId, dc.ToDynamic(), _event);
                            }
                        }
                        else if (orderedData)
                        {
                            throw new InvalidOperationException("属性消息对应的设备不存在。");
                        }
                    }
                }
            }
            catch (Exception ex) when (storageCompleted && SonnetDbWriteRecovery.WasRejected(ex))
            {
                throw SonnetDbWriteRecovery.AfterEarlierWrites(ex);
            }
            catch (DbUpdateException due) when (!orderedData && !SonnetDbWriteRecovery.ShouldPropagate(due))
            {
                var dc = msg.ToDictionary();
                string json = JsonObjectSerializer.Serialize(dc);
                _logger.LogError(due, "StoreAttributeData DbUpdateException:" + due.InnerException?.Message + Environment.NewLine + json + Environment.NewLine);
            }
            catch (Exception ex) when (!orderedData && !SonnetDbWriteRecovery.ShouldPropagate(ex))
            {
                var dc = msg.ToDictionary();
                string json = JsonObjectSerializer.Serialize(dc);
                _logger.LogError(ex, "StoreAttributeData:" + ex.Message + Environment.NewLine + json + Environment.NewLine);
            }
        }

        private sealed record DeviceAttributeTarget(Guid Id, string Name);

        public async Task RunRules(Guid deviceId, object obj, EventType attribute)
        {
            var runRules = _eventBusOption.RunRules;
            if (runRules == null)
            {
                return;
            }

            if (SonnetDbWriteRecovery.IsEnabled && attribute is EventType.Telemetry or EventType.TelemetryArray or EventType.Attribute)
            {
                foreach (var handler in Delegate.EnumerateInvocationList(runRules))
                    await handler(deviceId, obj, attribute);
            }
            else
            {
                await runRules(deviceId, obj, attribute);
            }
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
                        if (alm.Code == (int)ApiCode.Success && alm.Data != null)
                        {
                            alarmDto.warnDataId = alm.Data.Id;
                            alarmDto.CreateDateTime = alm.Data.AckDateTime;
                            if (alm.Data.Propagate)
                            {
                                await RunRules(alm.Data.OriginatorId, alarmDto, EventType.Alarm);
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

        /// <summary>
        /// 写入单条遥测消息，存储失败时由事件总线负责重试和死信处理。
        /// </summary>
        /// <param name="msg">设备遥测消息。</param>
        public async Task StoreTelemetryData(PlayloadData msg)
        {
            (bool result, List<TelemetryData> telemetries) result;
            using (SonnetMqProcessingMetrics.Measure(SonnetMqProcessingMetrics.TelemetryStore))
            using (SonnetMqProcessingMetrics.BeginStorage(SonnetMqProcessingMetrics.TelemetryStore))
            {
                result = await _storage.StoreTelemetryAsync(msg);
            }
            if (!result.result)
            {
                throw new InvalidOperationException($"Telemetry storage failed. DeviceId={msg.DeviceId}");
            }

            using var rules = SonnetMqProcessingMetrics.Measure(SonnetMqProcessingMetrics.TelemetryRules);
            var data = from t in result.telemetries
                       select new TelemetryDataDto() { DateTime = t.DateTime, DataType = t.Type, KeyName = t.KeyName, Value = t.ToObject() };
            var array = data.ToList();
            ExpandoObject exps = array.ToDynamic();
            await RunTelemetryRulesAfterStorageAsync(msg.DeviceId, exps, array);
        }

        public async Task StoreTelemetryDataBatch(IReadOnlyCollection<PlayloadData> messages)
        {
            if (messages.Count == 0)
            {
                return;
            }

            TelemetryBatchStoreResult storeResult;
            using (SonnetMqProcessingMetrics.Measure(SonnetMqProcessingMetrics.TelemetryStore))
            using (SonnetMqProcessingMetrics.BeginStorage(SonnetMqProcessingMetrics.TelemetryStore))
            {
                storeResult = await _storage.StoreTelemetryBatchAsync(messages);
            }
            if (!storeResult.Result)
            {
                throw new InvalidOperationException($"Telemetry batch storage failed. MessageCount={storeResult.MessageCount}");
            }

            var hasNoRules = _eventBusOption.HasNoTelemetryRulesAsync;
            if (hasNoRules != null)
            {
                using var check = SonnetMqProcessingMetrics.Measure(SonnetMqProcessingMetrics.TelemetryRuleCheck);
                try
                {
                    if (await hasNoRules(messages))
                        return;
                }
                catch (Exception ex) when (SonnetDbWriteRecovery.WasRejected(ex))
                {
                    throw SonnetDbWriteRecovery.AfterEarlierWrites(ex);
                }
            }

            using var rules = SonnetMqProcessingMetrics.Measure(SonnetMqProcessingMetrics.TelemetryRules);
            foreach (var msg in messages)
            {
                var result = msg.ToDictionary()
                    .Select(kp =>
                    {
                        var telemetry = new TelemetryData { DateTime = msg.ts, DeviceId = msg.DeviceId, KeyName = kp.Key, DataSide = msg.DataSide };
                        telemetry.FillKVToMe(kp);
                        return telemetry;
                    });
                var array = result.Select(t => new TelemetryDataDto() { DateTime = t.DateTime, DataType = t.Type, KeyName = t.KeyName, Value = t.ToObject() }).ToList();
                ExpandoObject exps = array.ToDynamic();
                await RunTelemetryRulesAfterStorageAsync(msg.DeviceId, exps, array);
            }
        }

        /// <summary>执行已完成存储的遥测规则。规则阶段的写入门禁不能触发整条消息重写。</summary>
        private async Task RunTelemetryRulesAfterStorageAsync(Guid deviceId, ExpandoObject values, List<TelemetryDataDto> rows)
        {
            try
            {
                await RunRules(deviceId, values, EventType.Telemetry);
                await RunRules(deviceId, rows, EventType.TelemetryArray);
            }
            catch (Exception ex) when (SonnetDbWriteRecovery.WasRejected(ex))
            {
                throw SonnetDbWriteRecovery.AfterEarlierWrites(ex);
            }
        }

        public async Task DeleteDevice(Guid deviceId)
        {
            await RunRules(deviceId, new object(), EventType.DeleteDevice);
        }
        public async Task CreateDevice(Guid deviceId)
        {
            await RunRules(deviceId, new object(), EventType.CreateDevice);
        }

        public async Task Active(Guid devid, ActivityStatus activity, DateTime? eventTimeUtc = null)
        {
            var statusTimeUtc = NormalizeEventTimeUtc(eventTimeUtc);
            var msg = new PlayloadData();
            msg.DeviceId = devid;
            msg.DataCatalog = DataCatalog.AttributeData;
            msg.DataSide = DataSide.ServerSide;
            msg.MsgBody = new Dictionary<string, object>();
            msg.MsgBody.Add(activity == ActivityStatus.Activity ? Constants._LastActivityDateTime : Constants._InactivityAlarmDateTime, statusTimeUtc);
            msg.MsgBody.Add(Constants._Active, activity == ActivityStatus.Activity);
            await StoreAttributeData(msg, activity == ActivityStatus.Activity ? EventType.Activity : EventType.Inactivity);
        }

        public async Task Connect(Guid devid, ConnectStatus devicestatus)
        {
            var msg = new PlayloadData();
            msg.DeviceId = devid;
            msg.DataCatalog = DataCatalog.AttributeData;
            msg.DataSide = DataSide.ServerSide;
            msg.MsgBody = new Dictionary<string, object>();
            msg.MsgBody.Add(Constants._Connected, devicestatus == ConnectStatus.Connected);
            msg.MsgBody.Add(devicestatus == ConnectStatus.Connected ? Constants._LastConnectDateTime : Constants._LastDisconnectDateTime, DateTime.UtcNow);
            await StoreAttributeData(msg, devicestatus == ConnectStatus.Connected ? EventType.Connected : EventType.Disconnected);
        }

        private static DateTime NormalizeEventTimeUtc(DateTime? eventTimeUtc)
        {
            if (eventTimeUtc is not { } value || value == DateTime.MinValue)
            {
                return DateTime.UtcNow;
            }

            return value.Kind == DateTimeKind.Utc
                ? value
                : value.ToUniversalTime();
        }



    }
}
