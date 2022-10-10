using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using HealthChecks.UI.Core.Data;
using k8s.KubeConfigModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using IoTSharp.Contracts;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthChecksController : ControllerBase
    {
        private IServiceScopeFactory __serviceScopeFactory;

        public HealthChecksController(IServiceScopeFactory scopeFactor)
        {
            __serviceScopeFactory = scopeFactor;
        }

        /// 返回指定id的客户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ApiResult<List<HealthCheckExecution>>> Get()
        {

            using (var scope = __serviceScopeFactory.CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<HealthChecksDb>())
            {
                var healthChecks = await db.Configurations.ToListAsync();

                var healthChecksExecutions = new List<HealthCheckExecution>();

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
                return new ApiResult<List<HealthCheckExecution>>(ApiCode.Success, "OK", healthChecksExecutions);
            }
        }
    }
}
