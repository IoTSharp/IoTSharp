using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Data.SonnetDB
{
    public sealed class SonnetDbHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _context;

        public SonnetDbHealthCheck(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
                return canConnect
                    ? HealthCheckResult.Healthy("SonnetDB relational storage is reachable.")
                    : HealthCheckResult.Unhealthy("SonnetDB relational storage is not reachable.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("SonnetDB relational storage check failed.", ex);
            }
        }
    }
}
