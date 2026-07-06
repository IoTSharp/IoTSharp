using IoTSharp.Contracts;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.Models;
using IoTSharp.Services;
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

namespace IoTSharp.Controllers
{
    /// <summary>
    /// Product 采集模板管理接口。
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CollectionTemplatesController : ControllerBase
    {
        private static readonly TimeSpan DefaultConfigurationPublishTtl = TimeSpan.FromMinutes(10);
        private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);

        private readonly ApplicationDbContext _context;
        private readonly ILogger<CollectionTemplatesController> _logger;

        public CollectionTemplatesController(ApplicationDbContext context, ILogger<CollectionTemplatesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 查询 Product 下的采集模板。
        /// </summary>
        /// <param name="productId">Product ID。</param>
        /// <param name="query">分页和名称筛选。</param>
        /// <returns>采集模板分页结果。</returns>
        [HttpGet]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<PagedData<CollectionTemplateDto>>> List([FromQuery] Guid productId, [FromQuery] QueryDto query)
        {
            var profile = this.GetUserProfile();
            query ??= new QueryDto();
            query.Limit = Math.Clamp(query.Limit < 1 ? 10 : query.Limit, 1, 100);

            if (productId == Guid.Empty)
            {
                return new ApiResult<PagedData<CollectionTemplateDto>>(ApiCode.NotFoundProduct, "Product not found", new PagedData<CollectionTemplateDto>());
            }

            var productExists = await ProductInScope(productId, profile.Tenant, profile.Customer).AnyAsync();
            if (!productExists)
            {
                return new ApiResult<PagedData<CollectionTemplateDto>>(ApiCode.NotFoundProduct, "Product not found", new PagedData<CollectionTemplateDto>());
            }

            var templates = TemplateGraph()
                .Where(c => c.ProductId == productId && c.TenantId == profile.Tenant && c.CustomerId == profile.Customer && !c.Deleted);

            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                templates = templates.Where(c => c.Name.Contains(query.Name) || c.TemplateKey.Contains(query.Name));
            }

            var total = await templates.CountAsync();
            var rows = await templates
                .OrderBy(c => c.TemplateKey)
                .Skip(query.Offset * query.Limit)
                .Take(query.Limit)
                .ToListAsync();

            return new ApiResult<PagedData<CollectionTemplateDto>>(ApiCode.Success, "OK", new PagedData<CollectionTemplateDto>
            {
                total = total,
                rows = rows.Select(CollectionTemplateService.ToDto).ToList()
            });
        }

        /// <summary>
        /// 获取采集模板详情。
        /// </summary>
        /// <param name="id">采集模板 ID。</param>
        /// <returns>采集模板详情。</returns>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<CollectionTemplateDto>> Get(Guid id)
        {
            var profile = this.GetUserProfile();
            var template = await FindTemplateAsync(id, profile.Tenant, profile.Customer);
            if (template == null)
            {
                return new ApiResult<CollectionTemplateDto>(ApiCode.CantFindObject, "Collection template not found", null);
            }

            return new ApiResult<CollectionTemplateDto>(ApiCode.Success, "OK", CollectionTemplateService.ToDto(template));
        }

        /// <summary>
        /// 新增采集模板。
        /// </summary>
        /// <param name="dto">采集模板。</param>
        /// <returns>新增后的采集模板。</returns>
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<CollectionTemplateDto>>> Create([FromBody] CollectionTemplateUpsertDto dto)
        {
            if (dto == null || dto.ProductId == Guid.Empty)
            {
                return Ok(new ApiResult<CollectionTemplateDto>(ApiCode.InValidData, "Collection template payload is invalid", null));
            }

            var profile = this.GetUserProfile();
            var product = await ProductInScope(dto.ProductId, profile.Tenant, profile.Customer)
                .Include(c => c.Tenant)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync();
            if (product == null)
            {
                return Ok(new ApiResult<CollectionTemplateDto>(ApiCode.NotFoundProduct, "Product not found", null));
            }

            var now = DateTime.UtcNow;
            var updatedBy = ResolveUserName(profile);
            var template = new CollectionTemplate();
            CollectionTemplateService.ApplyUpsert(template, dto, product, updatedBy, now, isCreate: true);

            if (await TemplateKeyExistsAsync(product.Id, template.TemplateKey))
            {
                return Ok(new ApiResult<CollectionTemplateDto>(ApiCode.InValidData, "Collection template key already exists", null));
            }

            var validation = CollectionTemplateService.Validate(template);
            if (template.Status == CollectionTemplateStatus.Active && !validation.Success)
            {
                return Ok(new ApiResult<CollectionTemplateDto>(ApiCode.InValidData, FirstDiagnosticMessage(validation), null));
            }

            _context.CollectionTemplates.Add(template);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created collection template {TemplateId} for product {ProductId}", template.Id, product.Id);
            return Ok(new ApiResult<CollectionTemplateDto>(ApiCode.Success, "OK", CollectionTemplateService.ToDto(template)));
        }

