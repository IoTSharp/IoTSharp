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
using Storage.Net.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// ReleasePackage 最小软件包发布接口。
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ReleasePackagesController : ControllerBase
    {
        private const string PackageIdKey = "packageId";
        private const string PackageVersionKey = "packageVersion";
        private const string PackageSha256Key = "sha256";
        private const string AuditActionUpload = "ReleasePackageUpload";
        private const string AuditActionPublish = "ReleasePackagePublish";
        private const string AuditActionDispatch = "EdgeTaskDispatch";
        private static readonly TimeSpan DefaultSoftwareUpdateTtl = TimeSpan.FromHours(2);
        private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);

        private readonly ApplicationDbContext _context;
        private readonly IBlobStorage _blob;
        private readonly ILogger<ReleasePackagesController> _logger;

        public ReleasePackagesController(
            ApplicationDbContext context,
            IBlobStorage blob,
            ILogger<ReleasePackagesController> logger)
        {
            _context = context;
            _blob = blob;
            _logger = logger;
        }

        /// <summary>
        /// 查询最小软件包列表。
        /// </summary>
        /// <param name="query">分页和筛选条件。</param>
        /// <returns>软件包分页结果。</returns>
        [HttpGet]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<PagedData<ReleasePackageDto>>> List([FromQuery] ReleasePackageQueryDto query)
        {
            var profile = this.GetUserProfile();
            query ??= new ReleasePackageQueryDto();
            query.Limit = Math.Clamp(query.Limit < 1 ? 10 : query.Limit, 1, 100);

            var packages = _context.ReleasePackages
                .Where(c => !c.Deleted && c.TenantId == profile.Tenant && c.CustomerId == profile.Customer);

            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                packages = packages.Where(c => c.Name.Contains(query.Name) || c.PackageKey.Contains(query.Name));
            }

            if (query.PackageType.HasValue)
            {
                packages = packages.Where(c => c.PackageType == query.PackageType.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.RuntimeType))
            {
                packages = packages.Where(c => c.TargetRuntimeType == query.RuntimeType);
            }

            if (!string.IsNullOrWhiteSpace(query.Version))
            {
                packages = packages.Where(c => c.Version == query.Version);
            }

            var total = await packages.LongCountAsync();
            var rows = await packages
                .OrderByDescending(c => c.CreatedAt)
                .Skip(query.Offset * query.Limit)
                .Take(query.Limit)
                .ToListAsync();

            return new ApiResult<PagedData<ReleasePackageDto>>(ApiCode.Success, "OK", new PagedData<ReleasePackageDto>
            {
                total = total,
                rows = rows.Select(ToReleasePackageDto).ToList()
            });
        }

        /// <summary>
        /// 获取软件包详情。
        /// </summary>
        /// <param name="id">软件包 ID。</param>
        /// <returns>软件包详情。</returns>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<ReleasePackageDto>> Get(Guid id)
        {
            var profile = this.GetUserProfile();
            var package = await FindPackageAsync(id, profile.Tenant, profile.Customer);
            if (package == null)
            {
                return new ApiResult<ReleasePackageDto>(ApiCode.CantFindObject, "Release package not found", null);
            }

            return new ApiResult<ReleasePackageDto>(ApiCode.Success, "OK", ToReleasePackageDto(package));
        }

        /// <summary>
        /// 上传最小软件包并记录版本、目标运行时和校验和。
        /// </summary>
        /// <param name="request">上传表单。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>已保存的软件包元数据。</returns>
        [HttpPost("Upload")]
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<ReleasePackageDto>>> Upload(
            [FromForm] ReleasePackageUploadRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request?.File == null || request.File.Length <= 0)
            {
                return Ok(new ApiResult<ReleasePackageDto>(ApiCode.NotFile, "Release package file is required", null));
            }

            if (request.PackageType != ReleasePackageType.Software)
            {
                return Ok(new ApiResult<ReleasePackageDto>(ApiCode.InValidData, "Only Software release packages are supported in #046", null));
            }

            var packageKey = NormalizeRequired(request.PackageKey);
            var packageName = NormalizeRequired(request.Name);
            var version = NormalizeRequired(request.Version);
            var runtimeType = NormalizeRequired(request.TargetRuntimeType);
            if (string.IsNullOrWhiteSpace(packageKey) ||
                string.IsNullOrWhiteSpace(packageName) ||
                string.IsNullOrWhiteSpace(version) ||
                string.IsNullOrWhiteSpace(runtimeType))
            {
                return Ok(new ApiResult<ReleasePackageDto>(ApiCode.InValidData, "packageKey, name, version and targetRuntimeType are required", null));
            }

            var profile = this.GetUserProfile();
            var exists = await _context.ReleasePackages.AnyAsync(c =>
                !c.Deleted &&
                c.TenantId == profile.Tenant &&
                c.CustomerId == profile.Customer &&
                c.PackageType == request.PackageType &&
                c.PackageKey == packageKey &&
                c.Version == version &&
                c.TargetRuntimeType == runtimeType,
                cancellationToken);
            if (exists)
            {
                return Ok(new ApiResult<ReleasePackageDto>(ApiCode.AlreadyExists, "Release package version already exists for target runtime", null));
            }

            string metadataJson;
            try
            {
                metadataJson = NormalizeMetadataJson(request.Metadata);
            }
            catch (JsonException)
            {
                return Ok(new ApiResult<ReleasePackageDto>(ApiCode.InValidData, "metadata must be a JSON object", null));
            }

            var now = DateTime.UtcNow;
            var updatedBy = ResolveUserName(profile);
            var fileName = Path.GetFileName(request.File.FileName);
            var contentType = string.IsNullOrWhiteSpace(request.File.ContentType)
                ? "application/octet-stream"
                : request.File.ContentType;
            var package = new ReleasePackage
            {
                Id = Guid.NewGuid(),
                ContractVersion = EdgeNodeContractVersions.ReleasePackageV1,
                PackageType = request.PackageType,
                PackageKey = packageKey,
                Name = packageName,
                Version = version,
                TargetRuntimeType = runtimeType,
                TargetRuntimeVersion = request.TargetRuntimeVersion?.Trim() ?? string.Empty,
                FileName = fileName,
                ContentType = contentType,
                Size = request.File.Length,
                DownloadToken = Guid.NewGuid().ToString("N"),
                Metadata = metadataJson,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = updatedBy,
                UpdatedBy = updatedBy,
                TenantId = profile.Tenant,
                CustomerId = profile.Customer
            };

            package.BlobPath = BuildBlobPath(profile.Tenant, package.Id, fileName);
            var tempFile = Path.GetTempFileName();
            try
            {
                package.Sha256 = await CopyToTempAndHashAsync(request.File, tempFile, cancellationToken);
                await _blob.WriteFileAsync(package.BlobPath, tempFile);
            }
            finally
            {
                TryDeleteTempFile(tempFile);
            }

            _context.ReleasePackages.Add(package);
            AddReleasePackageAudit(profile, package, AuditActionUpload, new
            {
                package.Id,
                package.PackageType,
                package.PackageKey,
                package.Version,
                package.TargetRuntimeType,
                package.TargetRuntimeVersion,
                package.FileName,
                package.Size,
                package.Sha256
            }, "Uploaded", now);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Uploaded release package {PackageId} {PackageKey} {Version} for runtime {RuntimeType}",
                package.Id,
                package.PackageKey,
                package.Version,
                package.TargetRuntimeType);

            return Ok(new ApiResult<ReleasePackageDto>(ApiCode.Success, "OK", ToReleasePackageDto(package)));
        }

        /// <summary>
        /// 将软件包发布为 SoftwareUpdate EdgeTask。
        /// </summary>
        /// <param name="id">软件包 ID。</param>
        /// <param name="request">发布请求。</param>
        /// <returns>软件包和创建出的 EdgeTask。</returns>
        [HttpPost("{id:guid}/Publish")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<ReleasePackagePublishResultDto>>> Publish(
            Guid id,
            [FromBody] ReleasePackagePublishRequestDto request)
        {
            var profile = this.GetUserProfile();
            var package = await FindPackageAsync(id, profile.Tenant, profile.Customer);
            if (package == null)
            {
                return Ok(new ApiResult<ReleasePackagePublishResultDto>(ApiCode.CantFindObject, "Release package not found", null));
            }

            request ??= new ReleasePackagePublishRequestDto();
            if (request.EdgeNodeId == Guid.Empty)
            {
                return Ok(new ApiResult<ReleasePackagePublishResultDto>(ApiCode.NotFoundDevice, "Edge node not found", null));
            }

            var gateway = await GatewayInScope(request.EdgeNodeId, profile.Tenant, profile.Customer)
                .Include(c => c.DeviceIdentity)
                .Include(c => c.Tenant)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync();
            if (gateway == null)
            {
                return Ok(new ApiResult<ReleasePackagePublishResultDto>(ApiCode.NotFoundDevice, "Edge node not found", null));
            }

            if (request.TaskId is { } requestedTaskId && requestedTaskId != Guid.Empty)
            {
                var taskExists = await _context.EdgeTasks.AnyAsync(task => task.Id == requestedTaskId && !task.Deleted);
                if (taskExists)
                {
                    return Ok(new ApiResult<ReleasePackagePublishResultDto>(ApiCode.InValidData, "taskId already exists", null));
                }
            }

            var runtimeType = Coalesce(request.RuntimeType, package.TargetRuntimeType);
            if (!RuntimeMatchesPackage(package.TargetRuntimeType, runtimeType))
            {
                return Ok(new ApiResult<ReleasePackagePublishResultDto>(ApiCode.InValidData, "runtimeType does not match package targetRuntimeType", null));
            }

            var now = DateTime.UtcNow;
            var updatedBy = ResolveUserName(profile);
            var node = await EnsureEdgeNodeAsync(gateway);
            var taskRequest = CreateSoftwareUpdateTaskRequest(package, gateway, node, request, runtimeType, updatedBy, now);
            var edgeTask = CreateFormalEdgeTask(taskRequest, gateway, node, SerializeOrNull(taskRequest) ?? "{}");

            _context.EdgeTasks.Add(edgeTask);
            AddReleasePackageAudit(profile, package, AuditActionPublish, new
            {
                package.Id,
                package.PackageKey,
                package.Version,
                taskId = edgeTask.Id,
                targetKey = edgeTask.TargetKey,
                runtimeType = edgeTask.RuntimeType,
                instanceId = edgeTask.InstanceId
            }, EdgeTaskStatus.Pending.ToString(), now);
            AddEdgeTaskDispatchAudit(profile, edgeTask, package, now);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Published release package {PackageId} to edge {GatewayId} with task {TaskId}",
                package.Id,
                gateway.Id,
                edgeTask.Id);

            return Ok(new ApiResult<ReleasePackagePublishResultDto>(
                ApiCode.Success,
                "OK",
                new ReleasePackagePublishResultDto
                {
                    Package = ToReleasePackageDto(package),
                    Task = taskRequest
                }));
        }

        /// <summary>
        /// 执行端下载软件包文件。
        /// </summary>
        /// <param name="id">软件包 ID。</param>
        /// <param name="token">发布任务中携带的下载令牌。</param>
        /// <param name="sha256">可选的 SHA256 校验和。</param>
        /// <returns>软件包文件流。</returns>
        [AllowAnonymous]
        [HttpGet("{id:guid}/Download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> Download(Guid id, [FromQuery] string token, [FromQuery] string sha256)
        {
            var package = await _context.ReleasePackages.FirstOrDefaultAsync(c => c.Id == id && !c.Deleted);
            if (package == null)
            {
                return NotFound(new ApiResult(ApiCode.CantFindObject, "Release package not found"));
            }

            if (string.IsNullOrWhiteSpace(token) ||
                !string.Equals(token, package.DownloadToken, StringComparison.Ordinal))
            {
                return Unauthorized(new ApiResult(ApiCode.NotAuthorized, "Invalid release package download token"));
            }

            if (!string.IsNullOrWhiteSpace(sha256) &&
                !string.Equals(sha256, package.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ApiResult(ApiCode.InValidData, "sha256 does not match release package"));
            }

            var blob = await _blob.GetBlobAsync(package.BlobPath);
            if (blob == null || !blob.IsFile)
            {
                return NotFound(new ApiResult(ApiCode.NotFile, "Release package file not found"));
            }

            var stream = await _blob.OpenReadAsync(package.BlobPath);
            return File(stream, Coalesce(package.ContentType, "application/octet-stream"), package.FileName);
        }

        private async Task<ReleasePackage> FindPackageAsync(Guid id, Guid tenantId, Guid customerId)
            => await _context.ReleasePackages
                .FirstOrDefaultAsync(c => c.Id == id
                    && !c.Deleted
                    && c.TenantId == tenantId
                    && c.CustomerId == customerId);

        private IQueryable<Device> GatewayInScope(Guid gatewayId, Guid tenantId, Guid customerId)
            => _context.Device
                .Where(c => c.Id == gatewayId
                    && !c.Deleted
                    && c.DeviceType == DeviceType.Gateway
                    && c.Tenant.Id == tenantId
                    && c.Customer.Id == customerId);

        /// <summary>
        /// 获取或创建与 Gateway 一一对应的 EdgeNode，用于软件更新任务寻址和状态归属。
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
        /// 创建软件更新任务请求。
        /// </summary>
        /// <param name="package">软件包。</param>
        /// <param name="gateway">承载任务通道的 Gateway 设备。</param>
        /// <param name="node">平台侧 EdgeNode。</param>
        /// <param name="request">发布请求。</param>
        /// <param name="runtimeType">目标运行时类型。</param>
        /// <param name="updatedBy">操作者显示名或账号。</param>
        /// <param name="now">任务创建时间。</param>
        /// <returns>EdgeTask 请求 DTO。</returns>
        private EdgeTaskRequestDto CreateSoftwareUpdateTaskRequest(
            ReleasePackage package,
            Device gateway,
            EdgeNode node,
            ReleasePackagePublishRequestDto request,
            string runtimeType,
            string updatedBy,
            DateTime now)
        {
            var instanceId = Coalesce(request.InstanceId, node?.InstanceId ?? string.Empty);
            var targetType = request.TargetType ?? ResolveDefaultTargetType(runtimeType);
            var targetKey = BuildEdgeTargetKey(gateway.Id, runtimeType, instanceId);
            var downloadUrl = Url.Action(nameof(Download), "ReleasePackages", new
            {
                id = package.Id,
                token = package.DownloadToken,
                sha256 = package.Sha256
            }, Request.Scheme) ?? $"/api/ReleasePackages/{package.Id:D}/Download";
            var metadata = new Dictionary<string, string>(request.Metadata ?? [], StringComparer.OrdinalIgnoreCase)
            {
                ["source"] = "release-package-publish",
                ["operator"] = updatedBy ?? string.Empty,
                ["packageId"] = package.Id.ToString("D"),
                ["packageKey"] = package.PackageKey ?? string.Empty,
                ["packageVersion"] = package.Version ?? string.Empty
            };

            return new EdgeTaskRequestDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = request.TaskId is { } taskId && taskId != Guid.Empty ? taskId : Guid.NewGuid(),
                TaskType = EdgeTaskType.SoftwareUpdate,
                CreatedAt = now,
                ExpireAt = request.ExpireAt?.ToUniversalTime() ?? now.Add(DefaultSoftwareUpdateTtl),
                Address = new EdgeTaskAddressDto
                {
                    TargetType = targetType,
                    DeviceId = gateway.Id,
                    RuntimeType = runtimeType,
                    InstanceId = instanceId,
                    TargetKey = targetKey
                },
                Parameters = new Dictionary<string, object>
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

        private static ReleasePackageDto ToReleasePackageDto(ReleasePackage package)
        {
            return new ReleasePackageDto
            {
                ContractVersion = Coalesce(package.ContractVersion, EdgeNodeContractVersions.ReleasePackageV1),
                Id = package.Id,
                PackageType = package.PackageType,
                PackageKey = package.PackageKey ?? string.Empty,
                Name = package.Name ?? string.Empty,
                Version = package.Version ?? string.Empty,
                TargetRuntimeType = package.TargetRuntimeType ?? string.Empty,
                TargetRuntimeVersion = package.TargetRuntimeVersion ?? string.Empty,
                FileName = package.FileName ?? string.Empty,
                ContentType = package.ContentType ?? string.Empty,
                Size = package.Size,
                Sha256 = package.Sha256 ?? string.Empty,
                Metadata = DeserializeObjectMap(package.Metadata),
                CreatedAt = package.CreatedAt,
                UpdatedAt = package.UpdatedAt,
                CreatedBy = package.CreatedBy ?? string.Empty,
                UpdatedBy = package.UpdatedBy ?? string.Empty
            };
        }

        private static async Task<string> CopyToTempAndHashAsync(
            IFormFile file,
            string tempFile,
            CancellationToken cancellationToken)
        {
            await using var input = file.OpenReadStream();
            await using var output = System.IO.File.Create(tempFile);
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = new byte[81920];
            int read;
            while ((read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                hash.AppendData(buffer, 0, read);
            }

            return Convert.ToHexString(hash.GetHashAndReset());
        }

        private static string BuildBlobPath(Guid tenantId, Guid packageId, string fileName)
            => $"release-packages/{tenantId:N}/{packageId:N}/{fileName}";

        private static string BuildEdgeTargetKey(Guid gatewayId, string runtimeType, string instanceId)
        {
            var normalizedRuntimeType = string.IsNullOrWhiteSpace(runtimeType) ? EdgeRuntimeTypes.Gateway : runtimeType.Trim();
            return string.IsNullOrWhiteSpace(instanceId)
                ? $"{gatewayId}:{normalizedRuntimeType}"
                : $"{gatewayId}:{normalizedRuntimeType}:{instanceId.Trim()}";
        }

        private static EdgeTaskTargetType ResolveDefaultTargetType(string runtimeType)
            => string.Equals(runtimeType, EdgeRuntimeTypes.Gateway, StringComparison.OrdinalIgnoreCase)
                ? EdgeTaskTargetType.GatewayRuntime
                : EdgeTaskTargetType.EdgeNode;

        private static bool RuntimeMatchesPackage(string packageRuntimeType, string runtimeType)
        {
            return string.IsNullOrWhiteSpace(packageRuntimeType) ||
                packageRuntimeType == "*" ||
                string.Equals(packageRuntimeType, runtimeType, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeMetadataJson(string metadata)
        {
            if (string.IsNullOrWhiteSpace(metadata))
            {
                return "{}";
            }

            using var document = JsonDocument.Parse(metadata);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("metadata must be a JSON object");
            }

            return JsonSerializer.Serialize(document.RootElement, WebJsonOptions);
        }

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

        private static string NormalizeRequired(string value)
            => value?.Trim() ?? string.Empty;

        private static string Coalesce(string primary, string fallback)
            => string.IsNullOrWhiteSpace(primary) ? fallback : primary;

        private static string SerializeOrNull<T>(T value)
        {
            if (value == null)
            {
                return null;
            }

            return JsonSerializer.Serialize(value, WebJsonOptions);
        }

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

        private void AddReleasePackageAudit(
            UserProfile profile,
            ReleasePackage package,
            string actionName,
            object actionData,
            string actionResult,
            DateTime activeAt)
        {
            _context.AuditLog.Add(new AuditLog
            {
                TenantId = package.TenantId,
                CustomerId = package.CustomerId,
                UserId = profile.Id.ToString("D"),
                UserName = ResolveUserName(profile),
                ObjectID = package.Id,
                ObjectName = $"{package.PackageKey}:{package.Version}",
                ObjectType = ObjectType.ReleasePackage,
                ActionName = actionName,
                ActionData = SerializeOrNull(actionData) ?? "{}",
                ActionResult = actionResult ?? string.Empty,
                ActiveDateTime = activeAt == default ? DateTime.UtcNow : activeAt.ToUniversalTime()
            });
        }

        private void AddEdgeTaskDispatchAudit(
            UserProfile profile,
            EdgeTask task,
            ReleasePackage package,
            DateTime activeAt)
        {
            _context.AuditLog.Add(new AuditLog
            {
                TenantId = task.TenantId,
                CustomerId = task.CustomerId,
                UserId = profile.Id.ToString("D"),
                UserName = ResolveUserName(profile),
                ObjectID = task.Id,
                ObjectName = $"{task.TaskType}:{task.TargetKey}",
                ObjectType = ObjectType.EdgeTask,
                ActionName = AuditActionDispatch,
                ActionData = SerializeOrNull(new
                {
                    taskId = task.Id,
                    taskType = task.TaskType.ToString(),
                    targetKey = task.TargetKey,
                    packageId = package.Id,
                    packageKey = package.PackageKey,
                    packageVersion = package.Version,
                    sha256 = package.Sha256
                }) ?? "{}",
                ActionResult = EdgeTaskStatus.Pending.ToString(),
                ActiveDateTime = activeAt == default ? DateTime.UtcNow : activeAt.ToUniversalTime()
            });
        }

        private static void TryDeleteTempFile(string tempFile)
        {
            if (string.IsNullOrWhiteSpace(tempFile))
            {
                return;
            }

            try
            {
                if (System.IO.File.Exists(tempFile))
                {
                    System.IO.File.Delete(tempFile);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
