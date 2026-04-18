using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
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
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class EdgeTaskController : ControllerBase
    {
        private const string TaskContractVersion = "edge-task-v1";
        private const string EdgeTaskRequestLastKey = "_edge.task.request.last";
        private const string EdgeTaskRequestStatusKey = "_edge.task.request.status";
        private const string EdgeTaskRequestCreatedAtKey = "_edge.task.request.createdAt";
        private const string EdgeTaskReceiptLastKey = "_edge.task.receipt.last";
        private const string EdgeTaskReceiptStatusKey = "_edge.task.receipt.status";
        private const string EdgeTaskReceiptReportedAtKey = "_edge.task.receipt.reportedAt";
        private const string EdgeTaskHistoryKeyPrefix = "_edge.task.history.";
        private static readonly IReadOnlyDictionary<EdgeTaskStatus, EdgeTaskStatus[]> AllowedTransitions =
            new Dictionary<EdgeTaskStatus, EdgeTaskStatus[]>
            {
                [EdgeTaskStatus.Pending] = [EdgeTaskStatus.Sent],
                [EdgeTaskStatus.Sent] = [EdgeTaskStatus.Accepted, EdgeTaskStatus.TimedOut],
                [EdgeTaskStatus.Accepted] = [EdgeTaskStatus.Running, EdgeTaskStatus.Cancelled],
                [EdgeTaskStatus.Running] = [EdgeTaskStatus.Succeeded, EdgeTaskStatus.Failed, EdgeTaskStatus.TimedOut, EdgeTaskStatus.Cancelled],
                [EdgeTaskStatus.Succeeded] = [],
                [EdgeTaskStatus.Failed] = [],
                [EdgeTaskStatus.TimedOut] = [],
                [EdgeTaskStatus.Cancelled] = []
            };

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

            var latestAttrs = new Dictionary<string, object>
            {
                [EdgeTaskRequestLastKey] = SerializeOrNull(request) ?? "{}",
                [EdgeTaskRequestStatusKey] = EdgeTaskStatus.Pending.ToString(),
                [EdgeTaskRequestCreatedAtKey] = request.CreatedAt.ToUniversalTime()
            };

            await _context.SaveAsync<AttributeLatest>(latestAttrs, device.Id, DataSide.ServerSide);
            await SaveTaskHistoryAsync(device.Id, request.TaskId, "request", request.CreatedAt.ToUniversalTime(), SerializeOrNull(request) ?? "{}", EdgeTaskStatus.Pending.ToString());

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

            var attrs = new Dictionary<string, object>
            {
                [EdgeTaskReceiptLastKey] = SerializeOrNull(request) ?? "{}",
                [EdgeTaskReceiptStatusKey] = request.Status.ToString(),
                [EdgeTaskReceiptReportedAtKey] = request.ReportedAt.ToUniversalTime()
            };

            await _context.SaveAsync<AttributeLatest>(attrs, deviceId.Value, DataSide.ServerSide);
            await SaveTaskHistoryAsync(deviceId.Value, request.TaskId, "receipt", request.ReportedAt.ToUniversalTime(), SerializeOrNull(request) ?? "{}", request.Status.ToString());

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

            var receiptValue = await _context.AttributeLatest
                .Where(attr => attr.DeviceId == deviceId && attr.KeyName == EdgeTaskReceiptLastKey)
                .Select(attr => attr.Value_String)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(receiptValue))
            {
                return new ApiResult<EdgeTaskReceiptDto>(ApiCode.Success, "OK", null);
            }

            var receipt = JsonSerializer.Deserialize<EdgeTaskReceiptDto>(receiptValue);
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
                    status = item.Value_Json
                })
                .ToListAsync();

            return new ApiResult<List<object>>(ApiCode.Success, "OK", records.Cast<object>().ToList());
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
                terminalStates = new[]
                {
                    EdgeTaskStatus.Succeeded.ToString(),
                    EdgeTaskStatus.Failed.ToString(),
                    EdgeTaskStatus.TimedOut.ToString(),
                    EdgeTaskStatus.Cancelled.ToString()
                }
            });
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
                Value_Json = status
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
    }
}