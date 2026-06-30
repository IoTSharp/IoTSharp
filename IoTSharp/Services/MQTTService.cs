using IoTSharp.EventBus;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace IoTSharp.Services
{
    public class MQTTService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly MqttServer _serverEx;
        private readonly IPublisher _queue;
        private readonly FlowRuleProcessor _flowRuleProcessor;
        private readonly IMemoryCache _mqttAuthCache;
        private readonly ConcurrentDictionary<string, Device> _mqttAuthIndex = new(StringComparer.Ordinal);
        private readonly Channel<DeviceConnectStatus> _connectStatusQueue;
        private readonly MqttClientSetting _mcsetting;
        private readonly AppSettings _settings;
        private static readonly TimeSpan MqttAuthCacheExpiration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan MqttAuthFailureCacheExpiration = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan MqttAuthDatabaseWaitTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan MqttAuthIndexRefreshInterval = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan MqttConnectStatusDedupeExpiration = TimeSpan.FromMinutes(2);
        private static readonly SemaphoreSlim MqttAuthDatabaseGate = new(4, 4);
        private static readonly SemaphoreSlim MqttAuthIndexRefreshGate = new(1, 1);
        private volatile bool _mqttAuthIndexLoaded;
        private DateTime _mqttAuthIndexLoadedAt = DateTime.MinValue;

        public MQTTService(ILogger<MQTTService> logger, IServiceScopeFactory scopeFactor, MqttServer serverEx
           , IOptions<AppSettings> options, IPublisher queue, FlowRuleProcessor flowRuleProcessor, IMemoryCache mqttAuthCache
            )
        {
            _mcsetting = options.Value.MqttClient;
            _settings = options.Value;
            _logger = logger;
            _scopeFactor = scopeFactor;
            _serverEx = serverEx;
            _queue = queue;
            _flowRuleProcessor = flowRuleProcessor;
            _mqttAuthCache = mqttAuthCache;
            _connectStatusQueue = Channel.CreateBounded<DeviceConnectStatus>(new BoundedChannelOptions(10000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });
            _ = Task.Run(ProcessConnectStatusQueueAsync);
            _ = Task.Run(() => RefreshMqttAuthIndexAsync());
        }

        private static long clients = 0;

        internal Task Server_ClientConnectedAsync(ClientConnectedEventArgs e)
        {
            _logger.LogDebug("MQTT client connected. ClientId={ClientId}, Endpoint={Endpoint}, UserName={UserName}", e.ClientId, e.RemoteEndPoint.ToString(), e.UserName);
            clients++;
            return Task.CompletedTask;
        }

        private static DateTime uptime = DateTime.MinValue;

        private static Device CloneMqttSessionDevice(Device device)
        {
            return new Device
            {
                Id = device.Id,
                Name = device.Name,
                DeviceType = device.DeviceType,
                Timeout = device.Timeout,
                Deleted = device.Deleted,
                DeviceModelId = device.DeviceModelId
            };
        }

        private static string BuildMqttAuthCacheKey(string clientId, string userName, string password, string thumbprint)
        {
            var source = $"{clientId}:{userName}:{password}:{thumbprint}";
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)));
            return $"mqtt_auth:{hash}";
        }

        private static string BuildMqttAuthIndexKey(IdentityType identityType, string identityId, string identityValue)
        {
            var source = $"{identityType}:{identityId}:{identityValue}";
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)));
            return $"mqtt_auth_index:{hash}";
        }

        private static string BuildProduceMqttAuthIndexKey(string deviceName, string produceToken)
        {
            var source = $"ProduceToken:{deviceName}:{produceToken}";
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)));
            return $"mqtt_produce_auth_index:{hash}";
        }

        private static IEnumerable<string> BuildMqttAuthIndexKeys(string userName, string password, string thumbprint)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                yield return BuildMqttAuthIndexKey(IdentityType.AccessToken, userName, string.Empty);
                if (!string.IsNullOrEmpty(password))
                {
                    yield return BuildMqttAuthIndexKey(IdentityType.DevicePassword, userName, password);
                    yield return BuildProduceMqttAuthIndexKey(userName, password);
                }
            }

            if (!string.IsNullOrEmpty(thumbprint))
            {
                yield return BuildMqttAuthIndexKey(IdentityType.X509Certificate, thumbprint, string.Empty);
            }
        }

        private static string BuildMqttConnectStatusCacheKey(Guid deviceId, ConnectStatus status)
        {
            return $"mqtt_connect_status:{deviceId:N}:{status}";
        }

        private sealed class MqttAuthCacheEntry
        {
            public bool Succeeded { get; init; }

            public Device Device { get; init; }
        }

        private void AddDeviceIdentityToMqttAuthIndex(DeviceIdentity identity)
        {
            if (identity?.Device == null || identity.Device.Deleted)
            {
                return;
            }

            var identityValue = identity.IdentityType == IdentityType.DevicePassword ? identity.IdentityValue : string.Empty;
            _mqttAuthIndex[BuildMqttAuthIndexKey(identity.IdentityType, identity.IdentityId, identityValue)] = CloneMqttSessionDevice(identity.Device);
        }

        private async Task RefreshMqttAuthIndexAsync(bool waitForCurrentRefresh = false)
        {
            var acquired = waitForCurrentRefresh
                ? await MqttAuthIndexRefreshGate.WaitAsync(MqttAuthDatabaseWaitTimeout)
                : await MqttAuthIndexRefreshGate.WaitAsync(0);
            if (!acquired)
            {
                return;
            }

            try
            {
                if (_mqttAuthIndexLoaded && DateTime.UtcNow - _mqttAuthIndexLoadedAt < MqttAuthIndexRefreshInterval)
                {
                    return;
                }

                using var scope = _scopeFactor.CreateScope();
                using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var identities = await dbContext.DeviceIdentities
                    .AsNoTracking()
                    .Include(d => d.Device)
                    .Where(d => d.Device != null && !d.Device.Deleted)
                    .ToListAsync();
                var nextIndex = new ConcurrentDictionary<string, Device>(StringComparer.Ordinal);
                foreach (var identity in identities)
                {
                    var identityValue = identity.IdentityType == IdentityType.DevicePassword ? identity.IdentityValue : string.Empty;
                    nextIndex[BuildMqttAuthIndexKey(identity.IdentityType, identity.IdentityId, identityValue)] = CloneMqttSessionDevice(identity.Device);
                }

                try
                {
                    var produces = await dbContext.Produces
                        .AsNoTracking()
                        .Where(p => !p.Deleted && p.ProduceToken != null)
                        .Select(p => new { p.Id, p.ProduceToken })
                        .ToListAsync();

                    var produceTokenById = produces
                        .Where(p => !string.IsNullOrEmpty(p.ProduceToken))
                        .ToDictionary(p => p.Id, p => p.ProduceToken);

                    if (produceTokenById.Count > 0)
                    {
                        var devices = await dbContext.Device
                            .AsNoTracking()
                            .Where(d => !d.Deleted)
                            .Select(d => new
                            {
                                d.Id,
                                d.Name,
                                d.DeviceType,
                                d.Timeout,
                                d.Deleted,
                                d.DeviceModelId,
                                ProduceId = EF.Property<Guid?>(d, "ProduceId")
                            })
                            .ToListAsync();

                        foreach (var device in devices)
                        {
                            if (device.ProduceId.HasValue
                                && !string.IsNullOrEmpty(device.Name)
                                && produceTokenById.TryGetValue(device.ProduceId.Value, out var produceToken))
                            {
                                nextIndex[BuildProduceMqttAuthIndexKey(device.Name, produceToken)] = new Device
                                {
                                    Id = device.Id,
                                    Name = device.Name,
                                    DeviceType = device.DeviceType,
                                    Timeout = device.Timeout,
                                    Deleted = device.Deleted,
                                    DeviceModelId = device.DeviceModelId
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "MQTT produce auth index refresh skipped.");
                }

                _mqttAuthIndex.Clear();
                foreach (var item in nextIndex)
                {
                    _mqttAuthIndex[item.Key] = item.Value;
                }

                _mqttAuthIndexLoaded = true;
                _mqttAuthIndexLoadedAt = DateTime.UtcNow;
                _logger.LogInformation("MQTT auth index refreshed. Count={Count}", _mqttAuthIndex.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT auth index refresh failed.");
            }
            finally
            {
                MqttAuthIndexRefreshGate.Release();
            }
        }

        private async Task EnsureMqttAuthIndexLoadedAsync()
        {
            if (_mqttAuthIndexLoaded && DateTime.UtcNow - _mqttAuthIndexLoadedAt < MqttAuthIndexRefreshInterval)
            {
                return;
            }

            await RefreshMqttAuthIndexAsync(waitForCurrentRefresh: true);
        }

        private bool TryAcceptFromMqttAuthIndex(ValidatingConnectionEventArgs e, string userName, string password, string thumbprint)
        {
            foreach (var indexKey in BuildMqttAuthIndexKeys(userName, password, thumbprint))
            {
                if (_mqttAuthIndex.TryGetValue(indexKey, out var device) && device != null && !device.Deleted)
                {
                    AcceptConnection(e, device, userName, e.RemoteEndPoint.ToString());
                    return true;
                }
            }

            return false;
        }

        private void CacheSuccessfulAuth(string key, Device device)
        {
            _mqttAuthCache.Set(key, new MqttAuthCacheEntry
            {
                Succeeded = true,
                Device = CloneMqttSessionDevice(device)
            }, MqttAuthCacheExpiration);
        }

        private void CacheFailedAuth(string key)
        {
            _mqttAuthCache.Set(key, new MqttAuthCacheEntry { Succeeded = false }, MqttAuthFailureCacheExpiration);
        }

        private void QueueConnectStatus(Guid deviceId, ConnectStatus status)
        {
            var dedupeKey = BuildMqttConnectStatusCacheKey(deviceId, status);
            if (_mqttAuthCache.TryGetValue(dedupeKey, out _))
            {
                return;
            }

            _mqttAuthCache.Set(dedupeKey, true, MqttConnectStatusDedupeExpiration);
            if (!_connectStatusQueue.Writer.TryWrite(new DeviceConnectStatus(deviceId, status)))
            {
                _logger.LogWarning("MQTT connect status queue is full. DeviceId={DeviceId}, Status={Status}", deviceId, status);
            }
        }

        private async Task ProcessConnectStatusQueueAsync()
        {
            await foreach (var status in _connectStatusQueue.Reader.ReadAllAsync())
            {
                try
                {
                    await _queue.PublishConnect(status.DeviceId, status.ConnectStatus);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish MQTT connect status. DeviceId={DeviceId}, Status={Status}", status.DeviceId, status.ConnectStatus);
                    await Task.Delay(1000);
                }
            }
        }

        private void AcceptConnection(ValidatingConnectionEventArgs e, Device device, string userName, string endpoint)
        {
            var sessionDevice = CloneMqttSessionDevice(device);
            e.SessionItems.Add(nameof(Device), sessionDevice);
            e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
            CacheSuccessfulAuth(BuildMqttAuthCacheKey(e.ClientId, e.UserName, e.Password, e.ClientCertificate?.Thumbprint), sessionDevice);
            QueueConnectStatus(sessionDevice.Id, ConnectStatus.Connected);
            _logger.LogDebug("MQTT device accepted. DeviceName={DeviceName}, DeviceId={DeviceId}, UserName={UserName}, Endpoint={Endpoint}", sessionDevice.Name, sessionDevice.Id, userName, endpoint);
        }

        internal Task Server_Started(EventArgs e)
        {
            _logger.LogInformation($"MqttServer is  started");
            uptime = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        internal Task Server_Stopped(EventArgs e)
        {
            _logger.LogInformation($"Server is stopped");
            return Task.CompletedTask;
        }

        private async Task<Device> FoundDevice(string clientid)
        {
            Device device = null;
            var clients = await _serverEx.GetClientsAsync();
            var client = clients.FirstOrDefault(c => c.Id == clientid);
            if (client != null)
            {
                device = client.Session.Items[nameof(Device)] as Device;
                if (device == null)
                {
                    if (clientid != _mcsetting.MqttBroker)
                    {
                        _logger.LogWarning($"未能找到客户端{clientid}回话附加的设备信息，现在断开此链接。 ");
                        await client.DisconnectAsync();
                    }
                }
            }
            else
            {
                _logger.LogWarning($"未能找到客户端{clientid}上下文信息");
            }
            return device;
        }

        internal async Task Server_ClientDisconnected(ClientDisconnectedEventArgs args)
        {
            try
            {
                var dev = args.SessionItems[nameof(Device)] as Device;
                if (dev != null)
                {
                    QueueConnectStatus(dev.Id, ConnectStatus.Disconnected);
                }
                else
                {
                    _logger.LogDebug($"Server_ClientDisconnected ClientId:{args.ClientId} DisconnectType:{args.DisconnectType}, 未能在缓存中找到");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Server_ClientDisconnected ClientId:{args.ClientId} DisconnectType:{args.DisconnectType},{ex.Message}");
            }
        }

        private long Subscribed;

        internal Task Server_ClientSubscribedTopic(ClientSubscribedTopicEventArgs e)
        {
            _logger.LogDebug($"Client [{e.ClientId}] subscribed [{e.TopicFilter}]");

            if (e.TopicFilter.Topic.StartsWith("$SYS/"))
            {
            }
            if (e.TopicFilter.Topic.StartsWith("devices/telemetry", StringComparison.OrdinalIgnoreCase))
            {
            }
            else
            {
                Subscribed++;
            }
            return Task.CompletedTask;
        }

        internal Task Server_ClientUnsubscribedTopic(ClientUnsubscribedTopicEventArgs e)
        {
            _logger.LogDebug($"Client [{e.ClientId}] unsubscribed[{e.TopicFilter}]");
            if (!e.TopicFilter.StartsWith("$SYS/"))
            {
                Subscribed--;
            }
            return Task.CompletedTask;
        }

        internal async Task Server_ClientConnectionValidator(ValidatingConnectionEventArgs e)
        {
            try
            {
                var obj = e;

                // jy 特殊处理 ::1
                var isLoopback = false;
                if (obj.RemoteEndPoint.ToString().StartsWith("::1") == true)
                {
                    isLoopback = true;
                }
                else
                {
                    Uri uri = new Uri("mqtt://" + obj.RemoteEndPoint.ToString());
                    isLoopback = uri.IsLoopback;
                }

                if (isLoopback && !string.IsNullOrEmpty(e.ClientId) && e.ClientId == _mcsetting.MqttBroker && !string.IsNullOrEmpty(e.UserName))
                {
                    e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                    return;
                }

                string _thumbprint = string.Empty;
                if (_settings.MqttBroker.EnableTls)
                {
                    _thumbprint = e.ClientCertificate?.Thumbprint;
                }
                _logger.LogDebug(
                    "MQTT validating connection. ClientId={ClientId}, Endpoint={Endpoint}, Username={Username}, PasswordProvided={PasswordProvided}",
                    obj.ClientId,
                    obj.RemoteEndPoint.ToString(),
                    obj.UserName,
                    !string.IsNullOrEmpty(obj.Password));

                var authCacheKey = BuildMqttAuthCacheKey(obj.ClientId, obj.UserName, obj.Password, _thumbprint);
                if (_mqttAuthCache.TryGetValue<MqttAuthCacheEntry>(authCacheKey, out var authCache))
                {
                    if (authCache?.Succeeded == true && authCache.Device != null && !authCache.Device.Deleted)
                    {
                        e.SessionItems.Add(nameof(Device), authCache.Device);
                        e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                        QueueConnectStatus(authCache.Device.Id, ConnectStatus.Connected);
                        return;
                    }

                    if (authCache?.Succeeded == false)
                    {
                        e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                        return;
                    }
                }

                await EnsureMqttAuthIndexLoadedAsync();
                if (TryAcceptFromMqttAuthIndex(e, obj.UserName, obj.Password, _thumbprint))
                {
                    return;
                }

                if (!await MqttAuthDatabaseGate.WaitAsync(MqttAuthDatabaseWaitTimeout))
                {
                    e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.ServerUnavailable;
                    e.ReasonString = "MQTT authentication is busy. Please retry later.";
                    _logger.LogWarning("MQTT authentication database gate timed out. ClientId={ClientId}, Endpoint={Endpoint}", obj.ClientId, obj.RemoteEndPoint.ToString());
                    return;
                }

                try
                {
                    using (var scope = _scopeFactor.CreateScope())
                    using (var _dbContextcv = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        var mcr = _dbContextcv.DeviceIdentities.Include(d => d.Device).FirstOrDefault(mc =>
                                              (mc.IdentityType == IdentityType.AccessToken && mc.IdentityId == obj.UserName) ||
                                             (mc.IdentityType == IdentityType.X509Certificate && mc.IdentityId == _thumbprint) ||
                                             (mc.IdentityType == IdentityType.DevicePassword && mc.IdentityId == obj.UserName && mc.IdentityValue == obj.Password)
                                             );
                        if (mcr != null)
                        {
                            try
                            {
                                AddDeviceIdentityToMqttAuthIndex(mcr);
                                AcceptConnection(e, mcr.Device, obj.UserName, obj.RemoteEndPoint.ToString());
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "ConnectionRefusedServerUnavailable {0}", ex.Message);
                                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.ServerUnavailable;
                            }
                        }
                        else
                        {
                            var ak = !string.IsNullOrEmpty(obj.Password)
                                ? _dbContextcv.Produces.Include(ak => ak.Customer).Include(ak => ak.Tenant).Include(ak => ak.Devices).FirstOrDefault(ak => ak.ProduceToken == obj.Password && ak.ProduceToken != null && ak.Deleted == false)
                                : null;
                            if (ak != null)
                            {
                                Device createdDevice = null;
                                if (!ak.Devices.Any(d => d.Name == obj.UserName && d.Deleted == false))
                                {
                                    createdDevice = new Device() { Name = obj.UserName, DeviceType = ak.DefaultDeviceType, Timeout = ak.DefaultTimeout };
                                    createdDevice.Tenant = ak.Tenant;
                                    createdDevice.Customer = ak.Customer;
                                    _dbContextcv.Device.Add(createdDevice);
                                    _dbContextcv.AfterCreateDevice(createdDevice, ak.Id, obj.UserName, obj.Password);
                                    _dbContextcv.SaveChanges();
                                }

                                var mcp = ak.Devices.FirstOrDefault(d => d.Name == obj.UserName && d.Deleted == false) ?? createdDevice;
                                if (mcp != null)
                                {
                                    _mqttAuthIndex[BuildProduceMqttAuthIndexKey(obj.UserName, obj.Password)] = CloneMqttSessionDevice(mcp);
                                    AcceptConnection(e, mcp, obj.UserName, obj.RemoteEndPoint.ToString());
                                    _logger.LogDebug("Produce device accepted. ProduceName={ProduceName}, DeviceName={DeviceName}, DeviceId={DeviceId}, ClientName={ClientName}, Endpoint={Endpoint}", ak.Name, mcp.Name, mcp.Id, obj.UserName, obj.RemoteEndPoint.ToString());
                                }
                                else
                                {
                                    CacheFailedAuth(authCacheKey);
                                    e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                                    _logger.LogInformation($"Bad device name or ProduceToken {obj.UserName},connection {obj.RemoteEndPoint.ToString()} refused");
                                }
                            }
                            else
                            {
                                CacheFailedAuth(authCacheKey);
                                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                                _logger.LogInformation($"Bad username or password {obj.UserName},connection {obj.RemoteEndPoint.ToString()} refused");
                            }
                        }
                    }
                }
                finally
                {
                    MqttAuthDatabaseGate.Release();
                }
            }
            catch (Exception ex)
            {
                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.ImplementationSpecificError;
                e.ReasonString = ex.Message;
                _logger.LogError(ex, "ImplementationSpecificError {0}", ex.Message);
            }
        }
    }
}
