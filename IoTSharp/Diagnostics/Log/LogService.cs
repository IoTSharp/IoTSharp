using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace IoTSharp.Diagnostics.Log
{
    public class LogService : IHostedService
    {
        private readonly LinkedList<LogEntry> _logEntries = new LinkedList<LogEntry>();
        private readonly SystemStatusService _systemStatusService;
        private readonly LogServiceOptions _options;

        private int _informationsCount;
        private int _warningsCount;
        private int _errorsCount;

        public LogService(StorageService storageService, SystemStatusService systemStatusService)
        {
            _systemStatusService = systemStatusService ?? throw new ArgumentNullException(nameof(systemStatusService));

            if (!storageService.TryReadOrCreate(out _options, LogServiceOptions.Filename))
            {
                _options = new LogServiceOptions();
            }
        }



        public void Publish(DateTime timestamp, LogLevel logLevel, string source, string message, Exception exception)
        {
            var newLogEntry = new LogEntry(timestamp, logLevel, source, message, exception?.ToString());

            lock (_logEntries)
            {
                if (newLogEntry.Level == LogLevel.Error)
                {
                    _errorsCount++;
                }
                else if (newLogEntry.Level == LogLevel.Warning)
                {
                    _warningsCount++;
                }
                else if (newLogEntry.Level == LogLevel.Information)
                {
                    _informationsCount++;
                }

                _logEntries.AddFirst(newLogEntry);

                if (_logEntries.Count > _options.MessageCount)
                {
                    var removedLogEntry = _logEntries.Last.Value;

                    if (removedLogEntry.Level == LogLevel.Error)
                    {
                        _errorsCount--;
                    }
                    else if (removedLogEntry.Level == LogLevel.Warning)
                    {
                        _warningsCount--;
                    }
                    else if (removedLogEntry.Level == LogLevel.Information)
                    {
                        _informationsCount--;
                    }

                    _logEntries.RemoveLast();
                }

                UpdateSystemStatus();
            }
        }

        public void Clear()
        {
            lock (_logEntries)
            {
                _logEntries.Clear();

                _informationsCount = 0;
                _warningsCount = 0;
                _errorsCount = 0;

                UpdateSystemStatus();
            }
        }

        public List<LogEntry> GetEntries(LogEntryFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var logEntries = new List<LogEntry>();

            lock (_logEntries)
            {
                foreach (var logEntry in _logEntries)
                {
                    if (logEntry.Level == LogLevel.Information && !filter.IncludeInformations)
                    {
                        continue;
                    }

                    if (logEntry.Level == LogLevel.Warning && !filter.IncludeWarnings)
                    {
                        continue;
                    }

                    if (logEntry.Level == LogLevel.Error && !filter.IncludeErrors)
                    {
                        continue;
                    }

                    logEntries.Add(logEntry);

                    if (logEntries.Count >= filter.TakeCount)
                    {
                        break;
                    }
                }
            }

            return logEntries;
        }

        private void UpdateSystemStatus()
        {
            _systemStatusService.Set("log.informations_count", _informationsCount);
            _systemStatusService.Set("log.warnings_count", _warningsCount);
            _systemStatusService.Set("log.errors_count", _errorsCount);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
