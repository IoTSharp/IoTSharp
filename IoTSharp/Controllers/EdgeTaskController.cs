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

            var receiptPayload = SerializeOrNull(request) ?? "{}";
            ApplyFormalReceipt(formalTask, request);
            _context.EdgeTaskReceipts.Add(CreateFormalEdgeTaskReceipt(formalTask, request, receiptPayload, DateTime.UtcNow));
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

                if (!receiptGroups.TryGetValue(task.Id, out var taskReceipts))
                {
                    continue;
                }

                formalRecords.AddRange(taskReceipts.Select(receipt => new EdgeTaskHistoryRecord
                {
                    key = $"{EdgeTaskHistoryKeyPrefix}{task.Id:N}.receipt",
                    at = receipt.ReportedAt,
                    payload = receipt.Payload ?? SerializeOrNull(ToEdgeTaskReceiptDto(receipt)) ?? "{}",
                    status = receipt.Status.ToString()
                }));
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

            var receiptPayload = SerializeOrNull(request) ?? "{}";
            ApplyFormalReceipt(formalTask, request);
            _context.EdgeTaskReceipts.Add(CreateFormalEdgeTaskReceipt(formalTask, request, receiptPayload, DateTime.UtcNow));
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

            var rows = formalTasks
                .Select(task => ToEdgeTaskTimelineDto(
                    task,
                    deviceNames,
                    receiptsByTask.TryGetValue(task.Id, out var receipts) ? receipts : Array.Empty<EdgeTaskReceipt>()))
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
        /// 将正式任务模型转换为管理端现有 timeline 形状。
        /// </summary>
        /// <param name="task">正式任务模型。</param>
        /// <param name="deviceNames">Gateway 设备名索引。</param>
        /// <returns>任务时间线 DTO。</returns>
        private static EdgeTaskTimelineDto ToEdgeTaskTimelineDto(
            EdgeTask task,
            IReadOnlyDictionary<Guid, string> deviceNames,
            IReadOnlyList<EdgeTaskReceipt> receipts)
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
            if (ok || gateway?.DeviceType != DeviceType.Gateway || gateway.Deleted)
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

        private sealed class EdgeTaskHistoryRecord
        {
            public string key { get; set; }

            public DateTime at { get; set; }

            public string payload { get; set; }

            public string status { get; set; }
        }
    }
}
