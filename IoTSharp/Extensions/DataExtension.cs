using IoTSharp.Data;
using IoTSharp.Queue;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Modes.Gcm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dic = System.Collections.Generic.Dictionary<string, string>;

namespace IoTSharp.Extensions
{
    public static class DataExtension
    {
        internal static (bool ok, Device device) GetDeviceByToken(this ApplicationDbContext _context, string access_token)
        {
            var deviceIdentity = from id in _context.DeviceIdentities.Include(di => di.Device) where id.IdentityId == access_token && id.IdentityType == IdentityType.AccessToken select id;
            var devices = from dev in _context.Device where deviceIdentity.Any() && dev.Id == deviceIdentity.FirstOrDefault().Device.Id select dev;
            bool ok = deviceIdentity == null || !devices.Any();
            return (ok, devices.FirstOrDefault());
        }
        /// <summary>
        /// Save Data to Device's and <typeparamref name="L"/>
        /// </summary>
        /// <typeparam name="L">Latest</typeparam>
        /// <param name="data"></param>
        /// <param name="device"></param>
        /// <param name="dataSide"></param>
        /// <param name="_context"></param>
        /// <returns></returns>
        internal static async Task<(int ret, Dic exceptions)> SaveAsync<L>(this ApplicationDbContext _context, Dictionary<string, object> data, Device device, DataSide dataSide) where L : DataStorage, new()
        {
            Dic exceptions = _context.PreparingData<L>(data, device, dataSide);
            int ret = await _context.SaveChangesAsync();
            return (ret, exceptions);
        }
        /// <summary>
        /// Preparing Data to Device's   <typeparamref name="L"/>
        /// </summary>
        /// <typeparam name="L"></typeparam>
        /// <param name="_context"></param>
        /// <param name="data"></param>
        /// <param name="device"></param>
        /// <param name="dataSide"></param>
        /// <returns></returns>
        internal static Dic PreparingData<L>(this ApplicationDbContext _context, Dictionary<string, object> data, Device device, DataSide dataSide)
            where L : DataStorage, new()
        {

            Dic exceptions = new Dic();
            var devdata = from tx in _context.Set<L>() where tx.DeviceId == device.Id select tx;
            data.ToList().ForEach(kp =>
            {
                try
                {
                    if (kp.Key != null)
                    {
                        var tl = from tx in devdata  where  tx.KeyName == kp.Key && tx.DataSide == dataSide select tx;
                        if (tl.Any())
                        {
                            tl.First().FillKVToMe(kp);
                        }
                        else 
                        {
                            var t2 = new L() { DateTime = DateTime.Now, DeviceId = device.Id, KeyName = kp.Key, DataSide = dataSide };
                            t2.Catalog = (typeof(L) == typeof(AttributeLatest) ? DataCatalog.AttributeLatest
                                                       : ((typeof(L) == typeof(TelemetryLatest) ? DataCatalog.TelemetryLatest
                                                       : 0)));
                            _context.Set<L>().Add(t2);
                            t2.FillKVToMe(kp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(kp.Key, ex.Message);
                }
            });
            return exceptions;
        }

        public static object JPropertyToObject(this JProperty property)
        {
            object obj = null;
            switch (property.Value.Type)
            {
                case JTokenType.Integer:
                    obj = property.Value.ToObject<int>();
                    break;
                case JTokenType.Float:
                    obj = property.Value.ToObject<float>();
                    break;
                case JTokenType.String:
                    obj = property.Value.ToObject<string>();
                    break;
                case JTokenType.Boolean:
                    obj = property.Value.ToObject<bool>();
                    break;
                case JTokenType.Date:
                    obj = property.Value.ToObject<DateTime>();
                    break;
                case JTokenType.Bytes:
                    obj = property.Value.ToObject<byte[]>();
                    break;
                case JTokenType.Guid:
                    obj = property.Value.ToObject<Guid>();
                    break;
                case JTokenType.Uri:
                    obj = property.Value.ToObject<Uri>();
                    break;
                case JTokenType.TimeSpan:
                    obj = property.Value.ToObject<TimeSpan>();
                    break;
                default:
                    obj = property.Value;
                    break;
            }
            return obj;
        }
        internal static void FillKVToMe<T>(this T tdata, KeyValuePair<string, object> kp) where T : IDataStorage
        {


            switch (Type.GetTypeCode(kp.Value.GetType()))
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
                    else
                    {
                        tdata.Type = DataType.Json;
                        tdata.Value_Json = Newtonsoft.Json.JsonConvert.SerializeObject(kp.Value);
                    }
                    break;
            }
        }
        public static Dictionary<string, object> ConvertPayloadToDictionary(this MqttApplicationMessage msg)
        {
            return JToken.Parse(msg.ConvertPayloadToString())?.JsonToDictionary();
        }

        public static Dictionary<string, object> JsonToDictionary(this JToken jojb)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            if (jojb.Type != JTokenType.Array)
            {
                jojb.Children().ToList().ForEach(a => keyValues.Add(((JProperty)a).Name, ((JProperty)a).JPropertyToObject()));
            }
            else
            {
                jojb.Children().ToList().ForEach(jt => jt.Children().ToList().ForEach(a => keyValues.Add(((JProperty)a).Name, ((JProperty)a).JPropertyToObject())));
            }
            return keyValues;
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
