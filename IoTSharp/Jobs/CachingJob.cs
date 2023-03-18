using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SilkierQuartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Jobs
{
    [SilkierQuartz(1,0)]
    public class CachingJob : IJob
    {
        private readonly ILogger<CachingJob> _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IEasyCachingProvider _caching;

        public CachingJob(ILogger<CachingJob> logger, IServiceScopeFactory scopeFactor ,  IEasyCachingProviderFactory factory, IOptions<AppSettings> options)
        {
            _logger = logger;
            _scopeFactor = scopeFactor;
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(options.Value.CachingUseIn)}";
            _caching = factory.GetCachingProvider(_hc_Caching);

        }
        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _scopeFactor.CreateScope())
            using (var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
            {
                try
                {
                    var tdds = await _db.Tenant.Select(t => t.Id).ToListAsync();
                    tdds.ForEach(t =>
                    {
                        _caching.GetKanBanCache(t, _db);
                    });
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, "处理看板缓存时遇到问题。");
                }
            }
        }
    }
}
