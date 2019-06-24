using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IoTSharp.Diagnostics
{
    public class DiagnosticsService : IHostedService
    {
        private readonly List<OperationsPerSecondCounter> _operationsPerSecondCounters = new List<OperationsPerSecondCounter>();
        
        private readonly ILogger _logger;

        public DiagnosticsService(ILogger<DiagnosticsService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
         return    ResetOperationsPerSecondCountersAsync(cancellationToken);
        }
        

        public OperationsPerSecondCounter CreateOperationsPerSecondCounter(string uid)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            var operationsPerSecondCounter = new OperationsPerSecondCounter(uid);

            lock (_operationsPerSecondCounters)
            {
                _operationsPerSecondCounters.Add(operationsPerSecondCounter);
            }

            return operationsPerSecondCounter;
        }

        private async Task ResetOperationsPerSecondCountersAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);

                    lock (_operationsPerSecondCounters)
                    {
                        foreach (var operationsPerSecondCounter in _operationsPerSecondCounters)
                        {
                            operationsPerSecondCounter.Reset();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Error while resetting operations per second counters.");
                }
            }
        }


      
        
 
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("DiagnosticsService stop !");
            return Task.CompletedTask;
        }
    }
}
