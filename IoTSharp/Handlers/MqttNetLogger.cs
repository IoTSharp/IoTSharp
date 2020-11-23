using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Handlers
{
    public class MqttNetScopedLogger : MqttNetLogger, IMqttNetScopedLogger
    {
        private   string _source;
        private ILogger _logger;
        public MqttNetScopedLogger(string source, ILogger<MqttNetLogger> logger ):base(logger)
        {
            _source = source;
            _logger = logger;
        }

        public void Publish(MqttNetLogLevel logLevel, string message, object[] parameters, Exception exception) 
            => Publish(logLevel, _source, message, parameters, exception);
    }
    public class MqttNetLogger :  IMqttNetLogger
    {
        private ILogger<MqttNetLogger> _logger;

        public MqttNetLogger(ILogger<MqttNetLogger> logger )
        {
            _logger = logger;
        }

        public IMqttNetScopedLogger CreateScopedLogger(string source)
        {
            return new MqttNetScopedLogger(source,_logger);
        }

        public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
        {
            switch (logLevel)
            {
                case MqttNetLogLevel.Verbose:
                    _logger.LogTrace(exception,message,parameters);
                    break;

                case MqttNetLogLevel.Info:
                    _logger.LogInformation(exception, message, parameters);
                    break;

                case MqttNetLogLevel.Warning:
                    _logger.LogWarning(exception, message, parameters);
                    break;

                case MqttNetLogLevel.Error:
                    _logger.LogError(exception, message, parameters);
                    break;

                default:
                    break;
            }
        }
    }
}
