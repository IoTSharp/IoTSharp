using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IoTSharp.Contracts
{
    public static class IoTSharpClaimTypes
    {
        public const string Customer = "https://schemas.iotsharp.net/ws/2019/01/identity/claims/customer";
        public const string Tenant = "https://schemas.iotsharp.net/ws/2019/01/identity/claims/tenant";
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
        ExceptionDeviceIdentity = 10010,
        RPCFailed = 10011,
        RPCTimeout = 10012,
        CustomerDoesNotHaveDevice = 10013,
        CreateUserFailed = 10014,

        CantFindObject = 10015,
        InValidData = 10016,
        NotFoundProduce = 10017,
        NotFile = 10018,
        Empty = 10019,
    }

 

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConnectStatus
    {
        /// <summary>
        /// 已断开
        /// </summary>
        Disconnected = 0,
        /// <summary>
        /// 已连接
        /// </summary>
        Connected =1
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActivityStatus
    {
        /// <summary>
        /// 非活动
        /// </summary>
        Inactivity = 0,
        /// <summary>
        /// 活跃
        /// </summary>
        Activity = 1
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataCatalog
    {
        None,
        AttributeData,
        AttributeLatest,
        TelemetryData,
        TelemetryLatest,
        ProduceData
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
        InMemory,
        Cassandra,
        ClickHouse
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
    public enum GatewayType
    {
        Unknow=0,
        Customize,
        Modbus,
        Bacnet,
        OPC_UA,
        CanOpen,
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CoApRes
    {
        Attributes,
        Telemetry,
        Alarm
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuleType
    {
        RuleNode,
        RuleSwitcher
    }
    /// <summary>
    /// 原来的事件 ， 现在改为 流程规则运行类型。 
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FlowRuleRunType
    {
        Normal=1,
        TestPurpose=2
    }
    /// <summary>
    /// 原来的挂载点， 现在叫事件类型
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventType
    {
        None=-1,
        /// <summary>
        /// 原始数据
        /// </summary>
        RAW = 0,
        /// <summary>
        /// 遥测数据对象
        /// </summary>
        Telemetry =1,
        /// <summary>
        /// 属性数据对象  
        /// </summary>
        Attribute=2,
        /// <summary>
        /// 远程控制
        /// </summary>
        RPC=3,
        /// <summary>
        /// 在线
        /// </summary>
        Connected = 4,
        /// <summary>
        /// 离线
        /// </summary>
        Disconnected = 5,
        /// <summary>
        /// 遥测数据， key value type datetime 
        /// </summary>
        TelemetryArray = 6,
        /// <summary>
        /// 告警挂载点
        /// </summary>
        Alarm = 7,
        DeleteDevice = 8,
        CreateDevice = 9,
        /// <summary>
        /// 活动事件
        /// </summary>
        Activity = 10,
        /// <summary>
        /// 非活跃状态
        /// </summary>
        Inactivity = 11
    }
    /// <summary>
    /// 聚合数据
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Aggregate
    {
        /// <summary>
        /// 不使用
        /// </summary>
        None,
        /// <summary>
        /// 平均数
        /// </summary>
        Mean,
        /// <summary>
        /// 中值
        /// </summary>
        Median,
        /// <summary>
        /// 最后一个值
        /// </summary>
        Last,
        /// <summary>
        /// 第一个值
        /// </summary>
        First,
        /// <summary>
        /// 最大
        /// </summary>
        Max,
        /// <summary>
        /// 最小
        /// </summary>
        Min,
        /// <summary>
        /// 合计
        /// </summary>
        Sum
    }
    /// <summary>
    /// 告警状态
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AlarmStatus
    {
        /// <summary>
        /// 激活未应答
        /// </summary>
        Active_UnAck = 0,
        /// <summary>
        /// 激活已应答
        /// </summary>
        Active_Ack,
        /// <summary>
        /// 清除未应答
        /// </summary>
        Cleared_UnAck,
        /// <summary>
        /// 清除已应答
        /// </summary>
        Cleared_Act
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServerityLevel
    {
        NoChange=-1,
        /// <summary>
        /// 不确定
        /// </summary>
        Indeterminate = 0,
        /// <summary>
        /// 警告
        /// </summary>
        Warning,
        /// <summary>
        /// 次要
        /// </summary>
        Minor,
        /// <summary>
        /// 重要
        /// </summary>
        Major,
        /// <summary>
        /// 错误
        /// </summary>
        Critical
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OriginatorType
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknow,
        /// <summary>
        /// 设备
        /// </summary>
        Device,
        /// <summary>
        /// 网关
        /// </summary>
        Gateway,
        /// <summary>
        /// 资产
        /// </summary>
        Asset
    }


}