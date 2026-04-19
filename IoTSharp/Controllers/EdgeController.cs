using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.EventBus;
using IoTSharp.Extensions;
using IoTSharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class EdgeController : ControllerBase
    {
        private static readonly string[] EdgeKeys =
        [
            Constants._Active,
            Constants._LastActivityDateTime,
            Constants._EdgeRuntimeType,
            Constants._EdgeRuntimeName,
            Constants._EdgeVersion,
            Constants._EdgeInstanceId,
            Constants._EdgePlatform,
            Constants._EdgeHostName,
            Constants._EdgeIpAddress,
            Constants._EdgeStatus,
            Constants._EdgeHealthy,
            Constants._EdgeLastHeartbeatDateTime,
            Constants._EdgeLastRegistrationDateTime,
            Constants._EdgeCapabilities,
            Constants._EdgeMetadata,
            Constants._EdgeMetrics,
            Constants._EdgeUptimeSeconds,
            Constants._EdgeCollectionConfigVersion,
            Constants._EdgeCollectionConfigUpdatedAt,
            "_edge.task.receipt.status",
            "_edge.task.receipt.reportedAt"
        ];

        private readonly ApplicationDbContext _context;
        private readonly IPublisher _queue;
        private readonly ILogger _logger;

        public EdgeController(ApplicationDbContext context, IPublisher queue, ILogger<EdgeController> logger)
        {
            _context = context;
            _queue = queue;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<PagedData<EdgeNodeDto>>> Get([FromQuery] EdgeNodeQueryDto query)
        {
            var profile = this.GetUserProfile();
            query ??= new EdgeNodeQueryDto();
            query.Limit = query.Limit < 5 ? 5 : query.Limit;

            var gateways = await _context.Device
                .Include(c => c.DeviceIdentity)
                .Where(c => c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && !c.Deleted && c.DeviceType == DeviceType.Gateway)
                .ToListAsync();

            var attrs = await QueryEdgeAttributes(gateways.Select(c => c.Id));
            var data = gateways.Select(gateway => ToEdgeNodeDto(gateway, attrs.Where(c => c.DeviceId == gateway.Id).ToList())).ToList();
            var filtered = ApplyFilters(data, query);
            var ordered = ApplySorting(filtered, query);
            var pagedRows = ordered.Skip(query.Offset * query.Limit).Take(query.Limit).ToList();

            return new ApiResult<PagedData<EdgeNodeDto>>(ApiCode.Success, "OK", new PagedData<EdgeNodeDto>
            {
                total = ordered.Count,
                rows = pagedRows
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<EdgeNodeDto>> Get(Guid id)
        {
            var profile = this.GetUserProfile();
            var gateway = await _context.Device
                .Include(c => c.DeviceIdentity)
                .FirstOrDefaultAsync(c => c.Id == id && c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && !c.Deleted && c.DeviceType == DeviceType.Gateway);

            if (gateway == null)
            {
                return new ApiResult<EdgeNodeDto>(ApiCode.NotFoundDevice, "Edge node not found", null);
            }

            var attrs = await QueryEdgeAttributes([gateway.Id]);
            return new ApiResult<EdgeNodeDto>(ApiCode.Success, "OK", ToEdgeNodeDto(gateway, attrs));
        }

        [AllowAnonymous]
        [HttpPost("{access_token}/Register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<EdgeRegistrationResultDto>>> Register(string access_token, [FromBody] EdgeRegistrationDto request)
        {
            if (request == null)
            {
                return Ok(new ApiResult<EdgeRegistrationResultDto>(ApiCode.InValidData, "Registration payload is required", null));
            }

            var gateway = GetGatewayByAccessToken(access_token);
            if (gateway == null)
            {
                return Ok(new ApiResult<EdgeRegistrationResultDto>(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token", null));
            }

            var now = DateTime.UtcNow;
            var attrs = new Dictionary<string, object>
            {
                [Constants._EdgeRuntimeType] = request.RuntimeType ?? string.Empty,
                [Constants._EdgeRuntimeName] = string.IsNullOrWhiteSpace(request.RuntimeName) ? gateway.Name : request.RuntimeName,
                [Constants._EdgeVersion] = request.Version ?? string.Empty,
                [Constants._EdgeInstanceId] = request.InstanceId ?? string.Empty,
                [Constants._EdgePlatform] = request.Platform ?? string.Empty,
                [Constants._EdgeHostName] = request.HostName ?? string.Empty,
                [Constants._EdgeIpAddress] = request.IpAddress ?? string.Empty,
                [Constants._EdgeStatus] = "Registered",
                [Constants._EdgeLastRegistrationDateTime] = now
            };

            var metadata = SerializeOrNull(request.Metadata);
            if (!string.IsNullOrWhiteSpace(metadata))
            {
                attrs[Constants._EdgeMetadata] = metadata;
            }

            await _context.SaveAsync<AttributeLatest>(attrs, gateway.Id, DataSide.ServerSide);
            await _queue.PublishActive(gateway.Id, ActivityStatus.Activity);

            _logger.LogInformation("Edge runtime {RuntimeType}/{RuntimeName} registered for gateway {GatewayId}", request.RuntimeType, request.RuntimeName ?? gateway.Name, gateway.Id);

            return Ok(new ApiResult<EdgeRegistrationResultDto>(ApiCode.Success, "OK", new EdgeRegistrationResultDto
            {
                DeviceId = gateway.Id,
                Name = gateway.Name,
                AccessToken = access_token,
                ContractVersion = "edge-v1",
                Timeout = gateway.Timeout,
                RegisteredAt = now
            }));
        }

        [AllowAnonymous]
        [HttpPost("{access_token}/Heartbeat")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult>> Heartbeat(string access_token, [FromBody] EdgeHeartbeatDto request)
        {
            var gateway = GetGatewayByAccessToken(access_token);
            if (gateway == null)
            {
                return Ok(new ApiResult(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token"));
            }

            var heartbeatAt = request?.Timestamp?.ToUniversalTime() ?? DateTime.UtcNow;
            var attrs = new Dictionary<string, object>
            {
                [Constants._EdgeLastHeartbeatDateTime] = heartbeatAt,
                [Constants._EdgeStatus] = string.IsNullOrWhiteSpace(request?.Status) ? "Running" : request.Status
            };

            if (!string.IsNullOrWhiteSpace(request?.IpAddress))
            {
                attrs[Constants._EdgeIpAddress] = request.IpAddress;
            }

            if (request?.Healthy != null)
            {
                attrs[Constants._EdgeHealthy] = request.Healthy.Value;
            }

            if (request?.UptimeSeconds != null)
            {
                attrs[Constants._EdgeUptimeSeconds] = request.UptimeSeconds.Value;
            }

            var metrics = SerializeOrNull(request?.Metrics);
            if (!string.IsNullOrWhiteSpace(metrics))
            {
                attrs[Constants._EdgeMetrics] = metrics;
            }

            await _context.SaveAsync<AttributeLatest>(attrs, gateway.Id, DataSide.ServerSide);
            await _queue.PublishActive(gateway.Id, ActivityStatus.Activity);
            return Ok(new ApiResult(ApiCode.Success, "OK"));
        }

        [AllowAnonymous]
        [HttpPost("{access_token}/Capabilities")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult>> Capabilities(string access_token, [FromBody] EdgeCapabilityReportDto request)
        {
            var gateway = GetGatewayByAccessToken(access_token);
            if (gateway == null)
            {
                return Ok(new ApiResult(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token"));
            }

            var attrs = new Dictionary<string, object>
            {
                [Constants._EdgeCapabilities] = SerializeOrNull(new
                {
                    Protocols = request?.Protocols ?? [],
                    Features = request?.Features ?? [],
                    Tasks = request?.Tasks ?? []
                }) ?? "{}"
            };

            var metadata = SerializeOrNull(request?.Metadata);
            if (!string.IsNullOrWhiteSpace(metadata))
            {
                attrs[Constants._EdgeMetadata] = metadata;
            }

            await _context.SaveAsync<AttributeLatest>(attrs, gateway.Id, DataSide.ServerSide);
            await _queue.PublishActive(gateway.Id, ActivityStatus.Activity);
            return Ok(new ApiResult(ApiCode.Success, "OK"));
        }

        [HttpGet("{id:guid}/CollectionConfig")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<EdgeCollectionConfigurationDto>> GetCollectionConfig(Guid id)
        {
            var profile = this.GetUserProfile();
            var gateway = await GetGatewayForProfileAsync(id, profile.Tenant, profile.Customer);
            if (gateway == null)
            {
                return new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.NotFoundDevice, "Edge node not found", null);
            }

            return new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.Success, "OK", await ReadCollectionConfigAsync(gateway.Id));
        }

        [HttpPut("{id:guid}/CollectionConfig")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<EdgeCollectionConfigurationDto>>> SaveCollectionConfig(Guid id, [FromBody] EdgeCollectionConfigurationUpdateDto request)
        {
            var profile = this.GetUserProfile();
            var gateway = await GetGatewayForProfileAsync(id, profile.Tenant, profile.Customer);
            if (gateway == null)
            {
                return Ok(new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.NotFoundDevice, "Edge node not found", null));
            }

            var tasks = request?.Tasks?.ToArray() ?? [];
            var validationError = ValidateCollectionTasks(tasks);
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                return Ok(new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.InValidData, validationError, null));
            }

            var version = await GetCurrentCollectionConfigVersionAsync(gateway.Id) + 1;
            var updatedAt = DateTime.UtcNow;
            var normalizedTasks = tasks.Select(task => NormalizeCollectionTask(task, gateway.Id, version)).ToArray();
            var document = new EdgeCollectionConfigurationDto
            {
                EdgeNodeId = gateway.Id,
                Version = version,
                UpdatedAt = updatedAt,
                UpdatedBy = string.IsNullOrWhiteSpace(profile.Name) ? profile.Email : profile.Name,
                Tasks = normalizedTasks
            };

            await _context.SaveAsync<AttributeLatest>(new Dictionary<string, object>
            {
                [Constants._EdgeCollectionConfig] = JsonSerializer.Serialize(document),
                [Constants._EdgeCollectionConfigVersion] = version,
                [Constants._EdgeCollectionConfigUpdatedAt] = updatedAt
            }, gateway.Id, DataSide.ServerSide);

            _logger.LogInformation("Saved collection configuration version {Version} for edge node {GatewayId}", version, gateway.Id);
            return Ok(new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.Success, "OK", document));
        }

        [AllowAnonymous]
        [HttpGet("{access_token}/CollectionConfig")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<EdgeCollectionConfigurationDto>>> PullCollectionConfig(string access_token)
        {
            var gateway = GetGatewayByAccessToken(access_token);
            if (gateway == null)
            {
                return Ok(new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token", null));
            }

            return Ok(new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.Success, "OK", await ReadCollectionConfigAsync(gateway.Id)));
        }

        private Device GetGatewayByAccessToken(string accessToken)
        {
            var (ok, gateway) = _context.GetDeviceByToken(accessToken);
            if (ok || gateway?.DeviceType != DeviceType.Gateway || gateway.Deleted)
            {
                return null;
            }

            return gateway;
        }

        private async Task<Device> GetGatewayForProfileAsync(Guid id, Guid tenantId, Guid customerId)
        {
            return await _context.Device
                .Include(c => c.DeviceIdentity)
                .FirstOrDefaultAsync(c => c.Id == id && c.Customer.Id == customerId && c.Tenant.Id == tenantId && !c.Deleted && c.DeviceType == DeviceType.Gateway);
        }

        private async Task<List<AttributeLatest>> QueryEdgeAttributes(IEnumerable<Guid> deviceIds)
        {
            var ids = deviceIds?.Distinct().ToList() ?? [];
            if (ids.Count == 0)
            {
                return [];
            }

            return await _context.AttributeLatest
                .Where(c => ids.Contains(c.DeviceId) && EdgeKeys.Contains(c.KeyName))
                .ToListAsync();
        }

        private EdgeNodeDto ToEdgeNodeDto(Device gateway, List<AttributeLatest> attrs)
        {
            return new EdgeNodeDto
            {
                Id = gateway.Id,
                Name = gateway.Name,
                AccessToken = gateway.DeviceIdentity?.IdentityId,
                Timeout = gateway.Timeout,
                Active = GetBoolean(attrs, Constants._Active),
                LastActivityDateTime = GetDateTime(attrs, Constants._LastActivityDateTime),
                RuntimeType = GetString(attrs, Constants._EdgeRuntimeType),
                RuntimeName = GetString(attrs, Constants._EdgeRuntimeName),
                Version = GetString(attrs, Constants._EdgeVersion),
                InstanceId = GetString(attrs, Constants._EdgeInstanceId),
                Platform = GetString(attrs, Constants._EdgePlatform),
                HostName = GetString(attrs, Constants._EdgeHostName),
                IpAddress = GetString(attrs, Constants._EdgeIpAddress),
                Status = GetString(attrs, Constants._EdgeStatus),
                Healthy = GetNullableBoolean(attrs, Constants._EdgeHealthy),
                UptimeSeconds = GetNullableLong(attrs, Constants._EdgeUptimeSeconds),
                LastHeartbeatDateTime = GetDateTime(attrs, Constants._EdgeLastHeartbeatDateTime),
                LastRegistrationDateTime = GetDateTime(attrs, Constants._EdgeLastRegistrationDateTime),
                Capabilities = GetString(attrs, Constants._EdgeCapabilities),
                Metadata = GetString(attrs, Constants._EdgeMetadata),
                Metrics = GetString(attrs, Constants._EdgeMetrics),
                LastTaskStatus = GetString(attrs, "_edge.task.receipt.status"),
                LastReceiptDateTime = GetDateTime(attrs, "_edge.task.receipt.reportedAt")
            };
        }

        private static List<EdgeNodeDto> ApplyFilters(List<EdgeNodeDto> source, EdgeNodeQueryDto query)
        {
            IEnumerable<EdgeNodeDto> result = source;

            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                result = result.Where(edge => !string.IsNullOrWhiteSpace(edge.Name) && edge.Name.Contains(query.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.RuntimeType))
            {
                result = result.Where(edge => string.Equals(edge.RuntimeType, query.RuntimeType, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                result = result.Where(edge => string.Equals(edge.Status, query.Status, StringComparison.OrdinalIgnoreCase));
            }

            if (query.Healthy.HasValue)
            {
                result = result.Where(edge => edge.Healthy == query.Healthy.Value);
            }

            if (query.Active.HasValue)
            {
                result = result.Where(edge => edge.Active == query.Active.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Version))
            {
                result = result.Where(edge => !string.IsNullOrWhiteSpace(edge.Version) && edge.Version.Contains(query.Version, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Platform))
            {
                result = result.Where(edge => !string.IsNullOrWhiteSpace(edge.Platform) && edge.Platform.Contains(query.Platform, StringComparison.OrdinalIgnoreCase));
            }

            return result.ToList();
        }

        private static List<EdgeNodeDto> ApplySorting(List<EdgeNodeDto> source, EdgeNodeQueryDto query)
        {
            var field = string.IsNullOrWhiteSpace(query.Sorter) ? "LastHeartbeatDateTime" : query.Sorter;
            var descending = string.Equals(query.Sort, "desc", StringComparison.OrdinalIgnoreCase);

            return field switch
            {
                nameof(EdgeNodeDto.Name) => ApplyOrder(source, edge => edge.Name, descending),
                nameof(EdgeNodeDto.RuntimeType) => ApplyOrder(source, edge => edge.RuntimeType, descending),
                nameof(EdgeNodeDto.RuntimeName) => ApplyOrder(source, edge => edge.RuntimeName, descending),
                nameof(EdgeNodeDto.Version) => ApplyOrder(source, edge => edge.Version, descending),
                nameof(EdgeNodeDto.Status) => ApplyOrder(source, edge => edge.Status, descending),
                nameof(EdgeNodeDto.Healthy) => ApplyOrder(source, edge => edge.Healthy, descending),
                nameof(EdgeNodeDto.Active) => ApplyOrder(source, edge => edge.Active, descending),
                nameof(EdgeNodeDto.Platform) => ApplyOrder(source, edge => edge.Platform, descending),
                nameof(EdgeNodeDto.HostName) => ApplyOrder(source, edge => edge.HostName, descending),
                nameof(EdgeNodeDto.IpAddress) => ApplyOrder(source, edge => edge.IpAddress, descending),
                nameof(EdgeNodeDto.LastActivityDateTime) => ApplyOrder(source, edge => edge.LastActivityDateTime, descending),
                nameof(EdgeNodeDto.LastHeartbeatDateTime) => ApplyOrder(source, edge => edge.LastHeartbeatDateTime, descending),
                _ => ApplyOrder(source, edge => edge.LastHeartbeatDateTime, true)
            };
        }

        private static List<EdgeNodeDto> ApplyOrder<TKey>(IEnumerable<EdgeNodeDto> source, Func<EdgeNodeDto, TKey> keySelector, bool descending)
        {
            return descending
                ? source.OrderByDescending(keySelector).ThenBy(edge => edge.Name).ToList()
                : source.OrderBy(keySelector).ThenBy(edge => edge.Name).ToList();
        }

        private static string GetString(List<AttributeLatest> attrs, string key) => attrs.FirstOrDefault(c => c.KeyName == key)?.Value_String;

        private static bool GetBoolean(List<AttributeLatest> attrs, string key) => attrs.FirstOrDefault(c => c.KeyName == key)?.Value_Boolean ?? false;

        private static bool? GetNullableBoolean(List<AttributeLatest> attrs, string key) => attrs.FirstOrDefault(c => c.KeyName == key)?.Value_Boolean;

        private static long? GetNullableLong(List<AttributeLatest> attrs, string key) => attrs.FirstOrDefault(c => c.KeyName == key)?.Value_Long;

        private static DateTime? GetDateTime(List<AttributeLatest> attrs, string key) => attrs.FirstOrDefault(c => c.KeyName == key)?.Value_DateTime;

        private async Task<EdgeCollectionConfigurationDto> ReadCollectionConfigAsync(Guid gatewayId)
        {
            var storedJson = await _context.AttributeLatest
                .Where(attr => attr.DeviceId == gatewayId && attr.KeyName == Constants._EdgeCollectionConfig)
                .Select(attr => attr.Value_String)
                .FirstOrDefaultAsync();

            var storedVersion = await GetCurrentCollectionConfigVersionAsync(gatewayId);
            var storedUpdatedAt = await _context.AttributeLatest
                .Where(attr => attr.DeviceId == gatewayId && attr.KeyName == Constants._EdgeCollectionConfigUpdatedAt)
                .Select(attr => attr.Value_DateTime)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(storedJson))
            {
                return CreateEmptyCollectionConfig(gatewayId, storedVersion, storedUpdatedAt);
            }

            try
            {
                var document = JsonSerializer.Deserialize<EdgeCollectionConfigurationDto>(storedJson);
                if (document == null)
                {
                    return CreateEmptyCollectionConfig(gatewayId, storedVersion, storedUpdatedAt);
                }

                return document with
                {
                    EdgeNodeId = gatewayId,
                    Version = document.Version > 0 ? document.Version : storedVersion,
                    UpdatedAt = document.UpdatedAt != default ? document.UpdatedAt : (storedUpdatedAt ?? DateTime.UtcNow),
                    Tasks = (document.Tasks ?? []).Select(task => NormalizeCollectionTask(task, gatewayId, document.Version > 0 ? document.Version : storedVersion)).ToArray()
                };
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Failed to deserialize collection configuration for edge node {GatewayId}", gatewayId);
                return CreateEmptyCollectionConfig(gatewayId, storedVersion, storedUpdatedAt);
            }
        }

        private async Task<int> GetCurrentCollectionConfigVersionAsync(Guid gatewayId)
        {
            return (int?)await _context.AttributeLatest
                .Where(attr => attr.DeviceId == gatewayId && attr.KeyName == Constants._EdgeCollectionConfigVersion)
                .Select(attr => attr.Value_Long)
                .FirstOrDefaultAsync() ?? 0;
        }

        private static EdgeCollectionConfigurationDto CreateEmptyCollectionConfig(Guid gatewayId, int version, DateTime? updatedAt)
        {
            return new EdgeCollectionConfigurationDto
            {
                EdgeNodeId = gatewayId,
                Version = version,
                UpdatedAt = updatedAt ?? DateTime.UtcNow,
                Tasks = []
            };
        }

        private static string ValidateCollectionTasks(IEnumerable<CollectionTaskDto> tasks)
        {
            foreach (var task in tasks ?? [])
            {
                if (string.IsNullOrWhiteSpace(task.TaskKey))
                {
                    return "task.taskKey is required";
                }

                if (task.Connection == null || string.IsNullOrWhiteSpace(task.Connection.ConnectionName))
                {
                    return $"task '{task.TaskKey}' requires connection.connectionName";
                }

                if (task.Devices == null || task.Devices.Count == 0)
                {
                    return $"task '{task.TaskKey}' requires at least one device";
                }

                foreach (var device in task.Devices)
                {
                    if (string.IsNullOrWhiteSpace(device.DeviceKey))
                    {
                        return $"task '{task.TaskKey}' requires device.deviceKey";
                    }

                    foreach (var point in device.Points ?? [])
                    {
                        if (string.IsNullOrWhiteSpace(point.PointKey) || string.IsNullOrWhiteSpace(point.PointName))
                        {
                            return $"task '{task.TaskKey}' requires point.pointKey and point.pointName";
                        }

                        if (point.Mapping == null || string.IsNullOrWhiteSpace(point.Mapping.TargetName))
                        {
                            return $"task '{task.TaskKey}' requires point.mapping.targetName";
                        }
                    }
                }
            }

            return string.Empty;
        }

        private static CollectionTaskDto NormalizeCollectionTask(CollectionTaskDto task, Guid gatewayId, int version)
        {
            return task with
            {
                Id = task.Id == Guid.Empty ? Guid.NewGuid() : task.Id,
                Version = task.Version > 0 ? task.Version : version,
                EdgeNodeId = gatewayId
            };
        }

        private static string SerializeOrNull<T>(T value)
        {
            if (value == null)
            {
                return null;
            }

            return JsonSerializer.Serialize(value);
        }
    }
}
