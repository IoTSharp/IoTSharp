using IoTSharp.Contracts;
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
using EdgeNodeQueryDto = IoTSharp.Dtos.EdgeNodeQueryDto;
using EdgeTaskAuditLogDto = IoTSharp.Dtos.EdgeTaskAuditLogDto;
using EdgeTaskRetryRequestDto = IoTSharp.Dtos.EdgeTaskRetryRequestDto;
using EdgeTaskRetryResultDto = IoTSharp.Dtos.EdgeTaskRetryResultDto;
using EdgeTaskTimelineDto = IoTSharp.Dtos.EdgeTaskTimelineDto;
using EdgeTaskTimelineNodeDto = IoTSharp.Dtos.EdgeTaskTimelineNodeDto;


namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class EdgeTaskController : ControllerBase
    {
        private const string TaskContractVersion = EdgeNodeContractVersions.EdgeTaskV1;
        private const string EdgeTaskHistoryKeyPrefix = "_edge.task.history.";
        private const string ConfigurationVersionIdKey = "configurationVersionId";
        private const string ConfigurationVersionKey = "configurationVersion";
        private const string ConfigurationHashKey = "configurationHash";
        private const string ReleasePackageIdKey = "packageId";
        private const string ReleasePackageVersionKey = "packageVersion";
        private const string ReleasePackageSha256Key = "sha256";
        private const string TargetDeviceIdKey = "targetDeviceId";
        private const string DeviceIdKey = "deviceId";
        private const string ScriptCrc32Key = "scriptCrc32";
        private const string AssignmentUpdatedBy = "edge-task-receipt";
        private const string AssignmentTaskStatusUpdatedBy = "edge-task-status";
        private const string AuditActionDispatch = "EdgeTaskDispatch";
        private const string AuditActionRetry = "EdgeTaskRetry";
        private const string AuditActionTerminalReceipt = "EdgeTaskTerminalReceipt";
        private const string RetryOfTaskIdKey = "retryOfTaskId";
        private const string RetryRootTaskIdKey = "retryRootTaskId";
        private const string RetryReasonKey = "retryReason";
        private const string RetryOperatorKey = "retryOperator";
        private const string RetryAttemptKey = "retryAttempt";
        private static readonly TimeSpan DefaultRetryTtl = TimeSpan.FromDays(1);
        private static readonly IReadOnlyDictionary<EdgeTaskStatus, EdgeTaskStatus[]> AllowedTransitions = EdgeTaskStateMachine.AllowedTransitions;
        private static readonly JsonSerializerOptions ContractJsonOptions = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

        private readonly ApplicationDbContext _context;
        private readonly ILogger<EdgeTaskController> _logger;

        public EdgeTaskController(ApplicationDbContext context, ILogger<EdgeTaskController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("Dispatch")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ActionResult<ApiResult<EdgeTaskRequestDto>>> Dispatch([FromBody] EdgeTaskRequestDto request)
        {
            if (request == null)
            {
                return Ok(new ApiResult<EdgeTaskRequestDto>(ApiCode.InValidData, "Task payload is required", null));
            }

            if (!string.Equals(request.ContractVersion, TaskContractVersion, StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new ApiResult<EdgeTaskRequestDto>(ApiCode.InValidData, $"Unsupported contractVersion: {request.ContractVersion}", null));
            }

            if (request.TaskId == Guid.Empty)
            {
                return Ok(new ApiResult<EdgeTaskRequestDto>(ApiCode.InValidData, "taskId is required", null));
            }

            if (request.Address == null || string.IsNullOrWhiteSpace(request.Address.TargetKey))
            {
                return Ok(new ApiResult<EdgeTaskRequestDto>(ApiCode.InValidData, "address.targetKey is required", null));
            }

            var profile = this.GetUserProfile();
            var deviceId = await ResolveDeviceIdAsync(request.Address.TargetKey, request.Address.InstanceId, request.Address.DeviceId);
            if (deviceId == null)
            {
                return Ok(new ApiResult<EdgeTaskRequestDto>(ApiCode.InValidData, "Unable to resolve target device for dispatch", null));
            }

            var device = await _context.Device
                .Include(d => d.Customer)
                .Include(d => d.Tenant)
                .FirstOrDefaultAsync(d => d.Id == deviceId && !d.Deleted && d.Tenant.Id == profile.Tenant && d.Customer.Id == profile.Customer);

            if (device == null)
            {
                return Ok(new ApiResult<EdgeTaskRequestDto>(ApiCode.NotFoundDevice, "Edge device not found", null));
            }

            var now = DateTime.UtcNow;
            request.ContractVersion = TaskContractVersion;
            request.CreatedAt = request.CreatedAt == default ? now : request.CreatedAt.ToUniversalTime();
            request.Address.DeviceId = device.Id;

            var requestPayload = SerializeOrNull(request) ?? "{}";
            var formalTask = await _context.EdgeTasks.FirstOrDefaultAsync(task => task.Id == request.TaskId && !task.Deleted);
            if (formalTask != null && formalTask.GatewayId != device.Id)
            {
                return Ok(new ApiResult<EdgeTaskRequestDto>(ApiCode.InValidData, "taskId already exists for another edge target", null));
            }

            if (formalTask != null)
            {
                return Ok(new ApiResult<EdgeTaskRequestDto>(ApiCode.Success, "OK", ToEdgeTaskRequestDto(formalTask)));
            }

            formalTask = await CreateFormalEdgeTaskAsync(request, device, requestPayload);
            _context.EdgeTasks.Add(formalTask);
            AddUserEdgeTaskAudit(
                profile,
                formalTask,
                AuditActionDispatch,
                new
                {
                    taskId = formalTask.Id,
                    taskType = formalTask.TaskType.ToString(),
                    targetKey = formalTask.TargetKey,
                    runtimeType = formalTask.RuntimeType,
                    instanceId = formalTask.InstanceId
                },
                EdgeTaskStatus.Pending.ToString(),
                now);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Dispatched edge task {TaskId} to {TargetKey}, type {TaskType}, runtime {RuntimeType}, instance {InstanceId}",
                request.TaskId,
                request.Address.TargetKey,
                request.TaskType,
                request.Address.RuntimeType,
                request.Address.InstanceId);

            return Ok(new ApiResult<EdgeTaskRequestDto>(ApiCode.Success, "OK", request));
        }

        [AllowAnonymous]
        [HttpPost("Receipt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ActionResult<ApiResult<EdgeTaskReceiptDto>>> Receipt([FromBody] EdgeTaskReceiptDto request)
        {
            if (request == null)
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, "Receipt payload is required", null));
            }

            if (!string.Equals(request.ContractVersion, TaskContractVersion, StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, $"Unsupported contractVersion: {request.ContractVersion}", null));
            }

            if (request.TaskId == Guid.Empty)
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, "taskId is required", null));
            }

            if (string.IsNullOrWhiteSpace(request.TargetKey))
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, "targetKey is required", null));
            }

            if (request.Progress is < 0 or > 100)
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, "progress must be between 0 and 100", null));
            }

            if (request.Status == EdgeTaskStatus.Running && request.Progress == null)
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, "progress is required when status is Running", null));
            }

            if (request.Status is EdgeTaskStatus.Succeeded or EdgeTaskStatus.Failed or EdgeTaskStatus.TimedOut or EdgeTaskStatus.Cancelled && request.Progress is > 0 and < 100)
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, "terminal status progress should be null, 0 or 100", null));
            }

            var deviceId = await ResolveDeviceIdAsync(request.TargetKey, request.InstanceId, null);
            if (deviceId == null)
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, "Unable to resolve target device for receipt", null));
            }

            var formalTask = await GetFormalEdgeTaskAsync(deviceId.Value, request.TaskId);
            if (formalTask == null)
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, "Task request not found for receipt", null));
            }

            if (!IsTransitionAllowed(formalTask.Status, request.Status))
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, $"Invalid edge task transition: {formalTask.Status} -> {request.Status}", null));
            }

            request.ReportedAt = request.ReportedAt == default ? DateTime.UtcNow : request.ReportedAt.ToUniversalTime();
            request.TargetType = formalTask.TargetType;
            request.RuntimeType = string.IsNullOrWhiteSpace(request.RuntimeType) ? formalTask.RuntimeType ?? string.Empty : request.RuntimeType;
            request.InstanceId = string.IsNullOrWhiteSpace(request.InstanceId) ? formalTask.InstanceId ?? string.Empty : request.InstanceId;

            var collectionReceiptError = await ApplyCollectionConfigurationReceiptAsync(formalTask, request);
            if (!string.IsNullOrWhiteSpace(collectionReceiptError))
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, collectionReceiptError, null));
            }

            var releasePackageReceiptError = await ApplyReleasePackageReceiptAsync(formalTask, request);
            if (!string.IsNullOrWhiteSpace(releasePackageReceiptError))
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, releasePackageReceiptError, null));
            }

            var receiptPayload = SerializeOrNull(request) ?? "{}";
            var receivedAt = DateTime.UtcNow;
            ApplyFormalReceipt(formalTask, request);
            _context.EdgeTaskReceipts.Add(CreateFormalEdgeTaskReceipt(formalTask, request, receiptPayload, receivedAt));
            await ApplyReleaseTaskReceiptAsync(formalTask, request, receiptPayload, receivedAt);
            if (IsTerminalStatus(request.Status))
            {
                AddRuntimeEdgeTaskAudit(
                    formalTask,
                    request,
                    AuditActionTerminalReceipt,
                    new
                    {
                        taskId = formalTask.Id,
                        status = request.Status.ToString(),
                        message = request.Message ?? string.Empty,
                        progress = request.Progress,
                        reportedAt = request.ReportedAt
                    },
                    request.Status.ToString());
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Received edge task receipt {TaskId} for {TargetKey}, runtime {RuntimeType}, instance {InstanceId}, status {Status}",
                request.TaskId,
                request.TargetKey,
                request.RuntimeType,
                request.InstanceId,
                request.Status);

            return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.Success, "OK", request));
        }

        [HttpGet("Receipt/{deviceId}")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ApiResult<EdgeTaskReceiptDto>> GetLatestReceipt(Guid deviceId)
        {
            var profile = this.GetUserProfile();
            var device = await _context.Device
                .Include(d => d.Customer)
                .Include(d => d.Tenant)
                .FirstOrDefaultAsync(d => d.Id == deviceId && !d.Deleted && d.Tenant.Id == profile.Tenant && d.Customer.Id == profile.Customer);

            if (device == null)
            {
                return new ApiResult<EdgeTaskReceiptDto>(ApiCode.NotFoundDevice, "Edge device not found", null);
            }

            var formalReceipt = await _context.EdgeTaskReceipts
                .Where(receipt => receipt.GatewayId == deviceId && !receipt.Deleted)
                .OrderByDescending(receipt => receipt.ReportedAt)
                .ThenByDescending(receipt => receipt.ReceivedAt)
                .FirstOrDefaultAsync();

            if (formalReceipt != null)
            {
                return new ApiResult<EdgeTaskReceiptDto>(ApiCode.Success, "OK", ToEdgeTaskReceiptDto(formalReceipt));
            }

            var receiptValue = await _context.EdgeTasks
                .Where(task => task.GatewayId == deviceId && !task.Deleted && task.LastReceiptPayload != null && task.LastReceiptPayload != string.Empty)
                .OrderByDescending(task => task.LastReceiptAt ?? task.UpdatedAt)
                .Select(task => task.LastReceiptPayload)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(receiptValue))
            {
                return new ApiResult<EdgeTaskReceiptDto>(ApiCode.Success, "OK", null);
            }

            var receipt = JsonSerializer.Deserialize<EdgeTaskReceiptDto>(receiptValue, ContractJsonOptions);
            return new ApiResult<EdgeTaskReceiptDto>(ApiCode.Success, "OK", receipt);
        }

        [HttpGet("History/{deviceId}")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ApiResult<List<object>>> History(Guid deviceId)
        {
            var profile = this.GetUserProfile();
            var device = await _context.Device
                .Include(d => d.Customer)
                .Include(d => d.Tenant)
                .FirstOrDefaultAsync(d => d.Id == deviceId && !d.Deleted && d.Tenant.Id == profile.Tenant && d.Customer.Id == profile.Customer);

            if (device == null)
            {
                return new ApiResult<List<object>>(ApiCode.NotFoundDevice, "Edge device not found", null);
            }

            var formalTasks = await _context.EdgeTasks
                .Where(task => task.GatewayId == deviceId && !task.Deleted)
                .OrderByDescending(task => task.UpdatedAt)
                .Take(50)
                .ToListAsync();

            var taskIds = formalTasks.Select(task => task.Id).ToList();
            var receipts = await _context.EdgeTaskReceipts
                .Where(receipt => taskIds.Contains(receipt.TaskId) && !receipt.Deleted)
                .OrderByDescending(receipt => receipt.ReportedAt)
                .Take(200)
                .ToListAsync();
            var receiptGroups = receipts.GroupBy(receipt => receipt.TaskId).ToDictionary(group => group.Key, group => group.ToList());
            var auditLogs = await _context.AuditLog
                .Where(log => taskIds.Contains(log.ObjectID) && log.ObjectType == ObjectType.EdgeTask)
                .OrderByDescending(log => log.ActiveDateTime)
                .Take(200)
                .ToListAsync();
            var auditGroups = auditLogs.GroupBy(log => log.ObjectID).ToDictionary(group => group.Key, group => group.ToList());
            var formalRecords = new List<EdgeTaskHistoryRecord>();

            foreach (var task in formalTasks)
            {
                formalRecords.Add(new EdgeTaskHistoryRecord
                {
                    key = $"{EdgeTaskHistoryKeyPrefix}{task.Id:N}.request",
                    at = task.CreatedAt,
                    payload = task.RequestPayload ?? "{}",
                    status = EdgeTaskStatus.Pending.ToString()
                });

                if (task.SentAt != null)
                {
                    formalRecords.Add(new EdgeTaskHistoryRecord
                    {
                        key = $"{EdgeTaskHistoryKeyPrefix}{task.Id:N}.dispatch",
                        at = task.SentAt.Value,
                        payload = task.RequestPayload ?? "{}",
                        status = EdgeTaskStatus.Sent.ToString()
                    });
                }

                if (receiptGroups.TryGetValue(task.Id, out var taskReceipts))
                {
                    formalRecords.AddRange(taskReceipts.Select(receipt => new EdgeTaskHistoryRecord
                    {
                        key = $"{EdgeTaskHistoryKeyPrefix}{task.Id:N}.receipt",
                        at = receipt.ReportedAt,
                        payload = receipt.Payload ?? SerializeOrNull(ToEdgeTaskReceiptDto(receipt)) ?? "{}",
                        status = receipt.Status.ToString()
                    }));
                }

                if (auditGroups.TryGetValue(task.Id, out var taskAuditLogs))
                {
                    formalRecords.AddRange(taskAuditLogs.Select(log => new EdgeTaskHistoryRecord
                    {
                        key = $"{EdgeTaskHistoryKeyPrefix}{task.Id:N}.audit.{log.Id:N}",
                        at = log.ActiveDateTime,
                        payload = log.ActionData ?? "{}",
                        status = log.ActionName ?? "Audit"
                    }));
                }
            }

            return new ApiResult<List<object>>(
                ApiCode.Success,
                "OK",
                formalRecords.OrderByDescending(record => record.at).Take(50).Cast<object>().ToList());
        }

        [AllowAnonymous]
        [HttpGet("Dispatch/{accessToken}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ApiResult<List<EdgeTaskRequestDto>>> PullPendingDispatch(string accessToken, [FromQuery] int take = 10)
        {
            var gateway = GetGatewayByAccessToken(accessToken);
            if (gateway == null)
            {
                return new ApiResult<List<EdgeTaskRequestDto>>(ApiCode.NotFoundDevice, "gateway access token not found", null);
            }

            var takeCount = Math.Clamp(take, 1, 50);
            var now = DateTime.UtcNow;
            var formalTasks = await _context.EdgeTasks
                .Where(task => task.GatewayId == gateway.Id
                    && !task.Deleted
                    && (task.ExpireAt == null || task.ExpireAt > now)
                    && (task.Status == EdgeTaskStatus.Pending || task.Status == EdgeTaskStatus.Sent))
                .OrderBy(task => task.CreatedAt)
                .Take(takeCount)
                .ToListAsync();

            if (formalTasks.Count > 0)
            {
                foreach (var formalTask in formalTasks)
                {
                    if (formalTask.Status == EdgeTaskStatus.Pending)
                    {
                        ApplyFormalStatus(formalTask, EdgeTaskStatus.Sent, now, null, null, null);
                        await ApplyCollectionAssignmentTaskStatusAsync(
                            formalTask,
                            EdgeTaskStatus.Sent,
                            now,
                            "配置发布任务已被执行端拉取",
                            null);
                        await ApplyReleaseTaskStatusAsync(
                            formalTask,
                            EdgeTaskStatus.Sent,
                            now,
                            "发布任务已被执行端拉取",
                            null);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return new ApiResult<List<EdgeTaskRequestDto>>(ApiCode.Success, "OK", formalTasks.Select(ToEdgeTaskRequestDto).ToList());
        }

        [AllowAnonymous]
        [HttpPost("Dispatch/{accessToken}/Accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ActionResult<ApiResult>> AcceptDispatch(string accessToken, [FromBody] EdgeTaskReceiptDto request)
        {
            var gateway = GetGatewayByAccessToken(accessToken);
            if (gateway == null)
            {
                return Ok(new ApiResult(ApiCode.NotFoundDevice, "gateway access token not found"));
            }

            if (request == null || request.TaskId == Guid.Empty)
            {
                return Ok(new ApiResult(ApiCode.InValidData, "taskId is required"));
            }

            if (!string.IsNullOrWhiteSpace(request.ContractVersion) &&
                !string.Equals(request.ContractVersion, TaskContractVersion, StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new ApiResult(ApiCode.InValidData, $"Unsupported contractVersion: {request.ContractVersion}"));
            }

            var formalTask = await GetFormalEdgeTaskAsync(gateway.Id, request.TaskId);
            if (formalTask == null)
            {
                return Ok(new ApiResult(ApiCode.InValidData, "Task request not found for acceptance"));
            }

            if (!IsTransitionAllowed(formalTask.Status, EdgeTaskStatus.Accepted))
            {
                return Ok(new ApiResult(ApiCode.InValidData, $"Invalid edge task transition: {formalTask.Status} -> {EdgeTaskStatus.Accepted}"));
            }

            request.ContractVersion = string.IsNullOrWhiteSpace(request.ContractVersion) ? TaskContractVersion : request.ContractVersion;
            request.TargetKey = string.IsNullOrWhiteSpace(request.TargetKey) ? formalTask.TargetKey ?? gateway.Id.ToString() : request.TargetKey;
            request.TargetType = formalTask.TargetType;
            request.RuntimeType = string.IsNullOrWhiteSpace(request.RuntimeType) ? formalTask.RuntimeType ?? string.Empty : request.RuntimeType;
            request.InstanceId = string.IsNullOrWhiteSpace(request.InstanceId) ? formalTask.InstanceId ?? string.Empty : request.InstanceId;
            request.Status = EdgeTaskStatus.Accepted;
            request.ReportedAt = request.ReportedAt == default ? DateTime.UtcNow : request.ReportedAt;
            request.ReportedAt = request.ReportedAt.ToUniversalTime();

            var collectionReceiptError = await ApplyCollectionConfigurationReceiptAsync(formalTask, request);
            if (!string.IsNullOrWhiteSpace(collectionReceiptError))
            {
                return Ok(new ApiResult(ApiCode.InValidData, collectionReceiptError));
            }

            var receiptPayload = SerializeOrNull(request) ?? "{}";
            var receivedAt = DateTime.UtcNow;
            ApplyFormalReceipt(formalTask, request);
            _context.EdgeTaskReceipts.Add(CreateFormalEdgeTaskReceipt(formalTask, request, receiptPayload, receivedAt));
            await ApplyReleaseTaskReceiptAsync(formalTask, request, receiptPayload, receivedAt);
            await _context.SaveChangesAsync();
            return Ok(new ApiResult(ApiCode.Success, "OK"));
        }

        [HttpGet("List")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ApiResult<PagedData<EdgeTaskTimelineDto>>> List([FromQuery] EdgeNodeQueryDto query)
        {
            var profile = this.GetUserProfile();
            query ??= new EdgeNodeQueryDto();
            query.Limit = query.Limit < 10 ? 10 : query.Limit;

            var devices = await _context.Device
                .Include(d => d.Customer)
                .Include(d => d.Tenant)
                .Where(d => !d.Deleted && d.DeviceType == DeviceType.Gateway && d.Tenant.Id == profile.Tenant && d.Customer.Id == profile.Customer)
                .ToListAsync();

            var deviceIds = devices.Select(d => d.Id).ToList();
            var deviceNames = devices.ToDictionary(d => d.Id, d => string.IsNullOrWhiteSpace(d.Name) ? d.Id.ToString() : d.Name);

            var formalTasks = await _context.EdgeTasks
                .Where(task => deviceIds.Contains(task.GatewayId) && !task.Deleted)
                .OrderByDescending(task => task.UpdatedAt)
                .Take(500)
                .ToListAsync();

            var taskIds = formalTasks.Select(task => task.Id).ToList();
            var formalReceipts = await _context.EdgeTaskReceipts
                .Where(receipt => taskIds.Contains(receipt.TaskId) && !receipt.Deleted)
                .OrderBy(receipt => receipt.ReportedAt)
                .ThenBy(receipt => receipt.ReceivedAt)
                .ToListAsync();
            var receiptsByTask = formalReceipts
                .GroupBy(receipt => receipt.TaskId)
                .ToDictionary(group => group.Key, group => (IReadOnlyList<EdgeTaskReceipt>)group.ToList());
            var auditLogs = await _context.AuditLog
                .Where(log => taskIds.Contains(log.ObjectID) && log.ObjectType == ObjectType.EdgeTask)
                .OrderBy(log => log.ActiveDateTime)
                .ToListAsync();
            var auditsByTask = auditLogs
                .GroupBy(log => log.ObjectID)
                .ToDictionary(group => group.Key, group => (IReadOnlyList<AuditLog>)group.ToList());

            var rows = formalTasks
                .Select(task => ToEdgeTaskTimelineDto(
                    task,
                    deviceNames,
                    receiptsByTask.TryGetValue(task.Id, out var receipts) ? receipts : Array.Empty<EdgeTaskReceipt>(),
                    auditsByTask.TryGetValue(task.Id, out var audits) ? audits : Array.Empty<AuditLog>()))
                .OrderByDescending(item => item.LastUpdatedAt)
                .ToList();

            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                rows = rows.Where(item => item.DeviceName.Contains(query.Name, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(query.RuntimeType))
            {
                rows = rows.Where(item => string.Equals(item.RuntimeType, query.RuntimeType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                rows = rows.Where(item => string.Equals(item.CurrentStatus, query.Status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return new ApiResult<PagedData<EdgeTaskTimelineDto>>(ApiCode.Success, "OK", new PagedData<EdgeTaskTimelineDto>
            {
                total = rows.Count,
                rows = rows.Skip(query.Offset * query.Limit).Take(query.Limit).ToList()
            });
        }

        [HttpPost("{taskId:guid}/Retry")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ActionResult<ApiResult<EdgeTaskRetryResultDto>>> Retry(Guid taskId, [FromBody] EdgeTaskRetryRequestDto request)
        {
            var profile = this.GetUserProfile();
            request ??= new EdgeTaskRetryRequestDto();

            var originalTask = await _context.EdgeTasks
                .FirstOrDefaultAsync(task => task.Id == taskId
                    && !task.Deleted
                    && task.TenantId == profile.Tenant
                    && task.CustomerId == profile.Customer);

            if (originalTask == null)
            {
                return Ok(new ApiResult<EdgeTaskRetryResultDto>(ApiCode.CantFindObject, "Edge task not found", null));
            }

            if (!IsRetryableFailureStatus(originalTask.Status))
            {
                return Ok(new ApiResult<EdgeTaskRetryResultDto>(
                    ApiCode.InValidData,
                    $"Only Failed or TimedOut edge tasks can be retried, current status is {originalTask.Status}",
                    null));
            }

            var retryTaskId = request.TaskId is { } requestedTaskId && requestedTaskId != Guid.Empty
                ? requestedTaskId
                : Guid.NewGuid();
            var taskExists = await _context.EdgeTasks.AnyAsync(task => task.Id == retryTaskId && !task.Deleted);
            if (taskExists)
            {
                return Ok(new ApiResult<EdgeTaskRetryResultDto>(ApiCode.InValidData, "retry taskId already exists", null));
            }

            var gateway = await _context.Device
                .Include(device => device.Tenant)
                .Include(device => device.Customer)
                .FirstOrDefaultAsync(device => device.Id == originalTask.GatewayId
                    && !device.Deleted
                    && device.DeviceType == DeviceType.Gateway
                    && device.Tenant.Id == profile.Tenant
                    && device.Customer.Id == profile.Customer);

            if (gateway == null)
            {
                return Ok(new ApiResult<EdgeTaskRetryResultDto>(ApiCode.NotFoundDevice, "Edge device not found", null));
            }

            var now = DateTime.UtcNow;
            var operatorName = ResolveUserName(profile);
            var retryRequest = ToEdgeTaskRequestDto(originalTask);
            retryRequest.ContractVersion = TaskContractVersion;
            retryRequest.TaskId = retryTaskId;
            retryRequest.CreatedAt = now;
            retryRequest.ExpireAt = request.ExpireAt?.ToUniversalTime() ?? now.Add(ResolveRetryTtl(originalTask));
            retryRequest.Address.DeviceId = gateway.Id;
            retryRequest.Metadata = BuildRetryMetadata(originalTask, retryRequest.Metadata, request, operatorName, await CountRetryAttemptsAsync(originalTask.Id));

            var retryPayload = SerializeOrNull(retryRequest) ?? "{}";
            var retryTask = await CreateFormalEdgeTaskAsync(retryRequest, gateway, retryPayload);
            _context.EdgeTasks.Add(retryTask);
            await ApplyCollectionAssignmentTaskStatusAsync(
                retryTask,
                EdgeTaskStatus.Pending,
                now,
                BuildRetryAssignmentMessage(request.Reason),
                null);

            AddUserEdgeTaskAudit(
                profile,
                originalTask,
                AuditActionRetry,
                new
                {
                    originalTaskId = originalTask.Id,
                    retryTaskId = retryTask.Id,
                    originalStatus = originalTask.Status.ToString(),
                    reason = request.Reason ?? string.Empty
                },
                "RetryCreated",
                now);
            AddUserEdgeTaskAudit(
                profile,
                retryTask,
                AuditActionRetry,
                new
                {
                    originalTaskId = originalTask.Id,
                    retryTaskId = retryTask.Id,
                    originalStatus = originalTask.Status.ToString(),
                    reason = request.Reason ?? string.Empty
                },
                EdgeTaskStatus.Pending.ToString(),
                now);

            await _context.SaveChangesAsync();

            return Ok(new ApiResult<EdgeTaskRetryResultDto>(
                ApiCode.Success,
                "OK",
                new EdgeTaskRetryResultDto
                {
                    OriginalTask = ToEdgeTaskDto(originalTask),
                    RetryTask = retryRequest
                }));
        }

        [HttpGet("{taskId:guid}/Audit")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async System.Threading.Tasks.Task<ApiResult<List<EdgeTaskAuditLogDto>>> Audit(Guid taskId)
        {
            var profile = this.GetUserProfile();
            var exists = await _context.EdgeTasks.AnyAsync(task => task.Id == taskId
                && !task.Deleted
                && task.TenantId == profile.Tenant
                && task.CustomerId == profile.Customer);

            if (!exists)
            {
                return new ApiResult<List<EdgeTaskAuditLogDto>>(ApiCode.CantFindObject, "Edge task not found", null);
            }

            var logs = await _context.AuditLog
                .Where(log => log.ObjectID == taskId
                    && log.ObjectType == ObjectType.EdgeTask
                    && log.TenantId == profile.Tenant
                    && log.CustomerId == profile.Customer)
                .OrderByDescending(log => log.ActiveDateTime)
                .Take(100)
                .ToListAsync();

            return new ApiResult<List<EdgeTaskAuditLogDto>>(
                ApiCode.Success,
                "OK",
                logs.Select(ToEdgeTaskAuditLogDto).ToList());
        }

        [HttpGet("StateMachine")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public ApiResult<object> StateMachine()
        {
            var data = AllowedTransitions.ToDictionary(
                pair => pair.Key.ToString(),
                pair => pair.Value.Select(status => status.ToString()).ToArray());

            return new ApiResult<object>(ApiCode.Success, "OK", new
            {
                contractVersion = TaskContractVersion,
                states = Enum.GetNames(typeof(EdgeTaskStatus)),
                transitions = data,
                terminalStates = EdgeTaskStateMachine.TerminalStates.Select(status => status.ToString()).ToArray()
            });
        }

        /// <summary>
        /// 根据下发请求创建正式 EdgeTask 主模型。
        /// </summary>
        /// <param name="request">平台下发的任务请求。</param>
        /// <param name="gateway">承载任务通道的 Gateway 设备。</param>
        /// <param name="requestPayload">规范化后的请求 JSON。</param>
        /// <returns>已初始化但尚未保存的 EdgeTask。</returns>
        private async System.Threading.Tasks.Task<EdgeTask> CreateFormalEdgeTaskAsync(EdgeTaskRequestDto request, Device gateway, string requestPayload)
        {
            var edgeNodeId = await _context.EdgeNodes
                .Where(node => node.GatewayId == gateway.Id && !node.Deleted)
                .Select(node => (Guid?)node.Id)
                .FirstOrDefaultAsync();

            return new EdgeTask
            {
                Id = request.TaskId,
                ContractVersion = request.ContractVersion,
                TaskType = request.TaskType,
                TargetType = request.Address.TargetType,
                GatewayId = gateway.Id,
                Gateway = gateway,
                EdgeNodeId = edgeNodeId,
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
                Tenant = gateway.Tenant,
                CustomerId = gateway.CustomerId ?? gateway.Customer?.Id,
                Customer = gateway.Customer
            };
        }

        /// <summary>
        /// 读取指定 Gateway 通道上的正式 EdgeTask。
        /// </summary>
        /// <param name="gatewayId">承载任务通道的 Gateway 设备 ID。</param>
        /// <param name="taskId">任务 ID。</param>
        /// <returns>找到时返回正式任务实体。</returns>
        private async System.Threading.Tasks.Task<EdgeTask> GetFormalEdgeTaskAsync(Guid gatewayId, Guid taskId)
        {
            return await _context.EdgeTasks
                .FirstOrDefaultAsync(task => task.GatewayId == gatewayId && task.Id == taskId && !task.Deleted);
        }

        /// <summary>
        /// 将正式任务模型还原为执行端拉取使用的任务请求 DTO。
        /// </summary>
        /// <param name="task">正式任务模型。</param>
        /// <returns>任务请求 DTO。</returns>
        private static EdgeTaskRequestDto ToEdgeTaskRequestDto(EdgeTask task)
        {
            var storedRequest = DeserializeEdgeTaskRequest(task.RequestPayload);
            if (storedRequest != null)
            {
                return storedRequest;
            }

            return new EdgeTaskRequestDto
            {
                ContractVersion = task.ContractVersion,
                TaskId = task.Id,
                TaskType = task.TaskType,
                CreatedAt = task.CreatedAt,
                ExpireAt = task.ExpireAt,
                Address = new EdgeTaskAddressDto
                {
                    TargetType = task.TargetType,
                    DeviceId = task.GatewayId,
                    RuntimeType = task.RuntimeType ?? string.Empty,
                    InstanceId = task.InstanceId ?? string.Empty,
                    TargetKey = task.TargetKey ?? task.GatewayId.ToString()
                },
                Parameters = DeserializeObjectDictionary(task.Parameters),
                Metadata = DeserializeStringDictionary(task.Metadata)
            };
        }

        /// <summary>
        /// 将正式任务模型转换为平台侧状态快照。
        /// </summary>
        /// <param name="task">正式任务模型。</param>
        /// <returns>EdgeTask 当前态 DTO。</returns>
        private static EdgeTaskDto ToEdgeTaskDto(EdgeTask task)
        {
            return new EdgeTaskDto
            {
                ContractVersion = string.IsNullOrWhiteSpace(task.ContractVersion) ? TaskContractVersion : task.ContractVersion,
                TaskId = task.Id,
                TaskType = task.TaskType,
                Address = new EdgeTaskAddressDto
                {
                    TargetType = task.TargetType,
                    DeviceId = task.GatewayId,
                    RuntimeType = task.RuntimeType ?? string.Empty,
                    InstanceId = task.InstanceId ?? string.Empty,
                    TargetKey = task.TargetKey ?? task.GatewayId.ToString()
                },
                Status = task.Status,
                Message = task.Message ?? string.Empty,
                Progress = task.Progress,
                CreatedAt = task.CreatedAt,
                ExpireAt = task.ExpireAt,
                SentAt = task.SentAt,
                AcceptedAt = task.AcceptedAt,
                StartedAt = task.StartedAt,
                CompletedAt = task.CompletedAt,
                LastReceiptAt = task.LastReceiptAt,
                Parameters = DeserializeObjectDictionary(task.Parameters),
                Metadata = DeserializeStringDictionary(task.Metadata)
            };
        }

        /// <summary>
        /// 将审计实体转换为 EdgeTask 审计查询 DTO。
        /// </summary>
        /// <param name="log">审计实体。</param>
        /// <returns>审计查询 DTO。</returns>
        private static EdgeTaskAuditLogDto ToEdgeTaskAuditLogDto(AuditLog log)
        {
            return new EdgeTaskAuditLogDto
            {
                Id = log.Id,
                TaskId = log.ObjectID,
                ActionName = log.ActionName ?? string.Empty,
                ActionData = log.ActionData ?? "{}",
                ActionResult = log.ActionResult ?? string.Empty,
                UserName = log.UserName ?? string.Empty,
                ActiveDateTime = log.ActiveDateTime
            };
        }

        /// <summary>
        /// 构造重试任务元数据，用于跨任务追踪原始失败任务。
        /// </summary>
        /// <param name="originalTask">原失败任务。</param>
        /// <param name="metadata">原任务元数据。</param>
        /// <param name="request">重试请求。</param>
        /// <param name="operatorName">操作者名称。</param>
        /// <param name="previousRetryCount">原任务已发起过的重试次数。</param>
        /// <returns>合并后的新任务元数据。</returns>
        private static Dictionary<string, string> BuildRetryMetadata(
            EdgeTask originalTask,
            Dictionary<string, string> metadata,
            EdgeTaskRetryRequestDto request,
            string operatorName,
            int previousRetryCount)
        {
            var values = new Dictionary<string, string>(metadata ?? [], StringComparer.OrdinalIgnoreCase);
            if (request?.Metadata != null)
            {
                foreach (var pair in request.Metadata)
                {
                    if (!string.IsNullOrWhiteSpace(pair.Key))
                    {
                        values[pair.Key] = pair.Value ?? string.Empty;
                    }
                }
            }

            var originalMetadata = DeserializeStringDictionary(originalTask.Metadata);
            var retryRootTaskId = TryGetGuid(originalMetadata, RetryRootTaskIdKey)
                ?? TryGetGuid(originalMetadata, RetryOfTaskIdKey)
                ?? originalTask.Id;

            values[RetryOfTaskIdKey] = originalTask.Id.ToString("D");
            values[RetryRootTaskIdKey] = retryRootTaskId.ToString("D");
            values[RetryReasonKey] = request?.Reason ?? string.Empty;
            values[RetryOperatorKey] = operatorName ?? string.Empty;
            values[RetryAttemptKey] = (previousRetryCount + 1).ToString();
            return values;
        }

        /// <summary>
        /// 生成配置发布分配上展示的重试状态说明。
        /// </summary>
        /// <param name="reason">人工填写的重试原因。</param>
        /// <returns>面向管理端展示的状态说明。</returns>
        private static string BuildRetryAssignmentMessage(string reason)
        {
            return string.IsNullOrWhiteSpace(reason)
                ? "配置发布失败后已创建重试任务"
                : $"配置发布失败后已创建重试任务：{reason}";
        }

        /// <summary>
        /// 按原任务生命周期计算重试任务默认过期时间窗口。
        /// </summary>
        /// <param name="task">原任务。</param>
        /// <returns>用于新任务的 TTL。</returns>
        private static TimeSpan ResolveRetryTtl(EdgeTask task)
        {
            if (task.ExpireAt.HasValue)
            {
                var ttl = task.ExpireAt.Value.ToUniversalTime() - task.CreatedAt.ToUniversalTime();
                if (ttl > TimeSpan.Zero)
                {
                    return ttl;
                }
            }

            return DefaultRetryTtl;
        }

        /// <summary>
        /// 将正式任务模型转换为管理端现有 timeline 形状。
        /// </summary>
        /// <param name="task">正式任务模型。</param>
        /// <param name="deviceNames">Gateway 设备名索引。</param>
        /// <returns>任务时间线 DTO。</returns>
        private static EdgeTaskTimelineDto ToEdgeTaskTimelineDto(
            EdgeTask task,
            IReadOnlyDictionary<Guid, string> deviceNames,
            IReadOnlyList<EdgeTaskReceipt> receipts,
            IReadOnlyList<AuditLog> auditLogs)
        {
            var events = new List<EdgeTaskTimelineNodeDto>
            {
                new()
                {
                    Category = "request",
                    Status = EdgeTaskStatus.Pending.ToString(),
                    Message = string.Empty,
                    At = task.CreatedAt,
                    Payload = task.RequestPayload ?? "{}"
                }
            };

            if (task.SentAt != null)
            {
                events.Add(new EdgeTaskTimelineNodeDto
                {
                    Category = "dispatch",
                    Status = EdgeTaskStatus.Sent.ToString(),
                    Message = string.Empty,
                    At = task.SentAt.Value,
                    Payload = task.RequestPayload ?? "{}"
                });
            }

            if (receipts != null && receipts.Count > 0)
            {
                foreach (var receipt in receipts.OrderBy(receipt => receipt.ReportedAt).ThenBy(receipt => receipt.ReceivedAt))
                {
                    events.Add(new EdgeTaskTimelineNodeDto
                    {
                        Category = "receipt",
                        Status = receipt.Status.ToString(),
                        Message = receipt.Message ?? string.Empty,
                        At = receipt.ReportedAt,
                        Payload = receipt.Payload ?? SerializeOrNull(ToEdgeTaskReceiptDto(receipt)) ?? "{}"
                    });
                }
            }
            else if (task.LastReceiptAt != null)
            {
                events.Add(new EdgeTaskTimelineNodeDto
                {
                    Category = "receipt",
                    Status = task.Status.ToString(),
                    Message = task.Message ?? string.Empty,
                    At = task.LastReceiptAt.Value,
                    Payload = task.LastReceiptPayload ?? "{}"
                });
            }

            if (auditLogs != null && auditLogs.Count > 0)
            {
                foreach (var audit in auditLogs.OrderBy(log => log.ActiveDateTime))
                {
                    events.Add(new EdgeTaskTimelineNodeDto
                    {
                        Category = "audit",
                        Status = audit.ActionName ?? "Audit",
                        Message = audit.ActionResult ?? string.Empty,
                        At = audit.ActiveDateTime,
                        Payload = audit.ActionData ?? "{}"
                    });
                }
            }

            return new EdgeTaskTimelineDto
            {
                DeviceId = task.GatewayId,
                DeviceName = deviceNames.TryGetValue(task.GatewayId, out var deviceName) ? deviceName : task.GatewayId.ToString(),
                TaskId = task.Id,
                RuntimeType = task.RuntimeType ?? string.Empty,
                InstanceId = task.InstanceId ?? string.Empty,
                CurrentStatus = task.Status.ToString(),
                LastUpdatedAt = task.UpdatedAt,
                Events = events.OrderBy(item => item.At).ToList()
            };
        }

        /// <summary>
        /// 根据执行端回执创建正式 EdgeTaskReceipt 历史记录。
        /// </summary>
        /// <param name="task">对应的正式任务模型。</param>
        /// <param name="receipt">执行端回执。</param>
        /// <param name="receiptPayload">规范化后的回执 JSON。</param>
        /// <param name="receivedAt">平台接收时间。</param>
        /// <returns>已初始化但尚未保存的回执实体。</returns>
        private static EdgeTaskReceipt CreateFormalEdgeTaskReceipt(
            EdgeTask task,
            EdgeTaskReceiptDto receipt,
            string receiptPayload,
            DateTime receivedAt)
        {
            var reportedAt = receipt.ReportedAt == default ? receivedAt : receipt.ReportedAt.ToUniversalTime();

            return new EdgeTaskReceipt
            {
                Id = Guid.NewGuid(),
                TaskId = task.Id,
                ContractVersion = string.IsNullOrWhiteSpace(receipt.ContractVersion) ? TaskContractVersion : receipt.ContractVersion,
                TargetType = task.TargetType,
                GatewayId = task.GatewayId,
                EdgeNodeId = task.EdgeNodeId,
                TargetKey = string.IsNullOrWhiteSpace(receipt.TargetKey) ? task.TargetKey ?? task.GatewayId.ToString() : receipt.TargetKey,
                RuntimeType = string.IsNullOrWhiteSpace(receipt.RuntimeType) ? task.RuntimeType ?? string.Empty : receipt.RuntimeType,
                InstanceId = string.IsNullOrWhiteSpace(receipt.InstanceId) ? task.InstanceId ?? string.Empty : receipt.InstanceId,
                Status = receipt.Status,
                Message = receipt.Message ?? string.Empty,
                Progress = receipt.Progress,
                Result = SerializeOrNull(receipt.Result) ?? "{}",
                Metadata = SerializeOrNull(receipt.Metadata) ?? "{}",
                Payload = receiptPayload ?? "{}",
                ReportedAt = reportedAt,
                ReceivedAt = receivedAt == default ? DateTime.UtcNow : receivedAt.ToUniversalTime(),
                TenantId = task.TenantId,
                CustomerId = task.CustomerId
            };
        }

        /// <summary>
        /// 将正式回执实体转换为云边合同 DTO。
        /// </summary>
        /// <param name="receipt">正式回执实体。</param>
        /// <returns>用于 API 输出和兼容历史视图的回执 DTO。</returns>
        private static EdgeTaskReceiptDto ToEdgeTaskReceiptDto(EdgeTaskReceipt receipt)
        {
            return new EdgeTaskReceiptDto
            {
                ContractVersion = string.IsNullOrWhiteSpace(receipt.ContractVersion) ? TaskContractVersion : receipt.ContractVersion,
                TaskId = receipt.TaskId,
                TargetType = receipt.TargetType,
                TargetKey = receipt.TargetKey ?? string.Empty,
                RuntimeType = receipt.RuntimeType ?? string.Empty,
                InstanceId = receipt.InstanceId ?? string.Empty,
                Status = receipt.Status,
                Message = receipt.Message ?? string.Empty,
                ReportedAt = receipt.ReportedAt,
                Progress = receipt.Progress,
                Result = DeserializeObjectDictionary(receipt.Result),
                Metadata = DeserializeStringDictionary(receipt.Metadata)
            };
        }

        /// <summary>
        /// 将执行端回执合并到正式 EdgeTask 当前态。
        /// </summary>
        /// <param name="task">正式任务模型。</param>
        /// <param name="receipt">执行端回执。</param>
        private static void ApplyFormalReceipt(EdgeTask task, EdgeTaskReceiptDto receipt)
        {
            ApplyFormalStatus(task, receipt.Status, receipt.ReportedAt.ToUniversalTime(), receipt.Message, receipt.Progress, SerializeOrNull(receipt) ?? "{}");
        }

        /// <summary>
        /// 更新正式任务状态和关键时间点。
        /// </summary>
        /// <param name="task">正式任务模型。</param>
        /// <param name="status">新状态。</param>
        /// <param name="at">状态时间。</param>
        /// <param name="message">状态说明。</param>
        /// <param name="progress">进度。</param>
        /// <param name="receiptPayload">最近回执 JSON。</param>
        private static void ApplyFormalStatus(EdgeTask task, EdgeTaskStatus status, DateTime at, string message, int? progress, string receiptPayload)
        {
            var utc = at == default ? DateTime.UtcNow : at.ToUniversalTime();
            task.Status = status;
            task.UpdatedAt = utc;

            if (message != null)
            {
                task.Message = message;
            }

            if (progress != null)
            {
                task.Progress = progress;
            }

            if (!string.IsNullOrWhiteSpace(receiptPayload))
            {
                task.LastReceiptPayload = receiptPayload;
                task.LastReceiptAt = utc;
            }

            switch (status)
            {
                case EdgeTaskStatus.Sent:
                    task.SentAt ??= utc;
                    break;
                case EdgeTaskStatus.Accepted:
                    task.AcceptedAt ??= utc;
                    break;
                case EdgeTaskStatus.Running:
                    task.StartedAt ??= utc;
                    break;
                case EdgeTaskStatus.Succeeded:
                case EdgeTaskStatus.Failed:
                case EdgeTaskStatus.TimedOut:
                case EdgeTaskStatus.Cancelled:
                    task.CompletedAt ??= utc;
                    break;
            }
        }

        /// <summary>
        /// 统计某个失败任务已经发起过的重试次数。
        /// </summary>
        /// <param name="taskId">原任务 ID。</param>
        /// <returns>已记录的重试审计次数。</returns>
        private async System.Threading.Tasks.Task<int> CountRetryAttemptsAsync(Guid taskId)
        {
            return await _context.AuditLog.CountAsync(log => log.ObjectID == taskId
                && log.ObjectType == ObjectType.EdgeTask
                && log.ActionName == AuditActionRetry);
        }

        /// <summary>
        /// 记录用户发起的 EdgeTask 审计动作。
        /// </summary>
        /// <param name="profile">当前登录用户。</param>
        /// <param name="task">被操作任务。</param>
        /// <param name="actionName">动作名称。</param>
        /// <param name="actionData">动作数据。</param>
        /// <param name="actionResult">动作结果。</param>
        /// <param name="activeAt">动作发生时间。</param>
        private void AddUserEdgeTaskAudit(
            UserProfile profile,
            EdgeTask task,
            string actionName,
            object actionData,
            string actionResult,
            DateTime activeAt)
        {
            AddEdgeTaskAudit(
                task,
                profile?.Id.ToString("D") ?? string.Empty,
                ResolveUserName(profile),
                actionName,
                actionData,
                actionResult,
                activeAt);
        }

        /// <summary>
        /// 记录执行端上报的 EdgeTask 终态审计动作。
        /// </summary>
        /// <param name="task">被操作任务。</param>
        /// <param name="receipt">执行端回执。</param>
        /// <param name="actionName">动作名称。</param>
        /// <param name="actionData">动作数据。</param>
        /// <param name="actionResult">动作结果。</param>
        private void AddRuntimeEdgeTaskAudit(
            EdgeTask task,
            EdgeTaskReceiptDto receipt,
            string actionName,
            object actionData,
            string actionResult)
        {
            var runtimeName = $"edge-runtime:{(string.IsNullOrWhiteSpace(receipt.TargetKey) ? task.TargetKey : receipt.TargetKey)}";
            AddEdgeTaskAudit(
                task,
                task.GatewayId.ToString("D"),
                runtimeName,
                actionName,
                actionData,
                actionResult,
                receipt.ReportedAt == default ? DateTime.UtcNow : receipt.ReportedAt);
        }

        /// <summary>
        /// 写入 EdgeTask 审计实体。
        /// </summary>
        /// <param name="task">被操作任务。</param>
        /// <param name="userId">操作者 ID。</param>
        /// <param name="userName">操作者显示名。</param>
        /// <param name="actionName">动作名称。</param>
        /// <param name="actionData">动作数据。</param>
        /// <param name="actionResult">动作结果。</param>
        /// <param name="activeAt">动作发生时间。</param>
        private void AddEdgeTaskAudit(
            EdgeTask task,
            string userId,
            string userName,
            string actionName,
            object actionData,
            string actionResult,
            DateTime activeAt)
        {
            var utc = activeAt == default ? DateTime.UtcNow : activeAt.ToUniversalTime();
            _context.AuditLog.Add(new AuditLog
            {
                TenantId = task.TenantId,
                CustomerId = task.CustomerId,
                UserId = userId ?? string.Empty,
                UserName = userName ?? string.Empty,
                ObjectID = task.Id,
                ObjectName = $"{task.TaskType}:{task.TargetKey}",
                ObjectType = ObjectType.EdgeTask,
                ActionName = actionName ?? string.Empty,
                ActionData = SerializeOrNull(actionData) ?? "{}",
                ActionResult = actionResult ?? string.Empty,
                ActiveDateTime = utc
            });
        }

        /// <summary>
        /// 判断状态是否为可以人工重试的失败态。
        /// </summary>
        /// <param name="status">任务状态。</param>
        /// <returns>失败或超时时返回 true。</returns>
        private static bool IsRetryableFailureStatus(EdgeTaskStatus status)
        {
            return status is EdgeTaskStatus.Failed or EdgeTaskStatus.TimedOut;
        }

        /// <summary>
        /// 判断任务状态是否为终态。
        /// </summary>
        /// <param name="status">任务状态。</param>
        /// <returns>终态返回 true。</returns>
        private static bool IsTerminalStatus(EdgeTaskStatus status)
        {
            return EdgeTaskStateMachine.TerminalStates.Contains(status);
        }

        /// <summary>
        /// 在执行端拉取但尚未回执时，同步采集配置分配的发布任务状态，供管理端展示最近发布进展。
        /// </summary>
        /// <param name="task">正式 EdgeTask 任务。</param>
        /// <param name="status">需要同步的任务状态。</param>
        /// <param name="reportedAt">状态发生时间。</param>
        /// <param name="message">管理端可读状态说明。</param>
        /// <param name="progress">任务进度；未上报时为空。</param>
        private async System.Threading.Tasks.Task ApplyCollectionAssignmentTaskStatusAsync(
            EdgeTask task,
            EdgeTaskStatus status,
            DateTime reportedAt,
            string message,
            int? progress)
        {
            if (task.TaskType != EdgeTaskType.ConfigPullRequest)
            {
                return;
            }

            var parameters = DeserializeObjectDictionary(task.Parameters);
            var configurationVersionId = TryGetGuid(parameters, ConfigurationVersionIdKey);
            var configurationVersion = TryGetInt(parameters, ConfigurationVersionKey);
            var configurationHash = TryGetString(parameters, ConfigurationHashKey);

            if (configurationVersion is not > 0 || string.IsNullOrWhiteSpace(configurationHash))
            {
                return;
            }

            var assignment = await FindCollectionAssignmentForTaskAsync(
                task.GatewayId,
                configurationVersionId,
                configurationVersion.Value,
                configurationHash);

            if (assignment == null)
            {
                return;
            }

            var utc = reportedAt == default ? DateTime.UtcNow : reportedAt.ToUniversalTime();
            assignment.LastExecutionTaskId = task.Id;
            assignment.LastExecutionStatus = status;
            assignment.LastExecutionMessage = message ?? string.Empty;
            assignment.LastExecutionProgress = progress;
            assignment.LastExecutionAt = utc;
            assignment.UpdatedAt = utc;
            assignment.UpdatedBy = AssignmentTaskStatusUpdatedBy;
        }

        /// <summary>
        /// 处理采集配置任务的执行回执，核对配置版本和哈希后反写目标分配的执行态。
        /// </summary>
        /// <param name="task">正式 EdgeTask 任务。</param>
        /// <param name="receipt">执行端回执。</param>
        /// <returns>校验失败时返回错误消息；成功或非配置任务返回空字符串。</returns>
        private async System.Threading.Tasks.Task<string> ApplyCollectionConfigurationReceiptAsync(EdgeTask task, EdgeTaskReceiptDto receipt)
        {
            if (task.TaskType != EdgeTaskType.ConfigPullRequest)
            {
                return string.Empty;
            }

            var parameters = DeserializeObjectDictionary(task.Parameters);
            var expectedVersionId = TryGetGuid(parameters, ConfigurationVersionIdKey);
            var expectedVersion = TryGetInt(parameters, ConfigurationVersionKey);
            var expectedHash = TryGetString(parameters, ConfigurationHashKey);

            if (expectedVersion is not > 0 || string.IsNullOrWhiteSpace(expectedHash))
            {
                return "ConfigPullRequest task parameters require configurationVersion and configurationHash";
            }

            var resultVersionId = TryGetGuid(receipt.Result, ConfigurationVersionIdKey);
            var metadataVersionId = TryGetGuid(receipt.Metadata, ConfigurationVersionIdKey);
            var resultVersion = TryGetInt(receipt.Result, ConfigurationVersionKey);
            var metadataVersion = TryGetInt(receipt.Metadata, ConfigurationVersionKey);
            var resultHash = TryGetString(receipt.Result, ConfigurationHashKey);
            var metadataHash = TryGetString(receipt.Metadata, ConfigurationHashKey);

            if ((resultVersionId.HasValue && expectedVersionId.HasValue && resultVersionId.Value != expectedVersionId.Value) ||
                (metadataVersionId.HasValue && expectedVersionId.HasValue && metadataVersionId.Value != expectedVersionId.Value))
            {
                return "configurationVersionId does not match task parameters";
            }

            if ((resultVersion.HasValue && resultVersion.Value != expectedVersion.Value) ||
                (metadataVersion.HasValue && metadataVersion.Value != expectedVersion.Value))
            {
                return "configurationVersion does not match task parameters";
            }

            if ((!string.IsNullOrWhiteSpace(resultHash) && !string.Equals(resultHash, expectedHash, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(metadataHash) && !string.Equals(metadataHash, expectedHash, StringComparison.OrdinalIgnoreCase)))
            {
                return "configurationHash does not match task parameters";
            }

            if (receipt.Status == EdgeTaskStatus.Succeeded &&
                (resultVersion is not > 0 || string.IsNullOrWhiteSpace(resultHash)))
            {
                return "Succeeded configuration receipt requires result.configurationVersion and result.configurationHash";
            }

            var assignment = await FindCollectionAssignmentForTaskAsync(task.GatewayId, expectedVersionId, expectedVersion.Value, expectedHash);
            if (assignment == null)
            {
                return "Collection configuration assignment not found for task receipt";
            }

            ApplyCollectionAssignmentReceipt(assignment, task, receipt, expectedVersion.Value, expectedHash);
            return string.Empty;
        }

        /// <summary>
        /// 按任务参数定位本次配置发布对应的分配记录。
        /// </summary>
        /// <param name="gatewayId">承载任务通道的 Gateway 设备 ID。</param>
        /// <param name="configurationVersionId">配置版本快照 ID。</param>
        /// <param name="configurationVersion">配置版本号。</param>
        /// <param name="configurationHash">配置哈希。</param>
        /// <returns>匹配的配置分配；找不到时返回 null。</returns>
        private async System.Threading.Tasks.Task<EdgeCollectionAssignment> FindCollectionAssignmentForTaskAsync(
            Guid gatewayId,
            Guid? configurationVersionId,
            int configurationVersion,
            string configurationHash)
        {
            if (configurationVersionId.HasValue)
            {
                var byVersionId = await _context.EdgeCollectionAssignments
                    .Where(assignment => assignment.GatewayId == gatewayId
                        && assignment.CollectionConfigurationVersionId == configurationVersionId.Value
                        && !assignment.Deleted)
                    .OrderByDescending(assignment => assignment.Status == EdgeCollectionAssignmentStatus.Active)
                    .ThenByDescending(assignment => assignment.AssignedAt)
                    .FirstOrDefaultAsync();

                if (byVersionId != null)
                {
                    return byVersionId;
                }
            }

            return await _context.EdgeCollectionAssignments
                .Where(assignment => assignment.GatewayId == gatewayId
                    && assignment.ConfigurationVersion == configurationVersion
                    && assignment.ConfigurationHash == configurationHash
                    && !assignment.Deleted)
                .OrderByDescending(assignment => assignment.Status == EdgeCollectionAssignmentStatus.Active)
                .ThenByDescending(assignment => assignment.AssignedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 将配置执行回执合并到分配记录，成功态同时更新已应用版本。
        /// </summary>
        /// <param name="assignment">配置分配记录。</param>
        /// <param name="task">对应的正式任务。</param>
        /// <param name="receipt">执行端回执。</param>
        /// <param name="configurationVersion">已核对的配置版本。</param>
        /// <param name="configurationHash">已核对的配置哈希。</param>
        private static void ApplyCollectionAssignmentReceipt(
            EdgeCollectionAssignment assignment,
            EdgeTask task,
            EdgeTaskReceiptDto receipt,
            int configurationVersion,
            string configurationHash)
        {
            var reportedAt = receipt.ReportedAt == default ? DateTime.UtcNow : receipt.ReportedAt.ToUniversalTime();
            assignment.LastExecutionTaskId = task.Id;
            assignment.LastExecutionStatus = receipt.Status;
            assignment.LastExecutionMessage = receipt.Message ?? string.Empty;
            assignment.LastExecutionProgress = receipt.Progress;
            assignment.LastExecutionAt = reportedAt;
            assignment.UpdatedAt = reportedAt;
            assignment.UpdatedBy = AssignmentUpdatedBy;

            if (receipt.Status == EdgeTaskStatus.Succeeded)
            {
                assignment.AppliedConfigurationVersion = configurationVersion;
                assignment.AppliedConfigurationHash = configurationHash;
                assignment.AppliedAt = reportedAt;
            }
        }

        /// <summary>
        /// 将 EdgeTask 状态同步到绑定的 ReleaseTask，用于执行端拉取任务等非回执事件。
        /// </summary>
        /// <param name="task">正式 EdgeTask 任务。</param>
        /// <param name="status">EdgeTask 状态。</param>
        /// <param name="updatedAt">状态更新时间。</param>
        /// <param name="message">状态说明。</param>
        /// <param name="progress">执行进度。</param>
        private async System.Threading.Tasks.Task ApplyReleaseTaskStatusAsync(
            EdgeTask task,
            EdgeTaskStatus status,
            DateTime updatedAt,
            string message,
            int? progress)
        {
            var releaseTask = await _context.ReleaseTasks
                .FirstOrDefaultAsync(item => item.EdgeTaskId == task.Id && !item.Deleted);
            if (releaseTask == null)
            {
                return;
            }

            ApplyReleaseTaskStatus(releaseTask, status, updatedAt, message, progress);
            await RefreshReleasePlanSummaryAsync(releaseTask.PlanId, updatedAt);
        }

        /// <summary>
        /// 将 EdgeTask 正式回执投影为 ReleaseReceipt，并同步 ReleaseTask/ReleasePlan 当前态。
        /// </summary>
        /// <param name="task">正式 EdgeTask 任务。</param>
        /// <param name="receipt">执行端回执。</param>
        /// <param name="receiptPayload">原始回执 JSON。</param>
        /// <param name="receivedAt">平台接收时间。</param>
        private async System.Threading.Tasks.Task ApplyReleaseTaskReceiptAsync(
            EdgeTask task,
            EdgeTaskReceiptDto receipt,
            string receiptPayload,
            DateTime receivedAt)
        {
            var releaseTask = await _context.ReleaseTasks
                .FirstOrDefaultAsync(item => item.EdgeTaskId == task.Id && !item.Deleted);
            if (releaseTask == null)
            {
                return;
            }

            ApplyReleaseTaskStatus(releaseTask, receipt.Status, receipt.ReportedAt, receipt.Message, receipt.Progress);
            _context.ReleaseReceipts.Add(new ReleaseReceipt
            {
                Id = Guid.NewGuid(),
                PlanId = releaseTask.PlanId,
                ReleaseTaskId = releaseTask.Id,
                EdgeTaskId = task.Id,
                TargetType = releaseTask.TargetType,
                TargetId = releaseTask.TargetId,
                GatewayId = releaseTask.GatewayId,
                EdgeNodeId = releaseTask.EdgeNodeId,
                TargetKey = releaseTask.TargetKey ?? task.TargetKey ?? string.Empty,
                RuntimeType = releaseTask.RuntimeType ?? task.RuntimeType ?? string.Empty,
                InstanceId = releaseTask.InstanceId ?? task.InstanceId ?? string.Empty,
                Status = MapReleaseTaskStatus(receipt.Status, releaseTask.IsRollback),
                Message = receipt.Message ?? string.Empty,
                Progress = receipt.Progress,
                Result = SerializeOrNull(receipt.Result) ?? "{}",
                Metadata = SerializeOrNull(receipt.Metadata) ?? "{}",
                Payload = receiptPayload ?? "{}",
                ReportedAt = receipt.ReportedAt == default ? receivedAt : receipt.ReportedAt.ToUniversalTime(),
                ReceivedAt = receivedAt,
                TenantId = task.TenantId,
                CustomerId = task.CustomerId
            });

            await RefreshReleasePlanSummaryAsync(releaseTask.PlanId, receivedAt);
        }

        /// <summary>
        /// 应用单个发布任务状态，保留回滚任务的特殊成功/失败语义。
        /// </summary>
        /// <param name="releaseTask">发布任务。</param>
        /// <param name="status">EdgeTask 状态。</param>
        /// <param name="updatedAt">状态更新时间。</param>
        /// <param name="message">状态说明。</param>
        /// <param name="progress">执行进度。</param>
        private static void ApplyReleaseTaskStatus(
            ReleaseTask releaseTask,
            EdgeTaskStatus status,
            DateTime updatedAt,
            string message,
            int? progress)
        {
            var normalizedAt = updatedAt == default ? DateTime.UtcNow : updatedAt.ToUniversalTime();
            var releaseStatus = MapReleaseTaskStatus(status, releaseTask.IsRollback);
            releaseTask.Status = releaseStatus;
            releaseTask.Message = message ?? string.Empty;
            releaseTask.Progress = progress;
            releaseTask.UpdatedAt = normalizedAt;

            if (status != EdgeTaskStatus.Sent)
            {
                releaseTask.LastReceiptAt = normalizedAt;
            }

            if (IsTerminalReleaseTaskStatus(releaseStatus))
            {
                releaseTask.CompletedAt = normalizedAt;
            }
        }

        /// <summary>
        /// 根据发布任务集合刷新发布计划的聚合状态。
        /// </summary>
        /// <param name="planId">发布计划 ID。</param>
        /// <param name="updatedAt">更新时间。</param>
        private async System.Threading.Tasks.Task RefreshReleasePlanSummaryAsync(Guid planId, DateTime updatedAt)
        {
            var plan = await _context.ReleasePlans.FirstOrDefaultAsync(item => item.Id == planId && !item.Deleted);
            if (plan == null)
            {
                return;
            }

            var tasks = await _context.ReleaseTasks
                .Where(item => item.PlanId == planId && !item.Deleted)
                .ToListAsync();
            var normalizedAt = updatedAt == default ? DateTime.UtcNow : updatedAt.ToUniversalTime();

            plan.TotalTaskCount = tasks.Count;
            plan.PendingTaskCount = tasks.Count(item => item.Status == ReleaseTaskStatus.Pending);
            plan.RunningTaskCount = tasks.Count(IsActiveReleaseTaskStatus);
            plan.SucceededTaskCount = tasks.Count(item => item.Status is ReleaseTaskStatus.Succeeded or ReleaseTaskStatus.RolledBack);
            plan.FailedTaskCount = tasks.Count(IsFailedReleaseTaskStatus);
            plan.CurrentBatchNo = tasks.Count == 0 ? plan.CurrentBatchNo : Math.Max(plan.CurrentBatchNo, tasks.Max(item => item.BatchNo));
            plan.UpdatedAt = normalizedAt;
            plan.UpdatedBy = "edge-task-receipt";

            var rollbackTasks = tasks.Where(item => item.IsRollback).ToList();
            if (plan.Status == ReleasePlanStatus.RollingBack || rollbackTasks.Count > 0)
            {
                if (rollbackTasks.Count > 0 && rollbackTasks.All(item => IsTerminalReleaseTaskStatus(item.Status)))
                {
                    plan.Status = rollbackTasks.Any(IsFailedReleaseTaskStatus)
                        ? ReleasePlanStatus.RollbackFailed
                        : ReleasePlanStatus.RolledBack;
                    plan.CompletedAt = normalizedAt;
                }
                else
                {
                    plan.Status = ReleasePlanStatus.RollingBack;
                }

                return;
            }

            var activeOrDispatched = tasks.Any(item => IsActiveReleaseTaskStatus(item) || item is { Status: ReleaseTaskStatus.Pending, EdgeTaskId: not null });
            var pendingWithoutEdgeTask = tasks.Any(item => item.Status == ReleaseTaskStatus.Pending && item.EdgeTaskId == null);
            var failed = tasks.Any(IsFailedReleaseTaskStatus);
            var succeeded = tasks.Any(item => item.Status == ReleaseTaskStatus.Succeeded);

            if (tasks.Count > 0 && tasks.All(item => item.Status == ReleaseTaskStatus.Succeeded))
            {
                plan.Status = ReleasePlanStatus.Succeeded;
                plan.CompletedAt = normalizedAt;
            }
            else if (activeOrDispatched)
            {
                plan.Status = plan.Status == ReleasePlanStatus.Paused ? ReleasePlanStatus.Paused : ReleasePlanStatus.Running;
            }
            else if (failed && !pendingWithoutEdgeTask)
            {
                plan.Status = succeeded ? ReleasePlanStatus.PartiallySucceeded : ReleasePlanStatus.Failed;
                plan.CompletedAt = normalizedAt;
            }
            else if (pendingWithoutEdgeTask && plan.Status != ReleasePlanStatus.Paused)
            {
                plan.Status = plan.StartedAt == null ? ReleasePlanStatus.Draft : ReleasePlanStatus.WaitingConfirmation;
            }
        }

        private static ReleaseTaskStatus MapReleaseTaskStatus(EdgeTaskStatus status, bool isRollback)
        {
            return status switch
            {
                EdgeTaskStatus.Pending => ReleaseTaskStatus.Pending,
                EdgeTaskStatus.Sent => ReleaseTaskStatus.Sent,
                EdgeTaskStatus.Accepted => ReleaseTaskStatus.Accepted,
                EdgeTaskStatus.Running => ReleaseTaskStatus.Running,
                EdgeTaskStatus.Succeeded => isRollback ? ReleaseTaskStatus.RolledBack : ReleaseTaskStatus.Succeeded,
                EdgeTaskStatus.Failed => isRollback ? ReleaseTaskStatus.RollbackFailed : ReleaseTaskStatus.Failed,
                EdgeTaskStatus.TimedOut => ReleaseTaskStatus.TimedOut,
                EdgeTaskStatus.Cancelled => ReleaseTaskStatus.Cancelled,
                _ => ReleaseTaskStatus.Failed
            };
        }

        private static bool IsActiveReleaseTaskStatus(ReleaseTask task)
            => task.Status is ReleaseTaskStatus.Sent or ReleaseTaskStatus.Accepted or ReleaseTaskStatus.Running;

        private static bool IsFailedReleaseTaskStatus(ReleaseTask task)
            => IsFailedReleaseTaskStatus(task.Status);

        private static bool IsFailedReleaseTaskStatus(ReleaseTaskStatus status)
            => status is ReleaseTaskStatus.Failed or ReleaseTaskStatus.TimedOut or ReleaseTaskStatus.Cancelled or ReleaseTaskStatus.RollbackFailed;

        private static bool IsTerminalReleaseTaskStatus(ReleaseTaskStatus status)
            => status is ReleaseTaskStatus.Succeeded or ReleaseTaskStatus.Failed or ReleaseTaskStatus.TimedOut or ReleaseTaskStatus.Cancelled or ReleaseTaskStatus.RolledBack or ReleaseTaskStatus.RollbackFailed;

        /// <summary>
        /// 处理发布包任务的执行回执，核对包 ID、版本和 SHA256，避免执行端误报其他包的成功结果。
        /// </summary>
        /// <param name="task">正式 EdgeTask 任务。</param>
        /// <param name="receipt">执行端回执。</param>
        /// <returns>校验失败时返回错误消息；成功或非发布包任务返回空字符串。</returns>
        private async System.Threading.Tasks.Task<string> ApplyReleasePackageReceiptAsync(EdgeTask task, EdgeTaskReceiptDto receipt)
        {
            if (!IsReleasePackageTask(task.TaskType))
            {
                return string.Empty;
            }

            var parameters = DeserializeObjectDictionary(task.Parameters);
            var expectedPackageId = TryGetGuid(parameters, ReleasePackageIdKey);
            var expectedVersion = TryGetString(parameters, ReleasePackageVersionKey);
            var expectedSha256 = TryGetString(parameters, ReleasePackageSha256Key);

            if (!expectedPackageId.HasValue ||
                string.IsNullOrWhiteSpace(expectedVersion) ||
                string.IsNullOrWhiteSpace(expectedSha256))
            {
                return $"{task.TaskType} task parameters require packageId, packageVersion and sha256";
            }

            var packageExists = await _context.ReleasePackages.AnyAsync(package =>
                package.Id == expectedPackageId.Value &&
                !package.Deleted &&
                package.TenantId == task.TenantId &&
                package.CustomerId == task.CustomerId);
            if (!packageExists)
            {
                return "Release package not found for task receipt";
            }

            var resultPackageId = TryGetGuid(receipt.Result, ReleasePackageIdKey);
            var metadataPackageId = TryGetGuid(receipt.Metadata, ReleasePackageIdKey);
            var resultVersion = TryGetString(receipt.Result, ReleasePackageVersionKey);
            var metadataVersion = TryGetString(receipt.Metadata, ReleasePackageVersionKey);
            var resultSha256 = TryGetReleasePackageSha256(receipt.Result);
            var metadataSha256 = TryGetReleasePackageSha256(receipt.Metadata);
            var expectedDeviceId = TryGetGuid(parameters, TargetDeviceIdKey) ?? TryGetGuid(parameters, DeviceIdKey);
            var resultDeviceId = TryGetGuid(receipt.Result, TargetDeviceIdKey) ?? TryGetGuid(receipt.Result, DeviceIdKey);
            var metadataDeviceId = TryGetGuid(receipt.Metadata, TargetDeviceIdKey) ?? TryGetGuid(receipt.Metadata, DeviceIdKey);
            var expectedScriptCrc32 = TryGetString(parameters, ScriptCrc32Key);
            var resultScriptCrc32 = TryGetString(receipt.Result, ScriptCrc32Key);
            var metadataScriptCrc32 = TryGetString(receipt.Metadata, ScriptCrc32Key);

            if ((resultPackageId.HasValue && resultPackageId.Value != expectedPackageId.Value) ||
                (metadataPackageId.HasValue && metadataPackageId.Value != expectedPackageId.Value))
            {
                return "packageId does not match task parameters";
            }

            if ((!string.IsNullOrWhiteSpace(resultVersion) && !string.Equals(resultVersion, expectedVersion, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(metadataVersion) && !string.Equals(metadataVersion, expectedVersion, StringComparison.OrdinalIgnoreCase)))
            {
                return "packageVersion does not match task parameters";
            }

            if ((!string.IsNullOrWhiteSpace(resultSha256) && !string.Equals(resultSha256, expectedSha256, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(metadataSha256) && !string.Equals(metadataSha256, expectedSha256, StringComparison.OrdinalIgnoreCase)))
            {
                return "sha256 does not match task parameters";
            }

            if ((resultDeviceId.HasValue && expectedDeviceId.HasValue && resultDeviceId.Value != expectedDeviceId.Value) ||
                (metadataDeviceId.HasValue && expectedDeviceId.HasValue && metadataDeviceId.Value != expectedDeviceId.Value))
            {
                return "deviceId does not match task parameters";
            }

            if (!string.IsNullOrWhiteSpace(expectedScriptCrc32) &&
                ((!string.IsNullOrWhiteSpace(resultScriptCrc32) && !string.Equals(resultScriptCrc32, expectedScriptCrc32, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrWhiteSpace(metadataScriptCrc32) && !string.Equals(metadataScriptCrc32, expectedScriptCrc32, StringComparison.OrdinalIgnoreCase))))
            {
                return "scriptCrc32 does not match task parameters";
            }

            if (receipt.Status == EdgeTaskStatus.Succeeded &&
                (!resultPackageId.HasValue ||
                 string.IsNullOrWhiteSpace(resultVersion) ||
                 string.IsNullOrWhiteSpace(resultSha256)))
            {
                return $"Succeeded {task.TaskType} receipt requires result.packageId, result.packageVersion and result.sha256";
            }

            if (receipt.Status == EdgeTaskStatus.Succeeded &&
                task.TaskType is EdgeTaskType.DeviceScriptOta or EdgeTaskType.FirmwareOta &&
                expectedDeviceId.HasValue &&
                !resultDeviceId.HasValue)
            {
                return $"Succeeded {task.TaskType} receipt requires result.deviceId or result.targetDeviceId";
            }

            return string.Empty;
        }

        private static bool IsReleasePackageTask(EdgeTaskType taskType)
            => taskType is EdgeTaskType.SoftwareUpdate or EdgeTaskType.DeviceScriptOta or EdgeTaskType.FirmwareOta;

        private async System.Threading.Tasks.Task<Guid?> ResolveDeviceIdAsync(string targetKey, string instanceId, Guid? deviceId)
        {
            if (deviceId.HasValue && deviceId.Value != Guid.Empty)
            {
                return deviceId.Value;
            }

            if (!string.IsNullOrWhiteSpace(targetKey))
            {
                var parts = targetKey.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length > 0 && Guid.TryParse(parts[0], out var parsedDeviceId))
                {
                    return parsedDeviceId;
                }
            }

            if (!string.IsNullOrWhiteSpace(instanceId))
            {
                var matchedDeviceId = await _context.EdgeNodes
                    .Where(node => !node.Deleted && node.InstanceId == instanceId)
                    .Select(node => (Guid?)node.GatewayId)
                    .FirstOrDefaultAsync();

                if (matchedDeviceId != null)
                {
                    return matchedDeviceId;
                }
            }

            return null;
        }

        /// <summary>
        /// 判断边缘任务状态是否符合合同定义的单向流转；重复上报同一状态视为幂等。
        /// </summary>
        /// <param name="current">当前已记录状态。</param>
        /// <param name="next">运行时准备上报的新状态。</param>
        /// <returns>允许流转时为 true。</returns>
        private static bool IsTransitionAllowed(EdgeTaskStatus current, EdgeTaskStatus next)
        {
            if (current == next)
            {
                return true;
            }

            return EdgeTaskStateMachine.IsTransitionAllowed(current, next);
        }

        private Device GetGatewayByAccessToken(string accessToken)
        {
            var (ok, gateway) = _context.GetDeviceByToken(accessToken);
            if (ok || gateway == null || gateway.Deleted)
            {
                return null;
            }

            return gateway;
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
        /// 获取审计中使用的操作者显示名。
        /// </summary>
        /// <param name="profile">当前登录用户。</param>
        /// <returns>优先返回姓名，其次返回邮箱或用户 ID。</returns>
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

        private static EdgeTaskRequestDto DeserializeEdgeTaskRequest(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<EdgeTaskRequestDto>(payload, ContractJsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static Dictionary<string, object> DeserializeObjectDictionary(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(payload, ContractJsonOptions) ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static Dictionary<string, string> DeserializeStringDictionary(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(payload, ContractJsonOptions) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// 从对象字典中读取字符串值，按键名忽略大小写。
        /// </summary>
        /// <param name="values">待读取的对象字典。</param>
        /// <param name="key">键名。</param>
        /// <returns>找到时返回字符串值。</returns>
        private static string TryGetString(IReadOnlyDictionary<string, object> values, string key)
        {
            if (!TryGetDictionaryValue(values, key, out var value) || value == null)
            {
                return null;
            }

            return value switch
            {
                string text => text,
                Guid id => id.ToString("D"),
                JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
                JsonElement { ValueKind: JsonValueKind.Number } element => element.GetRawText(),
                JsonElement { ValueKind: JsonValueKind.True } => bool.TrueString,
                JsonElement { ValueKind: JsonValueKind.False } => bool.FalseString,
                _ => Convert.ToString(value)
            };
        }

        /// <summary>
        /// 从字符串字典中读取字符串值，按键名忽略大小写。
        /// </summary>
        /// <param name="values">待读取的字符串字典。</param>
        /// <param name="key">键名。</param>
        /// <returns>找到时返回字符串值。</returns>
        private static string TryGetString(IReadOnlyDictionary<string, string> values, string key)
        {
            return TryGetDictionaryValue(values, key, out var value) ? value : null;
        }

        /// <summary>
        /// 从对象字典中读取 Guid 值，按键名忽略大小写。
        /// </summary>
        /// <param name="values">待读取的对象字典。</param>
        /// <param name="key">键名。</param>
        /// <returns>找到并解析成功时返回 Guid。</returns>
        private static Guid? TryGetGuid(IReadOnlyDictionary<string, object> values, string key)
        {
            if (!TryGetDictionaryValue(values, key, out var value) || value == null)
            {
                return null;
            }

            if (value is Guid id)
            {
                return id;
            }

            if (value is JsonElement element)
            {
                return element.ValueKind == JsonValueKind.String && Guid.TryParse(element.GetString(), out var parsed)
                    ? parsed
                    : null;
            }

            return Guid.TryParse(Convert.ToString(value), out var result) ? result : null;
        }

        /// <summary>
        /// 从字符串字典中读取 Guid 值，按键名忽略大小写。
        /// </summary>
        /// <param name="values">待读取的字符串字典。</param>
        /// <param name="key">键名。</param>
        /// <returns>找到并解析成功时返回 Guid。</returns>
        private static Guid? TryGetGuid(IReadOnlyDictionary<string, string> values, string key)
        {
            return TryGetDictionaryValue(values, key, out var value) && Guid.TryParse(value, out var result)
                ? result
                : null;
        }

        /// <summary>
        /// 从对象字典中读取整数值，按键名忽略大小写。
        /// </summary>
        /// <param name="values">待读取的对象字典。</param>
        /// <param name="key">键名。</param>
        /// <returns>找到并解析成功时返回整数。</returns>
        private static int? TryGetInt(IReadOnlyDictionary<string, object> values, string key)
        {
            if (!TryGetDictionaryValue(values, key, out var value) || value == null)
            {
                return null;
            }

            if (value is int number)
            {
                return number;
            }

            if (value is long longNumber)
            {
                return longNumber is <= int.MaxValue and >= int.MinValue ? (int)longNumber : null;
            }

            if (value is JsonElement element)
            {
                return element.ValueKind switch
                {
                    JsonValueKind.Number when element.TryGetInt32(out var parsed) => parsed,
                    JsonValueKind.String when int.TryParse(element.GetString(), out var parsed) => parsed,
                    _ => null
                };
            }

            return int.TryParse(Convert.ToString(value), out var result) ? result : null;
        }

        /// <summary>
        /// 从字符串字典中读取整数值，按键名忽略大小写。
        /// </summary>
        /// <param name="values">待读取的字符串字典。</param>
        /// <param name="key">键名。</param>
        /// <returns>找到并解析成功时返回整数。</returns>
        private static int? TryGetInt(IReadOnlyDictionary<string, string> values, string key)
        {
            return TryGetDictionaryValue(values, key, out var value) && int.TryParse(value, out var result)
                ? result
                : null;
        }

        /// <summary>
        /// 读取软件包回执中的 SHA256，兼容 sha256 和 packageSha256 两种键名。
        /// </summary>
        /// <param name="values">待读取的对象字典。</param>
        /// <returns>找到时返回 SHA256 字符串。</returns>
        private static string TryGetReleasePackageSha256(IReadOnlyDictionary<string, object> values)
            => TryGetString(values, ReleasePackageSha256Key) ?? TryGetString(values, "packageSha256");

        /// <summary>
        /// 读取软件包回执中的 SHA256，兼容 sha256 和 packageSha256 两种键名。
        /// </summary>
        /// <param name="values">待读取的字符串字典。</param>
        /// <returns>找到时返回 SHA256 字符串。</returns>
        private static string TryGetReleasePackageSha256(IReadOnlyDictionary<string, string> values)
            => TryGetString(values, ReleasePackageSha256Key) ?? TryGetString(values, "packageSha256");

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

        private sealed class EdgeTaskHistoryRecord
        {
            public string key { get; set; }

            public DateTime at { get; set; }

            public string payload { get; set; }

            public string status { get; set; }
        }
    }
}
