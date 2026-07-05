using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Dic = System.Collections.Generic.Dictionary<string, System.Exception>;

namespace IoTSharp.Data
{
    public static class DataExtension
    {
        public static (bool ok, Device device) GetDeviceByTokenWithTenantCustomer(this ApplicationDbContext _context, string access_token)
        {
            var deviceIdentity = from id in _context.DeviceIdentities.Include(di => di.Device) where id.IdentityId == access_token && id.IdentityType == IdentityType.AccessToken select id;
            var devices = from dev in _context.Device.Include(g => g.Customer).Include(g => g.Tenant) where deviceIdentity.Any() && dev.Id == deviceIdentity.FirstOrDefault().Device.Id select dev;

            bool ok = deviceIdentity == null || !devices.Any();
            return (ok, devices.FirstOrDefault());
        }
        public static (bool ok, Device device) GetDeviceByToken(this ApplicationDbContext _context, string access_token)
        {
            var deviceIdentity = from id in _context.DeviceIdentities.Include(di => di.Device) where id.IdentityId == access_token && id.IdentityType == IdentityType.AccessToken select id;
            var devices = from dev in _context.Device.Include(c => c.Owner) where deviceIdentity.Any() && dev.Id == deviceIdentity.FirstOrDefault().Device.Id select dev;
            bool ok = deviceIdentity == null || !devices.Any();
            return (ok, devices.FirstOrDefault());
        }

