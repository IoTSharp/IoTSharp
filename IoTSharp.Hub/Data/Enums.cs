using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Data
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
    }

    public enum DataCatalog
    {
        None,
        AttributeData,
        AttributeLatest,
        TelemetryData,
        TelemetryLatest,
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
        Binary
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
}