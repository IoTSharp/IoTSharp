using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IoTSharp.Hub.Data;
using Microsoft.AspNetCore.Authorization;
using IoTSharp.Hub.Dtos;
using Dic = System.Collections.Generic.Dictionary<string, string>;
using DicKV = System.Collections.Generic.KeyValuePair<string, string>;

namespace IoTSharp.Hub.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DevicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Devices
        [HttpGet("Customers/{customerId}")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices(Guid customerId)
        {
            var f = from c in _context.Device where c.Customer.Id == customerId select c;
            if (!f.Any())
            {
                return NotFound(new ApiResult<Guid>(ApiCode.NotFoundCustomer, $"Customer {customerId} not found ", customerId));
            }
            else
            {
                return await f.ToArrayAsync();
            }
        }

        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/Identity")]
        public async Task<ActionResult<DeviceIdentity>> GetIdentity(Guid deviceId)
        {
            var devid = from did in _context.DeviceIdentities where did.Device.Id == deviceId select did;
            var deviceid = await devid.FirstOrDefaultAsync();
            if (deviceid == null)
            {
                return NotFound(new ApiResult(ApiCode.NotFoundDeviceIdentity, $"Device's Identity not found "));
            }
            return deviceid;
        }

        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/AttributeLatest")]
        public async Task<ActionResult<List<AttributeLatest>>> GetAttributeLatest(Guid deviceId)
        {
            var devid = from dev in _context.Device where dev.Id == deviceId select dev.AttributeLatest;
            if (!devid.Any())
            {
                return NotFound(new ApiResult(ApiCode.NotFoundDeviceIdentity, $"Device's Identity not found "));
            }
            return await devid.FirstOrDefaultAsync();
        }

        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/TelemetryLatest")]
        public async Task<ActionResult<List<TelemetryLatest>>> GetTelemetryLatest(Guid deviceId)
        {
            var devid = from dev in _context.Device where dev.Id == deviceId select dev.TelemetryLatest;
            if (!devid.Any())
            {
                return NotFound(new ApiResult(ApiCode.NotFoundDeviceIdentity, $"Device's Identity not found "));
            }
            return await devid.FirstOrDefaultAsync();
        }

        // GET: api/Devices/5
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> GetDevice(Guid id)
        {
            var device = await _context.Device.FindAsync(id);

            if (device == null)
            {
                return NotFound(new ApiResult<Guid>(ApiCode.NotFoundDevice, $"Device {id} not found ", id));
            }

            return device;
        }

        // PUT: api/Devices/5
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDevice(Guid id, Device device)
        {
            if (id != device.Id)
            {
                return BadRequest();
            }

            var cid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Customer);
            var tid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Tenant);
            var dev = _context.Device.First(d => d.Id == device.Id);
            var tenid = dev.Tenant.Id;
            var cusid = dev.Customer.Id;

            if (dev == null)
            {
                return BadRequest(new ApiResult(ApiCode.NotFoundDevice, $"{dev.Id} not found in database"));
            }
            else if (dev.Tenant?.Id.ToString() != tid.Value || dev.Customer?.Id.ToString() != cid.Value)
            {
                return BadRequest(new ApiResult(ApiCode.DoNotAllow, $"Do not allow access to devices from other customers or tenants"));
            }

            _context.Entry(device).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(id))
                {
                    return NotFound(new ApiResult<Guid>(ApiCode.NotFoundDevice, $"Device {id} not found ", id));
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Devices
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPost]
        public async Task<ActionResult<Device>> PostDevice(Device device)
        {
            var cid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Customer);
            var tid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Tenant);

            device.Tenant = _context.Tenant.Find(new Guid(tid.Value));
            device.Customer = _context.Customer.Find(new Guid(cid.Value));
            if (device.Tenant == null || device.Customer == null)
            {
                return NotFound(new ApiResult<Device>(ApiCode.NotFoundTenantOrCustomer, $"Not found Tenant or Customer ", device));
            }
            _context.Device.Add(device);
            _context.DeviceIdentities.Add(new DeviceIdentity()
            {
                Device = device,
                IdentityType = IdentityType.AccessToken,
                IdentityId = Guid.NewGuid().ToString().Replace("-", "")
            });
            await _context.SaveChangesAsync();
            return await GetDevice(device.Id);
        }

        // DELETE: api/Devices/5
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Device>> DeleteDevice(Guid id)
        {
            var device = await _context.Device.FindAsync(id);
            if (device == null)
            {
                return NotFound(new ApiResult<Guid>(ApiCode.NotFoundDevice, $"Device {id} not found ", id));
            }

            _context.Device.Remove(device);
            await _context.SaveChangesAsync();

            return device;
        }

        private bool DeviceExists(Guid id)
        {
            return _context.Device.Any(e => e.Id == id);
        }

        [AllowAnonymous]
        [HttpPost("{access_token}/Telemetry")]
        public async Task<ActionResult<ApiResult<Dic>>> Telemetry(string access_token, Dictionary<string, object> telemetrys)
        {
            Dic exceptions = new Dic();
            var deviceIdentity = from id in _context.DeviceIdentities where id.IdentityId == access_token && id.IdentityType == IdentityType.AccessToken select id;
            var device = from dev in _context.Device where deviceIdentity.Any() && dev.Id == deviceIdentity.FirstOrDefault().Device.Id select dev;
            if (deviceIdentity == null || !device.Any())
            {
                return NotFound(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a device's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                var result = await SaveAsync<TelemetryLatest, TelemetryData>(telemetrys, device.FirstOrDefault());
                return Ok(new ApiResult<Dic>(result.ret > 0 ? ApiCode.Success : ApiCode.NothingToDo, result.ret > 0 ? "OK" : "No Telemetry save", result.exceptions));
            }
        }

        [AllowAnonymous]
        [HttpPost("{access_token}/Attribute")]
        public async Task<ActionResult<ApiResult<Dic>>> Attribute(string access_token, Dictionary<string, object> attributes)
        {
            Dic exceptions = new Dic();
            var deviceIdentity = from id in _context.DeviceIdentities where id.IdentityId == access_token && id.IdentityType == IdentityType.AccessToken select id;
            var device = from dev in _context.Device where deviceIdentity.Any() && dev.Id == deviceIdentity.FirstOrDefault().Device.Id select dev;
            if (deviceIdentity == null || !device.Any())
            {
                return NotFound(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a device's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                var result = await SaveAsync<AttributeLatest, AttributeData>(attributes, device.FirstOrDefault());
                return Ok(new ApiResult<Dic>(result.ret > 0 ? ApiCode.Success : ApiCode.NothingToDo, result.ret > 0 ? "OK" : "No Attribute save", result.exceptions));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="L">Latest</typeparam>
        /// <typeparam name="D">Data</typeparam>
        /// <param name="data"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        private async Task<(int ret, Dic exceptions)> SaveAsync<L, D>(Dictionary<string, object> data, Device device) where L : DataStorage, new() where D : DataStorage, new()
        {
            Dic exceptions = new Dic();
            data.ToList().ForEach(kp =>
             {
                 try
                 {
                     var tdata = new D() { DateTime = DateTime.Now, Device = device, Id = Guid.NewGuid(), KeyName = kp.Key };
                     if (kp.Key != null)
                     {
                         FillKVToModel(kp, tdata);
                         tdata.Id = Guid.NewGuid();
                         _context.Set<D>().Add(tdata);
                     }
                     var tl = _context.Set<L>().FirstOrDefault(tx => tx.Device.Id == device.Id && tx.KeyName == kp.Key);
                     if (tl != null)
                     {
                         FillKVToModel(kp, tl);
                         tl.DateTime = DateTime.Now;
                     }
                     else
                     {
                         var t2 = new L() { DateTime = DateTime.Now, Device = device, Id = Guid.NewGuid(), KeyName = kp.Key };
                         FillKVToModel(kp, t2);
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

        private static void FillKVToModel<T>(KeyValuePair<string, object> kp, T tdata) where T : DataStorage
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