using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics;

namespace IoTSharp.MQTT
{
    public class LoggerAdapter : IMqttNetLogger
    {
        private readonly Dictionary<string, IMqttNetChildLogger> _childLoggers = new Dictionary<string, IMqttNetChildLogger>();
        private readonly ILogger _logger;

        public LoggerAdapter(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public event EventHandler<MqttNetLogMessagePublishedEventArgs> LogMessagePublished;

        public IMqttNetChildLogger CreateChildLogger(string source = null)
        {
            lock (_childLoggers)
            {
                if (!_childLoggers.TryGetValue(source ?? string.Empty, out var childLogger))
                {
                    childLogger = new MqttNetChildLogger(this, source);
                    _childLoggers[source ?? string.Empty] = childLogger;
                }

                return childLogger;
            }
        }

        public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
        {
            var newLogLevel = LogLevel.Debug;

            if (logLevel == MqttNetLogLevel.Warning)
            {
                newLogLevel = LogLevel.Warning;
            }
            else if (logLevel == MqttNetLogLevel.Error)
            {
                newLogLevel = LogLevel.Error;
            }
            else if (logLevel == MqttNetLogLevel.Info)
            {
                newLogLevel = LogLevel.Information;
            }
            else if (logLevel == MqttNetLogLevel.Verbose)
            {
                newLogLevel = LogLevel.Trace;
            }

            _logger.Log(newLogLevel, exception, message, parameters);
        }
    }
}
