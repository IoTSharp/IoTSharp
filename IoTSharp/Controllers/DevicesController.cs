using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IoTSharp.Data;
using Microsoft.AspNetCore.Authorization;
using IoTSharp.Dtos;
using Dic = System.Collections.Generic.Dictionary<string, string>;
using DicKV = System.Collections.Generic.KeyValuePair<string, string>;
using MQTTnet.Client;
using MQTTnet.Extensions.Rpc;
using MQTTnet.Protocol;
using IoTSharp.Extensions;
using MQTTnet.Exceptions;
using MQTTnet.Client.Options;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMqttClientOptions _mqtt;

        public DevicesController(ApplicationDbContext context, IMqttClientOptions mqtt)
        {
            _context = context;
            _mqtt = mqtt;
        }

        /// <summary>
        /// Get all of the customer's devices.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        // GET: api/Devices
        [HttpGet("Customers/{customerId}")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices(Guid customerId)
        {
            var f = from c in _context.Device where c.Customer.Id == customerId select c;
            if (!f.Any())
            {
                return NotFound(new ApiResult<Guid>(ApiCode.CustomerDoesNotHaveDevice, $"The customer does not have any device", customerId));
            }
            else
            {
                return await f.ToArrayAsync();
            }
        }

        /// <summary>
        /// Get a device's credentials
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Request attribute values from the server
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/AttributeLatest")]
        public async Task<ActionResult<List<AttributeLatest>>> GetAttributeLatest(Guid deviceId)
        {
            var devid = from dev in _context.Device.Include(d =>d.AttributeLatest) where dev.Id == deviceId select dev.AttributeLatest;
            if (!devid.Any())
            {
                return NotFound(new ApiResult(ApiCode.NotFoundDeviceIdentity, $"Device's Identity not found "));
            }
            return await devid.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Request telemetry values from the server
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/TelemetryLatest")]
        public async Task<ActionResult<List<TelemetryLatest>>> GetTelemetryLatest(Guid deviceId)
        {
            var devid = from dev in _context.Device.Include(d=>d.TelemetryLatest) where dev.Id == deviceId select dev.TelemetryLatest;
            if (!devid.Any())
            {
                return NotFound(new ApiResult(ApiCode.NotFoundDeviceIdentity, $"Device's Identity not found "));
            }
            return await devid.FirstOrDefaultAsync();
        }
        /// <summary>
        /// Request telemetry values from the server
        /// </summary>
        /// <param name="deviceId">Which device do you read?</param>
        /// <param name="keyName">Specify key name</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/TelemetryLatest/{keyName}")]
        public async Task<ActionResult<object>> GetTelemetryLatest(Guid deviceId, string keyName)
        {
            var dev = _context.Device.Find(deviceId);
            if (dev == null)
            {
                return NotFound(new ApiResult(ApiCode.NotFoundDeviceIdentity, $"Device's Identity not found "));
            }
            else
            {
                var kv = from t in _context.TelemetryLatest where t.Device == dev && t.KeyName == keyName select t;
                return (await kv.FirstOrDefaultAsync())?.ToObject();
            }
        }
        /// <summary>
        /// Request telemetry values from the server
        /// </summary>
        /// <param name="deviceId">Which device do you read?</param>
        /// <param name="keyName">Specify key name</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/AttributeLatest/{keyName}")]
        public async Task<ActionResult<object>> GetAttributeLatest(Guid deviceId,string keyName)
        {
            var dev = _context.Device.Find(deviceId);
            if (dev == null)
            {
                return NotFound(new ApiResult(ApiCode.NotFoundDeviceIdentity, $"Device's Identity not found "));
            }
            else
            {
                var kv = from t in _context.AttributeLatest where t.Device == dev && t.KeyName == keyName select t;
                return (await kv.FirstOrDefaultAsync())?.ToObject();
            }
        }

        /// <summary>
        /// Request telemetry values from the server
        /// </summary>
        /// <param name="deviceId">Which device do you read?</param>
        /// <param name="keyName">Specify key name</param>
        /// <param name="begin">For example: 2019-06-06 12:24</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/TelemetryLatest/{keyName}/{begin}")]
        public async Task<ActionResult<object[]>> GetTelemetryLatest(Guid deviceId, string keyName, DateTime begin)
        {
            var dev = _context.Device.Find(deviceId);
            if (dev == null)
            {
                return NotFound(new ApiResult(ApiCode.NotFoundDeviceIdentity, $"Device's Identity not found "));
            }
            else
            {
                var kv = from t in _context.TelemetryLatest where t.Device == dev && t.KeyName == keyName && t.DateTime >= begin   select t.ToObject();
                return  await kv.ToArrayAsync();
            }
        }
        /// <summary>
        /// Request telemetry values from the server
        /// </summary>
        /// <param name="deviceId">Which device do you read?</param>
        /// <param name="keyName">Specify key name</param>
        /// <param name="begin">For example: 2019-06-06 12:24</param>
        /// <param name="end">For example: 2019-06-06 12:24</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/TelemetryLatest/{keyName}/{begin}/{end}")]
        public async Task<ActionResult<object>> GetTelemetryLatest(Guid deviceId, string keyName, DateTime begin,DateTime end )
        {
            var dev = _context.Device.Find(deviceId);
            if (dev == null)
            {
                return NotFound(new ApiResult(ApiCode.NotFoundDeviceIdentity, $"Device's Identity not found "));
            }
            else
            {
                var kv = from t in _context.TelemetryLatest where t.Device == dev && t.KeyName == keyName && t.DateTime>=begin && t.DateTime <end select t.ToObject() ;
                return await kv.ToArrayAsync();
            }
        }

  


        /// <summary>
        /// Get a device's detail
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Modify a device
        /// </summary>
        /// <param name="id"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        // PUT: api/Devices/5
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDevice(Guid id, DevicePutDto device)
        {
            if (id != device.Id)
            {
                return BadRequest();
            }

            var cid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Customer);
            var tid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Tenant);
            var dev = _context.Device.Include(d=>d.Tenant).Include(d=>d.Customer).First(d => d.Id == device.Id);
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
            dev.Name = device.Name;
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

        /// <summary>
        /// Create a new device
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        // POST: api/Devices
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPost]
        public async Task<ActionResult<Device>> PostDevice(DevicePostDto device)
        {
            var cid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Customer);
            var tid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Tenant);
            var devvalue = new Device() { Name = device.Name, DeviceType = device.DeviceType };
            devvalue.Tenant = _context.Tenant.Find(new Guid(tid.Value));
            devvalue.Customer = _context.Customer.Find(new Guid(cid.Value));
            if (devvalue.Tenant == null || devvalue.Customer == null)
            {
                return NotFound(new ApiResult<DevicePostDto>(ApiCode.NotFoundTenantOrCustomer, $"Not found Tenant or Customer ", device));
            }
            _context.Device.Add(devvalue);
            _context.AfterCreateDevice(devvalue);
            await _context.SaveChangesAsync();
            return await GetDevice(devvalue.Id);
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

        /// <summary>
        /// Device rpc
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="method"></param>
        /// <param name="timeout"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{access_token}/Rpc/{method}")]
        public async Task<ActionResult<string>> Rpc(string access_token, string method, int timeout, object args)
        {
            ActionResult<string> result = null;
            var (ok, dev) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return NotFound(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a device's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                try
                {
                    var rpcClient = new RpcClient(_mqtt);
                    var _timeout = TimeSpan.FromSeconds(timeout);
                    var qos = MqttQualityOfServiceLevel.AtMostOnce;
                    var payload = Newtonsoft.Json.JsonConvert.SerializeObject(args);
                    await rpcClient.ConnectAsync();
                    var response = await rpcClient.ExecuteAsync(_timeout, dev.Id.ToString(), method, payload, qos);
                    await rpcClient.DisconnectAsync();
                    result = Ok(System.Text.Encoding.UTF8.GetString(response));
                }
                catch (MqttCommunicationTimedOutException ex1)
                {
                    result = BadRequest(new ApiResult(ApiCode.RPCTimeout, $"{dev.Id} RPC Timeout {ex1.Message}"));
                }
                catch (Exception ex)
                {
                    result = BadRequest(new ApiResult(ApiCode.RPCFailed, $"{dev.Id} RPCFailed {ex.Message}"));
                }
            }
            return result;
        }

        /// <summary>
        /// Upload  device telemetry to the server.
        /// </summary>
        /// <param name="access_token">Device 's access token</param>
        /// <param name="telemetrys"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{access_token}/Telemetry")]
        public async Task<ActionResult<ApiResult<Dic>>> Telemetry(string access_token, Dictionary<string, object> telemetrys)
        {
            Dic exceptions = new Dic();
            var (ok, device) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return NotFound(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a device's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                var result = await _context.SaveAsync<TelemetryLatest, TelemetryData>(telemetrys, device, DataSide.ClientSide);
                return Ok(new ApiResult<Dic>(result.ret > 0 ? ApiCode.Success : ApiCode.NothingToDo, result.ret > 0 ? "OK" : "No Telemetry save", result.exceptions));
            }
        }

        /// <summary>
        /// Get service-side device attributes from  the server.
        /// </summary>
        /// <param name="access_token">Device 's access token </param>
        ///<param name="dataSide">Specifying data side.</param>
        ///<param name="keys">Specifying Attribute's keys</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{access_token}/Attributes/{dataSide}")]
        public async Task<ActionResult<AttributeLatest>> Attributes(string access_token, DataSide dataSide, string keys)
        {
            Dic exceptions = new Dic();
            var (ok, device) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return NotFound(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a device's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                var deviceId = device.Id;
                try
                {
                    var attributes = from dev in _context.Device where dev.Id == deviceId select dev.AttributeLatest;
                    var arrys = await attributes.FirstOrDefaultAsync();
                    var fs = from at in arrys.ToArray() where at.DataSide == dataSide && keys.Split(',', options: StringSplitOptions.RemoveEmptyEntries).Contains(at.KeyName) select at;
                    return Ok(fs.ToArray());
                }
                catch (Exception ex)
                {
                    return BadRequest(new ApiResult(ApiCode.Exception, $"{deviceId}  {ex.Message}"));
                }
            }
        }

        /// <summary>
        /// Upload client-side device attributes to the server.
        /// </summary>
        /// <param name="access_token">Device 's access token </param>
        /// <param name="attributes">attributes</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{access_token}/Attributes")]
        public async Task<ActionResult<ApiResult<Dic>>> Attributes(string access_token, Dictionary<string, object> attributes)
        {
            Dic exceptions = new Dic();
            var (ok, dev) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return NotFound(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a device's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                var result = await _context.SaveAsync<AttributeLatest, AttributeData>(attributes, dev, DataSide.ClientSide);
                return Ok(new ApiResult<Dic>(result.ret > 0 ? ApiCode.Success : ApiCode.NothingToDo, result.ret > 0 ? "OK" : "No Attribute save", result.exceptions));
            }
        }
    }
}