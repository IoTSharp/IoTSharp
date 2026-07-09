using CoAP;
using CoAP.Server.Routing;
using System;
using System.Threading.Tasks;

namespace IoTSharp.Services.Coap
{
    /// <summary>
    /// IoTSharp 设备侧 CoAP 写入入口。
    /// </summary>
    [CoapResource]
    [CoapRoute(CoapPlatformRouteConventions.DevicesRoot + "/{" + CoapPlatformRouteConventions.DeviceRouteValueName + "}")]
    [CoapConsumes(MediaType.Undefined, MediaType.ApplicationJson, MediaType.TextPlain)]
    [CoapProduces(MediaType.TextPlain, MediaType.ApplicationJson)]
    [CoapResourceTitle("IoTSharp device access")]
    [CoapResourceType("iotsharp.device")]
    [CoapInterfaceDescription("iotsharp.access")]
    public sealed class DeviceCoapResource : CoapResourceBase
    {
        private readonly ICoapBusinessDispatcher _dispatcher;

        /// <summary>
        /// 创建设备侧 CoAP resource。
        /// </summary>
        /// <param name="dispatcher">平台业务分发服务。</param>
        public DeviceCoapResource(ICoapBusinessDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        /// <summary>
        /// 写入设备遥测数据。
        /// </summary>
        /// <param name="device">路由中的设备名称或设备 ID。</param>
        /// <param name="context">CoAP.NET 已匹配的请求上下文。</param>
        /// <returns>映射后的 CoAP 响应。</returns>
        [CoapPost(CoapPlatformRouteConventions.TelemetrySegment)]
        [CoapResourceTitle("Device telemetry upload")]
        public ValueTask<CoapRouteResult> UploadTelemetryAsync(string device, CoapRouteContext context)
        {
            return DispatchAsync(device, CoapPlatformOperation.Telemetry, context);
        }

        /// <summary>
        /// 写入设备属性数据。
        /// </summary>
        /// <param name="device">路由中的设备名称或设备 ID。</param>
        /// <param name="context">CoAP.NET 已匹配的请求上下文。</param>
        /// <returns>映射后的 CoAP 响应。</returns>
        [CoapPost(CoapPlatformRouteConventions.AttributesSegment)]
        [CoapResourceTitle("Device attributes upload")]
        public ValueTask<CoapRouteResult> UploadAttributesAsync(string device, CoapRouteContext context)
        {
            return DispatchAsync(device, CoapPlatformOperation.Attributes, context);
        }

        /// <summary>
        /// 写入设备告警数据。
        /// </summary>
        /// <param name="device">路由中的设备名称或设备 ID。</param>
        /// <param name="context">CoAP.NET 已匹配的请求上下文。</param>
        /// <returns>映射后的 CoAP 响应。</returns>
        [CoapPost(CoapPlatformRouteConventions.AlarmSegment)]
        [CoapResourceTitle("Device alarm upload")]
        public ValueTask<CoapRouteResult> UploadAlarmAsync(string device, CoapRouteContext context)
        {
            return DispatchAsync(device, CoapPlatformOperation.Alarm, context);
        }

        private ValueTask<CoapRouteResult> DispatchAsync(
            string device,
            CoapPlatformOperation operation,
            CoapRouteContext context)
        {
            var route = new CoapPlatformRouteMatch(CoapPlatformTargetKind.Device, device, operation);
            return CoapPlatformResourceDispatcher.DispatchAsync(_dispatcher, route, context);
        }
    }

    /// <summary>
    /// IoTSharp 网关侧 CoAP 写入入口。
    /// </summary>
    [CoapResource]
    [CoapRoute(CoapPlatformRouteConventions.GatewaysRoot + "/{" + CoapPlatformRouteConventions.GatewayRouteValueName + "}")]
    [CoapConsumes(MediaType.Undefined, MediaType.ApplicationJson, MediaType.TextPlain)]
    [CoapProduces(MediaType.TextPlain, MediaType.ApplicationJson)]
    [CoapResourceTitle("IoTSharp gateway access")]
    [CoapResourceType("iotsharp.gateway")]
    [CoapInterfaceDescription("iotsharp.access")]
    public sealed class GatewayCoapResource : CoapResourceBase
    {
        private readonly ICoapBusinessDispatcher _dispatcher;

