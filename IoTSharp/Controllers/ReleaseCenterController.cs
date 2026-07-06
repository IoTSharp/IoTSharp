using IoTSharp.Contracts;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
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

namespace IoTSharp.Controllers
{
    /// <summary>
    /// Release Center 第一版控制面，负责发布计划、灰度批次、暂停继续、回滚和发布回执查询。
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ReleaseCenterController : ControllerBase
    {
        private const string PackageIdKey = "packageId";
        private const string PackageVersionKey = "packageVersion";
        private const string PackageSha256Key = "sha256";
        private const string TargetDeviceIdKey = "targetDeviceId";
        private const string DeviceIdKey = "deviceId";
        private const string DeliveryChannelDeviceIdKey = "deliveryChannelDeviceId";
        private const string DeviceIdsMetadataKey = "deviceIds";
        private const string DefaultDeviceRuntimeType = "iotembedded";
        private const string AuditActionCreate = "ReleasePlanCreate";
        private const string AuditActionStart = "ReleasePlanStart";
        private const string AuditActionConfirm = "ReleasePlanConfirm";
        private const string AuditActionPause = "ReleasePlanPause";
        private const string AuditActionResume = "ReleasePlanResume";
        private const string AuditActionRollback = "ReleasePlanRollback";
        private static readonly TimeSpan DefaultSoftwareUpdateTtl = TimeSpan.FromHours(2);
        private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReleaseCenterController> _logger;

        public ReleaseCenterController(ApplicationDbContext context, ILogger<ReleaseCenterController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 查询发布计划列表。
        /// </summary>
        /// <param name="query">分页和筛选条件。</param>
        /// <returns>发布计划分页结果。</returns>
        [HttpGet("Plans")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ApiResult<PagedData<ReleasePlanDto>>> ListPlans([FromQuery] ReleasePlanQueryDto query)
        {
            var profile = this.GetUserProfile();
            query ??= new ReleasePlanQueryDto();
            query.Limit = Math.Clamp(query.Limit < 1 ? 10 : query.Limit, 1, 100);

            var plans = _context.ReleasePlans
                .Where(c => !c.Deleted && c.TenantId == profile.Tenant && c.CustomerId == profile.Customer);

            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                plans = plans.Where(c => c.Name.Contains(query.Name));
            }

            if (query.PlanType.HasValue)
            {
                plans = plans.Where(c => c.PlanType == query.PlanType.Value);
            }

            if (query.Status.HasValue)
            {
                plans = plans.Where(c => c.Status == query.Status.Value);
            }

            var total = await plans.LongCountAsync();
            var rows = await plans
                .OrderByDescending(c => c.UpdatedAt)
                .Skip(query.Offset * query.Limit)
                .Take(query.Limit)
                .ToListAsync();

            return new ApiResult<PagedData<ReleasePlanDto>>(ApiCode.Success, "OK", new PagedData<ReleasePlanDto>
            {
                total = total,
                rows = rows.Select(plan => ToReleasePlanDto(plan, null)).ToList()
            });
        }

        /// <summary>
        /// 获取发布计划详情。
        /// </summary>
        /// <param name="id">发布计划 ID。</param>
        /// <returns>发布计划和任务状态。</returns>
        [HttpGet("Plans/{id:guid}")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ApiResult<ReleasePlanDto>> GetPlan(Guid id)
        {
            var profile = this.GetUserProfile();
            var plan = await FindPlanAsync(id, profile);
            if (plan == null)
            {
                return new ApiResult<ReleasePlanDto>(ApiCode.CantFindObject, "Release plan not found", null);
            }

            var tasks = await LoadPlanTasksAsync(plan.Id);
            return new ApiResult<ReleasePlanDto>(ApiCode.Success, "OK", ToReleasePlanDto(plan, tasks));
        }

        /// <summary>
        /// 创建发布计划，并按确认策略决定是否立即下发首批任务。
        /// </summary>
        /// <param name="request">创建发布计划请求。</param>
        /// <returns>发布计划状态和本次创建的 EdgeTask。</returns>
        [HttpPost("Plans")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ActionResult<ApiResult<ReleasePlanOperationResultDto>>> CreatePlan([FromBody] ReleasePlanCreateRequestDto request)
        {
            if (request == null)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, "Release plan payload is required", null));
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, "name is required", null));
            }

