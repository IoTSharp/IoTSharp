using Castle.Core.Logging;
using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IoTSharp.Storage
{
    public class TimescaleDBStorage : EFStorage
    {

        readonly IServiceScopeFactory _scopeFactor;
       private Microsoft.Extensions.Logging.ILogger _logger;
        public TimescaleDBStorage(ILogger<TimescaleDBStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options
            ) : base(logger, scopeFactor, options)
        {
            _scopeFactor = scopeFactor;
            _logger = logger;
        }
        public override async Task<bool> CheckTelemetryStorage()
        {
           
            var ok=  await base.CheckTelemetryStorage();
            if (ok)
            {
                using var scope = _scopeFactor.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var have = context.Database.ExecuteScalar<long>("SELECT  count(0) FROM _timescaledb_catalog.hypertable where table_name='TelemetryData';");
                if (have == 0)
                {
                    context.Database.ExecuteNonQuery("SELECT create_hypertable('\"TelemetryData\"', 'DateTime', 'DeviceId', 2, create_default_indexes=>FALSE);");
                    context.Database.ExecuteNonQuery("CREATE INDEX ON \"TelemetryData\" (\"KeyName\", \"DateTime\" DESC);");
                    context.Database.ExecuteNonQuery("CREATE INDEX ON \"TelemetryData\" (\"DataSide\", \"DateTime\" DESC);");
                    context.Database.ExecuteNonQuery("CREATE INDEX ON \"TelemetryData\" (\"Type\", \"DateTime\" DESC);");
                    ok = true;
                }
            }
            return ok;
        }
    }
}