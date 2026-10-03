using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace IoTSharp.Contracts;

/// <summary>
/// Edge 运行时上报合同的有界输入校验。
/// </summary>
/// <remarks>
/// 这些限制用于保护控制面存储和审计链路。它们只限制载荷大小和时间顺序，
/// 不改变既有合同字段的语义，也不要求执行端使用新的合同版本。
/// </remarks>
public static class EdgeRuntimeContractValidation
{
    /// <summary>运行时身份字段的最大字符数。</summary>
    public const int MaxIdentityLength = 256;

    /// <summary>状态字段的最大字符数。</summary>
    public const int MaxStatusLength = 64;

    /// <summary>标签或元数据键的最大字符数。</summary>
    public const int MaxKeyLength = 128;

    /// <summary>字符串元数据值的最大字符数。</summary>
    public const int MaxMetadataValueLength = 1024;

    /// <summary>单次上报允许的元数据条目数。</summary>
    public const int MaxMetadataEntries = 64;

    /// <summary>单次心跳允许的指标条目数。</summary>
    public const int MaxMetricEntries = 64;

    /// <summary>单条指标序列化后的最大字符数。</summary>
    public const int MaxMetricValueLength = 2048;

    /// <summary>能力上报允许的列表项数量。</summary>
    public const int MaxCapabilityEntries = 128;

    /// <summary>能力上报允许的结构化任务能力数量。</summary>
    public const int MaxTaskCapabilityEntries = 64;

    /// <summary>能力上报允许的合同兼容声明数量。</summary>
    public const int MaxCompatibleContractEntries = 32;

    /// <summary>执行端上报时间允许领先平台时钟的最大范围。</summary>
    public static readonly TimeSpan MaxClockLead = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 校验运行时注册请求。
    /// </summary>
    /// <param name="request">注册载荷。</param>
    /// <param name="error">失败时的可读错误。</param>
    /// <returns>载荷可接受时返回 true。</returns>
    public static bool TryValidateRegistration(EdgeRegistrationDto? request, out string error)
    {
        if (request == null)
        {
            error = "Registration payload is required";
            return false;
        }

        if (!ValidateRequired(request.RuntimeType, "runtimeType", out error)
            || !ValidateOptional(request.RuntimeName, "runtimeName", out error)
            || !ValidateOptional(request.Version, "version", out error)
            || !ValidateOptional(request.InstanceId, "instanceId", out error)
            || !ValidateOptional(request.Platform, "platform", out error)
            || !ValidateOptional(request.HostName, "hostName", out error)
            || !ValidateOptional(request.IpAddress, "ipAddress", out error))
        {
            return false;
        }

        return ValidateStringMetadata(request.Metadata, out error);
    }

    /// <summary>
    /// 校验运行时心跳载荷和时间顺序。
    /// </summary>
    /// <param name="request">心跳载荷；允许为空以兼容旧执行端。</param>
    /// <param name="lastHeartbeat">平台已保存的最近心跳时间。</param>
    /// <param name="now">平台接收时间。</param>
    /// <param name="error">失败时的可读错误。</param>
    /// <returns>载荷可接受时返回 true。</returns>
    public static bool TryValidateHeartbeat(
        EdgeHeartbeatDto? request,
        DateTime? lastHeartbeat,
        DateTime now,
        out string error)
    {
        error = string.Empty;
        if (request == null)
        {
            return true;
        }

        if (!ValidateOptional(request.Status, "status", MaxStatusLength, out error)
            || !ValidateOptional(request.RuntimeType, "runtimeType", out error)
            || !ValidateOptional(request.InstanceId, "instanceId", out error)
            || !ValidateOptional(request.IpAddress, "ipAddress", out error))
        {
            return false;
        }

        if (request.UptimeSeconds is < 0)
        {
            error = "uptimeSeconds must be non-negative";
            return false;
        }

        var heartbeatAt = request.Timestamp?.ToUniversalTime() ?? now.ToUniversalTime();
        if (heartbeatAt > now.ToUniversalTime().Add(MaxClockLead))
        {
            error = $"timestamp cannot be more than {MaxClockLead.TotalMinutes:0} minutes in the future";
            return false;
        }

        if (lastHeartbeat.HasValue && heartbeatAt < lastHeartbeat.Value.ToUniversalTime())
        {
            error = "timestamp must not be older than the last heartbeat";
            return false;
        }

        return ValidateObjectMetadata(request.Metrics, MaxMetricEntries, MaxMetricValueLength, "metrics", out error);
    }