        /// <summary>
        /// 修改采集模板。
        /// </summary>
        /// <param name="id">采集模板 ID。</param>
        /// <param name="dto">采集模板。</param>
        /// <returns>修改后的采集模板。</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<CollectionTemplateDto>>> Update(Guid id, [FromBody] CollectionTemplateUpsertDto dto)
        {
            if (dto == null)
            {
                return Ok(new ApiResult<CollectionTemplateDto>(ApiCode.InValidData, "Collection template payload is invalid", null));
            }

            var profile = this.GetUserProfile();
            var template = await FindTemplateAsync(id, profile.Tenant, profile.Customer);
            if (template == null)
            {
                return Ok(new ApiResult<CollectionTemplateDto>(ApiCode.CantFindObject, "Collection template not found", null));
            }

            if (dto.ProductId != Guid.Empty && dto.ProductId != template.ProductId)
            {
                return Ok(new ApiResult<CollectionTemplateDto>(ApiCode.InValidData, "Collection template cannot be moved to another Product", null));
            }

            RemoveTemplateChildren(template);

            var now = DateTime.UtcNow;
            var updatedBy = ResolveUserName(profile);
            CollectionTemplateService.ApplyUpsert(template, dto with { ProductId = template.ProductId }, template.Product, updatedBy, now, isCreate: false);

            if (await TemplateKeyExistsAsync(template.ProductId, template.TemplateKey, template.Id))
            {
                return Ok(new ApiResult<CollectionTemplateDto>(ApiCode.InValidData, "Collection template key already exists", null));
            }

            var validation = CollectionTemplateService.Validate(template);
            if (template.Status == CollectionTemplateStatus.Active && !validation.Success)
            {
                return Ok(new ApiResult<CollectionTemplateDto>(ApiCode.InValidData, FirstDiagnosticMessage(validation), null));
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated collection template {TemplateId}", template.Id);
            return Ok(new ApiResult<CollectionTemplateDto>(ApiCode.Success, "OK", CollectionTemplateService.ToDto(template)));
        }

        /// <summary>
        /// 删除采集模板。
        /// </summary>
        /// <param name="id">采集模板 ID。</param>
        /// <returns>是否删除成功。</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            var profile = this.GetUserProfile();
            var template = await FindTemplateAsync(id, profile.Tenant, profile.Customer);
            if (template == null)
            {
                return new ApiResult<bool>(ApiCode.CantFindObject, "Collection template not found", false);
            }

            template.Deleted = true;
            template.UpdatedAt = DateTime.UtcNow;
            template.UpdatedBy = ResolveUserName(profile);
            await _context.SaveChangesAsync();
            return new ApiResult<bool>(ApiCode.Success, "OK", true);
        }