            if (request.PackageId == Guid.Empty)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, "packageId is required", null));
            }

            if (request.Targets == null || request.Targets.Count == 0)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, "targets are required", null));
            }

            var profile = this.GetUserProfile();
            var package = await FindPackageAsync(request.PackageId, profile);
            if (package == null)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.CantFindObject, "Release package not found", null));
            }

            if (!IsSupportedPlanPackage(request.PlanType, package.PackageType))
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(
                    ApiCode.InValidData,
                    $"Release plan type {request.PlanType} does not support package type {package.PackageType} in M5 first version",
                    null));
            }

            ReleasePackage rollbackPackage = null;
            if (request.RollbackPackageId is { } rollbackPackageId && rollbackPackageId != Guid.Empty)
            {
                rollbackPackage = await FindPackageAsync(rollbackPackageId, profile);
                if (rollbackPackage == null)
                {
                    return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.CantFindObject, "Rollback package not found", null));
                }
            }

            var resolvedTargets = new List<ResolvedReleaseTarget>();
            var resolvedTargetKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var target in request.Targets)
            {
                var resolved = await ResolveTargetsAsync(target, profile, package, request.PlanType);
                if (resolved.Error != null)
                {
                    return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, resolved.Error, null));
                }

                foreach (var resolvedTarget in resolved.Targets)
                {
                    if (resolvedTargetKeys.Add(resolvedTarget.TargetKey))
                    {
                        resolvedTargets.Add(resolvedTarget);
                    }
                }
            }

            if (resolvedTargets.Count == 0)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, "No release targets resolved", null));
            }

            var now = DateTime.UtcNow;
            var operatorName = ResolveUserName(profile);
            var batchSize = request.Strategy?.BatchSize ?? 0;
            var plan = new ReleasePlan
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                PlanType = request.PlanType,
                Status = request.ConfirmationPolicy == ReleaseConfirmationPolicy.ManualBeforeStart
                    ? ReleasePlanStatus.WaitingConfirmation
                    : ReleasePlanStatus.Draft,
                PackageId = package.Id,
                Package = package,
                RollbackPackageId = rollbackPackage?.Id,
                RollbackPackage = rollbackPackage,
                ConfirmationPolicy = request.ConfirmationPolicy,
                BatchSize = batchSize,
                ContinueOnFailure = request.Strategy?.ContinueOnFailure ?? false,
                Metadata = SerializeStringMap(request.Metadata),
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = operatorName,
                UpdatedBy = operatorName,
                TenantId = profile.Tenant,
                CustomerId = profile.Customer
            };

            var releaseTasks = BuildReleaseTasks(plan, package, resolvedTargets, batchSize, now);
            _context.ReleasePlans.Add(plan);
            _context.ReleaseTasks.AddRange(releaseTasks);

            var createdEdgeTasks = new List<EdgeTaskRequestDto>();
            if (request.AutoStart && request.ConfirmationPolicy != ReleaseConfirmationPolicy.ManualBeforeStart)
            {
                createdEdgeTasks.AddRange(DispatchSelectableTasks(
                    plan,
                    package,
                    releaseTasks,
                    dispatchAll: request.ConfirmationPolicy == ReleaseConfirmationPolicy.None,
                    now,
                    operatorName,
                    isRollback: false));
            }

            ApplyPlanSummary(plan, releaseTasks, now, preservePaused: false);
            AddReleasePlanAudit(profile, plan, AuditActionCreate, new
            {
                packageId = package.Id,
                package.PackageKey,
                package.Version,
                targetCount = releaseTasks.Count,
                plan.ConfirmationPolicy,
                plan.BatchSize,
                plan.ContinueOnFailure,
                autoStart = request.AutoStart
            }, plan.Status.ToString(), now);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Created release plan {ReleasePlanId} for package {PackageId} with {TargetCount} targets",
                plan.Id,
                package.Id,
                releaseTasks.Count);

            return Ok(new ApiResult<ReleasePlanOperationResultDto>(
                ApiCode.Success,
                "OK",
                new ReleasePlanOperationResultDto
                {
                    Plan = ToReleasePlanDto(plan, releaseTasks),
                    EdgeTasks = createdEdgeTasks
                }));
        }

        /// <summary>
        /// 启动发布计划，按确认策略下发首批或全部任务。
        /// </summary>
        /// <param name="id">发布计划 ID。</param>
        /// <param name="request">操作原因和元数据。</param>
        /// <returns>发布计划状态和本次下发的 EdgeTask。</returns>
        [HttpPost("Plans/{id:guid}/Start")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ActionResult<ApiResult<ReleasePlanOperationResultDto>>> Start(Guid id, [FromBody] ReleasePlanActionRequestDto request)
            => await DispatchNextBatchActionAsync(id, request, AuditActionStart, allowPaused: false);

        /// <summary>
        /// 人工确认并继续下发下一批任务。
        /// </summary>
        /// <param name="id">发布计划 ID。</param>
        /// <param name="request">确认原因和元数据。</param>
        /// <returns>发布计划状态和本次下发的 EdgeTask。</returns>
        [HttpPost("Plans/{id:guid}/Confirm")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ActionResult<ApiResult<ReleasePlanOperationResultDto>>> Confirm(Guid id, [FromBody] ReleasePlanActionRequestDto request)
            => await DispatchNextBatchActionAsync(id, request, AuditActionConfirm, allowPaused: false);

        /// <summary>
        /// 暂停发布计划。已下发的 EdgeTask 不会被撤回，后续批次不会继续下发。
        /// </summary>
        /// <param name="id">发布计划 ID。</param>
        /// <param name="request">暂停原因和元数据。</param>
        /// <returns>发布计划状态。</returns>
        [HttpPost("Plans/{id:guid}/Pause")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ActionResult<ApiResult<ReleasePlanOperationResultDto>>> Pause(Guid id, [FromBody] ReleasePlanActionRequestDto request)
        {
            var profile = this.GetUserProfile();
            var plan = await FindPlanAsync(id, profile);
            if (plan == null)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.CantFindObject, "Release plan not found", null));
            }

            if (IsFinalPlanStatus(plan.Status))
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, $"Release plan is already {plan.Status}", null));
            }

            var now = DateTime.UtcNow;
            plan.Status = ReleasePlanStatus.Paused;
            plan.UpdatedAt = now;
            plan.UpdatedBy = ResolveUserName(profile);
            AddReleasePlanAudit(profile, plan, AuditActionPause, BuildActionAuditData(request), plan.Status.ToString(), now);
            await _context.SaveChangesAsync();

            return Ok(await BuildOperationResultAsync(plan, []));
        }

        /// <summary>
        /// 继续已暂停的发布计划，并尝试下发下一批任务。
        /// </summary>
        /// <param name="id">发布计划 ID。</param>
        /// <param name="request">继续原因和元数据。</param>
        /// <returns>发布计划状态和本次下发的 EdgeTask。</returns>
        [HttpPost("Plans/{id:guid}/Resume")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ActionResult<ApiResult<ReleasePlanOperationResultDto>>> Resume(Guid id, [FromBody] ReleasePlanActionRequestDto request)
            => await DispatchNextBatchActionAsync(id, request, AuditActionResume, allowPaused: true);

        /// <summary>
        /// 使用回滚包创建回滚任务并下发到已参与发布的目标。
        /// </summary>
        /// <param name="id">发布计划 ID。</param>
        /// <param name="request">回滚请求。</param>
        /// <returns>发布计划状态和本次下发的回滚 EdgeTask。</returns>
        [HttpPost("Plans/{id:guid}/Rollback")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ActionResult<ApiResult<ReleasePlanOperationResultDto>>> Rollback(Guid id, [FromBody] ReleasePlanActionRequestDto request)
        {
            var profile = this.GetUserProfile();
            var plan = await FindPlanAsync(id, profile);
            if (plan == null)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.CantFindObject, "Release plan not found", null));
            }

            var rollbackPackageId = request?.RollbackPackageId ?? plan.RollbackPackageId;
            if (!rollbackPackageId.HasValue || rollbackPackageId.Value == Guid.Empty)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, "rollbackPackageId is required", null));
            }

            var rollbackPackage = await FindPackageAsync(rollbackPackageId.Value, profile);
            if (rollbackPackage == null)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.CantFindObject, "Rollback package not found", null));
            }

            if (!IsSupportedPlanPackage(plan.PlanType, rollbackPackage.PackageType))
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, "Rollback package type does not match release plan type", null));
            }

            var originalTasks = await _context.ReleaseTasks
                .Where(task => task.PlanId == plan.Id
                    && !task.Deleted
                    && !task.IsRollback
                    && task.EdgeTaskId != null)
                .OrderBy(task => task.BatchNo)
                .ThenBy(task => task.CreatedAt)
                .ToListAsync();
            if (originalTasks.Count == 0)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, "No dispatched release tasks can be rolled back", null));
            }

            var now = DateTime.UtcNow;
            var operatorName = ResolveUserName(profile);
            var maxBatch = await _context.ReleaseTasks
                .Where(task => task.PlanId == plan.Id && !task.Deleted)
                .Select(task => (int?)task.BatchNo)
                .MaxAsync() ?? 0;

            var rollbackTasks = originalTasks.Select(task => new ReleaseTask
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                Plan = plan,
                PackageId = rollbackPackage.Id,
                Package = rollbackPackage,
                TargetType = task.TargetType,
                TargetId = task.TargetId,
                GatewayId = task.GatewayId,
                EdgeNodeId = task.EdgeNodeId,
                TargetKey = task.TargetKey,
                RuntimeType = Coalesce(task.RuntimeType, rollbackPackage.TargetRuntimeType),
                InstanceId = task.InstanceId ?? string.Empty,
                BatchNo = maxBatch + 1,
                Status = ReleaseTaskStatus.Pending,
                IsRollback = true,
                Metadata = MergeMetadata(task.Metadata, new Dictionary<string, string>
                {
                    ["rollbackOfReleaseTaskId"] = task.Id.ToString("D"),
                    ["rollbackOperator"] = operatorName,
                    ["rollbackReason"] = request?.Reason ?? string.Empty
                }),
                CreatedAt = now,
                UpdatedAt = now,
                TenantId = plan.TenantId,
                CustomerId = plan.CustomerId
            }).ToList();

            _context.ReleaseTasks.AddRange(rollbackTasks);
            plan.Status = ReleasePlanStatus.RollingBack;
            plan.RollbackPackageId = rollbackPackage.Id;
            plan.UpdatedAt = now;
            plan.UpdatedBy = operatorName;

            var edgeTasks = DispatchSelectableTasks(plan, rollbackPackage, rollbackTasks, dispatchAll: true, now, operatorName, isRollback: true);
            var allTasks = await LoadPlanTasksAsync(plan.Id);
            allTasks.AddRange(rollbackTasks);
            ApplyPlanSummary(plan, allTasks, now, preservePaused: false);
            plan.Status = ReleasePlanStatus.RollingBack;
            AddReleasePlanAudit(profile, plan, AuditActionRollback, new
            {
                rollbackPackageId = rollbackPackage.Id,
                rollbackPackage.PackageKey,
                rollbackPackage.Version,
                rollbackTaskCount = rollbackTasks.Count,
                reason = request?.Reason ?? string.Empty,
                metadata = request?.Metadata ?? []
            }, plan.Status.ToString(), now);

            await _context.SaveChangesAsync();
            return Ok(await BuildOperationResultAsync(plan, edgeTasks));
        }

        /// <summary>
        /// 查询发布计划下的回执历史。
        /// </summary>
        /// <param name="id">发布计划 ID。</param>
        /// <returns>发布回执列表。</returns>
        [HttpGet("Plans/{id:guid}/Receipts")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ApiResult<List<ReleaseReceiptDto>>> GetReceipts(Guid id)
        {
            var profile = this.GetUserProfile();
            var plan = await FindPlanAsync(id, profile);
            if (plan == null)
            {
                return new ApiResult<List<ReleaseReceiptDto>>(ApiCode.CantFindObject, "Release plan not found", null);
            }

            var receipts = await _context.ReleaseReceipts
                .Where(receipt => receipt.PlanId == plan.Id && !receipt.Deleted)
                .OrderByDescending(receipt => receipt.ReportedAt)
                .ThenByDescending(receipt => receipt.ReceivedAt)
                .Take(500)
                .ToListAsync();

            return new ApiResult<List<ReleaseReceiptDto>>(ApiCode.Success, "OK", receipts.Select(ToReleaseReceiptDto).ToList());
        }

        private async System.Threading.Tasks.Task<ActionResult<ApiResult<ReleasePlanOperationResultDto>>> DispatchNextBatchActionAsync(
            Guid id,
            ReleasePlanActionRequestDto request,
            string auditAction,
            bool allowPaused)
        {
            var profile = this.GetUserProfile();
            var plan = await FindPlanAsync(id, profile);
            if (plan == null)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.CantFindObject, "Release plan not found", null));
            }

            if (plan.Status == ReleasePlanStatus.Paused && !allowPaused)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, "Release plan is paused", null));
            }

            if (IsFinalPlanStatus(plan.Status))
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, $"Release plan is already {plan.Status}", null));
            }

            var package = await FindPackageAsync(plan.PackageId ?? Guid.Empty, profile);
            if (package == null)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.CantFindObject, "Release package not found", null));
            }

            var tasks = await LoadPlanTasksAsync(plan.Id);
            if (HasActiveTasks(tasks))
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, "Wait for active release tasks to finish before continuing", null));
            }

            if (HasFailedTasks(tasks) && !plan.ContinueOnFailure && request?.Force != true)
            {
                return Ok(new ApiResult<ReleasePlanOperationResultDto>(ApiCode.InValidData, "Release plan has failed tasks; set force=true to continue", null));
            }

            var pending = SelectPendingBatch(tasks, dispatchAll: plan.ConfirmationPolicy == ReleaseConfirmationPolicy.None);
            if (pending.Count == 0)
            {
                var now = DateTime.UtcNow;
                ApplyPlanSummary(plan, tasks, now, preservePaused: false);
                await _context.SaveChangesAsync();
                return Ok(await BuildOperationResultAsync(plan, []));
            }

            var nowDispatch = DateTime.UtcNow;
            var operatorName = ResolveUserName(profile);
            var edgeTasks = DispatchSelectableTasks(plan, package, pending, dispatchAll: true, nowDispatch, operatorName, isRollback: false);
            ApplyPlanSummary(plan, tasks, nowDispatch, preservePaused: false);
            AddReleasePlanAudit(profile, plan, auditAction, BuildActionAuditData(request), plan.Status.ToString(), nowDispatch);
            await _context.SaveChangesAsync();

            return Ok(await BuildOperationResultAsync(plan, edgeTasks));
        }

        private async System.Threading.Tasks.Task<ReleasePlan> FindPlanAsync(Guid id, UserProfile profile)
            => await _context.ReleasePlans
                .FirstOrDefaultAsync(c => c.Id == id
                    && !c.Deleted
                    && c.TenantId == profile.Tenant
                    && c.CustomerId == profile.Customer);

        private async System.Threading.Tasks.Task<ReleasePackage> FindPackageAsync(Guid id, UserProfile profile)
            => await _context.ReleasePackages
                .FirstOrDefaultAsync(c => c.Id == id
                    && !c.Deleted
                    && c.TenantId == profile.Tenant
                    && c.CustomerId == profile.Customer);

        private async System.Threading.Tasks.Task<List<ReleaseTask>> LoadPlanTasksAsync(Guid planId)
            => await _context.ReleaseTasks
                .Where(task => task.PlanId == planId && !task.Deleted)
                .OrderBy(task => task.BatchNo)
                .ThenBy(task => task.CreatedAt)
                .ToListAsync();

        private async System.Threading.Tasks.Task<ResolvedReleaseTargets> ResolveTargetsAsync(
            ReleaseTargetDto target,
            UserProfile profile,
            ReleasePackage package,
            ReleasePlanType planType)
        {
            if (target == null)
            {
                return ResolvedReleaseTargets.Fail("target is required");
            }

            if (IsDeviceOtaPlan(planType))
            {
                return target.TargetType switch
                {
                    ReleaseTargetType.Device => await ResolveDeviceReleaseTargetAsync(target, profile, package),
                    ReleaseTargetType.AssetScope => await ResolveAssetScopeReleaseTargetsAsync(target, profile, package),
                    ReleaseTargetType.DeviceScope => await ResolveDeviceScopeReleaseTargetsAsync(target, profile, package),
                    _ => ResolvedReleaseTargets.Fail($"{target.TargetType} is not a valid target for {planType}; use Device, AssetScope or DeviceScope")
                };
            }

            if (target.TargetId == Guid.Empty)
            {
                return ResolvedReleaseTargets.Fail("targetId is required");
            }

            if (target.TargetType is ReleaseTargetType.Device or ReleaseTargetType.AssetScope or ReleaseTargetType.DeviceScope)
            {
                return ResolvedReleaseTargets.Fail($"{target.TargetType} release target requires DeviceScriptOta or FirmwareOta plan type");
            }

            if (target.TargetType == ReleaseTargetType.EdgeNode)
            {
                var node = await _context.EdgeNodes
                    .Include(c => c.Gateway)
                        .ThenInclude(c => c.Tenant)
                    .Include(c => c.Gateway)
                        .ThenInclude(c => c.Customer)
                    .FirstOrDefaultAsync(c => c.Id == target.TargetId
                        && !c.Deleted
                        && c.TenantId == profile.Tenant
                        && c.CustomerId == profile.Customer);

                if (node == null)
                {
                    return ResolvedReleaseTargets.Fail("EdgeNode target not found");
                }

                var runtimeType = Coalesce(target.RuntimeType, Coalesce(node.RuntimeType, package.TargetRuntimeType));
                var instanceId = Coalesce(target.InstanceId, node.InstanceId ?? string.Empty);
                var targetKey = Coalesce(target.TargetKey, BuildEdgeTargetKey(node.GatewayId, runtimeType, instanceId));
                return ResolvedReleaseTargets.Ok([
                    ResolvedReleaseTarget.Ok(
                        target.TargetType,
                        target.TargetId,
                        node.Gateway,
                        node,
                        runtimeType,
                        instanceId,
                        targetKey,
                        BuildTargetMetadata(target, target.TargetType, target.TargetId, null, node.GatewayId))
                ]);
            }

            var gateway = await GatewayInScope(target.TargetId, profile.Tenant, profile.Customer)
                .Include(c => c.Tenant)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync();
            if (gateway == null)
            {
                return ResolvedReleaseTargets.Fail("Gateway target not found");
            }

            var edgeNode = await EnsureEdgeNodeAsync(gateway);
            var gatewayRuntimeType = Coalesce(target.RuntimeType, Coalesce(package.TargetRuntimeType, EdgeRuntimeTypes.Gateway));
            var gatewayInstanceId = Coalesce(target.InstanceId, edgeNode?.InstanceId ?? string.Empty);
            var gatewayTargetKey = Coalesce(target.TargetKey, BuildEdgeTargetKey(gateway.Id, gatewayRuntimeType, gatewayInstanceId));
            return ResolvedReleaseTargets.Ok([
                ResolvedReleaseTarget.Ok(
                    target.TargetType,
                    target.TargetId,
                    gateway,
                    edgeNode,
                    gatewayRuntimeType,
                    gatewayInstanceId,
                    gatewayTargetKey,
                    BuildTargetMetadata(target, target.TargetType, target.TargetId, null, gateway.Id))
            ]);
        }

        private async System.Threading.Tasks.Task<ResolvedReleaseTargets> ResolveDeviceReleaseTargetAsync(
            ReleaseTargetDto target,
            UserProfile profile,
            ReleasePackage package)
        {
            if (target.TargetId == Guid.Empty)
            {
                return ResolvedReleaseTargets.Fail("targetId is required for Device release target");
            }

            var resolved = await ResolveSingleDeviceReleaseTargetAsync(target.TargetId, target, profile, package, ReleaseTargetType.Device, target.TargetId);
            return resolved.Error == null
                ? ResolvedReleaseTargets.Ok([resolved])
                : ResolvedReleaseTargets.Fail(resolved.Error);
        }

        private async System.Threading.Tasks.Task<ResolvedReleaseTargets> ResolveAssetScopeReleaseTargetsAsync(
            ReleaseTargetDto target,
            UserProfile profile,
            ReleasePackage package)
        {
            if (target.TargetId == Guid.Empty)
            {
                return ResolvedReleaseTargets.Fail("targetId is required for AssetScope release target");
            }

            var asset = await _context.Assets
                .Include(c => c.OwnedAssets)
                .Include(c => c.Tenant)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == target.TargetId
                    && !c.Deleted
                    && c.Tenant.Id == profile.Tenant
                    && c.Customer.Id == profile.Customer);
            if (asset == null)
            {
                return ResolvedReleaseTargets.Fail("AssetScope target not found");
            }

            var deviceIds = asset.OwnedAssets?
                .Select(relation => relation.DeviceId)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList() ?? [];
            if (deviceIds.Count == 0)
            {
                return ResolvedReleaseTargets.Fail("AssetScope target has no linked devices");
            }

            return await ResolveDeviceIdsAsync(deviceIds, target, profile, package, ReleaseTargetType.AssetScope, asset.Id);
        }

        private async System.Threading.Tasks.Task<ResolvedReleaseTargets> ResolveDeviceScopeReleaseTargetsAsync(
            ReleaseTargetDto target,
            UserProfile profile,
            ReleasePackage package)
        {
            var deviceIds = ParseDeviceIds(target.Metadata);
            if (deviceIds.Count == 0)
            {
                return ResolvedReleaseTargets.Fail("DeviceScope target requires metadata.deviceIds with one or more device IDs");
            }

            return await ResolveDeviceIdsAsync(deviceIds, target, profile, package, ReleaseTargetType.DeviceScope, target.TargetId);
        }

        private async System.Threading.Tasks.Task<ResolvedReleaseTargets> ResolveDeviceIdsAsync(
            IReadOnlyList<Guid> deviceIds,
            ReleaseTargetDto target,
            UserProfile profile,
            ReleasePackage package,
            ReleaseTargetType sourceTargetType,
            Guid sourceTargetId)
        {
            var results = new List<ResolvedReleaseTarget>();
            foreach (var deviceId in deviceIds.Distinct())
            {
                var resolved = await ResolveSingleDeviceReleaseTargetAsync(deviceId, target, profile, package, sourceTargetType, sourceTargetId);
                if (resolved.Error != null)
                {
                    return ResolvedReleaseTargets.Fail(resolved.Error);
                }

                results.Add(resolved);
            }

            return results.Count == 0
                ? ResolvedReleaseTargets.Fail("No device targets resolved")
                : ResolvedReleaseTargets.Ok(results);
        }

        private async System.Threading.Tasks.Task<ResolvedReleaseTarget> ResolveSingleDeviceReleaseTargetAsync(
            Guid deviceId,
            ReleaseTargetDto sourceTarget,
            UserProfile profile,
            ReleasePackage package,
            ReleaseTargetType sourceTargetType,
            Guid sourceTargetId)
        {
            var device = await DeviceInScope(deviceId, profile.Tenant, profile.Customer)
                .Include(c => c.Owner)
                .Include(c => c.Tenant)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync();
            if (device == null)
            {
                return ResolvedReleaseTarget.Fail($"Device target not found: {deviceId:D}");
            }

            if (device.DeviceType == DeviceType.Gateway)
            {
                return ResolvedReleaseTarget.Fail("Gateway runtime updates must use SoftwareUpdate with Gateway or EdgeNode target");
            }

            Device channel = device.Owner;
            if (channel is { Deleted: true })
            {
                return ResolvedReleaseTarget.Fail($"Device target channel is deleted: {deviceId:D}");
            }

            if (channel == null)
            {
                channel = device;
            }
            else if (channel.TenantId != profile.Tenant || channel.CustomerId != profile.Customer)
            {
                return ResolvedReleaseTarget.Fail($"Device target channel is outside current scope: {deviceId:D}");
            }

            var edgeNode = channel.DeviceType == DeviceType.Gateway
                ? await EnsureEdgeNodeAsync(channel)
                : null;
            var runtimeType = Coalesce(sourceTarget.RuntimeType, Coalesce(package.TargetRuntimeType, DefaultDeviceRuntimeType));
            var instanceId = Coalesce(sourceTarget.InstanceId, device.Name ?? string.Empty);
            var targetKey = sourceTargetType == ReleaseTargetType.Device
                ? Coalesce(sourceTarget.TargetKey, BuildDeviceTargetKey(channel.Id, device.Id, runtimeType, instanceId))
                : BuildDeviceTargetKey(channel.Id, device.Id, runtimeType, instanceId);
            var metadata = BuildTargetMetadata(sourceTarget, sourceTargetType, sourceTargetId, device.Id, channel.Id);

            return ResolvedReleaseTarget.Ok(
                ReleaseTargetType.Device,
                device.Id,
                channel,
                edgeNode,
                runtimeType,
                instanceId,
                targetKey,
                metadata);
        }

        private IQueryable<Device> GatewayInScope(Guid gatewayId, Guid tenantId, Guid customerId)
            => _context.Device
                .Where(c => c.Id == gatewayId
                    && !c.Deleted
                    && c.DeviceType == DeviceType.Gateway
                    && c.Tenant.Id == tenantId
                    && c.Customer.Id == customerId);

        private IQueryable<Device> DeviceInScope(Guid deviceId, Guid tenantId, Guid customerId)
            => _context.Device
                .Where(c => c.Id == deviceId
                    && !c.Deleted
                    && c.Tenant.Id == tenantId
                    && c.Customer.Id == customerId);

        /// <summary>
        /// 获取或创建与 Gateway 对应的 EdgeNode，保证 ReleaseTask 能明确归属运行时生命周期。
        /// </summary>
        /// <param name="gateway">Gateway 设备。</param>
        /// <returns>已跟踪的 EdgeNode。</returns>
        private async System.Threading.Tasks.Task<EdgeNode> EnsureEdgeNodeAsync(Device gateway)
        {
            var local = _context.EdgeNodes.Local.FirstOrDefault(c => c.GatewayId == gateway.Id && !c.Deleted);
            if (local != null)
            {
                return local;
            }

            var node = await _context.EdgeNodes
                .FirstOrDefaultAsync(c => c.GatewayId == gateway.Id && !c.Deleted);
            if (node != null)
            {
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
            return node;
        }

        private static List<ReleaseTask> BuildReleaseTasks(
            ReleasePlan plan,
            ReleasePackage package,
            IReadOnlyList<ResolvedReleaseTarget> targets,
            int batchSize,
            DateTime now)
        {
            var normalizedBatchSize = batchSize <= 0 ? targets.Count : batchSize;
            var tasks = new List<ReleaseTask>();
            for (var index = 0; index < targets.Count; index++)
            {
                var target = targets[index];
                tasks.Add(new ReleaseTask
                {
                    Id = Guid.NewGuid(),
                    PlanId = plan.Id,
                    Plan = plan,
                    PackageId = package.Id,
                    Package = package,
                    TargetType = target.TargetType,
                    TargetId = target.TargetId,
                    GatewayId = target.Gateway.Id,
                    EdgeNodeId = target.EdgeNode?.Id,
                    TargetKey = target.TargetKey,
                    RuntimeType = target.RuntimeType,
                    InstanceId = target.InstanceId,
                    BatchNo = index / normalizedBatchSize + 1,
                    Status = ReleaseTaskStatus.Pending,
                    Metadata = SerializeStringMap(target.Metadata),
                    CreatedAt = now,
                    UpdatedAt = now,
                    TenantId = plan.TenantId,
                    CustomerId = plan.CustomerId
                });
            }

            return tasks;
        }

        private List<EdgeTaskRequestDto> DispatchSelectableTasks(
            ReleasePlan plan,
            ReleasePackage package,
            IReadOnlyList<ReleaseTask> tasks,
            bool dispatchAll,
            DateTime now,
            string operatorName,
            bool isRollback)
        {
            var selected = SelectPendingBatch(tasks, dispatchAll);
            var edgeTasks = new List<EdgeTaskRequestDto>();
            foreach (var releaseTask in selected)
            {
                if (releaseTask.EdgeTaskId.HasValue)
                {
                    continue;
                }

                var taskRequest = CreateReleaseTaskRequest(plan, releaseTask, package, now, operatorName, isRollback);
                var formalTask = CreateFormalEdgeTask(taskRequest, releaseTask, SerializeOrNull(taskRequest) ?? "{}");
                _context.EdgeTasks.Add(formalTask);

                releaseTask.EdgeTaskId = formalTask.Id;
                releaseTask.DispatchedAt = now;
                releaseTask.UpdatedAt = now;
                releaseTask.Message = isRollback ? "回滚任务已创建，等待执行端拉取" : "发布任务已创建，等待执行端拉取";
                releaseTask.Metadata = MergeMetadata(releaseTask.Metadata, new Dictionary<string, string>
                {
                    ["edgeTaskId"] = formalTask.Id.ToString("D"),
                    ["releasePlanId"] = plan.Id.ToString("D"),
                    ["releaseTaskId"] = releaseTask.Id.ToString("D")
                });
                edgeTasks.Add(taskRequest);
            }

            plan.StartedAt ??= now;
            plan.CurrentBatchNo = selected.Count == 0 ? plan.CurrentBatchNo : selected.Max(task => task.BatchNo);
            plan.Status = isRollback ? ReleasePlanStatus.RollingBack : ReleasePlanStatus.Running;
            plan.UpdatedAt = now;
            plan.UpdatedBy = operatorName;
            return edgeTasks;
        }

        private EdgeTaskRequestDto CreateReleaseTaskRequest(
            ReleasePlan plan,
            ReleaseTask releaseTask,
            ReleasePackage package,
            DateTime now,
            string operatorName,
            bool isRollback)
        {
            var runtimeType = Coalesce(releaseTask.RuntimeType, package.TargetRuntimeType);
            var instanceId = releaseTask.InstanceId ?? string.Empty;
            var targetType = ResolveDefaultTargetType(releaseTask.TargetType, runtimeType);
            var edgeTaskType = ResolveEdgeTaskType(plan.PlanType);
            var downloadUrl = Url.Action(nameof(ReleasePackagesController.Download), "ReleasePackages", new
            {
                id = package.Id,
                token = package.DownloadToken,
                sha256 = package.Sha256
            }, Request.Scheme) ?? $"/api/ReleasePackages/{package.Id:D}/Download";
            var metadata = DeserializeStringMap(releaseTask.Metadata);
            metadata["source"] = "release-center";
            metadata["operator"] = operatorName ?? string.Empty;
            metadata["releasePlanId"] = plan.Id.ToString("D");
            metadata["releaseTaskId"] = releaseTask.Id.ToString("D");
            metadata["releaseBatchNo"] = releaseTask.BatchNo.ToString();
            metadata["isRollback"] = isRollback ? bool.TrueString : bool.FalseString;
            metadata["packageId"] = package.Id.ToString("D");
            metadata["packageKey"] = package.PackageKey ?? string.Empty;
            metadata["packageVersion"] = package.Version ?? string.Empty;

            var parameters = new Dictionary<string, object>
            {
                ["releasePackageContractVersion"] = EdgeNodeContractVersions.ReleasePackageV1,
                [PackageIdKey] = package.Id,
                ["packageType"] = package.PackageType.ToString(),
                ["packageKey"] = package.PackageKey ?? string.Empty,
                ["packageName"] = package.Name ?? string.Empty,
                [PackageVersionKey] = package.Version ?? string.Empty,
                ["targetRuntimeType"] = package.TargetRuntimeType ?? string.Empty,
                ["targetRuntimeVersion"] = package.TargetRuntimeVersion ?? string.Empty,
                ["fileName"] = package.FileName ?? string.Empty,
                ["contentType"] = package.ContentType ?? string.Empty,
                ["size"] = package.Size,
                [PackageSha256Key] = package.Sha256 ?? string.Empty,
                ["downloadUrl"] = downloadUrl,
                ["downloadToken"] = package.DownloadToken ?? string.Empty,
                ["releasePlanId"] = plan.Id,
                ["releaseTaskId"] = releaseTask.Id,
                ["releaseBatchNo"] = releaseTask.BatchNo,
                ["isRollback"] = isRollback,
                ["edgeNodeId"] = releaseTask.EdgeNodeId,
                ["gatewayId"] = releaseTask.GatewayId
            };
            if (releaseTask.TargetType == ReleaseTargetType.Device && releaseTask.TargetId.HasValue)
            {
                parameters[DeviceIdKey] = releaseTask.TargetId.Value;
                parameters[TargetDeviceIdKey] = releaseTask.TargetId.Value;
                parameters[DeliveryChannelDeviceIdKey] = releaseTask.GatewayId;
            }

            AddPackageMetadataParameters(parameters, package, edgeTaskType);

            return new EdgeTaskRequestDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = Guid.NewGuid(),
                TaskType = edgeTaskType,
                CreatedAt = now,
                ExpireAt = now.Add(DefaultSoftwareUpdateTtl),
                Address = new EdgeTaskAddressDto
                {
                    TargetType = targetType,
                    DeviceId = releaseTask.GatewayId,
                    RuntimeType = runtimeType,
                    InstanceId = instanceId,
                    TargetKey = releaseTask.TargetKey
                },
                Parameters = parameters,
                Metadata = metadata
            };
        }

        private static EdgeTask CreateFormalEdgeTask(EdgeTaskRequestDto request, ReleaseTask releaseTask, string requestPayload)
        {
            return new EdgeTask
            {
                Id = request.TaskId,
                ContractVersion = request.ContractVersion,
                TaskType = request.TaskType,
                TargetType = request.Address.TargetType,
                GatewayId = releaseTask.GatewayId.GetValueOrDefault(),
                EdgeNodeId = releaseTask.EdgeNodeId,
                TargetKey = request.Address.TargetKey ?? releaseTask.TargetKey,
                RuntimeType = request.Address.RuntimeType ?? string.Empty,
                InstanceId = request.Address.InstanceId ?? string.Empty,
                Status = EdgeTaskStatus.Pending,
                Parameters = SerializeOrNull(request.Parameters) ?? "{}",
                Metadata = SerializeOrNull(request.Metadata) ?? "{}",
                RequestPayload = requestPayload,
                CreatedAt = request.CreatedAt.ToUniversalTime(),
                ExpireAt = request.ExpireAt?.ToUniversalTime(),
                UpdatedAt = DateTime.UtcNow,
                TenantId = releaseTask.TenantId,
                CustomerId = releaseTask.CustomerId
            };
        }

        private static List<ReleaseTask> SelectPendingBatch(IReadOnlyList<ReleaseTask> tasks, bool dispatchAll)
        {
            var pending = tasks
                .Where(task => task.Status == ReleaseTaskStatus.Pending && task.EdgeTaskId == null)
                .OrderBy(task => task.BatchNo)
                .ThenBy(task => task.CreatedAt)
                .ToList();
            if (pending.Count == 0 || dispatchAll)
            {
                return pending;
            }

            var batchNo = pending[0].BatchNo;
            return pending.Where(task => task.BatchNo == batchNo).ToList();
        }

        private static void ApplyPlanSummary(ReleasePlan plan, IReadOnlyList<ReleaseTask> tasks, DateTime now, bool preservePaused)
        {
            plan.TotalTaskCount = tasks.Count;
            plan.PendingTaskCount = tasks.Count(task => task.Status == ReleaseTaskStatus.Pending);
            plan.RunningTaskCount = tasks.Count(IsActiveTaskStatus);
            plan.SucceededTaskCount = tasks.Count(task => task.Status is ReleaseTaskStatus.Succeeded or ReleaseTaskStatus.RolledBack);
            plan.FailedTaskCount = tasks.Count(IsFailedTaskStatus);
            plan.CurrentBatchNo = tasks.Count == 0 ? plan.CurrentBatchNo : Math.Max(plan.CurrentBatchNo, tasks.Max(task => task.BatchNo));
            plan.UpdatedAt = now;

            if (preservePaused && plan.Status == ReleasePlanStatus.Paused)
            {
                return;
            }

            var rollbackTasks = tasks.Where(task => task.IsRollback).ToList();
            if (plan.Status == ReleasePlanStatus.RollingBack || rollbackTasks.Count > 0)
            {
                if (rollbackTasks.Count > 0 && rollbackTasks.All(IsTerminalTask))
                {
                    plan.Status = rollbackTasks.Any(IsFailedTaskStatus)
                        ? ReleasePlanStatus.RollbackFailed
                        : ReleasePlanStatus.RolledBack;
                    plan.CompletedAt = now;
                }
                else
                {
                    plan.Status = ReleasePlanStatus.RollingBack;
                }

                return;
            }

            if (tasks.Count == 0)
            {
                plan.Status = ReleasePlanStatus.Draft;
                return;
            }

            var activeOrDispatched = tasks.Any(task => IsActiveTaskStatus(task) || task is { Status: ReleaseTaskStatus.Pending, EdgeTaskId: not null });
            var pendingWithoutEdgeTask = tasks.Any(task => task.Status == ReleaseTaskStatus.Pending && task.EdgeTaskId == null);
            var failed = tasks.Any(IsFailedTaskStatus);
            var succeeded = tasks.Any(task => task.Status == ReleaseTaskStatus.Succeeded);

            if (tasks.All(task => task.Status == ReleaseTaskStatus.Succeeded))
            {
                plan.Status = ReleasePlanStatus.Succeeded;
                plan.CompletedAt = now;
            }
            else if (activeOrDispatched)
            {
                plan.Status = ReleasePlanStatus.Running;
            }
            else if (failed && !pendingWithoutEdgeTask)
            {
                plan.Status = succeeded ? ReleasePlanStatus.PartiallySucceeded : ReleasePlanStatus.Failed;
                plan.CompletedAt = now;
            }
            else if (pendingWithoutEdgeTask)
            {
                plan.Status = plan.StartedAt == null ? ReleasePlanStatus.Draft : ReleasePlanStatus.WaitingConfirmation;
            }
        }

        private async System.Threading.Tasks.Task<ApiResult<ReleasePlanOperationResultDto>> BuildOperationResultAsync(
            ReleasePlan plan,
            List<EdgeTaskRequestDto> edgeTasks)
        {
            var tasks = await LoadPlanTasksAsync(plan.Id);
            return new ApiResult<ReleasePlanOperationResultDto>(
                ApiCode.Success,
                "OK",
                new ReleasePlanOperationResultDto
                {
                    Plan = ToReleasePlanDto(plan, tasks),
                    EdgeTasks = edgeTasks
                });
        }

        private static ReleasePlanDto ToReleasePlanDto(ReleasePlan plan, IReadOnlyList<ReleaseTask> tasks)
        {
            return new ReleasePlanDto
            {
                Id = plan.Id,
                Name = plan.Name ?? string.Empty,
                Description = plan.Description ?? string.Empty,
                PlanType = plan.PlanType,
                Status = plan.Status,
                PackageId = plan.PackageId,
                RollbackPackageId = plan.RollbackPackageId,
                ConfirmationPolicy = plan.ConfirmationPolicy,
                BatchSize = plan.BatchSize,
                ContinueOnFailure = plan.ContinueOnFailure,
                TotalTaskCount = plan.TotalTaskCount,
                PendingTaskCount = plan.PendingTaskCount,
                RunningTaskCount = plan.RunningTaskCount,
                SucceededTaskCount = plan.SucceededTaskCount,
                FailedTaskCount = plan.FailedTaskCount,
                CurrentBatchNo = plan.CurrentBatchNo,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                StartedAt = plan.StartedAt,
                CompletedAt = plan.CompletedAt,
                CreatedBy = plan.CreatedBy ?? string.Empty,
                UpdatedBy = plan.UpdatedBy ?? string.Empty,
                Metadata = DeserializeStringMap(plan.Metadata),
                Tasks = tasks?.Select(ToReleaseTaskDto).ToList() ?? []
            };
        }

        private static ReleaseTaskDto ToReleaseTaskDto(ReleaseTask task)
        {
            return new ReleaseTaskDto
            {
                Id = task.Id,
                PlanId = task.PlanId,
                PackageId = task.PackageId,
                TargetType = task.TargetType,
                TargetId = task.TargetId,
                GatewayId = task.GatewayId,
                EdgeNodeId = task.EdgeNodeId,
                TargetKey = task.TargetKey ?? string.Empty,
                RuntimeType = task.RuntimeType ?? string.Empty,
                InstanceId = task.InstanceId ?? string.Empty,
                BatchNo = task.BatchNo,
                Status = task.Status,
                IsRollback = task.IsRollback,
                EdgeTaskId = task.EdgeTaskId,
                Message = task.Message ?? string.Empty,
                Progress = task.Progress,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                DispatchedAt = task.DispatchedAt,
                CompletedAt = task.CompletedAt,
                LastReceiptAt = task.LastReceiptAt,
                Metadata = DeserializeStringMap(task.Metadata)
            };
        }

        private static ReleaseReceiptDto ToReleaseReceiptDto(ReleaseReceipt receipt)
        {
            return new ReleaseReceiptDto
            {
                Id = receipt.Id,
                PlanId = receipt.PlanId,
                ReleaseTaskId = receipt.ReleaseTaskId,
                EdgeTaskId = receipt.EdgeTaskId,
                Status = receipt.Status,
                Message = receipt.Message ?? string.Empty,
                Progress = receipt.Progress,
                Result = DeserializeObjectMap(receipt.Result),
                Metadata = DeserializeStringMap(receipt.Metadata),
                ReportedAt = receipt.ReportedAt,
                ReceivedAt = receipt.ReceivedAt
            };
        }

        private void AddReleasePlanAudit(
            UserProfile profile,
            ReleasePlan plan,
            string actionName,
            object actionData,
            string actionResult,
            DateTime activeAt)
        {
            _context.AuditLog.Add(new AuditLog
            {
                TenantId = plan.TenantId,
                CustomerId = plan.CustomerId,
                UserId = profile.Id.ToString("D"),
                UserName = ResolveUserName(profile),
                ObjectID = plan.Id,
                ObjectName = plan.Name ?? plan.Id.ToString("D"),
                ObjectType = ObjectType.ReleasePlan,
                ActionName = actionName,
                ActionData = SerializeOrNull(actionData) ?? "{}",
                ActionResult = actionResult ?? string.Empty,
                ActiveDateTime = activeAt == default ? DateTime.UtcNow : activeAt.ToUniversalTime()
            });
        }

        private static object BuildActionAuditData(ReleasePlanActionRequestDto request)
        {
            request ??= new ReleasePlanActionRequestDto();
            return new
            {
                reason = request.Reason ?? string.Empty,
                request.RollbackPackageId,
                request.Force,
                metadata = request.Metadata ?? []
            };
        }

        private static bool IsSupportedPlanPackage(ReleasePlanType planType, ReleasePackageType packageType)
            => planType switch
            {
                ReleasePlanType.SoftwareUpdate => IsRuntimeSoftwarePackage(packageType),
                ReleasePlanType.DeviceScriptOta => packageType == ReleasePackageType.DeviceScript,
                ReleasePlanType.FirmwareOta => packageType == ReleasePackageType.Firmware,
                _ => false
            };

        private static bool IsRuntimeSoftwarePackage(ReleasePackageType packageType)
            => packageType is ReleasePackageType.Software or ReleasePackageType.CollectorSoftware;

        private static bool IsDeviceOtaPlan(ReleasePlanType planType)
            => planType is ReleasePlanType.DeviceScriptOta or ReleasePlanType.FirmwareOta;

        private static EdgeTaskType ResolveEdgeTaskType(ReleasePlanType planType)
            => planType switch
            {
                ReleasePlanType.DeviceScriptOta => EdgeTaskType.DeviceScriptOta,
                ReleasePlanType.FirmwareOta => EdgeTaskType.FirmwareOta,
                _ => EdgeTaskType.SoftwareUpdate
            };

        private static EdgeTaskTargetType ResolveDefaultTargetType(ReleaseTargetType targetType, string runtimeType)
        {
            if (targetType == ReleaseTargetType.Device)
            {
                return EdgeTaskTargetType.Device;
            }

            if (targetType == ReleaseTargetType.Gateway ||
                string.Equals(runtimeType, EdgeRuntimeTypes.Gateway, StringComparison.OrdinalIgnoreCase))
            {
                return EdgeTaskTargetType.GatewayRuntime;
            }

            return EdgeTaskTargetType.EdgeNode;
        }

        private static string BuildEdgeTargetKey(Guid gatewayId, string runtimeType, string instanceId)
        {
            var normalizedRuntimeType = string.IsNullOrWhiteSpace(runtimeType) ? EdgeRuntimeTypes.Gateway : runtimeType.Trim();
            return string.IsNullOrWhiteSpace(instanceId)
                ? $"{gatewayId}:{normalizedRuntimeType}"
                : $"{gatewayId}:{normalizedRuntimeType}:{instanceId.Trim()}";
        }

        private static string BuildDeviceTargetKey(Guid channelDeviceId, Guid targetDeviceId, string runtimeType, string instanceId)
        {
            var normalizedRuntimeType = string.IsNullOrWhiteSpace(runtimeType) ? DefaultDeviceRuntimeType : runtimeType.Trim();
            return string.IsNullOrWhiteSpace(instanceId)
                ? $"{channelDeviceId}:device:{targetDeviceId}:{normalizedRuntimeType}"
                : $"{channelDeviceId}:device:{targetDeviceId}:{normalizedRuntimeType}:{instanceId.Trim()}";
        }

        private static Dictionary<string, string> BuildTargetMetadata(
            ReleaseTargetDto target,
            ReleaseTargetType sourceTargetType,
            Guid sourceTargetId,
            Guid? targetDeviceId,
            Guid channelDeviceId)
        {
            var metadata = new Dictionary<string, string>(target.Metadata ?? [], StringComparer.OrdinalIgnoreCase)
            {
                ["sourceTargetType"] = sourceTargetType.ToString(),
                ["deliveryChannelDeviceId"] = channelDeviceId.ToString("D")
            };

            if (sourceTargetId != Guid.Empty)
            {
                metadata["sourceTargetId"] = sourceTargetId.ToString("D");
            }

            if (targetDeviceId.HasValue)
            {
                metadata[TargetDeviceIdKey] = targetDeviceId.Value.ToString("D");
                metadata[DeviceIdKey] = targetDeviceId.Value.ToString("D");
            }

            return metadata;
        }

        private static List<Guid> ParseDeviceIds(IReadOnlyDictionary<string, string> metadata)
        {
            if (metadata == null)
            {
                return [];
            }

            var values = metadata.TryGetValue(DeviceIdsMetadataKey, out var direct)
                ? direct
                : metadata.FirstOrDefault(pair => string.Equals(pair.Key, DeviceIdsMetadataKey, StringComparison.OrdinalIgnoreCase)).Value;
            if (string.IsNullOrWhiteSpace(values))
            {
                return [];
            }

            return values
                .Split([',', ';', ' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => Guid.TryParse(value, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();
        }

        private static void AddPackageMetadataParameters(
            Dictionary<string, object> parameters,
            ReleasePackage package,
            EdgeTaskType taskType)
        {
            var packageMetadata = DeserializeObjectMap(package.Metadata);
            switch (taskType)
            {
                case EdgeTaskType.DeviceScriptOta:
                    CopyOptionalParameter(packageMetadata, parameters, "scriptCrc32");
                    CopyOptionalParameter(packageMetadata, parameters, "scriptSignature");
                    CopyOptionalParameter(packageMetadata, parameters, "scriptSlot");
                    CopyOptionalParameter(packageMetadata, parameters, "scriptLanguage");
                    CopyOptionalParameter(packageMetadata, parameters, "scriptEncoding");
                    break;
                case EdgeTaskType.FirmwareOta:
                    CopyOptionalParameter(packageMetadata, parameters, "firmwareSignature");
                    CopyOptionalParameter(packageMetadata, parameters, "firmwarePartition");
                    CopyOptionalParameter(packageMetadata, parameters, "firmwareFamily");
                    CopyOptionalParameter(packageMetadata, parameters, "bootloaderVersion");
                    CopyOptionalParameter(packageMetadata, parameters, "requiresReboot");
                    break;
            }
        }

        private static void CopyOptionalParameter(
            IReadOnlyDictionary<string, object> source,
            Dictionary<string, object> destination,
            string key)
        {
            if (TryGetDictionaryValue(source, key, out var value) && value != null)
            {
                destination[key] = value;
            }
        }

        private static bool HasActiveTasks(IEnumerable<ReleaseTask> tasks)
            => tasks.Any(task => IsActiveTaskStatus(task) || task is { Status: ReleaseTaskStatus.Pending, EdgeTaskId: not null });

        private static bool HasFailedTasks(IEnumerable<ReleaseTask> tasks)
            => tasks.Any(IsFailedTaskStatus);

        private static bool IsActiveTaskStatus(ReleaseTask task)
            => task.Status is ReleaseTaskStatus.Sent or ReleaseTaskStatus.Accepted or ReleaseTaskStatus.Running;

        private static bool IsFailedTaskStatus(ReleaseTask task)
            => task.Status is ReleaseTaskStatus.Failed or ReleaseTaskStatus.TimedOut or ReleaseTaskStatus.Cancelled or ReleaseTaskStatus.RollbackFailed;

        private static bool IsTerminalTask(ReleaseTask task)
            => task.Status is ReleaseTaskStatus.Succeeded or ReleaseTaskStatus.Failed or ReleaseTaskStatus.TimedOut or ReleaseTaskStatus.Cancelled or ReleaseTaskStatus.RolledBack or ReleaseTaskStatus.RollbackFailed;

        private static bool IsFinalPlanStatus(ReleasePlanStatus status)
            => status is ReleasePlanStatus.Succeeded or ReleasePlanStatus.PartiallySucceeded or ReleasePlanStatus.Failed or ReleasePlanStatus.Cancelled or ReleasePlanStatus.RolledBack or ReleasePlanStatus.RollbackFailed;

        private static string MergeMetadata(string currentJson, Dictionary<string, string> values)
        {
            var merged = DeserializeStringMap(currentJson);
            foreach (var value in values ?? [])
            {
                merged[value.Key] = value.Value ?? string.Empty;
            }

            return SerializeStringMap(merged);
        }

        private static string SerializeStringMap(IReadOnlyDictionary<string, string> values)
            => JsonSerializer.Serialize(values ?? new Dictionary<string, string>(), WebJsonOptions);

        private static string SerializeOrNull<T>(T value)
            => value == null ? null : JsonSerializer.Serialize(value, WebJsonOptions);

        private static Dictionary<string, string> DeserializeStringMap(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(payload, WebJsonOptions) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static Dictionary<string, object> DeserializeObjectMap(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(payload, WebJsonOptions) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        /// <summary>
        /// 按键名忽略大小写读取字典值。
        /// </summary>
        /// <typeparam name="T">字典值类型。</typeparam>
        /// <param name="values">待读取的字典。</param>
        /// <param name="key">键名。</param>
        /// <param name="value">找到的值。</param>
        /// <returns>找到时返回 true。</returns>
        private static bool TryGetDictionaryValue<T>(IReadOnlyDictionary<string, T> values, string key, out T value)
        {
            value = default;
            if (values == null || string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            if (values.TryGetValue(key, out value))
            {
                return true;
            }

            foreach (var pair in values)
            {
                if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = pair.Value;
                    return true;
                }
            }

            return false;
        }

        private static string Coalesce(string primary, string fallback)
            => string.IsNullOrWhiteSpace(primary) ? fallback ?? string.Empty : primary.Trim();

        private static string ResolveUserName(UserProfile profile)
        {
            if (profile == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(profile.Name))
            {
                return profile.Name;
            }

            return string.IsNullOrWhiteSpace(profile.Email) ? profile.Id.ToString("D") : profile.Email;
        }

        private sealed class ResolvedReleaseTarget
        {
            public string Error { get; private set; }

            public ReleaseTargetType TargetType { get; private set; }

            public Guid TargetId { get; private set; }

            public Device Gateway { get; private set; }

            public EdgeNode EdgeNode { get; private set; }

            public string RuntimeType { get; private set; }

            public string InstanceId { get; private set; }

            public string TargetKey { get; private set; }

            public Dictionary<string, string> Metadata { get; private set; } = [];

            public static ResolvedReleaseTarget Fail(string error)
                => new() { Error = error };

            public static ResolvedReleaseTarget Ok(
                ReleaseTargetType targetType,
                Guid targetId,
                Device gateway,
                EdgeNode edgeNode,
                string runtimeType,
                string instanceId,
                string targetKey,
                Dictionary<string, string> metadata)
                => new()
                {
                    TargetType = targetType,
                    TargetId = targetId,
                    Gateway = gateway,
                    EdgeNode = edgeNode,
                    RuntimeType = runtimeType ?? string.Empty,
                    InstanceId = instanceId ?? string.Empty,
                    TargetKey = targetKey ?? string.Empty,
                    Metadata = metadata ?? []
                };
        }

        private sealed class ResolvedReleaseTargets
        {
            public string Error { get; private set; }

            public IReadOnlyList<ResolvedReleaseTarget> Targets { get; private set; } = [];

            public static ResolvedReleaseTargets Fail(string error)
                => new() { Error = error };

            public static ResolvedReleaseTargets Ok(IReadOnlyList<ResolvedReleaseTarget> targets)
                => new() { Targets = targets ?? [] };
        }
    }
}
