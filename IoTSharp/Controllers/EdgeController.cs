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
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CollectionConfigurationVersionQueryDto = IoTSharp.Dtos.CollectionConfigurationVersionQueryDto;
using EdgeCollectionAssignmentQueryDto = IoTSharp.Dtos.EdgeCollectionAssignmentQueryDto;
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
            Constants._LastActivityDateTime
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
            var taskSummaries = await QueryEdgeTaskSummariesAsync(nodes.Select(c => c.GatewayId));
            var data = nodes.Select(node =>
            {
                taskSummaries.TryGetValue(node.GatewayId, out var taskSummary);
                return ToEdgeNodeDto(node, attrs.Where(c => c.DeviceId == node.GatewayId).ToList(), taskSummary);
            }).ToList();
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
            var taskSummaries = await QueryEdgeTaskSummariesAsync([gateway.Id]);
            taskSummaries.TryGetValue(gateway.Id, out var taskSummary);
            return new ApiResult<EdgeNodeDto>(ApiCode.Success, "OK", ToEdgeNodeDto(node, attrs, taskSummary));
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
            return new ApiResult<EdgeCapabilityDto>(ApiCode.Success, "OK", ToEdgeCapabilityDto(node));
        }

        [HttpGet("CollectionAssignments")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<PagedData<EdgeCollectionAssignmentDto>>> GetCollectionAssignments([FromQuery] EdgeCollectionAssignmentQueryDto query)
        {
            var profile = this.GetUserProfile();
            return new ApiResult<PagedData<EdgeCollectionAssignmentDto>>(
                ApiCode.Success,
                "OK",
                await QueryCollectionAssignmentsAsync(query, profile.Tenant, profile.Customer));
        }

        [HttpGet("{id:guid}/CollectionAssignments")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<PagedData<EdgeCollectionAssignmentDto>>> GetCollectionAssignments(Guid id, [FromQuery] EdgeCollectionAssignmentQueryDto query)
        {
            var profile = this.GetUserProfile();
            var gateway = await GetGatewayForProfileAsync(id, profile.Tenant, profile.Customer);
            if (gateway == null)
            {
                return new ApiResult<PagedData<EdgeCollectionAssignmentDto>>(ApiCode.NotFoundDevice, "Edge node not found", null);
            }

            query ??= new EdgeCollectionAssignmentQueryDto();
            query.GatewayId = gateway.Id;

            return new ApiResult<PagedData<EdgeCollectionAssignmentDto>>(
                ApiCode.Success,
                "OK",
                await QueryCollectionAssignmentsAsync(query, profile.Tenant, profile.Customer));
        }

        [HttpGet("CollectionConfigVersions")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<PagedData<CollectionConfigurationVersionDto>>> GetCollectionConfigVersions([FromQuery] CollectionConfigurationVersionQueryDto query)
        {
            var profile = this.GetUserProfile();
            return new ApiResult<PagedData<CollectionConfigurationVersionDto>>(
                ApiCode.Success,
                "OK",
                await QueryCollectionConfigurationVersionsAsync(query, profile.Tenant, profile.Customer));
        }

        [HttpGet("{id:guid}/CollectionConfigVersions")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<PagedData<CollectionConfigurationVersionDto>>> GetCollectionConfigVersions(Guid id, [FromQuery] CollectionConfigurationVersionQueryDto query)
        {
            var profile = this.GetUserProfile();
            var gateway = await GetGatewayForProfileAsync(id, profile.Tenant, profile.Customer);
            if (gateway == null)
            {
                return new ApiResult<PagedData<CollectionConfigurationVersionDto>>(ApiCode.NotFoundDevice, "Edge node not found", null);
            }

            query ??= new CollectionConfigurationVersionQueryDto();
            query.GatewayId = gateway.Id;

            return new ApiResult<PagedData<CollectionConfigurationVersionDto>>(
                ApiCode.Success,
                "OK",
                await QueryCollectionConfigurationVersionsAsync(query, profile.Tenant, profile.Customer));
        }

        [HttpGet("{id:guid}/CollectionConfigVersions/{version:int}")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<CollectionConfigurationVersionDto>> GetCollectionConfigVersion(Guid id, int version)
        {
            var profile = this.GetUserProfile();
            var gateway = await GetGatewayForProfileAsync(id, profile.Tenant, profile.Customer);
            if (gateway == null)
            {
                return new ApiResult<CollectionConfigurationVersionDto>(ApiCode.NotFoundDevice, "Edge node not found", null);
            }

            var item = await _context.CollectionConfigurationVersions
                .Include(c => c.Gateway)
                .Where(c => c.GatewayId == gateway.Id
                    && c.Version == version
                    && c.CustomerId == profile.Customer
                    && c.TenantId == profile.Tenant
                    && !c.Deleted)
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return new ApiResult<CollectionConfigurationVersionDto>(ApiCode.CantFindObject, "Collection configuration version not found", null);
            }

            return new ApiResult<CollectionConfigurationVersionDto>(ApiCode.Success, "OK", ToCollectionConfigurationVersionDto(item, includeConfiguration: true));
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
            var updatedBy = string.IsNullOrWhiteSpace(profile.Name) ? profile.Email : profile.Name;
            var normalizedTasks = tasks.Select(task => NormalizeCollectionTask(task, gateway.Id, version)).ToArray();
            var sourceType = string.IsNullOrWhiteSpace(request?.SourceType) ? "InlineCollectionConfig" : request.SourceType.Trim();
            var sourceId = request?.SourceId?.Trim() ?? string.Empty;
            var sourceVersion = string.IsNullOrWhiteSpace(request?.SourceVersion) ? version.ToString() : request.SourceVersion.Trim();
            var sourceMetadata = request?.SourceMetadata ?? new Dictionary<string, object>();
            var document = new EdgeCollectionConfigurationDto
            {
                EdgeNodeId = gateway.Id,
                Version = version,
                UpdatedAt = updatedAt,
                UpdatedBy = updatedBy,
                SourceType = sourceType,
                SourceId = sourceId,
                SourceVersion = sourceVersion,
                SourceMetadata = sourceMetadata,
                Tasks = normalizedTasks
            };
            var payload = JsonSerializer.Serialize(document, WebJsonOptions);
            var node = await EnsureEdgeNodeAsync(gateway);
            var sourceMetadataJson = SerializeOrNull(sourceMetadata) ?? "{}";
            var configurationVersion = CreateCollectionConfigurationVersion(gateway, node, document, payload, updatedBy, updatedAt, sourceType, sourceId, sourceVersion, sourceMetadataJson);
            _context.CollectionConfigurationVersions.Add(configurationVersion);
            await PrepareCollectionAssignmentAsync(gateway, node, document, payload, configurationVersion, updatedBy, updatedAt, sourceType, sourceId, sourceVersion, sourceMetadataJson);

            await _context.SaveAsync<AttributeLatest>(new Dictionary<string, object>
            {
                [Constants._EdgeCollectionConfig] = payload,
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

            var pullResult = await PullCollectionConfigForGatewayAsync(gateway.Id);
            return Ok(new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.Success, "OK", pullResult.Configuration));
        }

        [AllowAnonymous]
        [HttpGet("{access_token}/CollectionConfig/Pull")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<EdgeCollectionConfigurationPullResultDto>>> PullTargetCollectionConfig(string access_token)
        {
            var gateway = await GetGatewayByAccessTokenAsync(access_token);
            if (gateway == null)
            {
                return Ok(new ApiResult<EdgeCollectionConfigurationPullResultDto>(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token", null));
            }

            var pullResult = await PullCollectionConfigForGatewayAsync(gateway.Id);
            return Ok(new ApiResult<EdgeCollectionConfigurationPullResultDto>(ApiCode.Success, "OK", pullResult));
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

        /// <summary>
        /// 为执行端读取当前目标采集配置，并返回可核验的版本、哈希和分配快照。
        /// </summary>
        /// <param name="gatewayId">承载配置拉取通道的 Gateway 设备 ID。</param>
        /// <returns>执行端拉取结果。</returns>
        private async Task<EdgeCollectionConfigurationPullResultDto> PullCollectionConfigForGatewayAsync(Guid gatewayId)
        {
            var pulledAt = DateTime.UtcNow;
            var document = await ReadCollectionConfigAsync(gatewayId);
            var assignment = await ReadActiveCollectionAssignmentAsync(gatewayId, document.Version);
            var versionRecord = assignment?.CollectionConfigurationVersionId is { } versionId
                ? await ReadCollectionConfigurationVersionAsync(versionId)
                : null;

            versionRecord ??= await ReadCollectionConfigurationVersionAsync(gatewayId, document.Version);

            if (assignment != null)
            {
                assignment.LastPulledAt = pulledAt;
                assignment.UpdatedAt = pulledAt;
                await _context.SaveChangesAsync();
            }

            var configurationHash = assignment?.ConfigurationHash ?? versionRecord?.ConfigurationHash ?? string.Empty;
            if (string.IsNullOrWhiteSpace(configurationHash) && document.Version > 0)
            {
                configurationHash = ComputeSha256(JsonSerializer.Serialize(document, WebJsonOptions));
            }

            return new EdgeCollectionConfigurationPullResultDto
            {
                GatewayId = gatewayId,
                EdgeNodeId = versionRecord?.EdgeNodeId ?? assignment?.EdgeNodeId ?? document.EdgeNodeId,
                ConfigurationVersionId = assignment?.CollectionConfigurationVersionId ?? versionRecord?.Id,
                ConfigurationVersion = document.Version,
                ConfigurationHash = configurationHash,
                PulledAt = pulledAt,
                Assignment = assignment == null ? null : ToEdgeCollectionAssignmentDto(assignment),
                Configuration = document
            };
        }

        /// <summary>
        /// 按当前登录用户边界查询采集配置分配记录。
        /// </summary>
        /// <param name="query">分页和筛选条件。</param>
        /// <param name="tenantId">当前租户 ID。</param>
        /// <param name="customerId">当前客户 ID。</param>
        /// <returns>分页后的配置分配快照。</returns>
        private async Task<PagedData<EdgeCollectionAssignmentDto>> QueryCollectionAssignmentsAsync(
            EdgeCollectionAssignmentQueryDto query,
            Guid tenantId,
            Guid customerId)
        {
            query ??= new EdgeCollectionAssignmentQueryDto();
            query.Limit = Math.Clamp(query.Limit < 1 ? 10 : query.Limit, 1, 100);

            var assignments = _context.EdgeCollectionAssignments
                .Include(c => c.Gateway)
                .Where(c => c.CustomerId == customerId && c.TenantId == tenantId && !c.Deleted && !c.Gateway.Deleted);

            if (query.GatewayId.HasValue && query.GatewayId.Value != Guid.Empty)
            {
                assignments = assignments.Where(c => c.GatewayId == query.GatewayId.Value);
            }

            if (query.EdgeNodeId.HasValue && query.EdgeNodeId.Value != Guid.Empty)
            {
                assignments = assignments.Where(c => c.EdgeNodeId == query.EdgeNodeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.TargetType) && Enum.TryParse<EdgeTaskTargetType>(query.TargetType, true, out var targetType))
            {
                assignments = assignments.Where(c => c.TargetType == targetType);
            }

            if (query.Status.HasValue)
            {
                assignments = assignments.Where(c => c.Status == query.Status.Value);
            }

            if (query.ConfigurationVersion.HasValue)
            {
                assignments = assignments.Where(c => c.ConfigurationVersion == query.ConfigurationVersion.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.RuntimeType))
            {
                assignments = assignments.Where(c => c.RuntimeType == query.RuntimeType);
            }

            if (!string.IsNullOrWhiteSpace(query.TargetKey))
            {
                assignments = assignments.Where(c => c.TargetKey.Contains(query.TargetKey));
            }

            if (!string.IsNullOrWhiteSpace(query.SourceType))
            {
                assignments = assignments.Where(c => c.SourceType == query.SourceType);
            }

            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                assignments = assignments.Where(c => c.Gateway.Name.Contains(query.Name) || c.TargetKey.Contains(query.Name));
            }

            var total = await assignments.CountAsync();
            var rows = await ApplyCollectionAssignmentSorting(assignments, query)
                .Skip(query.Offset * query.Limit)
                .Take(query.Limit)
                .ToListAsync();

            return new PagedData<EdgeCollectionAssignmentDto>
            {
                total = total,
                rows = rows.Select(ToEdgeCollectionAssignmentDto).ToList()
            };
        }

        /// <summary>
        /// 按当前登录用户边界查询采集配置版本快照。
        /// </summary>
        /// <param name="query">分页和筛选条件。</param>
        /// <param name="tenantId">当前租户 ID。</param>
        /// <param name="customerId">当前客户 ID。</param>
        /// <returns>分页后的配置版本快照。</returns>
        private async Task<PagedData<CollectionConfigurationVersionDto>> QueryCollectionConfigurationVersionsAsync(
            CollectionConfigurationVersionQueryDto query,
            Guid tenantId,
            Guid customerId)
        {
            query ??= new CollectionConfigurationVersionQueryDto();
            query.Limit = Math.Clamp(query.Limit < 1 ? 10 : query.Limit, 1, 100);

            var versions = _context.CollectionConfigurationVersions
                .Include(c => c.Gateway)
                .Where(c => c.CustomerId == customerId && c.TenantId == tenantId && !c.Deleted && !c.Gateway.Deleted);

            if (query.GatewayId.HasValue && query.GatewayId.Value != Guid.Empty)
            {
                versions = versions.Where(c => c.GatewayId == query.GatewayId.Value);
            }

            if (query.EdgeNodeId.HasValue && query.EdgeNodeId.Value != Guid.Empty)
            {
                versions = versions.Where(c => c.EdgeNodeId == query.EdgeNodeId.Value);
            }

            if (query.Version.HasValue)
            {
                versions = versions.Where(c => c.Version == query.Version.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.ConfigurationHash))
            {
                versions = versions.Where(c => c.ConfigurationHash == query.ConfigurationHash);
            }

            if (!string.IsNullOrWhiteSpace(query.SourceType))
            {
                versions = versions.Where(c => c.SourceType == query.SourceType);
            }

            if (!string.IsNullOrWhiteSpace(query.SourceId))
            {
                versions = versions.Where(c => c.SourceId == query.SourceId);
            }

            if (!string.IsNullOrWhiteSpace(query.SourceVersion))
            {
                versions = versions.Where(c => c.SourceVersion == query.SourceVersion);
            }

            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                versions = versions.Where(c => c.Gateway.Name.Contains(query.Name) || c.SourceId.Contains(query.Name));
            }

            var total = await versions.CountAsync();
            var rows = await ApplyCollectionConfigurationVersionSorting(versions, query)
                .Skip(query.Offset * query.Limit)
                .Take(query.Limit)
                .ToListAsync();

            return new PagedData<CollectionConfigurationVersionDto>
            {
                total = total,
                rows = rows.Select(item => ToCollectionConfigurationVersionDto(item, query.IncludeConfiguration)).ToList()
            };
        }

        /// <summary>
        /// 准备采集配置分配记录，与配置版本快照和 AttributeLatest 兼容视图在同一次保存中提交。
        /// </summary>
        /// <param name="gateway">承载配置拉取通道的 Gateway 设备。</param>
        /// <param name="node">平台侧 EdgeNode。</param>
        /// <param name="document">待分配的采集配置文档。</param>
        /// <param name="payload">规范化后的采集配置 JSON。</param>
        /// <param name="configurationVersion">平台侧配置版本快照。</param>
        /// <param name="updatedBy">操作者显示名或账号标识。</param>
        /// <param name="now">本次分配的 UTC 时间。</param>
        /// <returns>已加入上下文但尚未保存的分配记录。</returns>
        private async Task<EdgeCollectionAssignment> PrepareCollectionAssignmentAsync(
            Device gateway,
            EdgeNode node,
            EdgeCollectionConfigurationDto document,
            string payload,
            CollectionConfigurationVersion configurationVersion,
            string updatedBy,
            DateTime now,
            string sourceType,
            string sourceId,
            string sourceVersion,
            string sourceMetadata)
        {
            var activeAssignments = await _context.EdgeCollectionAssignments
                .Where(c => c.GatewayId == gateway.Id && !c.Deleted && c.Status == EdgeCollectionAssignmentStatus.Active)
                .ToListAsync();

            foreach (var assignment in activeAssignments)
            {
                assignment.Status = EdgeCollectionAssignmentStatus.Superseded;
                assignment.UpdatedAt = now;
                assignment.UpdatedBy = updatedBy;
            }

            var runtimeType = Coalesce(node?.RuntimeType, EdgeRuntimeTypes.Gateway);
            var instanceId = node?.InstanceId ?? string.Empty;
            var newAssignment = new EdgeCollectionAssignment
            {
                Id = Guid.NewGuid(),
                CollectionConfigurationVersionId = configurationVersion?.Id,
                ContractVersion = EdgeNodeContractVersions.CollectionConfigV1,
                TargetType = ResolveCollectionAssignmentTargetType(runtimeType),
                GatewayId = gateway.Id,
                EdgeNodeId = node?.Id ?? gateway.Id,
                TargetKey = BuildEdgeTargetKey(gateway.Id, runtimeType, instanceId),
                RuntimeType = runtimeType,
                InstanceId = instanceId,
                ConfigurationVersion = document.Version,
                ConfigurationHash = configurationVersion?.ConfigurationHash ?? ComputeSha256(payload),
                TaskCount = configurationVersion?.TaskCount ?? document.Tasks?.Count ?? 0,
                Status = EdgeCollectionAssignmentStatus.Active,
                SourceType = string.IsNullOrWhiteSpace(sourceType) ? "InlineCollectionConfig" : sourceType,
                SourceId = sourceId ?? string.Empty,
                SourceVersion = string.IsNullOrWhiteSpace(sourceVersion) ? document.Version.ToString() : sourceVersion,
                Metadata = string.IsNullOrWhiteSpace(sourceMetadata) ? "{}" : sourceMetadata,
                AssignedAt = now,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = updatedBy,
                UpdatedBy = updatedBy,
                TenantId = gateway.TenantId ?? gateway.Tenant?.Id,
                CustomerId = gateway.CustomerId ?? gateway.Customer?.Id,
                Gateway = gateway
            };

            _context.EdgeCollectionAssignments.Add(newAssignment);
            return newAssignment;
        }

        /// <summary>
        /// 构建平台侧采集配置版本快照。
        /// </summary>
        /// <param name="gateway">承载配置拉取通道的 Gateway 设备。</param>
        /// <param name="node">平台侧 EdgeNode。</param>
        /// <param name="document">配置正文。</param>
        /// <param name="payload">规范化后的配置 JSON。</param>
        /// <param name="updatedBy">操作者显示名或账号标识。</param>
        /// <param name="now">本次生成的 UTC 时间。</param>
        /// <param name="sourceType">配置来源类型。</param>
        /// <param name="sourceId">配置来源标识。</param>
        /// <param name="sourceVersion">配置来源版本。</param>
        /// <param name="sourceMetadata">非敏感来源扩展信息 JSON。</param>
        /// <returns>已构建但尚未加入上下文的配置版本快照。</returns>
        private static CollectionConfigurationVersion CreateCollectionConfigurationVersion(
            Device gateway,
            EdgeNode node,
            EdgeCollectionConfigurationDto document,
            string payload,
            string updatedBy,
            DateTime now,
            string sourceType,
            string sourceId,
            string sourceVersion,
            string sourceMetadata)
        {
            return new CollectionConfigurationVersion
            {
                Id = Guid.NewGuid(),
                ContractVersion = EdgeNodeContractVersions.CollectionConfigV1,
                GatewayId = gateway.Id,
                EdgeNodeId = node?.Id ?? gateway.Id,
                Version = document.Version,
                ConfigurationHash = ComputeSha256(payload),
                TaskCount = document.Tasks?.Count ?? 0,
                SourceType = string.IsNullOrWhiteSpace(sourceType) ? "InlineCollectionConfig" : sourceType,
                SourceId = sourceId ?? string.Empty,
                SourceVersion = string.IsNullOrWhiteSpace(sourceVersion) ? document.Version.ToString() : sourceVersion,
                SourceMetadata = string.IsNullOrWhiteSpace(sourceMetadata) ? "{}" : sourceMetadata,
                Payload = payload,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = updatedBy,
                UpdatedBy = updatedBy,
                TenantId = gateway.TenantId ?? gateway.Tenant?.Id,
                CustomerId = gateway.CustomerId ?? gateway.Customer?.Id,
                Gateway = gateway
            };
        }

        private static IQueryable<EdgeCollectionAssignment> ApplyCollectionAssignmentSorting(
            IQueryable<EdgeCollectionAssignment> source,
            EdgeCollectionAssignmentQueryDto query)
        {
            var descending = string.Equals(query?.Sort, "desc", StringComparison.OrdinalIgnoreCase);

            return query?.Sorter switch
            {
                nameof(EdgeCollectionAssignmentDto.ConfigurationVersion) => descending
                    ? source.OrderByDescending(c => c.ConfigurationVersion).ThenByDescending(c => c.AssignedAt)
                    : source.OrderBy(c => c.ConfigurationVersion).ThenBy(c => c.AssignedAt),
                nameof(EdgeCollectionAssignmentDto.Status) => descending
                    ? source.OrderByDescending(c => c.Status).ThenByDescending(c => c.AssignedAt)
                    : source.OrderBy(c => c.Status).ThenBy(c => c.AssignedAt),
                nameof(EdgeCollectionAssignmentDto.RuntimeType) => descending
                    ? source.OrderByDescending(c => c.RuntimeType).ThenByDescending(c => c.AssignedAt)
                    : source.OrderBy(c => c.RuntimeType).ThenBy(c => c.AssignedAt),
                nameof(EdgeCollectionAssignmentDto.LastPulledAt) => descending
                    ? source.OrderByDescending(c => c.LastPulledAt).ThenByDescending(c => c.AssignedAt)
                    : source.OrderBy(c => c.LastPulledAt).ThenBy(c => c.AssignedAt),
                nameof(EdgeCollectionAssignmentDto.LastExecutionStatus) => descending
                    ? source.OrderByDescending(c => c.LastExecutionStatus).ThenByDescending(c => c.LastExecutionAt)
                    : source.OrderBy(c => c.LastExecutionStatus).ThenBy(c => c.LastExecutionAt),
                nameof(EdgeCollectionAssignmentDto.LastExecutionAt) => descending
                    ? source.OrderByDescending(c => c.LastExecutionAt).ThenByDescending(c => c.AssignedAt)
                    : source.OrderBy(c => c.LastExecutionAt).ThenBy(c => c.AssignedAt),
                nameof(EdgeCollectionAssignmentDto.AppliedConfigurationVersion) => descending
                    ? source.OrderByDescending(c => c.AppliedConfigurationVersion).ThenByDescending(c => c.AppliedAt)
                    : source.OrderBy(c => c.AppliedConfigurationVersion).ThenBy(c => c.AppliedAt),
                _ => source.OrderByDescending(c => c.AssignedAt)
            };
        }

        private static IQueryable<CollectionConfigurationVersion> ApplyCollectionConfigurationVersionSorting(
            IQueryable<CollectionConfigurationVersion> source,
            CollectionConfigurationVersionQueryDto query)
        {
            var descending = string.Equals(query?.Sort, "desc", StringComparison.OrdinalIgnoreCase);

            return query?.Sorter switch
            {
                nameof(CollectionConfigurationVersionDto.Version) => descending
                    ? source.OrderByDescending(c => c.Version).ThenByDescending(c => c.CreatedAt)
                    : source.OrderBy(c => c.Version).ThenBy(c => c.CreatedAt),
                nameof(CollectionConfigurationVersionDto.TaskCount) => descending
                    ? source.OrderByDescending(c => c.TaskCount).ThenByDescending(c => c.CreatedAt)
                    : source.OrderBy(c => c.TaskCount).ThenBy(c => c.CreatedAt),
                nameof(CollectionConfigurationVersionDto.SourceType) => descending
                    ? source.OrderByDescending(c => c.SourceType).ThenByDescending(c => c.CreatedAt)
                    : source.OrderBy(c => c.SourceType).ThenBy(c => c.CreatedAt),
                nameof(CollectionConfigurationVersionDto.UpdatedAt) => descending
                    ? source.OrderByDescending(c => c.UpdatedAt).ThenByDescending(c => c.Version)
                    : source.OrderBy(c => c.UpdatedAt).ThenBy(c => c.Version),
                _ => source.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Version)
            };
        }

        private static EdgeCollectionAssignmentDto ToEdgeCollectionAssignmentDto(EdgeCollectionAssignment assignment)
        {
            return new EdgeCollectionAssignmentDto
            {
                ContractVersion = Coalesce(assignment.ContractVersion, EdgeNodeContractVersions.CollectionConfigV1),
                Id = assignment.Id,
                CollectionConfigurationVersionId = assignment.CollectionConfigurationVersionId,
                TargetType = assignment.TargetType,
                GatewayId = assignment.GatewayId,
                EdgeNodeId = assignment.EdgeNodeId,
                TargetKey = assignment.TargetKey ?? string.Empty,
                RuntimeType = assignment.RuntimeType ?? string.Empty,
                InstanceId = assignment.InstanceId ?? string.Empty,
                ConfigurationVersion = assignment.ConfigurationVersion,
                ConfigurationHash = assignment.ConfigurationHash ?? string.Empty,
                TaskCount = assignment.TaskCount,
                Status = assignment.Status,
                SourceType = assignment.SourceType ?? string.Empty,
                SourceId = assignment.SourceId ?? string.Empty,
                SourceVersion = assignment.SourceVersion ?? string.Empty,
                AssignedAt = assignment.AssignedAt,
                LastPulledAt = assignment.LastPulledAt,
                LastExecutionTaskId = assignment.LastExecutionTaskId,
                LastExecutionStatus = assignment.LastExecutionStatus,
                LastExecutionMessage = assignment.LastExecutionMessage ?? string.Empty,
                LastExecutionProgress = assignment.LastExecutionProgress,
                LastExecutionAt = assignment.LastExecutionAt,
                AppliedConfigurationVersion = assignment.AppliedConfigurationVersion,
                AppliedConfigurationHash = assignment.AppliedConfigurationHash ?? string.Empty,
                AppliedAt = assignment.AppliedAt,
                RevokedAt = assignment.RevokedAt,
                CreatedAt = assignment.CreatedAt,
                UpdatedAt = assignment.UpdatedAt,
                CreatedBy = assignment.CreatedBy ?? string.Empty,
                UpdatedBy = assignment.UpdatedBy ?? string.Empty,
                Metadata = DeserializeObjectMap(assignment.Metadata)
            };
        }

        private static CollectionConfigurationVersionDto ToCollectionConfigurationVersionDto(
            CollectionConfigurationVersion item,
            bool includeConfiguration)
        {
            return new CollectionConfigurationVersionDto
            {
                ContractVersion = Coalesce(item.ContractVersion, EdgeNodeContractVersions.CollectionConfigV1),
                Id = item.Id,
                GatewayId = item.GatewayId,
                EdgeNodeId = item.EdgeNodeId,
                Version = item.Version,
                ConfigurationHash = item.ConfigurationHash ?? string.Empty,
                TaskCount = item.TaskCount,
                SourceType = item.SourceType ?? string.Empty,
                SourceId = item.SourceId ?? string.Empty,
                SourceVersion = item.SourceVersion ?? string.Empty,
                SourceMetadata = DeserializeObjectMap(item.SourceMetadata),
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                CreatedBy = item.CreatedBy ?? string.Empty,
                UpdatedBy = item.UpdatedBy ?? string.Empty,
                Configuration = includeConfiguration ? DeserializeCollectionConfiguration(item.Payload) : null
            };
        }

        private static string BuildEdgeTargetKey(Guid gatewayId, string runtimeType, string instanceId)
        {
            var normalizedRuntimeType = string.IsNullOrWhiteSpace(runtimeType) ? EdgeRuntimeTypes.Gateway : runtimeType.Trim();
            return string.IsNullOrWhiteSpace(instanceId)
                ? $"{gatewayId}:{normalizedRuntimeType}"
                : $"{gatewayId}:{normalizedRuntimeType}:{instanceId.Trim()}";
        }

        private static EdgeTaskTargetType ResolveCollectionAssignmentTargetType(string runtimeType)
        {
            return string.Equals(runtimeType, EdgeRuntimeTypes.Gateway, StringComparison.OrdinalIgnoreCase)
                ? EdgeTaskTargetType.GatewayRuntime
                : EdgeTaskTargetType.EdgeNode;
        }

        private static string ComputeSha256(string payload)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload ?? string.Empty)));
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
        /// 按 Gateway 汇总最近 EdgeTask 状态，供 Edge 列表展示最近任务进度。
        /// </summary>
        /// <param name="gatewayIds">承载 EdgeNode 的 Gateway 设备 ID 集合。</param>
        /// <returns>按 Gateway ID 索引的最近任务状态摘要。</returns>
        private async Task<Dictionary<Guid, EdgeTaskSummary>> QueryEdgeTaskSummariesAsync(IEnumerable<Guid> gatewayIds)
        {
            var ids = gatewayIds?.Distinct().ToList() ?? [];
            if (ids.Count == 0)
            {
                return [];
            }

            var tasks = await _context.EdgeTasks
                .Where(task => ids.Contains(task.GatewayId) && !task.Deleted)
                .OrderByDescending(task => task.UpdatedAt)
                .Select(task => new
                {
                    task.GatewayId,
                    task.Status,
                    task.UpdatedAt,
                    task.LastReceiptAt
                })
                .ToListAsync();

            return tasks
                .GroupBy(task => task.GatewayId)
                .ToDictionary(
                    group => group.Key,
                    group =>
                    {
                        var latest = group.OrderByDescending(task => task.UpdatedAt).First();
                        return new EdgeTaskSummary
                        {
                            Status = latest.Status.ToString(),
                            LastReceiptAt = latest.LastReceiptAt
                        };
                    });
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

        private EdgeNodeDto ToEdgeNodeDto(EdgeNode node, List<AttributeLatest> attrs, EdgeTaskSummary taskSummary)
        {
            var gateway = node.Gateway;
            var runtimeStatus = ToEdgeRuntimeStatusDto(node, attrs);
            var capability = ToEdgeCapabilityDto(node);
            var capabilities = node.Capabilities ?? string.Empty;
            var metadata = node.Metadata ?? string.Empty;
            var metrics = node.Metrics ?? string.Empty;

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
                LastTaskStatus = taskSummary?.Status ?? string.Empty,
                LastReceiptDateTime = taskSummary?.LastReceiptAt,
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
            var metadata = node.Metadata ?? string.Empty;
            var metrics = node.Metrics ?? string.Empty;

            return new EdgeRuntimeStatusDto
            {
                EdgeNodeId = node.Id,
                GatewayId = node.GatewayId,
                Active = GetBoolean(attrs, Constants._Active),
                LastActivityDateTime = GetDateTime(attrs, Constants._LastActivityDateTime),
                RuntimeType = node.RuntimeType ?? string.Empty,
                RuntimeName = node.RuntimeName ?? string.Empty,
                Version = node.Version ?? string.Empty,
                InstanceId = node.InstanceId ?? string.Empty,
                Platform = node.Platform ?? string.Empty,
                HostName = node.HostName ?? string.Empty,
                IpAddress = node.IpAddress ?? string.Empty,
                Status = node.Status ?? string.Empty,
                Healthy = node.Healthy,
                UptimeSeconds = node.UptimeSeconds,
                LastHeartbeatDateTime = node.LastHeartbeatDateTime,
                LastRegistrationDateTime = node.LastRegistrationDateTime,
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
        /// <returns>归一化后的 EdgeCapability 快照。</returns>
        private static EdgeCapabilityDto ToEdgeCapabilityDto(EdgeNode node)
        {
            var raw = node.Capabilities ?? string.Empty;
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
                : DeserializeObjectMap(node.Metadata ?? string.Empty);

            return capability with
            {
                ContractVersion = Coalesce(capability.ContractVersion, EdgeNodeContractVersions.EdgeCapabilityV1),
                EdgeNodeId = node.Id,
                GatewayId = node.GatewayId,
                RuntimeType = Coalesce(capability.RuntimeType, node.RuntimeType ?? string.Empty),
                RuntimeName = Coalesce(capability.RuntimeName, node.RuntimeName ?? string.Empty),
                Version = Coalesce(capability.Version, node.Version ?? string.Empty),
                InstanceId = Coalesce(capability.InstanceId, node.InstanceId ?? string.Empty),
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

        private static string Coalesce(string primary, string fallback) => string.IsNullOrWhiteSpace(primary) ? fallback : primary;

        private static bool GetBoolean(List<AttributeLatest> attrs, string key) => attrs.FirstOrDefault(c => c.KeyName == key)?.Value_Boolean ?? false;

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

        private static EdgeCollectionConfigurationDto DeserializeCollectionConfiguration(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<EdgeCollectionConfigurationDto>(payload, WebJsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private async Task<EdgeCollectionConfigurationDto> ReadCollectionConfigAsync(Guid gatewayId)
        {
            var storedVersionRecord = await ReadCurrentCollectionConfigurationVersionAsync(gatewayId);
            if (storedVersionRecord != null)
            {
                var storedDocument = DeserializeCollectionConfiguration(storedVersionRecord.Payload);
                if (storedDocument != null)
                {
                    return NormalizeCollectionDocument(
                        storedDocument,
                        gatewayId,
                        storedVersionRecord.Version,
                        storedVersionRecord.UpdatedAt,
                        storedVersionRecord.UpdatedBy,
                        storedVersionRecord.SourceType,
                        storedVersionRecord.SourceId,
                        storedVersionRecord.SourceVersion,
                        storedVersionRecord.SourceMetadata);
                }

                _logger.LogWarning("Failed to deserialize collection configuration version {VersionId} for edge node {GatewayId}", storedVersionRecord.Id, gatewayId);
            }

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
                var document = JsonSerializer.Deserialize<EdgeCollectionConfigurationDto>(storedJson, WebJsonOptions);
                if (document == null)
                {
                    return CreateEmptyCollectionConfig(gatewayId, storedVersion, storedUpdatedAt);
                }

                var version = document.Version > 0 ? document.Version : storedVersion;
                var assignment = await ReadActiveCollectionAssignmentAsync(gatewayId, version);
                return NormalizeCollectionDocument(
                    document,
                    gatewayId,
                    version,
                    storedUpdatedAt ?? DateTime.UtcNow,
                    document.UpdatedBy,
                    assignment?.SourceType,
                    assignment?.SourceId,
                    assignment?.SourceVersion,
                    assignment?.Metadata);
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Failed to deserialize collection configuration for edge node {GatewayId}", gatewayId);
                return CreateEmptyCollectionConfig(gatewayId, storedVersion, storedUpdatedAt);
            }
        }

        /// <summary>
        /// 读取 Gateway 当前最新的采集配置版本快照。
        /// </summary>
        /// <param name="gatewayId">承载配置拉取通道的 Gateway 设备 ID。</param>
        /// <returns>最新配置版本；不存在时返回 null。</returns>
        private Task<CollectionConfigurationVersion> ReadLatestCollectionConfigurationVersionAsync(Guid gatewayId)
        {
            return _context.CollectionConfigurationVersions
                .Where(c => c.GatewayId == gatewayId && !c.Deleted)
                .OrderByDescending(c => c.Version)
                .ThenByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 读取 Gateway 当前目标采集配置版本快照，优先采用 Active 分配指向的版本。
        /// </summary>
        /// <param name="gatewayId">承载配置拉取通道的 Gateway 设备 ID。</param>
        /// <returns>当前目标配置版本；不存在时返回 null。</returns>
        private async Task<CollectionConfigurationVersion> ReadCurrentCollectionConfigurationVersionAsync(Guid gatewayId)
        {
            var assignment = await ReadCurrentCollectionAssignmentAsync(gatewayId);
            if (assignment?.CollectionConfigurationVersionId is { } versionId)
            {
                var assignedVersion = await ReadCollectionConfigurationVersionAsync(versionId);
                if (assignedVersion != null)
                {
                    return assignedVersion;
                }
            }

            if (assignment?.ConfigurationVersion > 0)
            {
                var assignedVersion = await ReadCollectionConfigurationVersionAsync(gatewayId, assignment.ConfigurationVersion);
                if (assignedVersion != null)
                {
                    return assignedVersion;
                }
            }

            return await ReadLatestCollectionConfigurationVersionAsync(gatewayId);
        }

        /// <summary>
        /// 按配置版本记录 ID 读取采集配置版本快照。
        /// </summary>
        /// <param name="versionId">配置版本记录 ID。</param>
        /// <returns>配置版本快照；不存在时返回 null。</returns>
        private Task<CollectionConfigurationVersion> ReadCollectionConfigurationVersionAsync(Guid versionId)
        {
            return _context.CollectionConfigurationVersions
                .Where(c => c.Id == versionId && !c.Deleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 按 Gateway 和版本号读取采集配置版本快照。
        /// </summary>
        /// <param name="gatewayId">承载配置拉取通道的 Gateway 设备 ID。</param>
        /// <param name="configurationVersion">配置版本号。</param>
        /// <returns>配置版本快照；不存在时返回 null。</returns>
        private Task<CollectionConfigurationVersion> ReadCollectionConfigurationVersionAsync(Guid gatewayId, int configurationVersion)
        {
            if (configurationVersion <= 0)
            {
                return Task.FromResult<CollectionConfigurationVersion>(null);
            }

            return _context.CollectionConfigurationVersions
                .Where(c => c.GatewayId == gatewayId && c.Version == configurationVersion && !c.Deleted)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 归一化存储中的 collection-config-v1 文档，补齐来源、版本和任务目标信息。
        /// </summary>
        /// <param name="document">存储中的配置正文。</param>
        /// <param name="gatewayId">承载配置拉取通道的 Gateway 设备 ID。</param>
        /// <param name="version">配置版本。</param>
        /// <param name="updatedAt">更新时间。</param>
        /// <param name="updatedBy">更新人。</param>
        /// <param name="sourceType">来源类型。</param>
        /// <param name="sourceId">来源标识。</param>
        /// <param name="sourceVersion">来源版本。</param>
        /// <param name="sourceMetadata">来源扩展信息 JSON。</param>
        /// <returns>可返回给平台 UI 或执行端的配置正文。</returns>
        private static EdgeCollectionConfigurationDto NormalizeCollectionDocument(
            EdgeCollectionConfigurationDto document,
            Guid gatewayId,
            int version,
            DateTime updatedAt,
            string updatedBy,
            string sourceType,
            string sourceId,
            string sourceVersion,
            string sourceMetadata)
        {
            return document with
            {
                EdgeNodeId = gatewayId,
                Version = version,
                UpdatedAt = document.UpdatedAt != default ? document.UpdatedAt : updatedAt,
                UpdatedBy = Coalesce(document.UpdatedBy, updatedBy ?? string.Empty),
                SourceType = Coalesce(document.SourceType, sourceType ?? string.Empty),
                SourceId = Coalesce(document.SourceId, sourceId ?? string.Empty),
                SourceVersion = Coalesce(document.SourceVersion, sourceVersion ?? string.Empty),
                SourceMetadata = document.SourceMetadata?.Count > 0
                    ? document.SourceMetadata
                    : DeserializeObjectMap(sourceMetadata),
                Tasks = (document.Tasks ?? []).Select(task => NormalizeCollectionTask(task, gatewayId, version)).ToArray()
            };
        }

        /// <summary>
        /// 读取当前生效配置分配，用于给旧版配置文档补回来源信息。
        /// </summary>
        /// <param name="gatewayId">承载配置拉取通道的 Gateway 设备 ID。</param>
        /// <returns>当前生效分配；不存在时返回 null。</returns>
        private Task<EdgeCollectionAssignment> ReadCurrentCollectionAssignmentAsync(Guid gatewayId)
        {
            return _context.EdgeCollectionAssignments
                .Where(c => c.GatewayId == gatewayId
                    && !c.Deleted
                    && c.Status == EdgeCollectionAssignmentStatus.Active)
                .OrderByDescending(c => c.AssignedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 读取当前生效配置分配，用于给旧版配置文档补回来源信息。
        /// </summary>
        /// <param name="gatewayId">承载配置拉取通道的 Gateway 设备 ID。</param>
        /// <param name="configurationVersion">配置版本。</param>
        /// <returns>当前生效分配；不存在时返回 null。</returns>
        private Task<EdgeCollectionAssignment> ReadActiveCollectionAssignmentAsync(Guid gatewayId, int configurationVersion)
        {
            if (configurationVersion <= 0)
            {
                return Task.FromResult<EdgeCollectionAssignment>(null);
            }

            return _context.EdgeCollectionAssignments
                .Where(c => c.GatewayId == gatewayId
                    && c.ConfigurationVersion == configurationVersion
                    && !c.Deleted
                    && c.Status == EdgeCollectionAssignmentStatus.Active)
                .OrderByDescending(c => c.AssignedAt)
                .FirstOrDefaultAsync();
        }

        private async Task<int> GetCurrentCollectionConfigVersionAsync(Guid gatewayId)
        {
            var storedVersion = await _context.CollectionConfigurationVersions
                .Where(c => c.GatewayId == gatewayId && !c.Deleted)
                .Select(c => (int?)c.Version)
                .MaxAsync() ?? 0;

            if (storedVersion > 0)
            {
                return storedVersion;
            }

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
        /// Edge 列表所需的最近任务摘要，来源于正式 EdgeTask 模型。
        /// </summary>
        private sealed class EdgeTaskSummary
        {
            /// <summary>
            /// 最近任务状态。
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// 最近一次正式回执上报时间。
            /// </summary>
            public DateTime? LastReceiptAt { get; set; }
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
