using System;
using System.Collections.Generic;
using IoTSharp.Contracts;
using Xunit;

namespace IoTSharp.Test;

/// <summary>
/// Edge 运行时上报合同的输入边界和时间顺序回归测试。
/// </summary>
public sealed class EdgeRuntimeContractValidationTests
{
    [Fact]
    public void RegistrationRequiresRuntimeTypeAndBoundsMetadata()
    {
        var request = new EdgeRegistrationDto
        {
            RuntimeType = string.Empty
        };

        Assert.False(EdgeRuntimeContractValidation.TryValidateRegistration(request, out var missingType));
        Assert.Contains("runtimeType", missingType, StringComparison.OrdinalIgnoreCase);

        request.RuntimeType = "gateway";
        request.Metadata = new Dictionary<string, string>
        {
            ["profile"] = new string('x', EdgeRuntimeContractValidation.MaxMetadataValueLength + 1)
        };

        Assert.False(EdgeRuntimeContractValidation.TryValidateRegistration(request, out var oversizedMetadata));
        Assert.Contains("metadata.value", oversizedMetadata, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HeartbeatRejectsOlderAndFutureTimestamps()
    {
        var now = DateTime.UtcNow;
        var request = new EdgeHeartbeatDto { Timestamp = now.AddSeconds(-2) };

        Assert.False(EdgeRuntimeContractValidation.TryValidateHeartbeat(
            request,
            now.AddSeconds(-1),
            now,
            out var staleError));
        Assert.Contains("older", staleError, StringComparison.OrdinalIgnoreCase);

        request.Timestamp = now.Add(EdgeRuntimeContractValidation.MaxClockLead).AddSeconds(1);
        Assert.False(EdgeRuntimeContractValidation.TryValidateHeartbeat(request, null, now, out var futureError));
        Assert.Contains("future", futureError, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CapabilityAndReceiptPayloadsRejectUnboundedCollections()
    {
        var capability = new EdgeCapabilityReportDto
        {
            Features = CreateStrings(EdgeRuntimeContractValidation.MaxCapabilityEntries + 1)
        };

        Assert.False(EdgeRuntimeContractValidation.TryValidateCapabilities(capability, null, DateTime.UtcNow, out var capabilityError));
        Assert.Contains("features", capabilityError, StringComparison.OrdinalIgnoreCase);

        var receipt = new EdgeTaskReceiptDto
        {
            TargetKey = "gateway:gateway",
            Status = EdgeTaskStatus.Running,
            Progress = 50,
            Result = new Dictionary<string, object>
            {
                ["diagnostic"] = new string('x', EdgeRuntimeContractValidation.MaxMetricValueLength + 1)
            }
        };

        Assert.False(EdgeRuntimeContractValidation.TryValidateReceipt(receipt, null, DateTime.UtcNow, out var receiptError));
        Assert.Contains("result.diagnostic", receiptError, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CapabilityAndReceiptReportsRejectOlderSnapshots()
    {
        var now = DateTime.UtcNow;
        var capability = new EdgeCapabilityReportDto { ReportedAt = now.AddSeconds(-2) };
        Assert.False(EdgeRuntimeContractValidation.TryValidateCapabilities(
            capability,
            now.AddSeconds(-1),
            now,
            out var capabilityError));
        Assert.Contains("older", capabilityError, StringComparison.OrdinalIgnoreCase);

        var receipt = new EdgeTaskReceiptDto
        {
            TargetKey = "gateway:gateway",
            Status = EdgeTaskStatus.Running,
            Progress = 10,
            ReportedAt = now.AddSeconds(-2)
        };
        Assert.False(EdgeRuntimeContractValidation.TryValidateReceipt(
            receipt,
            now.AddSeconds(-1),
            now,
            out var receiptError));
        Assert.Contains("older", receiptError, StringComparison.OrdinalIgnoreCase);
    }

    private static string[] CreateStrings(int count)
    {
        var values = new string[count];
        for (var index = 0; index < values.Length; index++)
        {
            values[index] = $"feature-{index}";
        }

        return values;
    }
}
