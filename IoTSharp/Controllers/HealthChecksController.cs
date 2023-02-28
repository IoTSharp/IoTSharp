using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using IoTSharp.Contracts;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.Extensions.Options;
using EasyCaching.Core;
using HealthChecks.UI.Data;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// 健康检查
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HealthChecksController : ControllerBase
    {
        private readonly IEasyCachingProvider _caching;
        private readonly JsonSerializerSettings _jsonSerializationSettings;
        private IServiceScopeFactory __serviceScopeFactory;

        public HealthChecksController(IServiceScopeFactory scopeFactor, IEasyCachingProviderFactory factory, IOptions<AppSettings> options)
        {
            __serviceScopeFactory = scopeFactor;
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(options.Value.CachingUseIn)}";
            _caching = factory.GetCachingProvider(_hc_Caching);
            _jsonSerializationSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new[] { new StringEnumConverter() },
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };
        }
        ///<summary>
        /// 获取相关服务的健康检查信息
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Get()
        {
            var data = await _caching.GetAsync("HealthChecks", async () =>
              {
                  var healthChecksExecutions = new List<HealthCheckExecution>();
                  using (var scope = __serviceScopeFactory.CreateScope())
                  using (var db = scope.ServiceProvider.GetRequiredService<HealthChecksDb>())
                  {
                      var healthChecks = await db.Configurations.ToListAsync();



                      foreach (var item in healthChecks.OrderBy(h => h.Id))
                      {
                          var execution = await db.Executions
                                      .Include(le => le.Entries)
                                      .Where(le => le.Name == item.Name)
                                      .AsNoTracking()
                                      .SingleOrDefaultAsync();
                          if (execution != null)
                          {
                              execution.History = await db.HealthCheckExecutionHistories
                                  .Where(eh => EF.Property<int>(eh, "HealthCheckExecutionId") == execution.Id)
                                  .OrderByDescending(eh => eh.On)
                                  .Take(10)
                                  .ToListAsync();

                              healthChecksExecutions.Add(execution);
                          }
                      }

                  }
                  return healthChecksExecutions;
              }, TimeSpan.FromMinutes(10));
            return new JsonResult(data.Value, _jsonSerializationSettings);
        }
    }
}
