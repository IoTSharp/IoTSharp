using CoAP;
using CoAP.Server;
using EFCore.Sharding;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace IoTSharp
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TelemetryStorage
    {
        SingleTable,
        Sharding,
        Taos
    }
    public class AppSettings
    {
        public string JwtKey { get; set; }
        public string JwtIssuer { get; set; }
        public string JwtAudience { get; set; }
        public int JwtExpireHours { get; set; }
        /// <summary>
        /// Broker settings
        /// </summary>
        public MqttBrokerSetting MqttBroker { get; set; }
        /// <summary>
        /// mqtt client settings
        /// </summary>
        public MqttClientSetting MqttClient { get; set; }
        public Dictionary<string, string> ConnectionStrings { get; set; }
        public CoapConfig CoapServer { get; set; } = new CoapConfig();

        public ModBusServerSetting ModBusServer { get; set; } = new ModBusServerSetting();

        public TelemetryStorage TelemetryStorage { get; set; } = TelemetryStorage.SingleTable;

        public ShardingSetting Sharding { get; set; } = new ShardingSetting();
    }
    public class ShardingSetting
    {
        public DatabaseType DatabaseType { get; set; } = DatabaseType.PostgreSql;
        public ExpandByDateMode ExpandByDateMode { get; set; } = ExpandByDateMode.PerMonth;
    }
    public class ModBusServerSetting
    {
        public int Port { get; set; } = 502;
        public int TimeOut { get; set; } = 120000;
    }
    public class MqttClientSetting
    {
        /// <summary>
        /// built-in or IP、HostName
        /// </summary>
        public string MqttBroker { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
    }
    public class MqttBrokerSetting
    {
        public int Port { get; set; } = 1883;
        public int TlsPort { get; set; } = 8883;
        public bool EnableTls { get; set; } = false;
        public string Certificate { get; set; }
        public SslProtocols SslProtocol { get; set; } = SslProtocols.None;
        public bool PersistRetainedMessages { get; set; }
    }
}
