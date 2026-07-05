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
using EdgeTaskListItemDto = IoTSharp.Dtos.EdgeTaskListItemDto;
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
        private const string EdgeTaskRequestLastKey = "_edge.task.request.last";
        private const string EdgeTaskRequestStatusKey = "_edge.task.request.status";
        private const string EdgeTaskRequestCreatedAtKey = "_edge.task.request.createdAt";
        private const string EdgeTaskReceiptLastKey = "_edge.task.receipt.last";
        private const string EdgeTaskReceiptStatusKey = "_edge.task.receipt.status";
        private const string EdgeTaskReceiptReportedAtKey = "_edge.task.receipt.reportedAt";
        private const string EdgeTaskHistoryKeyPrefix = "_edge.task.history.";
        private const string EdgeTaskDispatchStatusKey = "_edge.task.dispatch.status";
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

            var latestAttrs = new Dictionary<string, object>
            {
                [EdgeTaskRequestLastKey] = requestPayload,
                [EdgeTaskRequestStatusKey] = EdgeTaskStatus.Pending.ToString(),
                [EdgeTaskRequestCreatedAtKey] = request.CreatedAt.ToUniversalTime()
            };

            await _context.SaveAsync<AttributeLatest>(latestAttrs, device.Id, DataSide.ServerSide);
            await _context.SaveAsync<AttributeLatest>(new Dictionary<string, object>
            {
                [EdgeTaskDispatchStatusKey] = EdgeTaskStatus.Pending.ToString()
            }, device.Id, DataSide.ServerSide);
            await SaveTaskHistoryAsync(device.Id, request.TaskId, "request", request.CreatedAt.ToUniversalTime(), requestPayload, EdgeTaskStatus.Pending.ToString());

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
            var currentStatus = formalTask?.Status ?? await GetLatestTaskStatusAsync(deviceId.Value, request.TaskId);
            if (currentStatus == null)
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, "Task request not found for receipt", null));
            }

            if (!IsTransitionAllowed(currentStatus.Value, request.Status))
            {
                return Ok(new ApiResult<EdgeTaskReceiptDto>(ApiCode.InValidData, $"Invalid edge task transition: {currentStatus.Value} -> {request.Status}", null));
            }

            request.ReportedAt = request.ReportedAt == default ? DateTime.UtcNow : request.ReportedAt.ToUniversalTime();
            if (formalTask != null)
            {
                ApplyFormalReceipt(formalTask, request);
                await _context.SaveChangesAsync();
            }

            var receiptPayload = SerializeOrNull(request) ?? "{}";
            var attrs = new Dictionary<string, object>
            {
                [EdgeTaskReceiptLastKey] = receiptPayload,
                [EdgeTaskReceiptStatusKey] = request.Status.ToString(),
                [EdgeTaskReceiptReportedAtKey] = request.ReportedAt.ToUniversalTime()
            };

            await _context.SaveAsync<AttributeLatest>(attrs, deviceId.Value, DataSide.ServerSide);
            await _context.SaveAsync<AttributeLatest>(new Dictionary<string, object>
            {
                [EdgeTaskDispatchStatusKey] = request.Status.ToString()
            }, deviceId.Value, DataSide.ServerSide);
            await SaveTaskHistoryAsync(deviceId.Value, request.TaskId, "receipt", request.ReportedAt.ToUniversalTime(), receiptPayload, request.Status.ToString());

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

            var receiptValue = await _context.EdgeTasks
                .Where(task => task.GatewayId == deviceId && !task.Deleted && task.LastReceiptPayload != null && task.LastReceiptPayload != string.Empty)
                .OrderByDescending(task => task.LastReceiptAt ?? task.UpdatedAt)
                .Select(task => task.LastReceiptPayload)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(receiptValue))
            {
                receiptValue = await _context.AttributeLatest
                .Where(attr => attr.DeviceId == deviceId && attr.KeyName == EdgeTaskReceiptLastKey)
                .Select(attr => attr.Value_String)
                .FirstOrDefaultAsync();
            }

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

            var records = await _context.TelemetryData
                .Where(item => item.DeviceId == deviceId && item.KeyName.StartsWith(EdgeTaskHistoryKeyPrefix))
                .OrderByDescending(item => item.DateTime)
                .Take(50)
                .Select(item => new
                {
                    key = item.KeyName,
                    at = item.DateTime,
                    payload = item.Value_String,
                    status = NormalizeStoredStatus(item.Value_Json)
                })
                .ToListAsync();

            return new ApiResult<List<object>>(ApiCode.Success, "OK", records.Cast<object>().ToList());
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
                var formalResults = new List<EdgeTaskRequestDto>();
                foreach (var formalTask in formalTasks)
                {
                    formalResults.Add(ToEdgeTaskRequestDto(formalTask));
                    if (formalTask.Status == EdgeTaskStatus.Pending)
                    {
                        ApplyFormalStatus(formalTask, EdgeTaskStatus.Sent, now, null, null, null);
                        await SaveTaskHistoryAsync(gateway.Id, formalTask.Id, "dispatch", now, formalTask.RequestPayload ?? "{}", EdgeTaskStatus.Sent.ToString());
                    }
                }

                await _context.SaveChangesAsync();
                return new ApiResult<List<EdgeTaskRequestDto>>(ApiCode.Success, "OK", formalResults);
            }

            var recentTaskHistory = await _context.TelemetryData
                .Where(item => item.DeviceId == gateway.Id)
                .OrderByDescending(item => item.DateTime)
                .Take(500)
                .ToListAsync();

            var records = recentTaskHistory
                .Where(item => item.KeyName.StartsWith(EdgeTaskHistoryKeyPrefix, StringComparison.Ordinal)
                    && item.KeyName.EndsWith(".request", StringComparison.Ordinal))
                .OrderBy(item => item.DateTime)
                .Take(takeCount)
                .ToList();

            var tasks = new List<EdgeTaskRequestDto>();
            foreach (var record in records)
            {
                var payload = record.Value_String;
                var task = string.IsNullOrWhiteSpace(payload) ? null : JsonSerializer.Deserialize<EdgeTaskRequestDto>(payload, ContractJsonOptions);
                if (task == null)
                {
                    continue;
                }

                var latestReceipt = await _context.TelemetryData
                    .Where(item => item.DeviceId == gateway.Id && item.KeyName == $"{EdgeTaskHistoryKeyPrefix}{task.TaskId:N}.receipt")
                    .OrderByDescending(item => item.DateTime)
                    .FirstOrDefaultAsync();
                var latestReceiptStatus = NormalizeStoredStatus(latestReceipt?.Value_Json);

                if (string.IsNullOrWhiteSpace(latestReceiptStatus) || latestReceiptStatus is nameof(EdgeTaskStatus.Pending) or nameof(EdgeTaskStatus.Sent))
                {
                    tasks.Add(task);
                    var currentStatus = await GetLatestTaskStatusAsync(gateway.Id, task.TaskId);
                    if (currentStatus == EdgeTaskStatus.Pending)
                    {
                        await SaveTaskStatusAsync(gateway.Id, task.TaskId, "dispatch", DateTime.UtcNow, payload, EdgeTaskStatus.Sent);
                    }
                }
            }

            return new ApiResult<List<EdgeTaskRequestDto>>(ApiCode.Success, "OK", tasks);
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
            var currentStatus = formalTask?.Status ?? await GetLatestTaskStatusAsync(gateway.Id, request.TaskId);
            if (currentStatus == null)
            {
                return Ok(new ApiResult(ApiCode.InValidData, "Task request not found for acceptance"));
            }

            if (!IsTransitionAllowed(currentStatus.Value, EdgeTaskStatus.Accepted))
            {
                return Ok(new ApiResult(ApiCode.InValidData, $"Invalid edge task transition: {currentStatus.Value} -> {EdgeTaskStatus.Accepted}"));
            }

            request.ContractVersion = string.IsNullOrWhiteSpace(request.ContractVersion) ? TaskContractVersion : request.ContractVersion;
            request.TargetKey = string.IsNullOrWhiteSpace(request.TargetKey) ? gateway.Id.ToString() : request.TargetKey;
            request.TargetType = EdgeTaskTargetType.EdgeNode;
            request.Status = EdgeTaskStatus.Accepted;
            request.ReportedAt = request.ReportedAt == default ? DateTime.UtcNow : request.ReportedAt;
            request.ReportedAt = request.ReportedAt.ToUniversalTime();

            var receiptPayload = SerializeOrNull(request) ?? "{}";
            if (formalTask != null)
            {
                ApplyFormalReceipt(formalTask, request);
                await _context.SaveChangesAsync();
            }

            await _context.SaveAsync<AttributeLatest>(new Dictionary<string, object>
            {
                [EdgeTaskDispatchStatusKey] = EdgeTaskStatus.Accepted.ToString()
            }, gateway.Id, DataSide.ServerSide);
            await SaveTaskHistoryAsync(gateway.Id, request.TaskId, "receipt", request.ReportedAt.ToUniversalTime(), receiptPayload, EdgeTaskStatus.Accepted.ToString());
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

            List<EdgeTaskTimelineDto> rows;
            if (formalTasks.Count > 0)
            {
                rows = formalTasks
                    .Select(task => ToEdgeTaskTimelineDto(task, deviceNames))
                    .OrderByDescending(item => item.LastUpdatedAt)
                    .ToList();
            }
            else
            {
                var records = await _context.TelemetryData
                    .Where(item => deviceIds.Contains(item.DeviceId) && item.KeyName.StartsWith(EdgeTaskHistoryKeyPrefix))
                    .OrderByDescending(item => item.DateTime)
                    .Take(500)
                    .ToListAsync();

                rows = records
                    .Select(item =>
                    {
                        var payload = DeserializeToDictionary(item.Value_String);
                        return new EdgeTaskListItemDto
                        {
                            DeviceId = item.DeviceId,
                            DeviceName = deviceNames.TryGetValue(item.DeviceId, out var deviceName) ? deviceName : item.DeviceId.ToString(),
                            TaskId = TryParseGuid(payload, "taskId"),
                            Category = item.KeyName.Split('.').LastOrDefault() ?? string.Empty,
                            RuntimeType = TryGetString(payload, "runtimeType"),
                            InstanceId = TryGetString(payload, "instanceId"),
                            Status = GetTaskListStatus(payload, item.Value_Json),
                            Message = TryGetString(payload, "message"),
                            At = item.DateTime,
                            Payload = item.Value_String
                        };
                    })
                    .Where(item => item.TaskId != Guid.Empty)
                    .GroupBy(item => new { item.DeviceId, item.TaskId })
                    .Select(group =>
                    {
                        var ordered = group.OrderBy(item => item.At).ToList();
                        var last = ordered.Last();
                        return new EdgeTaskTimelineDto
                        {
                            DeviceId = last.DeviceId,
                            DeviceName = last.DeviceName,
                            TaskId = last.TaskId,
                            RuntimeType = last.RuntimeType,
                            InstanceId = last.InstanceId,
                            CurrentStatus = last.Status,
                            LastUpdatedAt = last.At,
                            Events = ordered.Select(item => new EdgeTaskTimelineNodeDto
                            {
                                Category = item.Category,
                                Status = item.Status,
                                Message = item.Message,
                                At = item.At,
                                Payload = item.Payload
                            }).ToList()
                        };
                    })
                    .OrderByDescending(item => item.LastUpdatedAt)
                    .ToList();
            }

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
        private static EdgeTaskTimelineDto ToEdgeTaskTimelineDto(EdgeTask task, IReadOnlyDictionary<Guid, string> deviceNames)
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

            if (task.LastReceiptAt != null)
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
                var matchedDeviceId = await _context.AttributeLatest
                    .Where(attr => attr.KeyName == Constants._EdgeInstanceId && attr.Value_String == instanceId)
                    .Select(attr => (Guid?)attr.DeviceId)
                    .FirstOrDefaultAsync();

                if (matchedDeviceId != null)
                {
                    return matchedDeviceId;
                }
            }

            return null;
        }

        /// <summary>
        /// 读取指定边缘任务最近一次状态，作为运行时回执的状态机入口。
        /// </summary>
        /// <param name="deviceId">承载 EdgeNode 的 Gateway 设备 ID。</param>
        /// <param name="taskId">边缘任务 ID。</param>
        /// <returns>最近一次状态；未找到任务历史时返回空。</returns>
        private async System.Threading.Tasks.Task<EdgeTaskStatus?> GetLatestTaskStatusAsync(Guid deviceId, Guid taskId)
        {
            var formalStatus = await _context.EdgeTasks
                .Where(task => task.GatewayId == deviceId && task.Id == taskId && !task.Deleted)
                .Select(task => (EdgeTaskStatus?)task.Status)
                .FirstOrDefaultAsync();
            if (formalStatus != null)
            {
                return formalStatus;
            }

            var keyPrefix = $"{EdgeTaskHistoryKeyPrefix}{taskId:N}.";
            var status = (await _context.TelemetryData
                .Where(item => item.DeviceId == deviceId)
                .OrderByDescending(item => item.DateTime)
                .Take(500)
                .ToListAsync())
                .Where(item => item.KeyName.StartsWith(keyPrefix, StringComparison.Ordinal))
                .Select(item => item.Value_Json)
                .FirstOrDefault();

            status = NormalizeStoredStatus(status);
            return Enum.TryParse<EdgeTaskStatus>(status, true, out var parsed) ? parsed : null;
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

        /// <summary>
        /// 写入任务状态历史，并同步设备级最近分发状态，供管理端列表和运行时拉取使用。
        /// </summary>
        /// <param name="deviceId">承载 EdgeNode 的 Gateway 设备 ID。</param>
        /// <param name="taskId">边缘任务 ID。</param>
        /// <param name="category">历史事件类别。</param>
        /// <param name="at">事件时间。</param>
        /// <param name="payload">事件负载 JSON。</param>
        /// <param name="status">状态值。</param>
        private async System.Threading.Tasks.Task SaveTaskStatusAsync(Guid deviceId, Guid taskId, string category, DateTime at, string payload, EdgeTaskStatus status)
        {
            await _context.SaveAsync<AttributeLatest>(new Dictionary<string, object>
            {
                [EdgeTaskDispatchStatusKey] = status.ToString()
            }, deviceId, DataSide.ServerSide);
            await SaveTaskHistoryAsync(deviceId, taskId, category, at, payload, status.ToString());
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

        private async System.Threading.Tasks.Task SaveTaskHistoryAsync(Guid deviceId, Guid taskId, string category, DateTime at, string payload, string status)
        {
            var history = new TelemetryData
            {
                DeviceId = deviceId,
                KeyName = $"{EdgeTaskHistoryKeyPrefix}{taskId:N}.{category}",
                DateTime = at,
                DataSide = DataSide.ServerSide,
                Type = Contracts.DataType.String,
                Value_String = payload,
                Value_Json = SerializeStatusForJsonColumn(status)
            };

            _context.TelemetryData.Add(history);
            await _context.SaveChangesAsync();
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

        private static Dictionary<string, object> DeserializeToDictionary(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            return JsonSerializer.Deserialize<Dictionary<string, object>>(payload, ContractJsonOptions) ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        private static string TryGetString(Dictionary<string, object> payload, string key)
        {
            if (!payload.TryGetValue(key, out var value) || value == null)
            {
                return string.Empty;
            }

            return value is JsonElement element
                ? element.ValueKind == JsonValueKind.String ? element.GetString() ?? string.Empty : element.ToString()
                : value.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 获取任务列表展示状态，优先使用负载中的状态字段，否则回退到历史记录状态列。
        /// </summary>
        /// <param name="payload">任务历史负载。</param>
        /// <param name="storedStatus">任务历史状态列。</param>
        /// <returns>可展示的状态名称。</returns>
        private static string GetTaskListStatus(Dictionary<string, object> payload, string storedStatus)
        {
            var payloadStatus = TryGetString(payload, "status");
            return string.IsNullOrWhiteSpace(payloadStatus) ? NormalizeStoredStatus(storedStatus) : payloadStatus;
        }

        /// <summary>
        /// 将任务状态写成合法 JSON 字符串，避免 PostgreSQL JSON 列拒绝裸字符串。
        /// </summary>
        /// <param name="status">任务状态文本。</param>
        /// <returns>可写入 JSON 列的字符串。</returns>
        private static string SerializeStatusForJsonColumn(string status)
        {
            return JsonSerializer.Serialize(status ?? string.Empty);
        }

        /// <summary>
        /// 读取任务历史状态，兼容早期保存的裸状态值和当前保存的 JSON 字符串值。
        /// </summary>
        /// <param name="storedStatus">数据库中的状态字段。</param>
        /// <returns>规范化后的状态名称。</returns>
        private static string NormalizeStoredStatus(string storedStatus)
        {
            if (string.IsNullOrWhiteSpace(storedStatus))
            {
                return string.Empty;
            }

            if (storedStatus.Length >= 2 && storedStatus[0] == '"' && storedStatus[^1] == '"')
            {
                try
                {
                    return JsonSerializer.Deserialize<string>(storedStatus, ContractJsonOptions) ?? string.Empty;
                }
                catch (JsonException)
                {
                    return storedStatus;
                }
            }

            return storedStatus;
        }

        private static Guid TryParseGuid(Dictionary<string, object> payload, string key)
        {
            var value = TryGetString(payload, key);
            return Guid.TryParse(value, out var guid) ? guid : Guid.Empty;
        }
    }
}
