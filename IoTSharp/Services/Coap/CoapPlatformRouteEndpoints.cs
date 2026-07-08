using CoAP;
using CoAP.Server.Resources;
using CoAP.Server.Routing;
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
                CoAP.Server.Routing.CoapRoute.Post(template.Template, context => DispatchAsync(dispatcher, template, context)));
            return CoapRouteEndpoint.Create(routes);
        }

        private static async ValueTask<CoapRouteResult> DispatchAsync(
            ICoapBusinessDispatcher dispatcher,
            CoapPlatformRouteTemplate template,
            CoapRouteContext context)
        {
            var routeValueName = CoapPlatformRouteConventions.GetTargetRouteValueName(template.TargetKind);
            var targetName = context.RouteValues.TryGetValue(routeValueName, out var value) ? value : string.Empty;
            var businessContext = new CoapBusinessDispatchContext(
                new CoapPlatformRouteMatch(template.TargetKind, targetName, template.Operation),
                context);
            var result = await dispatcher.DispatchAsync(businessContext).ConfigureAwait(false);
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