    /// <summary>
    /// 校验能力上报载荷。
    /// </summary>
    /// <param name="request">能力上报载荷。</param>
    /// <param name="lastReportedAt">平台最近保存的能力报告时间。</param>
    /// <param name="now">平台接收时间。</param>
    /// <param name="error">失败时的可读错误。</param>
    /// <returns>载荷可接受时返回 true。</returns>
    public static bool TryValidateCapabilities(
        EdgeCapabilityReportDto? request,
        DateTime? lastReportedAt,
        DateTime now,
        out string error)
    {
        if (request == null)
        {
            error = "Capabilities payload is required";
            return false;
        }

        error = string.Empty;
        if (!ValidateDate(request.ReportedAt, lastReportedAt, now, "reportedAt", out error)
            || !ValidateStringList(request.Protocols, MaxCapabilityEntries, "protocols", out error)
            || !ValidateEnumList(request.SupportedProtocols, MaxCapabilityEntries, "supportedProtocols", out error)
            || !ValidateStringList(request.SupportedPointTypes, MaxCapabilityEntries, "supportedPointTypes", out error)
            || !ValidateEnumList(request.SupportedTransforms, MaxCapabilityEntries, "supportedTransforms", out error)
            || !ValidateEnumList(request.SupportedReportTriggers, MaxCapabilityEntries, "supportedReportTriggers", out error)
            || !ValidateStringList(request.Features, MaxCapabilityEntries, "features", out error)
            || !ValidateStringList(request.Tasks, MaxCapabilityEntries, "tasks", out error))
        {
            return false;
        }

        if (request.TaskCapabilities is { Length: > MaxTaskCapabilityEntries })
        {
            error = $"taskCapabilities cannot contain more than {MaxTaskCapabilityEntries} items";
            return false;
        }

        foreach (var item in request.TaskCapabilities ?? [])
        {
            if (item == null || !ValidateRequired(item.TaskType, "taskCapabilities.taskType", out error)
                || !ValidateOptional(item.ContractVersion, "taskCapabilities.contractVersion", out error)
                || !ValidateObjectMetadata(item.Metadata, MaxMetadataEntries, MaxMetricValueLength, "taskCapabilities.metadata", out error))
            {
                return false;
            }
        }

        if (request.CompatibleContracts is { Length: > MaxCompatibleContractEntries })
        {
            error = $"compatibleContracts cannot contain more than {MaxCompatibleContractEntries} items";
            return false;
        }

        foreach (var item in request.CompatibleContracts ?? [])
        {
            if (item == null || !ValidateRequired(item.ContractName, "compatibleContracts.contractName", out error)
                || !ValidateRequired(item.ContractVersion, "compatibleContracts.contractVersion", out error)
                || !ValidateOptional(item.MinPlatformVersion, "compatibleContracts.minPlatformVersion", out error)
                || !ValidateOptional(item.MaxPlatformVersion, "compatibleContracts.maxPlatformVersion", out error)
                || !ValidateObjectMetadata(item.Metadata, MaxMetadataEntries, MaxMetricValueLength, "compatibleContracts.metadata", out error))
            {
                return false;
            }
        }

        return ValidateObjectMetadata(request.Metadata, MaxMetadataEntries, MaxMetricValueLength, "metadata", out error);
    }

    /// <summary>
    /// 校验通用任务回执载荷，确保回执不会以异常大的对象或倒序时间污染任务历史。
    /// </summary>
    /// <param name="request">任务回执。</param>
    /// <param name="lastReportedAt">该任务最近一次回执时间。</param>
    /// <param name="now">平台接收时间。</param>
    /// <param name="error">失败时的可读错误。</param>
    /// <returns>载荷可接受时返回 true。</returns>
    public static bool TryValidateReceipt(
        EdgeTaskReceiptDto? request,
        DateTime? lastReportedAt,
        DateTime now,
        out string error)
    {
        if (request == null)
        {
            error = "Receipt payload is required";
            return false;
        }

        error = string.Empty;
        if (!ValidateRequired(request.TargetKey, "targetKey", out error)
            || !ValidateOptional(request.RuntimeType, "runtimeType", out error)
            || !ValidateOptional(request.InstanceId, "instanceId", out error)
            || !ValidateOptional(request.Message, "message", 2048, out error)
            || !ValidateDate(request.ReportedAt == default ? null : request.ReportedAt, lastReportedAt, now, "reportedAt", out error))
        {
            return false;
        }

        if (request.Progress is < 0 or > 100)
        {
            error = "progress must be between 0 and 100";
            return false;
        }

        if (request.Status == EdgeTaskStatus.Running && request.Progress == null)
        {
            error = "progress is required when status is Running";
            return false;
        }

        if (request.Status is EdgeTaskStatus.Succeeded or EdgeTaskStatus.Failed or EdgeTaskStatus.TimedOut or EdgeTaskStatus.Cancelled
            && request.Progress is > 0 and < 100)
        {
            error = "terminal status progress should be null, 0 or 100";
            return false;
        }

        return ValidateObjectMetadata(request.Result, MaxMetadataEntries, MaxMetricValueLength, "result", out error)
            && ValidateStringMetadata(request.Metadata, out error);
    }

