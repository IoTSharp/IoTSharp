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
        private readonly ApplicationDbContext _context;

        public TimescaleDBStorage(ILogger<TimescaleDBStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options
            ) : base(logger, scopeFactor, options)
        {
            var scope = scopeFactor.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        private bool _needcrtate = false;

        public override async   Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(RawMsg msg)
        {
            if (!_needcrtate)
            {
                var have = _context.Database.ExecuteScalar<long>("SELECT  count(0) FROM _timescaledb_catalog.hypertable where	 table_name='TelemetryData';");
                if (have == 0)
                {
                    _context.Database.ExecuteNonQuery("SELECT create_hypertable('\"TelemetryData\"', 'DateTime', 'DeviceId', 2, create_default_indexes=>FALSE);");
                    _context.Database.ExecuteNonQuery("CREATE INDEX ON \"TelemetryData\" (\"KeyName\", \"DateTime\" DESC);");
                    _context.Database.ExecuteNonQuery("CREATE INDEX ON \"TelemetryData\" (\"DataSide\", \"DateTime\" DESC);");
                    _context.Database.ExecuteNonQuery("CREATE INDEX ON \"TelemetryData\" (\"Type\", \"DateTime\" DESC);");
                }
                else
                {
                    _needcrtate = true;
                }
            }
            return await base.StoreTelemetryAsync(msg);
        }
    }
}