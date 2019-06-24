using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace IoTSharp.Diagnostics.Log
{
    public class LogServiceLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();

        private readonly LogService _logService;

        public LogServiceLoggerProvider(LogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (categoryName == null)
            {
                categoryName = string.Empty;
            }

            lock (_loggers)
            {
                if (_loggers.TryGetValue(categoryName, out var logger))
                {
                    return logger;
                }

                logger = new LogServiceLogger(_logService, categoryName);
                _loggers[categoryName] = logger;

                return logger;
            }
        }

        public void Dispose()
        {
        }
    }
}
