using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Sys;
using Microsoft.Extensions.Logging;

namespace IoTSharp.Diagnostics
{
    public class DiagnosticsService : IService
    {
        private readonly List<OperationsPerSecondCounter> _operationsPerSecondCounters = new List<OperationsPerSecondCounter>();
        private readonly SystemCancellationToken _systemCancellationToken;
        
        private readonly ILogger _logger;

        public DiagnosticsService(SystemCancellationToken systemCancellationToken, ILogger<DiagnosticsService> logger)
        {
            _systemCancellationToken = systemCancellationToken ?? throw new ArgumentNullException(nameof(systemCancellationToken));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Start()
        {
            Task.Run(() => ResetOperationsPerSecondCountersAsync(_systemCancellationToken.Token), _systemCancellationToken.Token).ConfigureAwait(false);
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
    }
}
