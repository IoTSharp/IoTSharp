using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dic = System.Collections.Generic.Dictionary<string, string>;
using DicKV = System.Collections.Generic.KeyValuePair<string, string>;

namespace IoTSharp.Extensions
{
    public static class DataExtension
    {
        internal static (bool ok, Device device) GetDeviceByToken(this ApplicationDbContext _context, string access_token)
        {
            var deviceIdentity = from id in _context.DeviceIdentities where id.IdentityId == access_token && id.IdentityType == IdentityType.AccessToken select id;
            var devices = from dev in _context.Device where deviceIdentity.Any() && dev.Id == deviceIdentity.FirstOrDefault().Device.Id select dev;
            bool ok = deviceIdentity == null || !devices.Any();
            return (ok, devices.FirstOrDefault());
        }
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="L">Latest</typeparam>
        /// <typeparam name="D">Data</typeparam>
        /// <param name="data"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        internal static async Task<(int ret, Dic exceptions)> SaveAsync<L, D>(this ApplicationDbContext _context, Dictionary<string, object> data, Device device, DataSide dataSide) where L : DataStorage, new() where D : DataStorage, new()
        {
            Dic exceptions = new Dic();
            data.ToList().ForEach(kp =>
            {
                try
                {
                    var tdata = new D() { DateTime = DateTime.Now, Device = device, Id = Guid.NewGuid(), KeyName = kp.Key };
                    if (kp.Key != null)
                    {
                        tdata.FillKVToMe(kp);
                        tdata.Id = Guid.NewGuid();
                        _context.Set<D>().Add(tdata);
                    }
                    var tl = _context.Set<L>().FirstOrDefault(tx => tx.Device.Id == device.Id && tx.KeyName == kp.Key);
                    if (tl != null)
                    {
                        tl.FillKVToMe(kp);
                        tl.DateTime = DateTime.Now;
                    }
                    else
                    {
                        var t2 = new L() { DateTime = DateTime.Now, Device = device, Id = Guid.NewGuid(), KeyName = kp.Key, DataSide = dataSide };
                        t2.FillKVToMe(kp);
                        _context.Set<L>().Add(t2);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(kp.Key, ex.Message);
                }
            });
            int ret = await _context.SaveChangesAsync();
            return (ret, exceptions);
        }
        internal static void FillKVToMe<T>(this T tdata, KeyValuePair<string, object> kp) where T : DataStorage
        {
            switch (Type.GetTypeCode(kp.Value.GetType()))
            {
                case TypeCode.Boolean:
                    tdata.Type = DataType.Boolean;
                    tdata.Value_Boolean = (bool)kp.Value;
                    break;

                case TypeCode.Double:
                case TypeCode.Single:
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
                    tdata.Value_Long = (long)kp.Value;
                    break;
                case TypeCode.String:
                case TypeCode.Char:
                    tdata.Type = DataType.String;
                    tdata.Value_String = (string)kp.Value;
                    break;
                case TypeCode.DateTime:
                    tdata.Type = DataType.DateTime;
                    tdata.Value_DateTime = (DateTime)kp.Value;
                    break;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                    break;
                case TypeCode.Object:
                default:
                    if (kp.Value.GetType() == typeof(byte[]))
                    {
                        tdata.Type = DataType.Binary;
                        tdata.Value_Boolean = (bool)kp.Value;
                    }
                    else if (kp.Value.GetType() == typeof(System.Xml.XmlDocument))
                    {
                        tdata.Type = DataType.XML;
                        tdata.Value_XML = ((System.Xml.XmlDocument)kp.Value).ToString();
                    }
                    else
                    {
                        tdata.Type = DataType.Json;
                        tdata.Value_Json = Newtonsoft.Json.JsonConvert.SerializeObject(kp.Value);
                    }
                    break;
            }
        }
    }
}
