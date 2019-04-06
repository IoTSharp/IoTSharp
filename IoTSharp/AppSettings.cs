using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace IoTSharp
{
    public class AppSettings
    {
        public string DataBase { get; set; }
        public string JwtKey { get; set; }
        public string JwtExpireDays { get; set; }
        public string JwtIssuer { get; set; }

        /// <summary>
        /// Broker settings
        /// </summary>
        public MqttBrokerSetting MqttBroker { get; set; }
        /// <summary>
        /// mqtt client settings
        /// </summary>
        public MqttClientSetting MqttClient { get; set; }

    }
    public class MqttClientSetting
    {
        /// <summary>
        /// built-in or IP、HostName
        /// </summary>
        public string MqttBroker { get; set; } = "built-in";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public int Port { get; set; } = 1883;
    }
    public class MqttBrokerSetting
    {
        public int Port { get; set; } = 1883;
        public int TlsPort { get; set; } = 8883;
        public bool EnableTls { get; set; } = false;
        public string Certificate { get; set; }
        public SslProtocols SslProtocol { get; set; } = SslProtocols.None;
    }
}
