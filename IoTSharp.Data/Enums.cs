using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public static class IoTSharpClaimTypes
    {
        public const string Customer = "http://schemas.iotsharp.net/ws/2019/01/identity/claims/customer";
        public const string Tenant = "http://schemas.iotsharp.net/ws/2019/01/identity/claims/tenant";
    }

    public enum ApiCode : int
    {
        Success = 10000,
        LoginError = 10001,
        Exception = 10002,
        AlreadyExists = 10003,
        NotFoundTenantOrCustomer = 10004,
        NotFoundDevice = 10005,
        NotFoundCustomer = 10006,
        NothingToDo = 10007,
        DoNotAllow = 10008,
        NotFoundTenant = 10009,
        NotFoundDeviceIdentity = 10010,
        RPCFailed = 10011,
        RPCTimeout = 10012,
        CustomerDoesNotHaveDevice = 10013,
        CreateUserFailed = 10014,

        CantFindObject = 10015,
        InValidData = 10016,
    }

    public enum DataCatalog
    {
        None,
        AttributeData,
        AttributeLatest,
        TelemetryData,
        TelemetryLatest,
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataSide
    {
        AnySide,
        ServerSide,
        ClientSide,
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserRole
    {
        Anonymous,
        NormalUser,
        CustomerAdmin,
        TenantAdmin,
        SystemAdmin,
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataType
    {
        Boolean,
        String,
        Long,
        Double,
        Json,
        XML,
        Binary,
        DateTime
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataBaseType
    {
        PostgreSql,
        SqlServer,
        MySql ,
        Oracle ,
        Sqlite,
        InMemory
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum IdentityType
    {
        AccessToken,
        DevicePassword,
        X509Certificate
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ObjectType
    {
        Unknow,
        Device,
        Tenant,
        Customer,
        User,
        MQTTBroker,
        MQTTClient

    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeviceType
    {
        Device = 0,
        Gateway = 1
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CoApRes
    {
        Attributes,
        Telemetry,
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuleType
    {
        RuleNode,
        RuleSwitcher
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventType
    {
        Normal=1,
        TestPurpose=2
    }



    public enum NodeStatus
    {
        Abort = -1,
        Created = 0,
        Processing =1,
        Suspend = 2,
        Complete = 1,
    }

    public enum MsgType
    {
        MQTT,
        CoAP,
        HTTP
    }
    public enum MountType
    {
        RAW = 0,
        Telemetry =1,
        Attribute=2,
        RPC=3,
        Online = 4,
        Offline=5
    }

    


}