        /// <summary>
        /// 校验采集模板。
        /// </summary>
        /// <param name="id">采集模板 ID。</param>
        /// <returns>模板校验诊断。</returns>
        [HttpPost("{id:guid}/Validate")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<CollectionTemplateValidationResultDto>> Validate(Guid id)
        {
            var profile = this.GetUserProfile();
            var template = await FindTemplateAsync(id, profile.Tenant, profile.Customer);
            if (template == null)
            {
                return new ApiResult<CollectionTemplateValidationResultDto>(ApiCode.CantFindObject, "Collection template not found", null);
            }

            return new ApiResult<CollectionTemplateValidationResultDto>(ApiCode.Success, "OK", CollectionTemplateService.Validate(template));
        }

        /// <summary>
        /// 基于正式采集模板预览点位转换结果。
        /// </summary>
        /// <param name="id">采集模板 ID。</param>
        /// <param name="request">预览请求。</param>
        /// <returns>点位预览结果。</returns>
        [HttpPost("{id:guid}/Preview")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<TaskPreviewResponseDto>>> Preview(Guid id, [FromBody] CollectionTemplatePreviewRequestDto request)
        {
            var profile = this.GetUserProfile();
            var template = await FindTemplateAsync(id, profile.Tenant, profile.Customer);
            if (template == null)
            {
                return Ok(new ApiResult<TaskPreviewResponseDto>(ApiCode.CantFindObject, "Collection template not found", null));
            }

            var response = CollectionTemplateService.Preview(template, request ?? new CollectionTemplatePreviewRequestDto());
            return Ok(new ApiResult<TaskPreviewResponseDto>(ApiCode.Success, "OK", response));
        }

        /// <summary>
        /// 生成 IoTEdge 可消费的 collection-config-v1 运行时配置。
        /// </summary>
        /// <param name="id">采集模板 ID。</param>
        /// <param name="request">运行时配置生成请求。</param>
        /// <returns>边缘运行时采集配置。</returns>
        [HttpPost("{id:guid}/RuntimeConfig")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<EdgeCollectionConfigurationDto>>> RuntimeConfig(Guid id, [FromBody] CollectionTemplateRuntimeConfigRequestDto request)
        {
            var profile = this.GetUserProfile();
            var template = await FindTemplateAsync(id, profile.Tenant, profile.Customer);
            if (template == null)
            {
                return Ok(new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.CantFindObject, "Collection template not found", null));
            }

            var validation = CollectionTemplateService.Validate(template);
            if (!validation.Success)
            {
                return Ok(new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.InValidData, FirstDiagnosticMessage(validation), null));
            }

            var edgeNodeId = request?.EdgeNodeId ?? Guid.Empty;
            if (edgeNodeId == Guid.Empty || !await GatewayInScope(edgeNodeId, profile.Tenant, profile.Customer).AnyAsync())
            {
                return Ok(new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.NotFoundDevice, "Edge node not found", null));
            }

            var version = Math.Max(1, request?.Version ?? template.Version);
            var configuration = CollectionTemplateService.BuildRuntimeConfiguration(template, edgeNodeId, version, ResolveUserName(profile), DateTime.UtcNow);
            return Ok(new ApiResult<EdgeCollectionConfigurationDto>(ApiCode.Success, "OK", configuration));
        }