        /// <summary>
        /// 创建网关侧 CoAP resource。
        /// </summary>
        /// <param name="dispatcher">平台业务分发服务。</param>
        public GatewayCoapResource(ICoapBusinessDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        /// <summary>
        /// 写入网关遥测数据。
        /// </summary>
        /// <param name="gateway">路由中的网关名称或网关设备 ID。</param>
        /// <param name="context">CoAP.NET 已匹配的请求上下文。</param>
        /// <returns>映射后的 CoAP 响应。</returns>
        [CoapPost(CoapPlatformRouteConventions.TelemetrySegment)]
        [CoapResourceTitle("Gateway telemetry upload")]
        public ValueTask<CoapRouteResult> UploadTelemetryAsync(string gateway, CoapRouteContext context)
        {
            return DispatchAsync(gateway, CoapPlatformOperation.Telemetry, context);
        }

        /// <summary>
        /// 写入网关属性数据。
        /// </summary>
        /// <param name="gateway">路由中的网关名称或网关设备 ID。</param>
        /// <param name="context">CoAP.NET 已匹配的请求上下文。</param>
        /// <returns>映射后的 CoAP 响应。</returns>
        [CoapPost(CoapPlatformRouteConventions.AttributesSegment)]
        [CoapResourceTitle("Gateway attributes upload")]
        public ValueTask<CoapRouteResult> UploadAttributesAsync(string gateway, CoapRouteContext context)
        {
            return DispatchAsync(gateway, CoapPlatformOperation.Attributes, context);
        }

        private ValueTask<CoapRouteResult> DispatchAsync(
            string gateway,
            CoapPlatformOperation operation,
            CoapRouteContext context)
        {
            var route = new CoapPlatformRouteMatch(CoapPlatformTargetKind.Gateway, gateway, operation);
            return CoapPlatformResourceDispatcher.DispatchAsync(_dispatcher, route, context);
        }
    }

    internal static class CoapPlatformResourceDispatcher
    {
        /// <summary>
        /// 将 CoAP.NET resource action 上下文转交给 IoTSharp 平台业务分发服务。
        /// </summary>
        /// <param name="dispatcher">平台业务分发服务。</param>
        /// <param name="route">平台入口匹配信息。</param>
        /// <param name="context">CoAP.NET 已匹配的请求上下文。</param>
        /// <returns>映射后的 CoAP 响应。</returns>
        public static async ValueTask<CoapRouteResult> DispatchAsync(
            ICoapBusinessDispatcher dispatcher,
            CoapPlatformRouteMatch route,
            CoapRouteContext context)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            if (context == null)
            {
                return CoapRouteResult.Text(StatusCode.BadRequest, "CoAP route context is required.");
            }

            var businessContext = new CoapBusinessDispatchContext(route, context);
            var result = await dispatcher.DispatchAsync(businessContext).ConfigureAwait(false);
            return ToCoapResult(result);
        }

        private static CoapRouteResult ToCoapResult(CoapBusinessDispatchResult result)
        {
            if (result == null)
            {
                return CoapRouteResult.Text(StatusCode.InternalServerError, "CoAP business dispatch returned no result.");
            }

            var statusCode = result.Status switch
            {
                CoapBusinessDispatchStatus.Success => StatusCode.Changed,
                CoapBusinessDispatchStatus.BadRequest => StatusCode.BadRequest,
                CoapBusinessDispatchStatus.Unauthorized => StatusCode.Unauthorized,
                CoapBusinessDispatchStatus.Forbidden => StatusCode.Forbidden,
                CoapBusinessDispatchStatus.NotFound => StatusCode.NotFound,
                CoapBusinessDispatchStatus.NotAcceptable => StatusCode.NotAcceptable,
                CoapBusinessDispatchStatus.UnsupportedOperation => StatusCode.NotImplemented,
                _ => StatusCode.InternalServerError
            };

            return result.Status == CoapBusinessDispatchStatus.Success
                ? CoapRouteResult.Status(statusCode)
                : CoapRouteResult.Text(statusCode, result.Message);
        }
    }
}
