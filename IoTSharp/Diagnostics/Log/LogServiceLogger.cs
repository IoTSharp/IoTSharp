using System;
using Microsoft.Extensions.Logging;

namespace IoTSharp.Diagnostics.Log
{
    public class LogServiceLogger : ILogger
    {
        private readonly LogService _logService;
        private readonly string _categoryName;

        public LogServiceLogger(LogService logService, string categoryName)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            _categoryName = categoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            if (!IsEnabled(logLevel))
            {
                return;
            }

            _logService.Publish(DateTime.UtcNow, logLevel, _categoryName, formatter(state, exception), exception);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Information;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