        /// <summary>
        /// 生成可提交给 Edge 配置保存接口的更新载荷，包含 ProductCollectionTemplate 来源信息。
        /// </summary>
        /// <param name="id">采集模板 ID。</param>
        /// <param name="request">运行时配置生成请求。</param>
        /// <returns>Edge 配置更新载荷。</returns>
        [HttpPost("{id:guid}/RuntimeConfigUpdate")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<EdgeCollectionConfigurationUpdateDto>>> RuntimeConfigUpdate(Guid id, [FromBody] CollectionTemplateRuntimeConfigRequestDto request)
        {
            var profile = this.GetUserProfile();
            var template = await FindTemplateAsync(id, profile.Tenant, profile.Customer);
            if (template == null)
            {
                return Ok(new ApiResult<EdgeCollectionConfigurationUpdateDto>(ApiCode.CantFindObject, "Collection template not found", null));
            }

            var validation = CollectionTemplateService.Validate(template);
            if (!validation.Success)
            {
                return Ok(new ApiResult<EdgeCollectionConfigurationUpdateDto>(ApiCode.InValidData, FirstDiagnosticMessage(validation), null));
            }

            var edgeNodeId = request?.EdgeNodeId ?? Guid.Empty;
            if (edgeNodeId == Guid.Empty || !await GatewayInScope(edgeNodeId, profile.Tenant, profile.Customer).AnyAsync())
            {
                return Ok(new ApiResult<EdgeCollectionConfigurationUpdateDto>(ApiCode.NotFoundDevice, "Edge node not found", null));
            }

            var version = Math.Max(1, request?.Version ?? template.Version);
            var configuration = CollectionTemplateService.BuildRuntimeConfiguration(template, edgeNodeId, version, ResolveUserName(profile), DateTime.UtcNow);
            var update = CollectionTemplateService.ToRuntimeConfigurationUpdate(template, configuration);
            return Ok(new ApiResult<EdgeCollectionConfigurationUpdateDto>(ApiCode.Success, "OK", update));
        }

        /// <summary>
        /// 从 Product 采集模板生成配置版本，并创建面向执行端的配置发布任务。
        /// </summary>
        /// <param name="id">采集模板 ID。</param>
        /// <param name="request">配置发布请求。</param>
        /// <returns>配置版本、目标分配和 EdgeTask 发布请求。</returns>
        [HttpPost("{id:guid}/PublishConfig")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<CollectionTemplateConfigurationPublishResultDto>>> PublishConfig(
            Guid id,
            [FromBody] CollectionTemplateConfigurationPublishRequestDto request)
        {
            var profile = this.GetUserProfile();
            var template = await FindTemplateAsync(id, profile.Tenant, profile.Customer);
            if (template == null)
            {
                return Ok(new ApiResult<CollectionTemplateConfigurationPublishResultDto>(ApiCode.CantFindObject, "Collection template not found", null));
            }

            if (!template.Enabled || template.Status != CollectionTemplateStatus.Active)
            {
                return Ok(new ApiResult<CollectionTemplateConfigurationPublishResultDto>(ApiCode.InValidData, "Collection template must be Active and enabled before publishing", null));
            }

            var validation = CollectionTemplateService.Validate(template);
            if (!validation.Success)
            {
                return Ok(new ApiResult<CollectionTemplateConfigurationPublishResultDto>(ApiCode.InValidData, FirstDiagnosticMessage(validation), null));
            }

            var edgeNodeId = request?.EdgeNodeId ?? Guid.Empty;
            if (edgeNodeId == Guid.Empty)
            {
                return Ok(new ApiResult<CollectionTemplateConfigurationPublishResultDto>(ApiCode.NotFoundDevice, "Edge node not found", null));
            }

            var gateway = await GatewayInScope(edgeNodeId, profile.Tenant, profile.Customer)
                .Include(c => c.DeviceIdentity)
                .Include(c => c.Tenant)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync();

            if (gateway == null)
            {
                return Ok(new ApiResult<CollectionTemplateConfigurationPublishResultDto>(ApiCode.NotFoundDevice, "Edge node not found", null));
            }

            if (request?.TaskId is { } requestedTaskId && requestedTaskId != Guid.Empty)
            {
                var taskExists = await _context.EdgeTasks.AnyAsync(task => task.Id == requestedTaskId && !task.Deleted);
                if (taskExists)
                {
                    return Ok(new ApiResult<CollectionTemplateConfigurationPublishResultDto>(ApiCode.InValidData, "taskId already exists", null));
                }
            }

            var now = DateTime.UtcNow;
            var updatedBy = ResolveUserName(profile);
            var node = await EnsureEdgeNodeAsync(gateway);
            var version = await GetNextCollectionConfigVersionAsync(gateway.Id);
            var configuration = CollectionTemplateService.BuildRuntimeConfiguration(template, gateway.Id, version, updatedBy, now);
            var payload = JsonSerializer.Serialize(configuration, WebJsonOptions);
            var sourceMetadata = SerializeOrNull(configuration.SourceMetadata) ?? "{}";
            var configurationVersion = CreateCollectionConfigurationVersion(gateway, node, configuration, payload, updatedBy, now, sourceMetadata);
            var assignment = await PrepareCollectionAssignmentAsync(gateway, node, configuration, configurationVersion, request, updatedBy, now, sourceMetadata);
            var taskRequest = CreateConfigurationPublishTaskRequest(template, gateway, node, configurationVersion, assignment, request, updatedBy, now);
            var edgeTask = CreateFormalEdgeTask(taskRequest, gateway, node, SerializeOrNull(taskRequest) ?? "{}");

            _context.CollectionConfigurationVersions.Add(configurationVersion);
            _context.EdgeTasks.Add(edgeTask);

            await _context.SaveAsync<AttributeLatest>(new Dictionary<string, object>
            {
                [Constants._EdgeCollectionConfig] = payload,
                [Constants._EdgeCollectionConfigVersion] = configuration.Version,
                [Constants._EdgeCollectionConfigUpdatedAt] = now
            }, gateway.Id, DataSide.ServerSide);

            _logger.LogInformation(
                "Published collection configuration version {Version} from template {TemplateId} to edge node {GatewayId} with task {TaskId}",
                configuration.Version,
                template.Id,
                gateway.Id,
                taskRequest.TaskId);

            return Ok(new ApiResult<CollectionTemplateConfigurationPublishResultDto>(
                ApiCode.Success,
                "OK",
                new CollectionTemplateConfigurationPublishResultDto
                {
                    ConfigurationVersion = ToCollectionConfigurationVersionDto(configurationVersion, configuration),
                    Assignment = ToEdgeCollectionAssignmentDto(assignment),
                    Task = taskRequest
                }));
        }

        private IQueryable<Product> ProductInScope(Guid productId, Guid tenantId, Guid customerId)
            => _context.Products
                .Where(c => c.Id == productId
                    && !c.Deleted
                    && c.Tenant.Id == tenantId
                    && c.Customer.Id == customerId);

        private IQueryable<Device> GatewayInScope(Guid gatewayId, Guid tenantId, Guid customerId)
            => _context.Device
                .Where(c => c.Id == gatewayId
                    && !c.Deleted
                    && c.DeviceType == DeviceType.Gateway
                    && c.Tenant.Id == tenantId
                    && c.Customer.Id == customerId);

        /// <summary>
        /// 获取或创建与 Gateway 一一对应的 EdgeNode，用于发布任务寻址和状态归属。
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

        /// <summary>
        /// 计算指定 Gateway 的下一个采集配置版本号。
        /// </summary>
        /// <param name="gatewayId">Gateway 设备 ID。</param>
        /// <returns>下一个版本号。</returns>
        private async Task<int> GetNextCollectionConfigVersionAsync(Guid gatewayId)
        {
            var storedVersion = await _context.CollectionConfigurationVersions
                .Where(c => c.GatewayId == gatewayId && !c.Deleted)
                .Select(c => (int?)c.Version)
                .MaxAsync() ?? 0;

            if (storedVersion <= 0)
            {
                storedVersion = (int?)await _context.AttributeLatest
                    .Where(attr => attr.DeviceId == gatewayId && attr.KeyName == Constants._EdgeCollectionConfigVersion)
                    .Select(attr => attr.Value_Long)
                    .FirstOrDefaultAsync() ?? 0;
            }

            return storedVersion + 1;
        }

        /// <summary>
        /// 构建采集配置版本快照实体。
        /// </summary>
        /// <param name="gateway">承载配置拉取通道的 Gateway 设备。</param>
        /// <param name="node">平台侧 EdgeNode。</param>
        /// <param name="configuration">运行时配置正文。</param>
        /// <param name="payload">规范化后的配置 JSON。</param>
        /// <param name="updatedBy">操作者显示名或账号。</param>
        /// <param name="now">生成时间。</param>
        /// <param name="sourceMetadata">非敏感来源扩展信息 JSON。</param>
        /// <returns>已初始化但尚未保存的配置版本快照。</returns>
        private static CollectionConfigurationVersion CreateCollectionConfigurationVersion(
            Device gateway,
            EdgeNode node,
            EdgeCollectionConfigurationDto configuration,
            string payload,
            string updatedBy,
            DateTime now,
            string sourceMetadata)
        {
            return new CollectionConfigurationVersion
            {
                Id = Guid.NewGuid(),
                ContractVersion = EdgeNodeContractVersions.CollectionConfigV1,
                GatewayId = gateway.Id,
                EdgeNodeId = node?.Id ?? gateway.Id,
                Version = configuration.Version,
                ConfigurationHash = ComputeSha256(payload),
                TaskCount = configuration.Tasks?.Count ?? 0,
                SourceType = configuration.SourceType ?? string.Empty,
                SourceId = configuration.SourceId ?? string.Empty,
                SourceVersion = configuration.SourceVersion ?? configuration.Version.ToString(),
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

        /// <summary>
        /// 准备采集配置目标分配，并将同一 Gateway 的旧 Active 分配标记为 Superseded。
        /// </summary>
        /// <param name="gateway">承载配置拉取通道的 Gateway 设备。</param>
        /// <param name="node">平台侧 EdgeNode。</param>
        /// <param name="configuration">运行时配置正文。</param>
        /// <param name="configurationVersion">配置版本快照。</param>
        /// <param name="request">发布请求。</param>
        /// <param name="updatedBy">操作者显示名或账号。</param>
        /// <param name="now">分配时间。</param>
        /// <param name="sourceMetadata">非敏感来源扩展信息 JSON。</param>
        /// <returns>已加入上下文但尚未保存的新分配。</returns>
        private async Task<EdgeCollectionAssignment> PrepareCollectionAssignmentAsync(
            Device gateway,
            EdgeNode node,
            EdgeCollectionConfigurationDto configuration,
            CollectionConfigurationVersion configurationVersion,
            CollectionTemplateConfigurationPublishRequestDto request,
            string updatedBy,
            DateTime now,
            string sourceMetadata)
        {
            var currentAssignments = await _context.EdgeCollectionAssignments
                .Where(c => c.GatewayId == gateway.Id && !c.Deleted && c.Status == EdgeCollectionAssignmentStatus.Active)
                .ToListAsync();

            foreach (var current in currentAssignments)
            {
                current.Status = EdgeCollectionAssignmentStatus.Superseded;
                current.UpdatedAt = now;
                current.UpdatedBy = updatedBy;
            }

            var runtimeType = Coalesce(request?.RuntimeType, node?.RuntimeType ?? EdgeRuntimeTypes.Gateway);
            var instanceId = Coalesce(request?.InstanceId, node?.InstanceId ?? string.Empty);
            var assignment = new EdgeCollectionAssignment
            {
                Id = Guid.NewGuid(),
                CollectionConfigurationVersionId = configurationVersion.Id,
                ContractVersion = EdgeNodeContractVersions.CollectionConfigV1,
                TargetType = request?.TargetType ?? EdgeTaskTargetType.GatewayRuntime,
                GatewayId = gateway.Id,
                Gateway = gateway,
                EdgeNodeId = node?.Id ?? gateway.Id,
                TargetKey = BuildEdgeTargetKey(gateway.Id, runtimeType, instanceId),
                RuntimeType = runtimeType,
                InstanceId = instanceId,
                ConfigurationVersion = configurationVersion.Version,
                ConfigurationHash = configurationVersion.ConfigurationHash,
                TaskCount = configurationVersion.TaskCount,
                Status = EdgeCollectionAssignmentStatus.Active,
                SourceType = configuration.SourceType ?? string.Empty,
                SourceId = configuration.SourceId ?? string.Empty,
                SourceVersion = configuration.SourceVersion ?? configuration.Version.ToString(),
                Metadata = string.IsNullOrWhiteSpace(sourceMetadata) ? "{}" : sourceMetadata,
                AssignedAt = now,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = updatedBy,
                UpdatedBy = updatedBy,
                TenantId = gateway.TenantId ?? gateway.Tenant?.Id,
                CustomerId = gateway.CustomerId ?? gateway.Customer?.Id
            };

            _context.EdgeCollectionAssignments.Add(assignment);
            return assignment;
        }

        /// <summary>
        /// 创建执行端通过 edge-task-v1 拉取的配置发布任务请求。
        /// </summary>
        /// <param name="template">发布来源采集模板。</param>
        /// <param name="gateway">承载任务通道的 Gateway 设备。</param>
        /// <param name="node">平台侧 EdgeNode。</param>
        /// <param name="configurationVersion">配置版本快照。</param>
        /// <param name="assignment">配置目标分配。</param>
        /// <param name="request">发布请求。</param>
        /// <param name="updatedBy">操作者显示名或账号。</param>
        /// <param name="now">任务创建时间。</param>
        /// <returns>EdgeTask 请求 DTO。</returns>
        private static EdgeTaskRequestDto CreateConfigurationPublishTaskRequest(
            CollectionTemplate template,
            Device gateway,
            EdgeNode node,
            CollectionConfigurationVersion configurationVersion,
            EdgeCollectionAssignment assignment,
            CollectionTemplateConfigurationPublishRequestDto request,
            string updatedBy,
            DateTime now)
        {
            var metadata = new Dictionary<string, string>(request?.Metadata ?? [], StringComparer.OrdinalIgnoreCase)
            {
                ["source"] = "collection-template-publish",
                ["operator"] = updatedBy ?? string.Empty,
                ["templateId"] = template.Id.ToString("D"),
                ["templateKey"] = template.TemplateKey ?? string.Empty,
                ["productId"] = template.ProductId.ToString("D"),
                ["assignmentId"] = assignment.Id.ToString("D")
            };

            return new EdgeTaskRequestDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = request?.TaskId is { } taskId && taskId != Guid.Empty ? taskId : Guid.NewGuid(),
                TaskType = EdgeTaskType.ConfigPullRequest,
                CreatedAt = now,
                ExpireAt = request?.ExpireAt?.ToUniversalTime() ?? now.Add(DefaultConfigurationPublishTtl),
                Address = new EdgeTaskAddressDto
                {
                    TargetType = assignment.TargetType,
                    DeviceId = gateway.Id,
                    RuntimeType = assignment.RuntimeType,
                    InstanceId = assignment.InstanceId,
                    TargetKey = assignment.TargetKey
                },
                Parameters = new Dictionary<string, object>
                {
                    ["configurationVersionId"] = configurationVersion.Id,
                    ["configurationVersion"] = configurationVersion.Version,
                    ["configurationHash"] = configurationVersion.ConfigurationHash ?? string.Empty,
                    ["collectionConfigContractVersion"] = EdgeNodeContractVersions.CollectionConfigV1,
                    ["sourceType"] = configurationVersion.SourceType ?? string.Empty,
                    ["sourceId"] = configurationVersion.SourceId ?? string.Empty,
                    ["sourceVersion"] = configurationVersion.SourceVersion ?? string.Empty,
                    ["edgeNodeId"] = node?.Id ?? gateway.Id,
                    ["gatewayId"] = gateway.Id
                },
                Metadata = metadata
            };
        }

        /// <summary>
        /// 将任务请求转换为正式 EdgeTask 当前态实体。
        /// </summary>
        /// <param name="request">任务请求。</param>
        /// <param name="gateway">承载任务通道的 Gateway 设备。</param>
        /// <param name="node">平台侧 EdgeNode。</param>
        /// <param name="requestPayload">规范化后的任务请求 JSON。</param>
        /// <returns>已初始化但尚未保存的 EdgeTask。</returns>
        private static EdgeTask CreateFormalEdgeTask(EdgeTaskRequestDto request, Device gateway, EdgeNode node, string requestPayload)
        {
            return new EdgeTask
            {
                Id = request.TaskId,
                ContractVersion = request.ContractVersion,
                TaskType = request.TaskType,
                TargetType = request.Address.TargetType,
                GatewayId = gateway.Id,
                Gateway = gateway,
                EdgeNodeId = node?.Id,
                TargetKey = request.Address.TargetKey ?? gateway.Id.ToString(),
                RuntimeType = request.Address.RuntimeType ?? string.Empty,
                InstanceId = request.Address.InstanceId ?? string.Empty,
                Status = EdgeTaskStatus.Pending,
                Parameters = SerializeOrNull(request.Parameters) ?? "{}",
                Metadata = SerializeOrNull(request.Metadata) ?? "{}",
                RequestPayload = requestPayload,
                CreatedAt = request.CreatedAt.ToUniversalTime(),
                ExpireAt = request.ExpireAt?.ToUniversalTime(),
                UpdatedAt = DateTime.UtcNow,
                TenantId = gateway.TenantId ?? gateway.Tenant?.Id,
                CustomerId = gateway.CustomerId ?? gateway.Customer?.Id
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
            EdgeCollectionConfigurationDto configuration)
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
                Configuration = configuration
            };
        }

        private static string BuildEdgeTargetKey(Guid gatewayId, string runtimeType, string instanceId)
        {
            var normalizedRuntimeType = string.IsNullOrWhiteSpace(runtimeType) ? EdgeRuntimeTypes.Gateway : runtimeType.Trim();
            return string.IsNullOrWhiteSpace(instanceId)
                ? $"{gatewayId}:{normalizedRuntimeType}"
                : $"{gatewayId}:{normalizedRuntimeType}:{instanceId.Trim()}";
        }

        private static string ComputeSha256(string payload)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload ?? string.Empty)));

        private static string Coalesce(string primary, string fallback)
            => string.IsNullOrWhiteSpace(primary) ? fallback : primary;

        private static Dictionary<string, object> DeserializeObjectMap(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json, WebJsonOptions) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        private static string SerializeOrNull<T>(T value)
        {
            if (value == null)
            {
                return null;
            }

            return JsonSerializer.Serialize(value, WebJsonOptions);
        }

        private IQueryable<CollectionTemplate> TemplateGraph()
            => _context.CollectionTemplates
                .Include(c => c.Product)
                .ThenInclude(c => c.Tenant)
                .Include(c => c.Product)
                .ThenInclude(c => c.Customer)
                .Include(c => c.Protocol)
                .Include(c => c.Connections)
                .Include(c => c.Points)
                .ThenInclude(c => c.SamplingPolicy)
                .Include(c => c.Points)
                .ThenInclude(c => c.Mapping)
                .Include(c => c.Points)
                .ThenInclude(c => c.Transforms);

        private Task<CollectionTemplate> FindTemplateAsync(Guid id, Guid tenantId, Guid customerId)
            => TemplateGraph()
                .FirstOrDefaultAsync(c => c.Id == id
                    && !c.Deleted
                    && c.TenantId == tenantId
                    && c.CustomerId == customerId);

        private Task<bool> TemplateKeyExistsAsync(Guid productId, string templateKey, Guid? exceptTemplateId = null)
            => _context.CollectionTemplates.AnyAsync(c => c.ProductId == productId
                && c.TemplateKey == templateKey
                && !c.Deleted
                && (!exceptTemplateId.HasValue || c.Id != exceptTemplateId.Value));

        private void RemoveTemplateChildren(CollectionTemplate template)
        {
            var points = template.Points ?? [];
            _context.CollectionTransformTemplates.RemoveRange(points.SelectMany(c => c.Transforms ?? []));
            _context.CollectionSamplingPolicies.RemoveRange(points.Where(c => c.SamplingPolicy != null).Select(c => c.SamplingPolicy));
            _context.CollectionMappingPolicies.RemoveRange(points.Where(c => c.Mapping != null).Select(c => c.Mapping));
            _context.CollectionPointTemplates.RemoveRange(points);
            _context.CollectionConnectionTemplates.RemoveRange(template.Connections ?? []);
            if (template.Protocol != null)
            {
                _context.CollectionProtocolTemplates.Remove(template.Protocol);
            }
        }

        private static string ResolveUserName(UserProfile profile)
            => string.IsNullOrWhiteSpace(profile.Name) ? profile.Email : profile.Name;

        private static string FirstDiagnosticMessage(CollectionTemplateValidationResultDto validation)
            => validation.Diagnostics.FirstOrDefault(c => c.Severity == CollectionTemplateDiagnosticSeverity.Error)?.Message
               ?? validation.Diagnostics.FirstOrDefault()?.Message
               ?? "Collection template is invalid";
    }
}
