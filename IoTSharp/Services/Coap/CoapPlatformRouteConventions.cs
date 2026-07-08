using System;
using System.Collections.Generic;

namespace IoTSharp.Services.Coap
{
    /// <summary>
    /// CoAP 平台入口面向的目标类型。
    /// </summary>
    public enum CoapPlatformTargetKind
    {
        Device,
        Gateway
    }

    /// <summary>
    /// CoAP 平台入口面向的业务写入动作。
    /// </summary>
    public enum CoapPlatformOperation
    {
        Telemetry,
        Attributes,
        Alarm
    }

    /// <summary>
    /// 描述 IoTSharp 推荐的 CoAP 平台入口模板。
    /// </summary>
    public sealed class CoapPlatformRouteTemplate
    {
        /// <summary>
        /// 创建一个平台入口模板描述。
        /// </summary>
        /// <param name="template">推荐的 CoAP Uri-Path 模板。</param>
        /// <param name="targetKind">入口指向的平台目标类型。</param>
        /// <param name="operation">入口指向的业务写入动作。</param>
        public CoapPlatformRouteTemplate(string template, CoapPlatformTargetKind targetKind, CoapPlatformOperation operation)
        {
            Template = template;
            TargetKind = targetKind;
            Operation = operation;
        }

        /// <summary>
        /// 推荐的 CoAP Uri-Path 模板。
        /// </summary>
        public string Template { get; }

        /// <summary>
        /// route 指向的平台目标类型。
        /// </summary>
        public CoapPlatformTargetKind TargetKind { get; }

        /// <summary>
        /// route 指向的业务写入动作。
        /// </summary>
        public CoapPlatformOperation Operation { get; }
    }

    /// <summary>
    /// CoAP 平台入口匹配后的业务信息。
    /// </summary>
    public readonly struct CoapPlatformRouteMatch
    {
        /// <summary>
        /// 创建一个平台入口匹配结果。
        /// </summary>
        /// <param name="targetKind">入口指向的平台目标类型。</param>
        /// <param name="targetName">入口中的设备或网关名称。</param>
        /// <param name="operation">入口指向的业务写入动作。</param>
        public CoapPlatformRouteMatch(CoapPlatformTargetKind targetKind, string targetName, CoapPlatformOperation operation)
        {
            TargetKind = targetKind;
            TargetName = targetName;
            Operation = operation;
        }

        /// <summary>
        /// 入口指向的平台目标类型。
        /// </summary>
        public CoapPlatformTargetKind TargetKind { get; }

        /// <summary>
        /// 入口中的设备或网关名称。
        /// </summary>
        public string TargetName { get; }

        /// <summary>
        /// 入口指向的业务写入动作。
        /// </summary>
        public CoapPlatformOperation Operation { get; }
    }

    /// <summary>
    /// IoTSharp 平台侧 CoAP 入口命名和 Uri-Path 约定。
    /// </summary>
    public static class CoapPlatformRouteConventions
    {
        public const string DevicesRoot = "devices";
        public const string GatewaysRoot = "gateways";
        public const string DeviceRouteValueName = "device";
        public const string GatewayRouteValueName = "gateway";
        public const string TelemetrySegment = "telemetry";
        public const string AttributesSegment = "attributes";
        public const string AlarmSegment = "alarm";

        private static readonly CoapPlatformRouteTemplate[] Routes = new[]
        {
            new CoapPlatformRouteTemplate("devices/{device}/telemetry", CoapPlatformTargetKind.Device, CoapPlatformOperation.Telemetry),
            new CoapPlatformRouteTemplate("devices/{device}/attributes", CoapPlatformTargetKind.Device, CoapPlatformOperation.Attributes),
            new CoapPlatformRouteTemplate("devices/{device}/alarm", CoapPlatformTargetKind.Device, CoapPlatformOperation.Alarm),
            new CoapPlatformRouteTemplate("gateways/{gateway}/telemetry", CoapPlatformTargetKind.Gateway, CoapPlatformOperation.Telemetry),
            new CoapPlatformRouteTemplate("gateways/{gateway}/attributes", CoapPlatformTargetKind.Gateway, CoapPlatformOperation.Attributes)
        };

        /// <summary>
        /// 平台推荐的 CoAP 写入入口模板。
        /// </summary>
        public static IReadOnlyList<CoapPlatformRouteTemplate> RecommendedRoutes => Routes;

        /// <summary>
        /// 按平台推荐约定匹配 CoAP Uri-Path 片段。
        /// </summary>
        /// <param name="segments">CoAP Uri-Path option 拆分后的路径片段。</param>
        /// <param name="match">匹配成功时返回业务入口信息。</param>
        /// <returns>路径符合平台推荐约定时返回 true。</returns>
        public static bool TryMatch(IReadOnlyList<string> segments, out CoapPlatformRouteMatch match)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            match = default;
            if (segments.Count != 3 || string.IsNullOrWhiteSpace(segments[1]))
            {
                return false;
            }

            if (!TryGetTargetKind(segments[0], out var targetKind))
            {
                return false;
            }

            if (!TryGetOperation(segments[2], targetKind, out var operation))
            {
                return false;
            }

            match = new CoapPlatformRouteMatch(targetKind, segments[1], operation);
            return true;
        }

        internal static string GetTargetRouteValueName(CoapPlatformTargetKind targetKind)
        {
            return targetKind == CoapPlatformTargetKind.Device ? DeviceRouteValueName : GatewayRouteValueName;
        }

        private static bool TryGetTargetKind(string segment, out CoapPlatformTargetKind targetKind)
        {
            if (string.Equals(segment, DevicesRoot, StringComparison.Ordinal))
            {
                targetKind = CoapPlatformTargetKind.Device;
                return true;
            }

            if (string.Equals(segment, GatewaysRoot, StringComparison.Ordinal))
            {
                targetKind = CoapPlatformTargetKind.Gateway;
                return true;
            }

            targetKind = default;
            return false;
        }

        private static bool TryGetOperation(string segment, CoapPlatformTargetKind targetKind, out CoapPlatformOperation operation)
        {
            if (string.Equals(segment, TelemetrySegment, StringComparison.Ordinal))
            {
                operation = CoapPlatformOperation.Telemetry;
                return true;
            }

            if (string.Equals(segment, AttributesSegment, StringComparison.Ordinal))
            {
                operation = CoapPlatformOperation.Attributes;
                return true;
            }

            if (targetKind == CoapPlatformTargetKind.Device && string.Equals(segment, AlarmSegment, StringComparison.Ordinal))
            {
                operation = CoapPlatformOperation.Alarm;
                return true;
            }

            operation = default;
            return false;
        }
    }
}
