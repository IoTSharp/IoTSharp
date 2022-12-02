using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.EventBus;
using IoTSharp.FlowRuleEngine;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client.Extensions.Msal;
using MongoDB.Driver.Core.Servers;
using System;
using System.Threading.Tasks;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// 运行指标信息
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPublisher _queue;
        private IEasyCachingProvider _caching;

        public MetricsController( ILogger<MetricsController> logger, IPublisher queue, IEasyCachingProviderFactory factory, IOptions<AppSettings> options)
        {
            _logger = logger;
            _queue = queue;
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(options.Value.CachingUseIn)}";
            _caching = factory.GetCachingProvider(_hc_Caching);
        }
        /// <summary>
        /// 返回事件总线的统计信息
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<EventBusMetrics>> EventBus()
        {
            try
            {
                var data = await _caching.GetAsync(nameof(EventBusMetrics), async ()=> await _queue.GetMetrics(),TimeSpan.FromMinutes(1));
                return new ApiResult<EventBusMetrics>(ApiCode.Success, "Ok", data.Value);
            }
            catch (Exception ex)
            {
                return new ApiResult<EventBusMetrics>(ApiCode.Exception, ex.Message, null);
            }
        }

    }
}