        /// <summary>
        /// Find a product (Product) by its ProductToken, including its Customer, Tenant and Devices.
        /// Returns (ok=true, null) when not found; (ok=false, Product) when found.
        /// </summary>
        public static (bool ok, Product Product) GetProductByToken(this ApplicationDbContext _context, string product_token)
        {
            if (string.IsNullOrEmpty(product_token))
            {
                return (true, null);
            }

            var Product = _context.Products
                .Include(p => p.Customer)
                .Include(p => p.Tenant)
                .Include(p => p.Devices)
                .FirstOrDefault(p => p.ProductToken == product_token && p.ProductToken != null && p.Deleted == false);
            bool ok = Product == null;
            return (ok, Product);
        }
        /// <summary>
        /// Save Data to Device's and <typeparamref name="L"/>
        /// </summary>
        /// <typeparam name="L">Latest</typeparam>
        /// <param name="data"></param>
        /// <param name="dataSide"></param>
        /// <param name="deviceId"></param>
        /// <param name="_context"></param>
        /// <returns></returns>
        public static async Task<(int ret, Dic exceptions)> SaveAsync<L>(this ApplicationDbContext _context, Dictionary<string, object> data, Guid deviceId, DataSide dataSide) where L : DataStorage, new()
        {
            //保存是会存在为空的数组进行保存的情况，此处做为空判断
            if (data.Any())
            {
                Dic exceptions = _context.PreparingData<L>(data, deviceId, dataSide);
                int ret = await _context.SaveChangesAsync();
                return (ret, exceptions);
            }
            else
            {
                Dic exceptions = new Dic
                {
                    { "", new Exception("参数为空") }
                };
                return (0, exceptions);
            }
        }
        /// <summary>
        /// Preparing Data to Device's   <typeparamref name="L"/>
        /// </summary>
        /// <typeparam name="L"></typeparam>
        /// <param name="_context"></param>
        /// <param name="data"></param>
        /// <param name="deviceId"></param>
        /// <param name="dataSide"></param>
        /// <returns></returns>
        public static Dic PreparingData<L>(this ApplicationDbContext _context, Dictionary<string, object> data, Guid deviceId, DataSide dataSide)
            where L : DataStorage, new()
        {
            Dic exceptions = new Dic();
            var keyNames = data
                .Where(kp => kp.Key != null && kp.Value != null)
                .Select(kp => kp.Key)
                .Distinct()
                .ToList();
            var deviceData = keyNames.Count == 0
                ? new Dictionary<string, L>()
                : _context.Set<L>()
                    .Where(tx => tx.DeviceId == deviceId && keyNames.Contains(tx.KeyName))
                    .ToDictionary(tx => tx.KeyName);
            var catalog = GetCatalog<L>();

            foreach (var kp in data)
            {
                try
                {
                    if (kp.Key != null && kp.Value != null)
                    {
                        if (deviceData.TryGetValue(kp.Key, out var tx))
                        {
                            tx.FillKVToMe(kp);
                            // TODO:jy 待重新设计主键
                            tx.DateTime = DateTime.UtcNow;
                            _context.Set<L>().Update(tx).State = EntityState.Modified;
                        }
                        else
                        {
                            var t2 = new L() { DateTime = DateTime.UtcNow, DeviceId = deviceId, KeyName = kp.Key, DataSide = dataSide };
                            t2.Catalog = catalog;
                            _context.Set<L>().Add(t2);
                            t2.FillKVToMe(kp);
                            deviceData.Add(kp.Key, t2);
                        }
                    }
                    else
                    {
                        exceptions.Add($"Key:{kp.Key}", new Exception("Key is null or value is null"));
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(kp.Key, ex);
                }
            }
            return exceptions;
        }

        public static Dic PreparingData<L>(this ApplicationDbContext _context, List<ProductData> attributes, Guid deviceId)
         where L : DataStorage, new()
        {
            Dic exceptions = new Dic();
            var keyNames = attributes
                .Where(kp => kp.KeyName != null)
                .Select(kp => kp.KeyName)
                .Distinct()
                .ToList();
            var deviceData = keyNames.Count == 0
                ? new Dictionary<string, L>()
                : _context.Set<L>()
                    .Where(tx => tx.DeviceId == deviceId && keyNames.Contains(tx.KeyName))
                    .ToDictionary(tx => tx.KeyName);
            var catalog = GetCatalog<L>();

            foreach (var kp in attributes)
            {
                try
                {
                    if (deviceData.TryGetValue(kp.KeyName, out var tx))
                    {
                        tx.DateTime = DateTime.UtcNow;
                        _context.Set<L>().Update(tx).State = EntityState.Modified;
                    }
                    else
                    {
                        var t2 = new L() { DateTime = DateTime.UtcNow, DeviceId = deviceId, KeyName = kp.KeyName };
                        t2.Catalog = catalog;
                        _context.Set<L>().Add(t2);
                        deviceData.Add(kp.KeyName, t2);
                    }

                }
                catch (Exception ex)
                {
                    exceptions.Add(kp.KeyName, ex);
                }
            }
            return exceptions;
        }

        private static DataCatalog GetCatalog<L>() where L : DataStorage, new()
        {
            return typeof(L) == typeof(AttributeLatest) ? DataCatalog.AttributeLatest
                : typeof(L) == typeof(TelemetryLatest) ? DataCatalog.TelemetryLatest
                : DataCatalog.None;
        }

        public static object JsonNodeToObject(this JsonNode node)
        {
            return node.ToClrObject();
        }
        public static void FillKVToMe<T>(this T tdata, KeyValuePair<string, object> kp) where T : IDataStorage
        {
            var tc = Type.GetTypeCode(kp.Value.GetType());

            switch (tc)
            {
                case TypeCode.Boolean:
                    tdata.Type = DataType.Boolean;
                    tdata.Value_Boolean = (bool)kp.Value;
                    break;
                case TypeCode.Single:
                    tdata.Type = DataType.Double;
                    tdata.Value_Double = double.Parse(kp.Value.ToString(), System.Globalization.NumberStyles.Float);
                    break;
                case TypeCode.Double:
                case TypeCode.Decimal:
                    tdata.Type = DataType.Double;
                    tdata.Value_Double = (double)kp.Value;
                    break;

                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Byte:
                case TypeCode.SByte:
                    tdata.Type = DataType.Long;
                    tdata.Value_Long = (Int64)Convert.ChangeType(kp.Value, TypeCode.Int64);
                    break;
                case TypeCode.String:
                case TypeCode.Char:
                    tdata.Type = DataType.String;
                    tdata.Value_String = (string)kp.Value;
                    break;
                case TypeCode.DateTime:
                    tdata.Type = DataType.DateTime;
                    tdata.Value_DateTime = ((DateTime)kp.Value);
                    break;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                    break;
                case TypeCode.Object:
                default:
                    if (kp.Value.GetType() == typeof(byte[]))
                    {
                        tdata.Type = DataType.Binary;
                        tdata.Value_Binary = (byte[])kp.Value;
                    }
                    else if (kp.Value.GetType() == typeof(System.Xml.XmlDocument))
                    {
                        tdata.Type = DataType.XML;
                        tdata.Value_XML = ((System.Xml.XmlDocument)kp.Value).InnerXml;
                    }
                    else if (kp.Value.GetType() == typeof(System.Text.Json.JsonElement))
                    {
                        var kvx = kp.Value as System.Text.Json.JsonElement?;
                        if (kvx.HasValue)
                        {
                            switch (kvx.Value.ValueKind)
                            {
                                case System.Text.Json.JsonValueKind.Undefined:
                                case System.Text.Json.JsonValueKind.Object:
                                    break;
                                case System.Text.Json.JsonValueKind.Array:
                                    break;
                                case System.Text.Json.JsonValueKind.String:
                                    tdata.Type = DataType.String;
                                    tdata.Value_String = kvx.Value.GetString();
                                    break;
                                case System.Text.Json.JsonValueKind.Number:
                                    tdata.Type = DataType.Double;
                                    tdata.Value_Double = kvx.Value.GetDouble();
                                    break;
                                case System.Text.Json.JsonValueKind.True:
                                case System.Text.Json.JsonValueKind.False:
                                    tdata.Type = DataType.Boolean;
                                    tdata.Value_Boolean = kvx.Value.GetBoolean();
                                    break;
                                case System.Text.Json.JsonValueKind.Null:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        tdata.Type = DataType.Json;
                        tdata.Value_Json = JsonObjectSerializer.Serialize(kp.Value);
                    }
                    break;
            }
        }
        public static string ToFieldName(this DataType _datatype)
        {
            string _fieldname = "";
            switch (_datatype)
            {
                case DataType.Boolean:
                    _fieldname = "value_boolean";
                    break;
                case DataType.String:
                    _fieldname = "value_string";
                    break;
                case DataType.Long:
                    _fieldname = "value_long";
                    break;
                case DataType.Double:
                    _fieldname = "value_double";
                    break;
                case DataType.Json:
                    _fieldname = "value_string";
                    break;
                case DataType.XML:
                    _fieldname = "value_string";
                    break;
                case DataType.Binary:
                    _fieldname = "value_string";
                    break;
                case DataType.DateTime:
                    _fieldname = "value_datetime";
                    break;
                default:
                    break;
            }
            return _fieldname;
        }

        public static void AttachValue(this TelemetryDataDto telemetry, DataType datatype, object _value)
        {
            telemetry.DataType = datatype;
            switch (datatype)
            {
                case DataType.Boolean:
                    telemetry.Value = (bool)_value;
                    break;
                case DataType.String:
                    telemetry.Value = (string)_value;
                    break;
                case DataType.Long:
                    telemetry.Value = (long)_value;
                    break;
                case DataType.Double:
                    telemetry.Value = (double)_value;
                    break;
                case DataType.Json:
                case DataType.XML:
                    telemetry.Value = (string)_value;
                    break;
                case DataType.Binary:
                    telemetry.Value = Hex.HexToBytes((string)_value);
                    break;
                case DataType.DateTime:
                    telemetry.Value = (DateTime)_value;
                    break;
                default:
                    break;
            }
        }
        public static object ToObject(this IDataStorage kxv)
        {
            object obj = null;
            if (kxv != null)
            {
                switch (kxv.Type)
                {
                    case DataType.Boolean:
                        obj = kxv.Value_Boolean;
                        break;
                    case DataType.String:
                        obj = kxv.Value_String;
                        break;
                    case DataType.Long:
                        obj = kxv.Value_Long;
                        break;
                    case DataType.Double:
                        obj = kxv.Value_Double;
                        break;
                    case DataType.Json:
                        obj = kxv.Value_Json;
                        break;
                    case DataType.XML:
                        obj = kxv.Value_XML;
                        break;
                    case DataType.Binary:
                        obj = kxv.Value_Binary;
                        break;
                    case DataType.DateTime:
                        obj = kxv.DateTime;
                        break;
                    default:
                        break;
                }
            }
            return obj;
        }

    }
}