    private static bool ValidateDate(DateTime? value, DateTime? previous, DateTime now, string name, out string error)
    {
        error = string.Empty;
        if (!value.HasValue)
        {
            return true;
        }

        var utc = value.Value.ToUniversalTime();
        if (utc > now.ToUniversalTime().Add(MaxClockLead))
        {
            error = $"{name} cannot be more than {MaxClockLead.TotalMinutes:0} minutes in the future";
            return false;
        }

        if (previous.HasValue && utc < previous.Value.ToUniversalTime())
        {
            error = $"{name} must not be older than the previous report";
            return false;
        }

        return true;
    }

    private static bool ValidateRequired(string? value, string name, out string error)
        => ValidateOptional(value, name, MaxIdentityLength, out error, required: true);

    private static bool ValidateOptional(string? value, string name, out string error)
        => ValidateOptional(value, name, MaxIdentityLength, out error, required: false);

    private static bool ValidateOptional(string? value, string name, int maxLength, out string error)
        => ValidateOptional(value, name, maxLength, out error, required: false);

    private static bool ValidateOptional(string? value, string name, int maxLength, out string error, bool required)
    {
        if (required && string.IsNullOrWhiteSpace(value))
        {
            error = $"{name} is required";
            return false;
        }

        if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
        {
            error = $"{name} cannot exceed {maxLength} characters";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool ValidateStringList(IEnumerable<string>? values, int maxEntries, string name, out string error)
    {
        var items = values?.ToArray() ?? [];
        if (items.Length > maxEntries)
        {
            error = $"{name} cannot contain more than {maxEntries} items";
            return false;
        }

        foreach (var item in items)
        {
            if (!ValidateOptional(item, name, out error))
            {
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    private static bool ValidateEnumList<TEnum>(IEnumerable<TEnum>? values, int maxEntries, string name, out string error)
        where TEnum : struct, Enum
    {
        if (values is ICollection<TEnum> collection && collection.Count > maxEntries)
        {
            error = $"{name} cannot contain more than {maxEntries} items";
            return false;
        }

        var count = 0;
        foreach (var _ in values ?? [])
        {
            count++;
            if (count > maxEntries)
            {
                error = $"{name} cannot contain more than {maxEntries} items";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    private static bool ValidateStringMetadata(IReadOnlyDictionary<string, string>? values, out string error)
    {
        if (values == null)
        {
            error = string.Empty;
            return true;
        }

        if (values.Count > MaxMetadataEntries)
        {
            error = $"metadata cannot contain more than {MaxMetadataEntries} items";
            return false;
        }

        foreach (var pair in values)
        {
            if (!ValidateOptional(pair.Key, "metadata.key", MaxKeyLength, out error, required: true)
                || !ValidateOptional(pair.Value, "metadata.value", MaxMetadataValueLength, out error))
            {
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    private static bool ValidateObjectMetadata(
        IReadOnlyDictionary<string, object>? values,
        int maxEntries,
        int maxValueLength,
        string name,
        out string error)
    {
        if (values == null)
        {
            error = string.Empty;
            return true;
        }

        if (values.Count > maxEntries)
        {
            error = $"{name} cannot contain more than {maxEntries} items";
            return false;
        }

        foreach (var pair in values)
        {
            if (!ValidateOptional(pair.Key, $"{name}.key", MaxKeyLength, out error, required: true))
            {
                return false;
            }

            try
            {
                var json = JsonSerializer.Serialize(pair.Value);
                if (json.Length > maxValueLength)
                {
                    error = $"{name}.{pair.Key} exceeds {maxValueLength} serialized characters";
                    return false;
                }
            }
            catch (JsonException)
            {
                error = $"{name}.{pair.Key} is not valid JSON";
                return false;
            }
            catch (NotSupportedException)
            {
                error = $"{name}.{pair.Key} is not supported JSON";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }
}
