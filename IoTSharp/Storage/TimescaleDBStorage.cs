using IoTSharp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silkier.EFCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IoTSharp.Storage
{
    public class TimescaleDBStorage : EFStorage
    {

        /// <summary>
        /// 解决单例注入问题 jy 
        /// </summary>
        readonly IServiceScopeFactory _scopeFactor;

        public TimescaleDBStorage(ILogger<TimescaleDBStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options
            ) : base(logger, scopeFactor, options)
        {
        
            _scopeFactor = scopeFactor;
        }

        private volatile bool _needcrtate = false;

        public override Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(RawMsg msg)
        {
            if (!_needcrtate)
            {
                //解决单例注入问题 jy 
                using var scope = _scopeFactor.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); 
                var have = context.Database.ExecuteScalar<long>("SELECT  count(0) FROM _timescaledb_catalog.hypertable where	 table_name='TelemetryData';");
                if (have == 0)
                {
                    context.Database.ExecuteNonQuery("SELECT create_hypertable('\"TelemetryData\"', 'DateTime', 'DeviceId', 2, create_default_indexes=>FALSE);");
                    context.Database.ExecuteNonQuery("CREATE INDEX ON \"TelemetryData\" (\"KeyName\", \"DateTime\" DESC);");
                    context.Database.ExecuteNonQuery("CREATE INDEX ON \"TelemetryData\" (\"DataSide\", \"DateTime\" DESC);");
                    context.Database.ExecuteNonQuery("CREATE INDEX ON \"TelemetryData\" (\"Type\", \"DateTime\" DESC);");
                }
                else
                {
                    _needcrtate = true;
                }
            }
            return base.StoreTelemetryAsync(msg);
        }
    }
}