using IoTSharp.Contracts;
using IoTSharp.Data;
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
using EdgeNodeQueryDto = IoTSharp.Dtos.EdgeNodeQueryDto;

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

        private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);

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
                .Include(c => c.Customer)
                .Include(c => c.Tenant)
                .Where(c => c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && !c.Deleted && c.DeviceType == DeviceType.Gateway)
                .ToListAsync();

            await EnsureEdgeNodesAsync(gateways);

            var nodes = await _context.EdgeNodes
                .Include(c => c.Gateway)
                .ThenInclude(c => c.DeviceIdentity)
                .Where(c => c.CustomerId == profile.Customer && c.TenantId == profile.Tenant && !c.Deleted && !c.Gateway.Deleted && c.Gateway.DeviceType == DeviceType.Gateway)
                .ToListAsync();

            var attrs = await QueryEdgeAttributes(gateways.Select(c => c.Id));
            var data = nodes.Select(node => ToEdgeNodeDto(node, attrs.Where(c => c.DeviceId == node.GatewayId).ToList())).ToList();
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
                .Include(c => c.Customer)
                .Include(c => c.Tenant)
                .FirstOrDefaultAsync(c => c.Id == id && c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && !c.Deleted && c.DeviceType == DeviceType.Gateway);

            if (gateway == null)
            {
                return new ApiResult<EdgeNodeDto>(ApiCode.NotFoundDevice, "Edge node not found", null);
            }

            var node = await EnsureEdgeNodeAsync(gateway);
            var attrs = await QueryEdgeAttributes([gateway.Id]);
            return new ApiResult<EdgeNodeDto>(ApiCode.Success, "OK", ToEdgeNodeDto(node, attrs));
        }

        [HttpGet("{id:guid}/RuntimeStatus")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<EdgeRuntimeStatusDto>> GetRuntimeStatus(Guid id)
        {
            var profile = this.GetUserProfile();
            var gateway = await GetGatewayForProfileAsync(id, profile.Tenant, profile.Customer);
            if (gateway == null)
            {
                return new ApiResult<EdgeRuntimeStatusDto>(ApiCode.NotFoundDevice, "Edge node not found", null);
            }

            var node = await EnsureEdgeNodeAsync(gateway);
            var attrs = await QueryEdgeAttributes([gateway.Id]);
            return new ApiResult<EdgeRuntimeStatusDto>(ApiCode.Success, "OK", ToEdgeRuntimeStatusDto(node, attrs));
        }

        [HttpGet("{id:guid}/Capability")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<EdgeCapabilityDto>> GetCapability(Guid id)
        {
            var profile = this.GetUserProfile();
            var gateway = await GetGatewayForProfileAsync(id, profile.Tenant, profile.Customer);
            if (gateway == null)
            {
                return new ApiResult<EdgeCapabilityDto>(ApiCode.NotFoundDevice, "Edge node not found", null);
            }

            var node = await EnsureEdgeNodeAsync(gateway);
            var attrs = await QueryEdgeAttributes([gateway.Id]);
            return new ApiResult<EdgeCapabilityDto>(ApiCode.Success, "OK", ToEdgeCapabilityDto(node, attrs));
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

            if (!IsSupportedEdgeRuntimeContract(request.ContractVersion))
            {
                return Ok(new ApiResult<EdgeRegistrationResultDto>(ApiCode.InValidData, $"Unsupported contractVersion: {request.ContractVersion}", null));
            }

            var gateway = await GetGatewayByAccessTokenAsync(access_token);
            if (gateway == null)
            {
                return Ok(new ApiResult<EdgeRegistrationResultDto>(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token", null));
            }

            var now = DateTime.UtcNow;
            var node = await EnsureEdgeNodeAsync(gateway);
            node.Name = gateway.Name;
            node.RuntimeType = request.RuntimeType ?? string.Empty;
            node.RuntimeName = string.IsNullOrWhiteSpace(request.RuntimeName) ? gateway.Name : request.RuntimeName;
            node.Version = request.Version ?? string.Empty;
            node.InstanceId = request.InstanceId ?? string.Empty;
            node.Platform = request.Platform ?? string.Empty;
            node.HostName = request.HostName ?? string.Empty;
            node.IpAddress = request.IpAddress ?? string.Empty;
            node.Status = EdgeNodeStatusNames.Registered;
            node.LastRegistrationDateTime = now;
            node.UpdatedAt = now;

            var attrs = new Dictionary<string, object>
            {
                [Constants._EdgeRuntimeType] = node.RuntimeType,
                [Constants._EdgeRuntimeName] = node.RuntimeName,
                [Constants._EdgeVersion] = node.Version,
                [Constants._EdgeInstanceId] = node.InstanceId,
                [Constants._EdgePlatform] = node.Platform,
                [Constants._EdgeHostName] = node.HostName,
                [Constants._EdgeIpAddress] = node.IpAddress,
                [Constants._EdgeStatus] = node.Status,
                [Constants._EdgeLastRegistrationDateTime] = node.LastRegistrationDateTime.Value
            };

            var metadata = SerializeOrNull(request.Metadata);
            if (!string.IsNullOrWhiteSpace(metadata))
            {
                node.Metadata = metadata;
                attrs[Constants._EdgeMetadata] = metadata;
            }

            await _context.SaveChangesAsync();
            await _context.SaveAsync<AttributeLatest>(attrs, gateway.Id, DataSide.ServerSide);
            await _queue.PublishActive(gateway.Id, ActivityStatus.Activity);

            _logger.LogInformation("Edge runtime {RuntimeType}/{RuntimeName} registered for gateway {GatewayId}", request.RuntimeType, request.RuntimeName ?? gateway.Name, gateway.Id);

            return Ok(new ApiResult<EdgeRegistrationResultDto>(ApiCode.Success, "OK", new EdgeRegistrationResultDto
            {
                DeviceId = gateway.Id,
                EdgeNodeId = node.Id,
                GatewayId = gateway.Id,
                Name = gateway.Name,
                AccessToken = access_token,
                ContractVersion = EdgeNodeContractVersions.EdgeRuntimeV1,
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
            var gateway = await GetGatewayByAccessTokenAsync(access_token);
            if (gateway == null)
            {
                return Ok(new ApiResult(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token"));
            }

            if (request != null && !IsSupportedEdgeRuntimeContract(request.ContractVersion))
            {
                return Ok(new ApiResult(ApiCode.InValidData, $"Unsupported contractVersion: {request.ContractVersion}"));
            }

            var heartbeatAt = request?.Timestamp?.ToUniversalTime() ?? DateTime.UtcNow;
            var node = await EnsureEdgeNodeAsync(gateway);
            node.LastHeartbeatDateTime = heartbeatAt;
            node.Status = string.IsNullOrWhiteSpace(request?.Status) ? EdgeNodeStatusNames.Running : request.Status;
            node.UpdatedAt = DateTime.UtcNow;

            var attrs = new Dictionary<string, object>
            {
                [Constants._EdgeLastHeartbeatDateTime] = heartbeatAt,
                [Constants._EdgeStatus] = node.Status
            };

            if (!string.IsNullOrWhiteSpace(request?.IpAddress))
            {
                node.IpAddress = request.IpAddress;
                attrs[Constants._EdgeIpAddress] = request.IpAddress;
            }

            if (!string.IsNullOrWhiteSpace(request?.RuntimeType))
            {
                node.RuntimeType = request.RuntimeType;
                attrs[Constants._EdgeRuntimeType] = request.RuntimeType;
            }

            if (!string.IsNullOrWhiteSpace(request?.InstanceId))
            {
                node.InstanceId = request.InstanceId;
                attrs[Constants._EdgeInstanceId] = request.InstanceId;
            }

            if (request?.Healthy != null)
            {
                node.Healthy = request.Healthy.Value;
                attrs[Constants._EdgeHealthy] = request.Healthy.Value;
            }

            if (request?.UptimeSeconds != null)
            {
                node.UptimeSeconds = request.UptimeSeconds.Value;
                attrs[Constants._EdgeUptimeSeconds] = request.UptimeSeconds.Value;
            }

            var metrics = SerializeOrNull(request?.Metrics);
            if (!string.IsNullOrWhiteSpace(metrics))
            {
                node.Metrics = metrics;
                attrs[Constants._EdgeMetrics] = metrics;
            }

            await _context.SaveChangesAsync();
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
            var gateway = await GetGatewayByAccessTokenAsync(access_token);
            if (gateway == null)
            {
                return Ok(new ApiResult(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token"));
            }

            if (request != null && !IsSupportedEdgeCapabilityContract(request.ContractVersion))
            {
                return Ok(new ApiResult(ApiCode.InValidData, $"Unsupported contractVersion: {request.ContractVersion}"));
            }

            var now = DateTime.UtcNow;
            var node = await EnsureEdgeNodeAsync(gateway);
            var capability = CreateEdgeCapabilityDto(node, request, now);
            node.Capabilities = SerializeOrNull(capability) ?? "{}";
            node.UpdatedAt = now;

            var attrs = new Dictionary<string, object>
            {
                [Constants._EdgeCapabilities] = node.Capabilities
            };

            var metadata = SerializeOrNull(request?.Metadata);
            if (!string.IsNullOrWhiteSpace(metadata))
            {
                node.Metadata = metadata;
                attrs[Constants._EdgeMetadata] = metadata;
            }

            await _context.SaveChangesAsync();
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
            var gateway = await GetGatewayByAccessTokenAsync(access_token);
            if (gateway == null)
            {
                return Ok(new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token", null));
            }

            return Ok(new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.Success, "OK", await ReadCollectionConfigAsync(gateway.Id)));
        }

        private async Task<Device> GetGatewayByAccessTokenAsync(string accessToken)
        {
            return await _context.Device
                .Include(c => c.DeviceIdentity)
                .Include(c => c.Customer)
                .Include(c => c.Tenant)
                .FirstOrDefaultAsync(c =>
                    c.DeviceIdentity.IdentityId == accessToken &&
                    c.DeviceIdentity.IdentityType == IdentityType.AccessToken &&
                    c.DeviceType == DeviceType.Gateway &&
                    !c.Deleted);
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

        /// <summary>
        /// 为历史 Gateway 补齐 EdgeNode 管理模型，避免升级后已有 Gateway 从 Edge 列表中消失。
        /// </summary>
        /// <param name="gateways">当前租户和客户可见的 Gateway 设备。</param>
        private async Task EnsureEdgeNodesAsync(IEnumerable<Device> gateways)
        {
            foreach (var gateway in gateways ?? [])
            {
                await EnsureEdgeNodeAsync(gateway);
            }
        }

        /// <summary>
        /// 获取或创建与 Gateway 一一对应的 EdgeNode。
        /// </summary>
        /// <param name="gateway">承载接入凭证的 Gateway 设备。</param>
        /// <returns>已跟踪的 EdgeNode 实体。</returns>
        private async Task<EdgeNode> EnsureEdgeNodeAsync(Device gateway)
        {
            if (gateway == null)
            {
                return null;
            }

            var node = await _context.EdgeNodes
                .Include(c => c.Gateway)
                .ThenInclude(c => c.DeviceIdentity)
                .FirstOrDefaultAsync(c => c.GatewayId == gateway.Id && !c.Deleted);

            if (node != null)
            {
                node.Gateway ??= gateway;
                if (string.IsNullOrWhiteSpace(node.Name))
                {
                    node.Name = gateway.Name;
                }

                return node;
            }

            if (gateway.Tenant == null || gateway.Customer == null || gateway.DeviceIdentity == null)
            {
                gateway = await _context.Device
                    .Include(c => c.DeviceIdentity)
                    .Include(c => c.Customer)
                    .Include(c => c.Tenant)
                    .FirstOrDefaultAsync(c => c.Id == gateway.Id);
            }

            if (gateway == null)
            {
                return null;
            }

            var now = DateTime.UtcNow;
            node = new EdgeNode
            {
                Id = gateway.Id,
                GatewayId = gateway.Id,
                Gateway = gateway,
                Name = gateway.Name,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                RuntimeName = gateway.Name,
                Status = EdgeNodeStatusNames.Pending,
                Tenant = gateway.Tenant,
                TenantId = gateway.TenantId ?? gateway.Tenant?.Id,
                Customer = gateway.Customer,
                CustomerId = gateway.CustomerId ?? gateway.Customer?.Id,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.EdgeNodes.Add(node);
            await _context.SaveChangesAsync();
            return node;
        }

        private EdgeNodeDto ToEdgeNodeDto(EdgeNode node, List<AttributeLatest> attrs)
        {
            var gateway = node.Gateway;
            var runtimeStatus = ToEdgeRuntimeStatusDto(node, attrs);
            var capability = ToEdgeCapabilityDto(node, attrs);
            var capabilities = Coalesce(node.Capabilities, GetString(attrs, Constants._EdgeCapabilities));
            var metadata = Coalesce(node.Metadata, GetString(attrs, Constants._EdgeMetadata));
            var metrics = Coalesce(node.Metrics, GetString(attrs, Constants._EdgeMetrics));

            return new EdgeNodeDto
            {
                Id = node.Id,
                GatewayId = node.GatewayId,
                Name = string.IsNullOrWhiteSpace(node.Name) ? gateway.Name : node.Name,
                AccessToken = gateway.DeviceIdentity?.IdentityId,
                Timeout = gateway.Timeout,
                Active = runtimeStatus.Active,
                LastActivityDateTime = runtimeStatus.LastActivityDateTime,
                RuntimeType = runtimeStatus.RuntimeType,
                RuntimeName = runtimeStatus.RuntimeName,
                Version = runtimeStatus.Version,
                InstanceId = runtimeStatus.InstanceId,
                Platform = runtimeStatus.Platform,
                HostName = runtimeStatus.HostName,
                IpAddress = runtimeStatus.IpAddress,
                Status = runtimeStatus.Status,
                Healthy = runtimeStatus.Healthy,
                UptimeSeconds = runtimeStatus.UptimeSeconds,
                LastHeartbeatDateTime = runtimeStatus.LastHeartbeatDateTime,
                LastRegistrationDateTime = runtimeStatus.LastRegistrationDateTime,
                Capabilities = capabilities,
                Capability = capability,
                Metadata = metadata,
                Metrics = metrics,
                LastTaskStatus = GetString(attrs, "_edge.task.receipt.status"),
                LastReceiptDateTime = GetDateTime(attrs, "_edge.task.receipt.reportedAt"),
                RuntimeStatus = runtimeStatus
            };
        }

        /// <summary>
        /// 统一构造 Edge 运行时状态快照，避免列表、详情和只读状态接口出现字段漂移。
        /// </summary>
        /// <param name="node">EdgeNode 持久化实体。</param>
        /// <param name="attrs">兼容旧属性键中的状态字段。</param>
        /// <returns>面向控制台、执行端诊断和 MCP 技能的状态快照。</returns>
        private EdgeRuntimeStatusDto ToEdgeRuntimeStatusDto(EdgeNode node, List<AttributeLatest> attrs)
        {
            var metadata = Coalesce(node.Metadata, GetString(attrs, Constants._EdgeMetadata));
            var metrics = Coalesce(node.Metrics, GetString(attrs, Constants._EdgeMetrics));

            return new EdgeRuntimeStatusDto
            {
                EdgeNodeId = node.Id,
                GatewayId = node.GatewayId,
                Active = GetBoolean(attrs, Constants._Active),
                LastActivityDateTime = GetDateTime(attrs, Constants._LastActivityDateTime),
                RuntimeType = Coalesce(node.RuntimeType, GetString(attrs, Constants._EdgeRuntimeType)),
                RuntimeName = Coalesce(node.RuntimeName, GetString(attrs, Constants._EdgeRuntimeName)),
                Version = Coalesce(node.Version, GetString(attrs, Constants._EdgeVersion)),
                InstanceId = Coalesce(node.InstanceId, GetString(attrs, Constants._EdgeInstanceId)),
                Platform = Coalesce(node.Platform, GetString(attrs, Constants._EdgePlatform)),
                HostName = Coalesce(node.HostName, GetString(attrs, Constants._EdgeHostName)),
                IpAddress = Coalesce(node.IpAddress, GetString(attrs, Constants._EdgeIpAddress)),
                Status = Coalesce(node.Status, GetString(attrs, Constants._EdgeStatus)),
                Healthy = node.Healthy ?? GetNullableBoolean(attrs, Constants._EdgeHealthy),
                UptimeSeconds = node.UptimeSeconds ?? GetNullableLong(attrs, Constants._EdgeUptimeSeconds),
                LastHeartbeatDateTime = node.LastHeartbeatDateTime ?? GetDateTime(attrs, Constants._EdgeLastHeartbeatDateTime),
                LastRegistrationDateTime = node.LastRegistrationDateTime ?? GetDateTime(attrs, Constants._EdgeLastRegistrationDateTime),
                UpdatedAt = node.UpdatedAt,
                Metadata = DeserializeObjectMap(metadata),
                Metrics = DeserializeObjectMap(metrics)
            };
        }

        /// <summary>
        /// 根据能力上报生成正式 EdgeCapability 快照，保留旧执行端 protocols/features/tasks 字段并补齐结构化能力。
        /// </summary>
        /// <param name="node">EdgeNode 持久化实体。</param>
        /// <param name="request">执行端能力上报载荷。</param>
        /// <param name="receivedAt">平台收到上报的 UTC 时间。</param>
        /// <returns>已补齐节点身份和兼容性声明的能力快照。</returns>
        private static EdgeCapabilityDto CreateEdgeCapabilityDto(EdgeNode node, EdgeCapabilityReportDto request, DateTime receivedAt)
        {
            var protocols = NormalizeStringList(request?.Protocols);
            var supportedProtocols = NormalizeProtocolTypes(request?.SupportedProtocols, protocols);
            var supportedPointTypes = NormalizeStringList(request?.SupportedPointTypes);
            var supportedTransforms = NormalizeEnumList(request?.SupportedTransforms);
            var supportedReportTriggers = NormalizeEnumList(request?.SupportedReportTriggers);
            var features = NormalizeStringList(request?.Features);
            var tasks = NormalizeStringList(request?.Tasks);
            var taskCapabilities = NormalizeTaskCapabilities(request?.TaskCapabilities, tasks);
            tasks = NormalizeStringList(tasks.Concat(taskCapabilities.Select(capability => capability.TaskType)));
            var compatibleContracts = NormalizeCompatibleContracts(request?.CompatibleContracts);

            if (compatibleContracts.Count == 0)
            {
                compatibleContracts = BuildDefaultCompatibleContracts(
                    protocols.Count > 0 || supportedProtocols.Count > 0 || supportedPointTypes.Count > 0 || supportedTransforms.Count > 0 || supportedReportTriggers.Count > 0,
                    tasks.Count > 0 || taskCapabilities.Count > 0);
            }

            return new EdgeCapabilityDto
            {
                EdgeNodeId = node.Id,
                GatewayId = node.GatewayId,
                RuntimeType = node.RuntimeType ?? string.Empty,
                RuntimeName = node.RuntimeName ?? string.Empty,
                Version = node.Version ?? string.Empty,
                InstanceId = node.InstanceId ?? string.Empty,
                ReportedAt = request?.ReportedAt?.ToUniversalTime() ?? receivedAt,
                UpdatedAt = receivedAt,
                Protocols = protocols,
                SupportedProtocols = supportedProtocols,
                SupportedPointTypes = supportedPointTypes,
                SupportedTransforms = supportedTransforms,
                SupportedReportTriggers = supportedReportTriggers,
                Features = features,
                Tasks = tasks,
                TaskCapabilities = taskCapabilities,
                CompatibleContracts = compatibleContracts,
                Metadata = request?.Metadata ?? []
            };
        }

        /// <summary>
        /// 从正式能力快照或历史 capabilities JSON 恢复 EdgeCapability，供列表、详情和只读接口复用。
        /// </summary>
        /// <param name="node">EdgeNode 持久化实体。</param>
        /// <param name="attrs">兼容旧属性键中的能力字段。</param>
        /// <returns>归一化后的 EdgeCapability 快照。</returns>
        private static EdgeCapabilityDto ToEdgeCapabilityDto(EdgeNode node, List<AttributeLatest> attrs)
        {
            var raw = Coalesce(node.Capabilities, GetString(attrs, Constants._EdgeCapabilities));
            var capability = DeserializeEdgeCapabilityDto(raw) ?? new EdgeCapabilityDto();
            var protocols = NormalizeStringList(capability.Protocols);
            var supportedProtocols = NormalizeProtocolTypes(capability.SupportedProtocols, protocols);
            var supportedPointTypes = NormalizeStringList(capability.SupportedPointTypes);
            var supportedTransforms = NormalizeEnumList(capability.SupportedTransforms);
            var supportedReportTriggers = NormalizeEnumList(capability.SupportedReportTriggers);
            var features = NormalizeStringList(capability.Features);
            var tasks = NormalizeStringList(capability.Tasks);
            var taskCapabilities = NormalizeTaskCapabilities(capability.TaskCapabilities, tasks);
            tasks = NormalizeStringList(tasks.Concat(taskCapabilities.Select(taskCapability => taskCapability.TaskType)));
            var compatibleContracts = NormalizeCompatibleContracts(capability.CompatibleContracts);

            if (compatibleContracts.Count == 0)
            {
                compatibleContracts = BuildDefaultCompatibleContracts(
                    protocols.Count > 0 || supportedProtocols.Count > 0 || supportedPointTypes.Count > 0 || supportedTransforms.Count > 0 || supportedReportTriggers.Count > 0,
                    tasks.Count > 0 || taskCapabilities.Count > 0);
            }

            var metadata = capability.Metadata is { Count: > 0 }
                ? capability.Metadata
                : DeserializeObjectMap(Coalesce(node.Metadata, GetString(attrs, Constants._EdgeMetadata)));

            return capability with
            {
                ContractVersion = Coalesce(capability.ContractVersion, EdgeNodeContractVersions.EdgeCapabilityV1),
                EdgeNodeId = node.Id,
                GatewayId = node.GatewayId,
                RuntimeType = Coalesce(capability.RuntimeType, Coalesce(node.RuntimeType, GetString(attrs, Constants._EdgeRuntimeType))),
                RuntimeName = Coalesce(capability.RuntimeName, Coalesce(node.RuntimeName, GetString(attrs, Constants._EdgeRuntimeName))),
                Version = Coalesce(capability.Version, Coalesce(node.Version, GetString(attrs, Constants._EdgeVersion))),
                InstanceId = Coalesce(capability.InstanceId, Coalesce(node.InstanceId, GetString(attrs, Constants._EdgeInstanceId))),
                UpdatedAt = capability.UpdatedAt ?? node.UpdatedAt,
                Protocols = protocols,
                SupportedProtocols = supportedProtocols,
                SupportedPointTypes = supportedPointTypes,
                SupportedTransforms = supportedTransforms,
                SupportedReportTriggers = supportedReportTriggers,
                Features = features,
                Tasks = tasks,
                TaskCapabilities = taskCapabilities,
                CompatibleContracts = compatibleContracts,
                Metadata = metadata
            };
        }

        private static EdgeCapabilityDto DeserializeEdgeCapabilityDto(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<EdgeCapabilityDto>(json, WebJsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static IReadOnlyList<string> NormalizeStringList(IEnumerable<string> values)
        {
            return (values ?? [])
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static IReadOnlyList<TEnum> NormalizeEnumList<TEnum>(IEnumerable<TEnum> values)
            where TEnum : struct, Enum
        {
            return (values ?? []).Distinct().ToArray();
        }

        private static IReadOnlyList<CollectionProtocolType> NormalizeProtocolTypes(IEnumerable<CollectionProtocolType> reportedProtocols, IEnumerable<string> protocolIds)
        {
            var protocols = new List<CollectionProtocolType>();
            foreach (var protocol in reportedProtocols ?? [])
            {
                if (protocol != CollectionProtocolType.Unknown && !protocols.Contains(protocol))
                {
                    protocols.Add(protocol);
                }
            }

            foreach (var protocolId in protocolIds ?? [])
            {
                var protocol = ParseProtocolType(protocolId);
                if (protocol != CollectionProtocolType.Unknown && !protocols.Contains(protocol))
                {
                    protocols.Add(protocol);
                }
            }

            return protocols;
        }

        private static CollectionProtocolType ParseProtocolType(string protocol)
        {
            if (string.IsNullOrWhiteSpace(protocol))
            {
                return CollectionProtocolType.Unknown;
            }

            var normalized = protocol.Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace(".", string.Empty, StringComparison.Ordinal);

            return normalized.ToLowerInvariant() switch
            {
                "modbus" or "modbustcp" or "modbusrtu" => CollectionProtocolType.Modbus,
                "opcua" or "opc" => CollectionProtocolType.OpcUa,
                "bacnet" => CollectionProtocolType.Bacnet,
                "iec104" => CollectionProtocolType.IEC104,
                "mqtt" => CollectionProtocolType.Mqtt,
                _ => Enum.TryParse<CollectionProtocolType>(protocol, true, out var parsed) ? parsed : CollectionProtocolType.Unknown
            };
        }

        private static IReadOnlyList<EdgeTaskCapabilityDto> NormalizeTaskCapabilities(IEnumerable<EdgeTaskCapabilityDto> reportedCapabilities, IEnumerable<string> taskNames)
        {
            var result = new List<EdgeTaskCapabilityDto>();
            foreach (var capability in reportedCapabilities ?? [])
            {
                if (capability == null || string.IsNullOrWhiteSpace(capability.TaskType))
                {
                    continue;
                }

                result.Add(capability with
                {
                    TaskType = capability.TaskType.Trim(),
                    ContractVersion = Coalesce(capability.ContractVersion, EdgeNodeContractVersions.EdgeTaskV1),
                    Metadata = capability.Metadata ?? []
                });
            }

            foreach (var taskName in NormalizeStringList(taskNames))
            {
                if (result.Any(capability => string.Equals(capability.TaskType, taskName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                result.Add(new EdgeTaskCapabilityDto
                {
                    TaskType = taskName,
                    ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                    SupportsProgress = !string.Equals(taskName, nameof(EdgeTaskType.RestartRuntime), StringComparison.OrdinalIgnoreCase)
                });
            }

            return result
                .GroupBy(capability => capability.TaskType, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToArray();
        }

        private static IReadOnlyList<EdgeContractCompatibilityDto> NormalizeCompatibleContracts(IEnumerable<EdgeContractCompatibilityDto> compatibleContracts)
        {
            return (compatibleContracts ?? [])
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.ContractName) && !string.IsNullOrWhiteSpace(item.ContractVersion))
                .Select(item => item with
                {
                    ContractName = item.ContractName.Trim(),
                    ContractVersion = item.ContractVersion.Trim(),
                    Metadata = item.Metadata ?? []
                })
                .GroupBy(item => $"{item.ContractName}:{item.ContractVersion}", StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToArray();
        }

        private static IReadOnlyList<EdgeContractCompatibilityDto> BuildDefaultCompatibleContracts(bool includeCollectionConfig, bool includeEdgeTask)
        {
            var contracts = new List<EdgeContractCompatibilityDto>
            {
                new() { ContractName = "edge-runtime", ContractVersion = EdgeNodeContractVersions.EdgeRuntimeV1 },
                new() { ContractName = "edge-capability", ContractVersion = EdgeNodeContractVersions.EdgeCapabilityV1 }
            };

            if (includeCollectionConfig)
            {
                contracts.Add(new EdgeContractCompatibilityDto
                {
                    ContractName = "collection-config",
                    ContractVersion = EdgeNodeContractVersions.CollectionConfigV1
                });
            }

            if (includeEdgeTask)
            {
                contracts.Add(new EdgeContractCompatibilityDto
                {
                    ContractName = "edge-task",
                    ContractVersion = EdgeNodeContractVersions.EdgeTaskV1
                });
            }

            return contracts;
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

        private static string Coalesce(string primary, string fallback) => string.IsNullOrWhiteSpace(primary) ? fallback : primary;

        private static bool GetBoolean(List<AttributeLatest> attrs, string key) => attrs.FirstOrDefault(c => c.KeyName == key)?.Value_Boolean ?? false;

        private static bool? GetNullableBoolean(List<AttributeLatest> attrs, string key) => attrs.FirstOrDefault(c => c.KeyName == key)?.Value_Boolean;

        private static long? GetNullableLong(List<AttributeLatest> attrs, string key) => attrs.FirstOrDefault(c => c.KeyName == key)?.Value_Long;

        private static DateTime? GetDateTime(List<AttributeLatest> attrs, string key) => attrs.FirstOrDefault(c => c.KeyName == key)?.Value_DateTime;

        private static Dictionary<string, object> DeserializeObjectMap(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

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

        /// <summary>
        /// 判断执行端上报的 Edge 运行时合同版本是否受当前平台支持。
        /// </summary>
        /// <param name="contractVersion">执行端上报的合同版本。</param>
        /// <returns>支持或兼容旧执行端时返回 true。</returns>
        private static bool IsSupportedEdgeRuntimeContract(string contractVersion)
        {
            return string.IsNullOrWhiteSpace(contractVersion)
                || string.Equals(contractVersion, EdgeNodeContractVersions.EdgeRuntimeV1, StringComparison.OrdinalIgnoreCase)
                || string.Equals(contractVersion, EdgeNodeContractVersions.EdgeNodeV1, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断执行端上报的 Edge 能力合同版本是否受当前平台支持。
        /// </summary>
        /// <param name="contractVersion">执行端上报的能力合同版本。</param>
        /// <returns>支持正式能力合同或兼容旧运行时合同版本时返回 true。</returns>
        private static bool IsSupportedEdgeCapabilityContract(string contractVersion)
        {
            return string.IsNullOrWhiteSpace(contractVersion)
                || string.Equals(contractVersion, EdgeNodeContractVersions.EdgeCapabilityV1, StringComparison.OrdinalIgnoreCase)
                || string.Equals(contractVersion, EdgeNodeContractVersions.EdgeRuntimeV1, StringComparison.OrdinalIgnoreCase)
                || string.Equals(contractVersion, EdgeNodeContractVersions.EdgeNodeV1, StringComparison.OrdinalIgnoreCase);
        }
    }
}
