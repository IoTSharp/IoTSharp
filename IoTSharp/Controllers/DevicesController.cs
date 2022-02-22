using DotNetCore.CAP;
using EasyCaching.Core;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using IoTSharp.Models;
using IoTSharp.Storage;
using IoTSharp.X509Extensions;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml;
using Dic = System.Collections.Generic.Dictionary<string, string>;
using DicKV = System.Collections.Generic.KeyValuePair<string, string>;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// 设备管理
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private const string _map_to_telemety_ = "_map_to_telemetry_";
        private const string _map_to_attribute_ = "_map_to_attribute_";
        private const string _map_to_devname = "_map_to_devname";
        private const string _map_var_devname = "$devname";
        private const string _map_to_devname_format = "_map_to_devname_format";
        private readonly ApplicationDbContext _context;
        private readonly MqttClientOptions _mqtt;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IStorage _storage;
        private readonly MqttServer _serverEx;
        private readonly AppSettings _setting;
        private readonly ICapPublisher _queue;
        private readonly FlowRuleProcessor _flowRuleProcessor;
        private readonly IEasyCachingProvider _caching;
        private readonly IServiceScopeFactory _scopeFactor;

        public DevicesController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, ILogger<DevicesController> logger, MqttServer serverEx, ApplicationDbContext context, MqttClientOptions mqtt, IStorage storage, IOptions<AppSettings> options, ICapPublisher queue
            , IEasyCachingProviderFactory factory, FlowRuleProcessor flowRuleProcessor, IServiceScopeFactory scopeFactor)
        {
          
            _context = context;
            _mqtt = mqtt;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _storage = storage;
            _serverEx = serverEx;
            _setting = options.Value;
            _queue = queue;
            _flowRuleProcessor = flowRuleProcessor;
            _caching = factory.GetCachingProvider("iotsharp");
            _scopeFactor = scopeFactor;
        }

        /// <summary>
        /// 获取指定客户的设备列表
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        // GET: api/Customers/All
        [HttpGet("Devices/Customers/All")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<Guid>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<List<Device>>> GetAllDevices([FromQuery] Guid customerId)
        {
            var f = from c in _context.Device where c.Customer.Id == customerId select c;
            if (!f.Any())
            {
                return new ApiResult<List<Device>>(ApiCode.CustomerDoesNotHaveDevice, $"The customer {customerId} does not have any device", null);
            }
            else
            {
                return new ApiResult<List<Device>>(ApiCode.Success, $"The customer {customerId} does not have any device", await f.ToListAsync());
            }
        }

        /// <summary>
        /// 获取指定客户的设备列表
        /// </summary>
        /// <returns></returns>
        // GET: api/Devices/Customers
        [HttpGet("Customers")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<Guid>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<PagedData<DeviceDetailDto>>> GetDevices([FromQuery] DeviceParam m)
        {
            var profile = this.GetUserProfile();

            if (m.limit > 0)
            {
                try
                {
                    Expression<Func<Device, bool>> condition = x => x.Customer.Id == m.customerId && x.Status > -1 && x.Tenant.Id == profile.Tenant;
                    if (!string.IsNullOrEmpty(m.Name))
                    {
                        condition = condition.And(x => x.Name.Contains(m.Name));
                    }

                    return new ApiResult<PagedData<DeviceDetailDto>>(ApiCode.Success, "OK", new PagedData<DeviceDetailDto>
                    {
                        total = await _context.Device.CountAsync(condition),
                        rows = _context.Device.Include(c => c.DeviceIdentity).OrderByDescending(c => c.LastActive).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList().Select(x => new DeviceDetailDto()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            LastActive = x.LastActive,
                            IdentityId = x.DeviceIdentity?.IdentityId,
                            IdentityValue = x.DeviceIdentity?.IdentityType == IdentityType.X509Certificate ? "" : x.DeviceIdentity?.IdentityValue,
                            Tenant = x.Tenant,
                            Customer = x.Customer,
                            DeviceType = x.DeviceType,
                            Online = x.Online,
                            Owner = x.Owner,
                            Timeout = x.Timeout,
                            IdentityType = x.DeviceIdentity?.IdentityType ?? IdentityType.AccessToken
                        }).ToList()
                    });
                }
                catch (Exception e)
                {
                    return new ApiResult<PagedData<DeviceDetailDto>>(ApiCode.Exception, e.Message, null);
                }
            }
            else
            {
                try
                {
                    Expression<Func<Device, bool>> condition = x => x.Customer.Id == m.customerId && x.Status > -1;
                    return new ApiResult<PagedData<DeviceDetailDto>>(ApiCode.Success, "OK", new PagedData<DeviceDetailDto>
                    {
                        total = await _context.Device.CountAsync(condition),
                        rows = await _context.Device.OrderByDescending(c => c.LastActive).Where(condition).Select(x => new DeviceDetailDto()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            LastActive = x.LastActive,
                            Tenant = x.Tenant,
                            Customer = x.Customer,
                            DeviceType = x.DeviceType,
                            Online = x.Online,
                            Owner = x.Owner,
                            Timeout = x.Timeout,
                        }).ToListAsync()
                    });
                }
                catch (Exception e)
                {
                    return new ApiResult<PagedData<DeviceDetailDto>>(ApiCode.Exception, e.Message, null);
                }
            }
        }

        /// <summary>
        /// 获取指定设备的认证方式信息
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/Identity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<DeviceIdentity>> GetIdentity(Guid deviceId)
        {
            var did = await _context.DeviceIdentities.FirstOrDefaultAsync(c => c.Device.Id == deviceId);
            if (did == null)
            {
                return new ApiResult<DeviceIdentity>(ApiCode.CantFindObject, "CantFindObject", null);
            }
            else if (did.IdentityType == IdentityType.X509Certificate)
            {
                return new ApiResult<DeviceIdentity>(ApiCode.Success, "OK", new DeviceIdentity() { Id = did.Id, IdentityType = did.IdentityType, IdentityId = did.IdentityId });
            }
            else
            {
                return new ApiResult<DeviceIdentity>(ApiCode.Success, "OK", did);
            }
        }

        /// <summary>
        /// 获取指定设备的认证方式信息
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/CreateX509Identity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<DeviceIdentity>> CreateX509Identity(Guid deviceId)
        {
            var did = await _context.DeviceIdentities.Include(d => d.Device).FirstOrDefaultAsync(c => c.Device.Id == deviceId);
            var cust = from c in _context.Device.Include(d => d.Customer).Include(d => d.Tenant) where c.Id == deviceId select c;
            var dev = cust.FirstOrDefault();
            if (did != null && dev != null)
            {
                var option = _setting.MqttBroker;

                SubjectAlternativeNameBuilder altNames = new SubjectAlternativeNameBuilder();
                altNames.AddUserPrincipalName(did.Device.Id.ToString());
                altNames.AddDnsName(_setting.MqttBroker.DomainName);
                altNames.AddUri(new Uri($"mqtt://{_setting.MqttBroker.ServerIPAddress}:{_setting.MqttBroker.TlsPort}"));
                string name = $"CN={dev.Name},C=CN,L={dev.Customer.Province ?? "IoTSharp"},ST={dev.Customer.City ?? "IoTSharp"},O={dev.Customer.Name},OU={dev.Tenant.Name}";
                var tlsclient = option.CACertificate.CreateTlsClientRSA(name, altNames);
                string x509CRT, x509Key;
                tlsclient.SavePem(out x509CRT, out x509Key);
                did.IdentityType = IdentityType.X509Certificate;
                did.IdentityId = tlsclient.Thumbprint;
                var pem = new
                {
                    PrivateKey = x509Key,
                    PublicKey = x509CRT
                };
                did.IdentityValue = Newtonsoft.Json.JsonConvert.SerializeObject(pem);
                await _context.SaveChangesAsync();
                return new ApiResult<DeviceIdentity>(ApiCode.Success, "OK", new DeviceIdentity() { Id = did.Id, IdentityType = did.IdentityType, IdentityId = did.IdentityId });
            }
            else
            {
                return new ApiResult<DeviceIdentity>(ApiCode.NotFoundDeviceIdentity, "Not found device identity", null);
            }
        }

        /// <summary>
        /// 下载证书
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns>一个压缩包，包含ca.crt client.crt client.key</returns>
        [HttpGet("{deviceId}/DownloadCertificates")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        public ActionResult DownloadCertificates(Guid deviceId)
        {
            try
            {
                var dt = _context.DeviceIdentities.Include(d => d.Device).FirstOrDefault(c => c.Device.Id == deviceId);
                if (dt == null || dt.IdentityType != IdentityType.X509Certificate || string.IsNullOrEmpty(dt.IdentityValue))
                {
                    return BadRequest(new ApiResult(ApiCode.NotFoundDevice, "未找到设备或设备公钥、秘钥为空"));
                }
                else
                {
                    var tsl = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(dt.IdentityValue, new
                    {
                        PrivateKey = "",
                        PublicKey = ""
                    });
                    if (tsl == null || string.IsNullOrEmpty(tsl.PrivateKey) || string.IsNullOrEmpty(tsl.PublicKey))
                    {
                        return BadRequest(new ApiResult(ApiCode.NotFoundDevice, "秘钥格式未能解析。可能是版本不通。 "));
                    }
                    else
                    {
                        string fileNameZip = $"client_{dt.Device.Id.ToString().Replace("-", "")}.zip";
                        byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(tsl.PublicKey);
                        byte[] fileBytes1 = System.Text.Encoding.UTF8.GetBytes(tsl.PrivateKey);
                        byte[] compressedBytes;
                        using (var outStream = new MemoryStream())
                        {
                            using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
                            {
                                var fileInArchive = archive.CreateEntry("client.crt", CompressionLevel.Optimal);
                                using (var entryStream = fileInArchive.Open())
                                using (var fileToCompressStream = new MemoryStream(fileBytes))
                                {
                                    fileToCompressStream.CopyTo(entryStream);
                                }

                                var fileInArchive1 = archive.CreateEntry("client.key", CompressionLevel.Optimal);
                                using (var entryStream = fileInArchive1.Open())
                                using (var fileToCompressStream = new MemoryStream(fileBytes1))
                                {
                                    fileToCompressStream.CopyTo(entryStream);
                                }
                                var fileInArchive2 = archive.CreateEntry("ca.crt", CompressionLevel.Optimal);
                                using (var entryStream = fileInArchive2.Open())
                                using (var fileToCompressStream = new FileStream(_setting.MqttBroker.CACertificateFile, FileMode.Open))
                                {
                                    fileToCompressStream.CopyTo(entryStream);
                                }
                            }
                            compressedBytes = outStream.ToArray();
                        }
                        return File(compressedBytes, "application/octet-stream", fileNameZip);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResult(ApiCode.NotFoundDevice, ex.Message));
            }
        }

        /// <summary>
        ///获取指定设备的最新属性
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/AttributeLatest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<List<AttributeDataDto>>> GetAttributeLatest(Guid deviceId)
        {
            Device dev = Found(deviceId);
            if (dev == null)
            {
                return new ApiResult<List<AttributeDataDto>>(ApiCode.CantFindObject, "Device's Identity not found", null);
            }
            else
            {
                var devid = from t in _context.AttributeLatest
                            where t.DeviceId == deviceId
                            select new AttributeDataDto()
                            {
                                DataSide = t.DataSide,
                                DateTime = t.DateTime,
                                KeyName = t.KeyName,
                                DataType = t.Type,
                                Value = t.ToObject()
                            };
                if (!devid.Any())
                {
                    return new ApiResult<List<AttributeDataDto>>(ApiCode.CantFindObject, "Device's Identity not found", null);
                }
                return new ApiResult<List<AttributeDataDto>>(ApiCode.Success, "Ok", await devid.ToListAsync());
            }
        }

        /// <summary>
        /// 获取指定设备指定keys的最新属性
        /// </summary>
        /// <param name="deviceId">Which device do you read?</param>
        /// <param name="keys">Specify key name list , use , or space or  ; to split </param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/AttributeLatest/{keys}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<List<AttributeDataDto>>> GetAttributeLatest(Guid deviceId, string keys)
        {
            Device dev = Found(deviceId);
            if (dev == null)
            {
                return new ApiResult<List<AttributeDataDto>>(ApiCode.NotFoundDevice, "Device's  not found", null);
            }
            else
            {
                var kv = from t in _context.AttributeLatest where t.DeviceId == t.DeviceId && keys.Split(',', ' ', ';').Contains(t.KeyName) select new AttributeDataDto() { DataSide = t.DataSide, DateTime = t.DateTime, KeyName = t.KeyName, DataType = t.Type, Value = t.ToObject() };

                return new ApiResult<List<AttributeDataDto>>(ApiCode.Success, "Ok", await kv.ToListAsync());
            }
        }

        private Device Found(Guid deviceId)
        {
            return FoundAsync(deviceId).GetAwaiter().GetResult();
        }

        private async Task<Device> FoundAsync(Guid deviceId)
        {
            Device dev = null;
            if (User.IsInRole(nameof(UserRole.TenantAdmin)))
            {
                var tid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Tenant);
                dev = await _context.Device.Include(d => d.Tenant).FirstOrDefaultAsync(d => d.Id == deviceId && d.Tenant.Id.ToString() == tid.Value && d.Status > -1);
            }
            else if (User.IsInRole(nameof(UserRole.NormalUser)))
            {
                var cid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Customer);
                dev = await _context.Device.Include(d => d.Customer).FirstOrDefaultAsync(d => d.Id == deviceId && d.Customer.Id.ToString() == cid.Value && d.Status > -1);
            }
            return dev;
        }

        /// <summary>
        ///获取指定设备的最新遥测数据
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/TelemetryLatest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<List<TelemetryDataDto>>> GetTelemetryLatest(Guid deviceId)
        {
            Device dev = Found(deviceId);
            if (dev == null)
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.NotFoundDeviceIdentity, "Device's Identity not found", null);
            }

            try
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.Success, "Ok",
                    await _storage.GetTelemetryLatest(deviceId));
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.Exception, ex.Message,
                    null);
            }
        }

        /// <summary>
        /// 获取指定设备的指定key 的遥测数据
        /// </summary>
        /// <param name="deviceId">Which device do you read?</param>
        /// <param name="keys">指定键值列表， 使用分号或者逗号分割 。 </param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/TelemetryLatest/{keys}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<List<TelemetryDataDto>>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            Device dev = Found(deviceId);
            if (dev == null)
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.NotFoundDeviceIdentity, "Device's Identity not found", null);
            }
            else
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.Success, "Ok", await _storage.GetTelemetryLatest(deviceId, keys));
            }
        }

        /// <summary>
        /// 获取指定设备和指定时间， 指定key的数据
        /// </summary>
        /// <param name="deviceId">Which device do you read?</param>
        /// <param name="keys">Specify key name list , use , or space or  ; to split </param>
        /// <param name="begin">开始以时间， 比如 2019-06-06 12:24</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/TelemetryLatest/{keyName}/{begin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<List<TelemetryDataDto>>> GetTelemetryData(Guid deviceId, string keys, DateTime begin)
        {
            Device dev = Found(deviceId);
            if (dev == null)
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.NotFoundDeviceIdentity, "Device's Identity not found", null);
            }
            else
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.Success, "Ok",
                    keys == "all"
                        ? await _storage.LoadTelemetryAsync(deviceId, begin)
                        : await _storage.LoadTelemetryAsync(deviceId, keys, begin));
            }
        }

        /// <summary>
        /// 返回指定设备的的遥测数据， 按照keyname 和指定时间范围获取，如果keyname 为 all  , 则返回全部key 的数据
        /// </summary>
        /// <param name="deviceId">Which device do you read?</param>
        /// <param name="keys">Specify key name list , use , or space or  ; to split </param>
        /// <param name="begin">For example: 2019-06-06 12:24</param>
        /// <param name="end">For example: 2019-06-06 12:24</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{deviceId}/TelemetryData/{keyName}/{begin}/{end}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<List<TelemetryDataDto>>> GetTelemetryData(Guid deviceId, string keys, DateTime begin, DateTime end)
        {
            Device dev = Found(deviceId);
            if (dev == null)
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.NotFoundDeviceIdentity, "Device's Identity not found", null);
            }
            else
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.Success, "Ok",
                    keys == "all" ? await _storage.LoadTelemetryAsync(deviceId, begin, end) : await _storage.LoadTelemetryAsync(deviceId, keys, begin, end));
            }
        }

        /// <summary>
        /// 获取设备详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Devices/5
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<Guid>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<Device>> GetDevice(Guid id)
        {
            Device device = await FoundAsync(id);
            if (device == null)
            {
                return new ApiResult<Device>(ApiCode.NotFoundDeviceIdentity, "Device's Identity not found", null);
            }
            return new ApiResult<Device>(ApiCode.Success, "Ok", device);
        }

        /// <summary>
        /// 修改设备
        /// </summary>
        /// <param name="id"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        // PUT: api/Devices/5
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult<Guid>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ApiResult<bool>> PutDevice(Guid id, DevicePutDto device)
        {
            if (id != device.Id)
            {
                return new ApiResult<bool>(ApiCode.InValidData, "Device's Identity not InValidData", false);
            }

            var cid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Customer);
            var tid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Tenant);
            var dev = _context.Device.Include(d => d.Tenant).Include(d => d.Customer).First(d => d.Id == device.Id);
            var tenid = dev.Tenant.Id;
            var cusid = dev.Customer.Id;

            if (dev == null)
            {
                return new ApiResult<bool>(ApiCode.NotFoundDeviceIdentity, "Device's Identity not found", false);
            }
            else if (dev.Tenant?.Id.ToString() != tid.Value || dev.Customer?.Id.ToString() != cid.Value)
            {
                return new ApiResult<bool>(ApiCode.DoNotAllow, "Do not allow access to devices from other customers or tenants", false);
                // return BadRequest(new ApiResult(ApiCode.DoNotAllow, $"Do not allow access to devices from other customers or tenants"));
            }

            //  dev.DeviceModel = _context.DeviceModels.FirstOrDefault(c => c.DeviceModelId == device.DeviceModelId);
            dev.Name = device.Name;
            dev.Timeout = device.Timeout;
            try
            {
                await _context.SaveChangesAsync();
                var identity = _context.DeviceIdentities.FirstOrDefault(c => c.Device.Id == dev.Id);
                if (identity != null)
                {
                    identity.IdentityType = device.IdentityType;
                    _context.DeviceIdentities.Update(identity); await _context.SaveChangesAsync();
                }
                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(id))
                {
                    //  return NotFound(new ApiResult<Guid>(ApiCode.NotFoundDevice, $"Device {id} not found ", id));

                    return new ApiResult<bool>(ApiCode.NotFoundDevice, "Device {id} not found ", false);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 创建设备， 客户ID和租户ID均为当前登录用户所属
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        // POST: api/Devices
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<DevicePostDto>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<Device>> PostDevice(DevicePostDto device)
        {
            var cid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Customer);
            var tid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Tenant);
            var devvalue = new Device()
            {
                Name = device.Name,
                DeviceType = device.DeviceType,
                Timeout = device.Timeout,
                LastActive = DateTime.Now,
                Status = 1,
                //   DeviceModel = _context.DeviceModels.FirstOrDefault(c => c.DeviceModelId == device.DeviceModelId),
                //CreateDate = DateTime.Today,
                //CreateMonth =DateTime.Now.ToString("yyyy-MM"),
                //CreateDateTime = DateTime.Now
            };
            devvalue.Tenant = _context.Tenant.Find(new Guid(tid.Value));
            devvalue.Customer = _context.Customer.Find(new Guid(cid.Value));

            if (devvalue.Tenant == null || devvalue.Customer == null)
            {
                return new ApiResult<Device>(ApiCode.NotFoundTenantOrCustomer, "Not found Tenant or Customer", null);
            }
            _context.Device.Add(devvalue);
            _context.AfterCreateDevice(devvalue);
            await _context.SaveChangesAsync();
            var identity = _context.DeviceIdentities.FirstOrDefault(c => c.Device.Id == devvalue.Id);
            if (identity != null)
            {
                identity.IdentityType = device.IdentityType;
                _context.DeviceIdentities.Update(identity);
                await _context.SaveChangesAsync();
            }

            await this._queue.PublishAsync("iotsharp.services.platform.addnewdevice", devvalue);
            return new ApiResult<Device>(ApiCode.Success, "Ok", await FoundAsync(devvalue.Id));
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<Guid>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<Device>> DeleteDevice(Guid id)
        {
            Device device = Found(id);
            if (device == null)
            {
                return new ApiResult<Device>(ApiCode.NotFoundTenantOrCustomer, "Device {id} not found", null);
            }

            device.Status = -1;
            _context.Device.Update(device);
            await _context.SaveChangesAsync();
            return new ApiResult<Device>(ApiCode.Success, "Ok", device);
        }

        private bool DeviceExists(Guid id)
        {
            return _context.Device.Any(e => e.Id == id);
        }

        /// <summary>
        /// 远程控制指定设备， 此方法通过给远程设备发送mqtt消息进行控制，设备在收到信息后回复结果，此方法才算调用结束
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="method"></param>
        /// <param name="timeout"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{access_token}/Rpc/{method}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<Dic>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<string>> Rpc(string access_token, string method, int timeout, object args)
        {
            ActionResult<string> result = null;
            _logger.LogInformation($"RPC  access_token:{access_token}   method:{method}  timeout: {timeout}  ");
            var (ok, dev) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                _logger.LogInformation($"RPC 通过 access_token:{access_token}  无法找到设备。  ");
                return Ok(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a device's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                try
                {
                    _logger.LogInformation($"RPC 通过 access_token:{access_token}  找到设备{dev.Name}  ");
                    var rpcClient = new RpcClient(_mqtt, _logger);
                    var _timeout = TimeSpan.FromSeconds(timeout);
                    var qos = MqttQualityOfServiceLevel.AtMostOnce;
                    var payload = Newtonsoft.Json.JsonConvert.SerializeObject(args);
                    await rpcClient.ConnectAsync();
                    byte[] response = null;
                    //如果是网关的子设备， 因为客户端无法知道Id，因此发至名称
                    if (dev.DeviceType == DeviceType.Device && string.IsNullOrEmpty(dev.Owner?.Name))
                    {
                        _logger.LogInformation($"RPC  设备{dev.Name} 的所有者名称不为空， 因此是子设备。 传入名称 作为topic ");
                        response = await rpcClient.ExecuteAsync(_timeout, dev.Name, method, payload, qos);
                    }
                    else
                    {
                        _logger.LogInformation($"RPC  设备{dev.Name} 是网关或者是独立设备， 传入设备ID{dev.Id}");
                        response = await rpcClient.ExecuteAsync(_timeout, dev.Id.ToString(), method, payload, qos);
                    }
                    await rpcClient.DisconnectAsync();
                    _logger.LogInformation($"RPC  设备{dev.Name} 调用完成 { System.Text.Encoding.UTF8.GetString(response)}");
                    result = Ok(System.Text.Encoding.UTF8.GetString(response));
                }
                catch (MqttCommunicationTimedOutException ex1)
                {
                    _logger.LogError(ex1, $"{dev.Id} RPC Timeout {ex1.Message}");
                    result = Ok(new ApiResult(ApiCode.RPCTimeout, $"{dev.Name} RPC Timeout {ex1.Message}"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{dev.Id} RPCFailed Timeout {ex.Message}");
                    result = Ok(new ApiResult(ApiCode.RPCFailed, $"{dev.Id} RPCFailed {ex.Message}"));
                }
            }
            return result;
        }

        /// <summary>
        /// HTTP方式上传遥测数据
        /// </summary>
        /// <param name="access_token">Device 's access token</param>
        /// <param name="telemetrys"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{access_token}/Telemetry")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<Dic>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<Dic>>> Telemetry(string access_token, Dictionary<string, object> telemetrys)
        {
            Dic exceptions = new Dic();
            var (ok, device) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return Ok(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a device's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                
                var result = await _context.SaveAsync<TelemetryLatest>(telemetrys, device.Id, DataSide.ClientSide);
                return Ok(new ApiResult<Dic>(result.ret > 0 ? ApiCode.Success : ApiCode.NothingToDo, result.ret > 0 ? "OK" : "No Telemetry save", new Dic(result.exceptions?.Select(f => new DicKV(f.Key, f.Value.Message)))));
            }
        }

    


        /// <summary>
        /// 获取服务测的设备熟悉
        /// </summary>
        /// <param name="access_token">Device 's access token </param>
        ///<param name="dataSide">Specifying data side.</param>
        ///<param name="keys">Specifying Attribute's keys</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{access_token}/Attributes/{dataSide}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult<Dic>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<AttributeLatest>> Attributes(string access_token, DataSide dataSide, string keys)
        {
            Dic exceptions = new Dic();
            var (ok, device) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return Ok(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a device's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                var deviceId = device.Id;
                try
                {
                    var attributes = from dev in _context.AttributeLatest where dev.DeviceId == deviceId select dev;
                    var fs = from at in await attributes.ToListAsync() where at.DataSide == dataSide && keys.Split(',', options: StringSplitOptions.RemoveEmptyEntries).Contains(at.KeyName) select at;
                    return Ok(new ApiResult<AttributeLatest[]>(ApiCode.Success, "Ok", fs.ToArray()));
                }
                catch (Exception ex)
                {
                    return Ok(new ApiResult(ApiCode.Exception, $"{deviceId}  {ex.Message}"));
                }
            }
        }

        /// <summary>
        /// 上传客户侧属性数据
        /// </summary>
        /// <param name="access_token">Device 's access token </param>
        /// <param name="attributes">attributes</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{access_token}/Attributes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<Dic>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult<Dic>>> Attributes(string access_token, Dictionary<string, object> attributes)
        {
            Dic exceptions = new Dic();
            var (ok, dev) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return Ok(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a device's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                var result = await _context.SaveAsync<AttributeLatest>(attributes, dev.Id, DataSide.ClientSide);
                return Ok(new ApiResult<Dic>(result.ret > 0 ? ApiCode.Success : ApiCode.NothingToDo, result.ret > 0 ? "OK" : "No Attribute save", new Dic(result.exceptions?.Select(f => new DicKV(f.Key, f.Value.Message)))));
            }
        }

        /// <summary>
        /// 上传原始Json或者xml 通过规则链进行解析。 
        /// </summary>
        /// <param name="access_token">Device 's access token </param>
        /// <param name="body"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{access_token}/PushDataToMap/{fromat}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<Dic>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult>> PushDataToMap(string access_token, [FromBody] string body, string format = "json")
        {

            var (ok, _dev) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return NotFound(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                string json = body;
                if (format == "xml")
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(body);
                    json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);
                }
                var atts = await _caching.GetAsync($"_map_{_dev.Id}", async () =>
                {
                    var guids =  from al in _context.AttributeLatest where al.DeviceId == _dev.Id  &&  (al.KeyName== _map_to_devname ||  al.KeyName.StartsWith(_map_to_attribute_) || al.KeyName.StartsWith(_map_to_telemety_)) select al;
                    return await guids.ToArrayAsync();
                }
             , TimeSpan.FromSeconds(_setting.RuleCachingExpiration));
                if (atts.HasValue)
                {
                    try
                    {
                        var jt = JToken.Parse(json);
                        var devnamekey = atts.Value.FirstOrDefault(g => g.KeyName == _map_to_devname);
                        if (devnamekey != null)
                        {
                            var devnameformatkey = atts.Value.FirstOrDefault(g => g.KeyName == _map_to_devname_format)?.Value_String;
                            var devname = (jt.SelectToken(devnamekey.Value_String) as JValue).ToObject<string>() ;
                            if (devname != null && !string.IsNullOrEmpty(devname))
                            {
                                var _devname = (devnameformatkey ?? _map_var_devname).Replace(_map_var_devname, devname);
                                if (!string.IsNullOrEmpty(_devname))
                                {
                                    var device = _dev.JudgeOrCreateNewDevice(devname, _scopeFactor, _logger);

                                    var pairs_att = new Dictionary<string, object>();
                                    var pairs_tel = new Dictionary<string, object>();
                                    atts.Value?.ToList().ForEach(g =>
                                    {
                                        var value =( jt.SelectToken(g.Value_String) as JValue)?.JValueToObject();
                                        if (value != null && g.KeyName?.Length>0)
                                        {
                                            if (g.KeyName.StartsWith(_map_to_attribute_) && g.KeyName.Length> _map_to_attribute_.Length)
                                            {
                                                pairs_att.Add(g.KeyName.Substring(_map_to_attribute_.Length), value);
                                            }
                                            else if (g.KeyName.StartsWith(_map_to_telemety_) && g.KeyName.Length > _map_to_telemety_.Length )
                                            {
                                                pairs_tel.Add(g.KeyName.Substring(_map_to_telemety_.Length), value);
                                            }
                                        }
                                    });
                                    if (pairs_tel.Any())
                                    {
                                        _queue.PublishTelemetryData(new RawMsg() { DeviceId = device.Id, MsgBody = pairs_tel, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.TelemetryData });
                                    }
                                    if (pairs_att.Any())
                                    {
                                        _queue.PublishAttributeData(new RawMsg() { DeviceId = device.Id, MsgBody = pairs_tel, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.AttributeData });
                                    }
                                }
                            }
                            else
                            {
                                _logger.LogInformation($"数据");
                            }
                        }
                        return Ok(new ApiResult(ApiCode.Success, "OK"));
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new ApiResult(ApiCode.Exception, ex.Message));
                    }
                }
                else
                {
                    _logger.LogInformation($"{_dev}的数据不符合规范， 也无相关规则链处理。");
                    return BadRequest(new ApiResult(ApiCode.InValidData, $"{_dev}的数据不符合规范， 也无相关规则链处理。"));
                }
            }
        }

        /// <summary>
        /// 上传原始Json或者xml 通过规则链进行解析。 
        /// </summary>
        /// <param name="access_token">Device 's access token </param>
        /// <param name="body">原始数据， Post 在Body里面。 </param>
        /// <param name="format">只支持json和 xml， XML会转换为 Json。</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{access_token}/PushDataToRuleChains/{fromat}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<Dic>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult>> PushDataToRuleChains(string access_token, [FromBody]string body,string format="json")
        {
        
            var (ok, _dev) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return NotFound(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                string json = body;
                if (format=="xml")
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(body);
                    json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);
                }
                var rules = await _caching.GetAsync($"ruleid_{_dev.Id}_raw", async () =>
                {
                    var guids = await _context.GerDeviceRulesIdList(_dev.Id, MountType.RAW);
                    return guids;
                }
             , TimeSpan.FromSeconds(_setting.RuleCachingExpiration));
                if (rules.HasValue)
                {
                    try
                    {
                        rules.Value.ToList().ForEach(async g =>
                        {
                            _logger.LogInformation($"{_dev.Id}的数据通过规则链{g}进行处理。");
                            var result = await _flowRuleProcessor.RunFlowRules(g, body, _dev.Id, EventType.Normal, null);

                        });
                        return Ok(new ApiResult(ApiCode.Success, "OK"));
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new ApiResult(ApiCode.Exception,ex.Message ));
                    }
                }
                else
                {
                    _logger.LogInformation($"{_dev}的数据不符合规范， 也无相关规则链处理。");
                    return BadRequest(new ApiResult(ApiCode.InValidData, $"{_dev}的数据不符合规范， 也无相关规则链处理。"));
                }
            }
        }

        /// <summary>
        /// 服务侧新增属性
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpPost("{access_token}/AddAttribute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<bool>> AddAttribute(string access_token, DeviceAttributeDto attribute)
        {
            if (_context.DataStorage.Any(c =>
                c.DeviceId == attribute.DeviceId && c.KeyName.ToLower() == attribute.KeyName.ToLower()))
            {
                return new ApiResult<bool>(ApiCode.AlreadyExists, "this field name is exist", false);
            }
            _context.DataStorage.Add(new DataStorage()
            {
                DataSide = attribute.DataSide,
                DeviceId = attribute.DeviceId,
                Type = attribute.Type,
                DateTime = DateTime.Now,
                KeyName = attribute.KeyName,
                Catalog = DataCatalog.AttributeLatest
            });
            await _context.SaveChangesAsync();
            return new ApiResult<bool>(ApiCode.Success, "Ok", true);
        }

        /// <summary>
        /// 服务侧和任意侧属性修改
        /// </summary>
        /// <param name="devid"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpPost("{devid}/EditAttribute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<Dic>> EditAttribute(Guid devid, DeviceAttrEditDto attributes)
        {
            var result = await _context.SaveAsync<AttributeLatest>(attributes.anyside, devid, DataSide.AnySide);
            var result1 = await _context.SaveAsync<AttributeLatest>(attributes.serverside, devid, DataSide.ServerSide);

            if (result.ret > 0 && result1.ret > 0)
            {
                return new ApiResult<Dic>(ApiCode.Success, "Ok", null);
            }
            if (result.ret == 0)
            {
                return new ApiResult<Dic>(ApiCode.AlreadyExists, "anyside attribute update failed", new Dic(result.exceptions?.Select(f => new DicKV(f.Key, f.Value.Message)) ?? Array.Empty<DicKV>()));
            }
            if (result1.ret == 0)
            {
                return new ApiResult<Dic>(ApiCode.AlreadyExists, "serverside attribute update failed", new Dic(result1.exceptions?.Select(f => new DicKV(f.Key, f.Value.Message)) ?? Array.Empty<DicKV>()));
            }

            return new ApiResult<Dic>(ApiCode.InValidData, " attributes update failed", null);
        }

        /// <summary>
        /// SessionStatus
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("SessionStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<IList<MqttSessionStatus>>> GetSessionStatus()
        {
            return new ApiResult<IList<MqttSessionStatus>>(ApiCode.Success, "OK", await _serverEx.GetSessionsAsync());
        }

        /// <summary>
        /// SessionStatus
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("ClientStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<IList<MqttClientStatus>>> GetClientStatus()
        {
            return new ApiResult<IList<MqttClientStatus>>(ApiCode.Success, "OK", await _serverEx.GetClientsAsync());
        }

        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("SessionsCount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<int>> GetSessionsCount()
        {
            return new ApiResult<int>(ApiCode.Success, "OK", (await _serverEx.GetClientsAsync()).Count);
        }
    }
}