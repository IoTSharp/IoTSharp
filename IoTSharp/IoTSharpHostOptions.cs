namespace IoTSharp
{
    /// <summary>
    /// 宿主层环境配置，统一承接 Kestrel、ACME 和健康检查端点需要的启动参数。
    /// </summary>
    internal sealed class IoTSharpHostOptions
    {
        /// <summary>
        /// 是否启用 ACME 自动证书相关监听与服务。
        /// </summary>
        public bool IOTSHARP_ACME { get; set; }

        /// <summary>
        /// ASP.NET Core URL 监听配置。
        /// </summary>
        public string ASPNETCORE_URLS { get; set; }

        /// <summary>
        /// ASP.NET Core HTTP 端口配置。
        /// </summary>
        public string ASPNETCORE_HTTP_PORTS { get; set; }

        /// <summary>
        /// ASP.NET Core HTTPS 端口配置。
        /// </summary>
        public string ASPNETCORE_HTTPS_PORTS { get; set; }

        /// <summary>
        /// 当前进程是否运行在容器中。
        /// </summary>
        public bool DOTNET_RUNNING_IN_CONTAINER { get; set; }
    }
}
