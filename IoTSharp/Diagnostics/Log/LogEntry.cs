using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IoTSharp.Diagnostics.Log
{
    public class LogEntry
    {
        public LogEntry(DateTime timestamp, LogLevel level, string source, string message, string exception)
        {
            Timestamp = timestamp;
            Level = level;
            Source = source;
            Message = message;
            Exception = exception;
        }

        public DateTime Timestamp { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LogLevel Level { get; }

        public string Source { get; }

        public string Message { get; }

        public string Exception { get; }
    }
}