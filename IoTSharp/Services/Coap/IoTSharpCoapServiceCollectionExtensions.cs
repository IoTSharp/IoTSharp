using Microsoft.Extensions.DependencyInjection;
using System;

namespace IoTSharp.Services.Coap
{
    /// <summary>
    /// IoTSharp CoAP 业务入口注册扩展；协议宿主、传输监听和资源映射由 CoAP.NET 提供。
    /// </summary>
    internal static class IoTSharpCoapServiceCollectionExtensions
    {
        /// <summary>
        /// 注册 IoTSharp 设备和网关 CoAP 业务 resource。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>服务集合。</returns>
        public static IServiceCollection AddIoTSharpCoapResources(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<ICoapBusinessDispatcher, CoapBusinessDispatcher>();
            services.AddCoapResources(options => options.AddReflectionResourceDiscovery());
            services.AddCoapJsonPayloadBinder(CoapJsonSerializerContext.Shared);
            return services;
        }
    }
}
