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
    }

    public enum DataCatalog
    {
        None,
        AttributeData,
        AttributeLatest,
        TelemetryData,
        TelemetryLatest,
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataSide
    {
        AnySide,
        ServerSide,
        ClientSide,
    }
    public enum UserRole
    {
        Anonymous,
        NormalUser,
        CustomerAdmin,
        TenantAdmin,
        SystemAdmin,
    }

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

    public enum DatabaseType
    {
        mssql,
        npgsql,
        sqlite
    }

    public enum IdentityType
    {
        AccessToken,
        DevicePassword,
        X509Certificate
    }
    public  enum ObjectType
    {
        Unknow,
        Device,
        Tenant,
        Customer,
        User,
        MQTTBroker,
        MQTTClient
          
    }
    public enum DeviceType
    {
        Device =0,
        Gateway =1
    }
}