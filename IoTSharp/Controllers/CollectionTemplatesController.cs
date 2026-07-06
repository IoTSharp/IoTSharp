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
using System.Linq;
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
