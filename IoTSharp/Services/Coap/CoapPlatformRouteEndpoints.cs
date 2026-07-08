using CoAP;
using CoAP.Server.Resources;
using CoAP.Server.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Services.Coap
{
    /// <summary>
    /// 将 IoTSharp 平台入口约定注册到 CoAP.NET route adapter。
    /// </summary>
    internal static class CoapPlatformRouteEndpoints
    {
        /// <summary>
        /// 创建 IoTSharp 平台 CoAP 入口的 Resource 根节点。
        /// </summary>
        /// <param name="dispatcher">CoAP 业务分发服务。</param>
        /// <returns>用于注册到 CoAP.NET server 的 Resource 根节点。</returns>
        public static IReadOnlyList<IResource> Create(ICoapBusinessDispatcher dispatcher)
        {
            var routes = CoapPlatformRouteConventions.RecommendedRoutes.Select(template =>
                CoAP.Server.Routing.CoapRoute.Post(
                    template.Template,
                    () => new CoapPlatformResource(dispatcher, template),
                    resource => resource.DispatchAsync()));
            return CoapRouteEndpoint.Create(routes);
        }

        private sealed class CoapPlatformResource : CoapResourceBase
        {
            private readonly ICoapBusinessDispatcher _dispatcher;
            private readonly CoapPlatformRouteTemplate _template;

            /// <summary>
            /// 创建 IoTSharp 平台入口 resource。
            /// </summary>
            /// <param name="dispatcher">CoAP 业务分发服务。</param>
            /// <param name="template">平台入口模板。</param>
            public CoapPlatformResource(ICoapBusinessDispatcher dispatcher, CoapPlatformRouteTemplate template)
            {
                _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
                _template = template ?? throw new ArgumentNullException(nameof(template));
            }

            /// <summary>
            /// 将当前 CoAP route 上下文转换为平台业务上下文并分发。
            /// </summary>
            /// <returns>可由 CoAP.NET route adapter 写回的协议响应。</returns>
            public async ValueTask<CoapRouteResult> DispatchAsync()
            {
                var routeValueName = CoapPlatformRouteConventions.GetTargetRouteValueName(_template.TargetKind);
                var targetName = RouteValues.TryGetValue(routeValueName, out var value) ? value : string.Empty;
                var businessContext = new CoapBusinessDispatchContext(
                    new CoapPlatformRouteMatch(_template.TargetKind, targetName, _template.Operation),
                    Context);
                var result = await _dispatcher.DispatchAsync(businessContext).ConfigureAwait(false);
                return ToCoapResult(result);
            }

            private static CoapRouteResult ToCoapResult(CoapBusinessDispatchResult result)
            {
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
}
