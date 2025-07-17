using IoTSharp.EventBus;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using IoTSharp.Gateways;
using IoTSharp.Models;
using IoTSharp.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Exceptions;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml;
using Dic = System.Collections.Generic.Dictionary<string, string>;
using DicKV = System.Collections.Generic.KeyValuePair<string, string>;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using IoTSharp.Extensions.X509;
using AutoMapper;
using MQTTnet;

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
        private readonly ApplicationDbContext _context;
        private readonly MqttClientOptions _mqtt;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IStorage _storage;
        private readonly MqttServer _serverEx;
        private readonly AppSettings _setting;
        private readonly IPublisher _queue;
        private readonly FlowRuleProcessor _flowRuleProcessor;
        private readonly IEasyCachingProvider _caching;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly ILoggerFactory _loggerFactory;

        public DevicesController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, ILogger<DevicesController> logger, MqttServer serverEx, ApplicationDbContext context, MqttClientOptions mqtt, IStorage storage, IOptions<AppSettings> options, IPublisher queue
            , IEasyCachingProviderFactory factory, FlowRuleProcessor flowRuleProcessor, IServiceScopeFactory scopeFactor, ILoggerFactory loggerFactory)
        {
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(options.Value.CachingUseIn)}";
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
            _caching = factory.GetCachingProvider(_hc_Caching);
            _scopeFactor = scopeFactor;
            _loggerFactory = loggerFactory;
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
                return new ApiResult<List<Device>>(ApiCode.Success, $"Successfully retrieved devices for customer {customerId}", await f.ToListAsync());
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
            m.Limit = m.Limit < 5 ? 5 : m.Limit;
            try
            {
            
                var query=from  c in _context.Device.Include(c => c.DeviceIdentity) where c.Customer.Id == m.customerId && !c.Deleted && c.Tenant.Id == profile.Tenant select c;
                if (m.OnlyActive)
                {
                    var al = from a in _context.AttributeLatest where   a.KeyName == Constants._Active &&a.Value_Boolean==true   select a.DeviceId;
                    query = from x in query where al.Contains( x.Id)   select x;
                }
                if (!string.IsNullOrEmpty(m.Name))
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(m.Name, @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$"))
                    {
                        var id = Guid.Parse(m.Name);
                        query = from  x in query  where   x.Id == id select x ;
                    }
                    else
                    {
                        query = from x in query where x.Name.Contains(m.Name) select x;
                    }
                }
                var lst = await query.Skip((m.Offset) * m.Limit).Take(m.Limit).Select(x => new DeviceDetailDto()
                {
                    Id = x.Id,
                    Name = x.Name,
                    IdentityId = x.DeviceIdentity.IdentityId,
                    IdentityValue = x.DeviceIdentity.IdentityType == IdentityType.X509Certificate ? "" : x.DeviceIdentity.IdentityValue,
                    DeviceType = x.DeviceType,
                    Owner = x.Owner,
                    TenantId=x.Tenant.Id,
                    TenantName=x.Tenant.Name,
                    CustomerId=x.Customer.Id,
                    CustomerName=x.Customer.Name,
                    Timeout = x.Timeout,
                    IdentityType = x.DeviceIdentity.IdentityType
                }).ToListAsync();
                await QueryActivityInfo(lst);
                return new ApiResult<PagedData<DeviceDetailDto>>(ApiCode.Success, "OK", new PagedData<DeviceDetailDto>
                {
                    total = await query.CountAsync(),
                    rows = lst
                });
            }
            catch (Exception e)
            {
                return new ApiResult<PagedData<DeviceDetailDto>>(ApiCode.Exception, e.Message, null);
            }

        }
        private async Task QueryActivityInfo(DeviceDetailDto dev)
        {
            var id = dev.Id;
            var al = from a in _context.AttributeLatest where id == a.DeviceId && (a.KeyName == Constants._Active || a.KeyName == Constants._LastActivityDateTime) select a;
            var alx = await al.ToListAsync();
            if (alx != null)
            {
                alx.ForEach(a =>
                {
                    if (a.KeyName == Constants._Active)
                    {
                        dev.Active = (bool)a.Value_Boolean;
                    }
                    else if (a.KeyName == Constants._LastActivityDateTime)
                    {
                        dev.LastActivityDateTime = a.Value_DateTime;
                    }
                });
            }
        }
        private async Task QueryActivityInfo(List<DeviceDetailDto> lst)
        {
            var devlst = lst.Select(c => c.Id).ToList();
            var al = from a in _context.AttributeLatest where devlst.Contains(a.DeviceId) && (a.KeyName == Constants._Active || a.KeyName == Constants._LastActivityDateTime) select a;
            var allist = await al.ToListAsync();
            allist.ForEach(a =>
            {
                var dev = lst.FirstOrDefault(d => d.Id == a.DeviceId);
                if (dev != null)
                {
                    if (a.KeyName == Constants._Active)
                    {
                        dev.Active = (bool)a.Value_Boolean;
                    }
                    else if (a.KeyName == Constants._LastActivityDateTime)
                    {
                        dev.LastActivityDateTime = a.Value_DateTime;
                    }
                }
            });
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
            var option = _setting.MqttBroker;
            if (option.CACertificate == null)
            {
                return new ApiResult<DeviceIdentity>(ApiCode.ExceptionDeviceIdentity, "Not found CACertificate", null);
            }
            else
            {
                var did = await _context.DeviceIdentities.Include(d => d.Device).FirstOrDefaultAsync(c => c.Device.Id == deviceId);
                var cust = from c in _context.Device.Include(d => d.Customer).Include(d => d.Tenant) where c.Id == deviceId select c;
                var dev = cust.FirstOrDefault();
                if (did != null && dev != null)
                {
               
                    if (Uri.TryCreate(_setting.MqttBroker.DomainName, UriKind.Absolute, out Uri _uri))
                    {
                        SubjectAlternativeNameBuilder altNames = new SubjectAlternativeNameBuilder();
                        altNames.AddUserPrincipalName(did.Device.Id.ToString());
                        altNames.AddDnsName(_uri.Host);
                        if (_setting.MqttBroker.TlsPort > 0 && _setting.MqttBroker.TlsPort < 65535)
                        {
                            altNames.AddUri(new Uri($"mqtt://{_uri.Host}:{_setting.MqttBroker.TlsPort}"));
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
                            return new ApiResult<DeviceIdentity>(ApiCode.ExceptionDeviceIdentity, "Please set valid MqttBroker TlsPort", null);
                        }
                    }

                    return new ApiResult<DeviceIdentity>(ApiCode.ExceptionDeviceIdentity, "Please set MqttBroker domain name", null);

                }
                else
                {
                    return new ApiResult<DeviceIdentity>(ApiCode.ExceptionDeviceIdentity, "Not found device identity", null);
                }
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
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public ActionResult DownloadCertificates(Guid deviceId)
        {
            try
            {
                var dt = _context.DeviceIdentities.Include(d => d.Device).FirstOrDefault(c => c.Device.Id == deviceId);
                if (dt == null || dt.IdentityType != IdentityType.X509Certificate || string.IsNullOrEmpty(dt.IdentityValue))
                {
                    return Ok(new ApiResult(ApiCode.NotFoundDevice, "未找到设备或设备公钥、秘钥为空"));
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
                        return Ok(new ApiResult(ApiCode.NotFoundDevice, "秘钥格式未能解析。可能是版本不通。 "));
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
                return Ok(new ApiResult(ApiCode.NotFoundDevice, ex.Message));
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
            Device dev = await FoundAsync(deviceId);
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
                                 DeviceId= t.DeviceId,
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
        ///获取指定Key和设备Id列表的最新属性
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpPost("AttributeLatestByKeyNameAndDeviceId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<List<AttributeDataDto>>> GetAttributeLatestByKeyNameAndDeviceId(AttributeLatestByKeyNameAndDeviceIdDto dto)
        {
            return new ApiResult<List<AttributeDataDto>>(ApiCode.Success, "Ok", await _context.AttributeLatest.Where(p => dto.deviceIds.Any(c => c == p.DeviceId) && dto.keyNames.Contains(p.KeyName)).Select(t =>
                new AttributeDataDto()
                {
                    DeviceId = t.DeviceId,
                    DataSide = t.DataSide,
                    DateTime = t.DateTime,
                    KeyName = t.KeyName,
                    DataType = t.Type,
                    Value = t.ToObject()
                }).ToListAsync());
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
            Device dev = await FoundAsync(deviceId);
            if (dev == null)
            {
                return new ApiResult<List<AttributeDataDto>>(ApiCode.NotFoundDevice, "Device's  not found", null);
            }
            else
            {
                string[] keyarys = keys.Split(',', ' ', ';');
                var kv = from t in _context.AttributeLatest where t.DeviceId == deviceId && keyarys.Contains(t.KeyName) select new AttributeDataDto() { DataSide = t.DataSide, DateTime = t.DateTime, KeyName = t.KeyName, DataType = t.Type, Value = t.ToObject() };

                return new ApiResult<List<AttributeDataDto>>(ApiCode.Success, "Ok", await kv.ToListAsync());
            }
        }



        private async Task<Device> FoundAsync(Guid deviceId)
        {
            Device dev = null;
            if (User.IsInRole(nameof(UserRole.TenantAdmin)))
            {
                var tid = Guid.Parse(User.Claims.First(c => c.Type == IoTSharpClaimTypes.Tenant).Value);
                dev = await _context.Device.Include(d => d.Tenant).Include(d=>d.Customer).Include(d=>d.DeviceIdentity).FirstOrDefaultAsync(d => d.Id == deviceId && d.Tenant.Id == tid && !d.Deleted);
            }
            else if (User.IsInRole(nameof(UserRole.NormalUser)))
            {
                var cid = Guid.Parse(User.Claims.First(c => c.Type == IoTSharpClaimTypes.Customer).Value);
                dev = await _context.Device.Include(d => d.Customer).Include(d => d.Customer).Include(d => d.DeviceIdentity).FirstOrDefaultAsync(d => d.Id == deviceId && d.Customer.Id == cid && !d.Deleted);
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
            Device dev = await FoundAsync(deviceId);
            if (dev == null)
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.ExceptionDeviceIdentity, "Device's Identity not found", null);
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
            Device dev = await FoundAsync(deviceId);
            if (dev == null)
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.ExceptionDeviceIdentity, "Device's Identity not found", null);
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
        [HttpGet("{deviceId}/TelemetryLatest/{keys}/{begin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<List<TelemetryDataDto>>> GetTelemetryData(Guid deviceId, string keys, DateTime begin)
        {
            Device dev = await FoundAsync(deviceId);
            if (dev == null)
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.ExceptionDeviceIdentity, "Device's Identity not found", null);
            }
            else
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.Success, "Ok",
                    await _storage.LoadTelemetryAsync(deviceId, keys == "all" ? string.Empty : keys, begin, DateTime.UtcNow, TimeSpan.Zero, Aggregate.None));
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
        [HttpGet("{deviceId}/TelemetryData/{keys}/{begin}/{end}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<List<TelemetryDataDto>>> GetTelemetryData(Guid deviceId, string keys, DateTime begin, DateTime end)
        {
            Device dev = await FoundAsync(deviceId);
            if (dev == null)
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.ExceptionDeviceIdentity, "Device's Identity not found", null);
            }
            else
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.Success, "Ok",
                  await _storage.LoadTelemetryAsync(deviceId, keys == "all" ? string.Empty : keys, begin, end, TimeSpan.Zero, Aggregate.None));
            }
        }

        /// <summary>
        /// 返回指定设备的的遥测数据， 按照keyname 和指定时间范围获取，如果keyname 为 all  , 则返回全部key 的数据
        /// </summary>
        /// <param name="deviceId">指定设备ID</param>
        /// <param name="queryDto">查询条件例子:
        ///{
        /// "keys": "",
        /// "begin": "2022-03-23T11:44:56.488Z",
        /// "every": "1.03:14:56:166",
        /// "aggregate": "Mean"
        /// }
        /// </param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpPost("{deviceId}/TelemetryData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<List<TelemetryDataDto>>> GetTelemetryData(Guid deviceId, TelemetryDataQueryDto queryDto)
        {
            Device dev = await FoundAsync(deviceId);
            if (dev == null)
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.ExceptionDeviceIdentity, "Device's Identity not found", null);
            }
            else
            {
                return new ApiResult<List<TelemetryDataDto>>(ApiCode.Success, "Ok",
                         await _storage.LoadTelemetryAsync(deviceId, queryDto.keys, queryDto.begin, queryDto.end, queryDto.every, queryDto.aggregate));
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
        public async Task<ApiResult<DeviceDetailDto>> GetDevice(Guid id)
        {
            Device x = await FoundAsync(id);
            if (x == null)
            {
                return new ApiResult<DeviceDetailDto>(ApiCode.ExceptionDeviceIdentity, "Device's Identity not found", null);
            }
            else
            {
                var device = new DeviceDetailDto()
                {
                    Id = x.Id,
                    Name = x.Name,
                    IdentityId = x.DeviceIdentity.IdentityId,
                    IdentityValue = x.DeviceIdentity.IdentityType == IdentityType.X509Certificate ? "" : x.DeviceIdentity.IdentityValue,
                    TenantName = x.Tenant.Name,
                    CustomerName = x.Customer.Name,
                    TenantId = x.Tenant.Id,
                    CustomerId= x.Customer.Id,
                    DeviceType = x.DeviceType,
                    Owner = x.Owner,
                    Timeout = x.Timeout,
                    IdentityType = x.DeviceIdentity.IdentityType
                };
                await QueryActivityInfo(device);
                return new ApiResult<DeviceDetailDto>(ApiCode.Success, "Ok", device);
            }
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
                return new ApiResult<bool>(ApiCode.ExceptionDeviceIdentity, "Device's Identity not found", false);
            }
            else if (dev.Tenant?.Id.ToString() != tid.Value || dev.Customer?.Id.ToString() != cid.Value)
            {
                return new ApiResult<bool>(ApiCode.DoNotAllow, "Do not allow access to devices from other customers or tenants", false);
            }
            dev.Name = device.Name;
            dev.Timeout = device.Timeout;
            dev.DeviceType = device.DeviceType;
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



        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPost("produce/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<DevicePostDto>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<Device>> PostDevice(Guid id, DevicePostProduceDto device)
        {
            var produce = await _context.Produces.Include(p => p.DefaultAttributes).FirstOrDefaultAsync(p => p.Id == id && p.Deleted==false);
            if (produce == null)
            {
                return new ApiResult<Device>(ApiCode.NotFoundProduce, "Not found Produce", null);
            }
            var dto = new DevicePostDto() { ProductId = id, Name = device.Name, DeviceType = produce.DefaultDeviceType, IdentityType = produce.DefaultIdentityType, Timeout = produce.DefaultTimeout };
            var dev = await PostDevice(dto);
            if (dev.Code == (int)ApiCode.Success)
            {
                MapperConfiguration mapperConfiguration = new MapperConfiguration(options => { options.CreateMap<ProduceData, AttributeLatest>(); },_loggerFactory);
                IMapper mapper = mapperConfiguration.CreateMapper();

                var atts = produce.DefaultAttributes.Select(c =>
                {
                    AttributeLatest atx = mapper.Map<ProduceData, AttributeLatest>(c);
                    atx.Catalog = DataCatalog.AttributeLatest;
                    atx.DeviceId = dev.Data.Id;
                    atx.DataSide = DataSide.ServerSide;
                    atx.DateTime = DateTime.UtcNow;
                    return atx;
                });
                _context.AttributeLatest.AddRange(atts);
                await _context.SaveChangesAsync();
                return new ApiResult<Device>(ApiCode.Success, "Ok", dev.Data);
            }
            else
            {
                return dev;
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
                Deleted = false,
                
            };
            devvalue.Tenant = _context.Tenant.Find(new Guid(tid.Value));
            devvalue.Customer = _context.Customer.Find(new Guid(cid.Value));

            if (devvalue.Tenant == null || devvalue.Customer == null)
            {
                return new ApiResult<Device>(ApiCode.NotFoundTenantOrCustomer, "Not found Tenant or Customer", null);
            }
            _context.Device.Add(devvalue);
            _context.AfterCreateDevice(devvalue,device.ProductId);
            await _context.SaveChangesAsync();
            var identity = _context.DeviceIdentities.FirstOrDefault(c => c.Device.Id == devvalue.Id);
            if (identity != null)
            {
                identity.IdentityType = device.IdentityType;
                _context.DeviceIdentities.Update(identity);
                await _context.SaveChangesAsync();
            }
            await _queue.PublishCreateDevice(devvalue.Id);
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
        public async Task<ApiResult<bool>> DeleteDevice(Guid id)
        {
            Device device = await FoundAsync(id);

            if (device == null)
            {
                return new ApiResult<bool>(ApiCode.NotFoundTenantOrCustomer, "Device {id} not found", false);
            }

            try
            {
                if (device.DeviceType == DeviceType.Device)
                {
                    var assets = await _context.Assets.Where(c => c.OwnedAssets.Any(d => d.DeviceId == device.Id) && c.Deleted==false)
                        .ToListAsync();

                    if (assets.Count > 0)
                    {
                        return new ApiResult<bool>(ApiCode.NotFoundTenantOrCustomer,
                            "Please remove the current device from the following known assets " +
                            assets.Aggregate("", (x, y) => x + "," + y.Name), false);
                    }

                    var cert = _context.DeviceIdentities.Include(d=>d.Device).FirstOrDefault(c => c.Device.Id == device.Id);
                    if (cert != null)
                    {
                        _context.DeviceIdentities.RemoveRange(cert);
                        await _context.SaveChangesAsync();
                    }

                    var attrs = await _context.DataStorage.Where(c => c.DeviceId == device.Id).ToArrayAsync();
                    if (attrs.Length > 0)
                    {
                        _context.DataStorage.RemoveRange(attrs);
                        await _context.SaveChangesAsync();
                    }

                    var devicerules = await _context.DeviceRules.Where(c => c.Device.Id == device.Id).ToArrayAsync();

                    if (devicerules.Length > 0)
                    {
                        _context.DeviceRules.RemoveRange(devicerules);
                        await _context.SaveChangesAsync();
                    }
                    _context.Device.Remove(device);
                    await _context.SaveChangesAsync();
                    return new ApiResult<bool>(ApiCode.Success, "Ok", true);
                }

                if (device.DeviceType == DeviceType.Gateway)
                {
                    var devices = await _context.Device.Where(c => c.Owner.Id == device.Id).ToArrayAsync();
                    if (devices.Length > 0)
                    {
                        return new ApiResult<bool>(ApiCode.NotFoundTenantOrCustomer,
                            "Please remove the following devices from the current gateway: " +
                            devices.Aggregate("", (x, y) => x + "," + y.Name), false);
                    }
                    var assets = await _context.Assets.Where(c => c.OwnedAssets.Any(d => d.DeviceId == device.Id)   && c.Deleted==false)
                        .ToArrayAsync();

                    if (assets.Length > 0)
                    {
                        return new ApiResult<bool>(ApiCode.NotFoundTenantOrCustomer,
                            "Please remove the current gateway from the following known assets " +
                            assets.Aggregate("", (x, y) => x + "," + y.Name), false);
                    }

                    var cert = _context.DeviceIdentities.Include(c=>c.Device).FirstOrDefault(c => c.Device.Id == device.Id);
                    if (cert != null)
                    {
                        _context.DeviceIdentities.RemoveRange(cert);
                        await _context.SaveChangesAsync();
                    }

                    var attrs = await _context.DataStorage.Where(c => c.DeviceId == device.Id).ToArrayAsync();
                    if (attrs.Length > 0)
                    {
                        _context.DataStorage.RemoveRange(attrs);
                        await _context.SaveChangesAsync();
                    }
                    var devicerules = await _context.DeviceRules.Where(c => c.Device.Id == device.Id).ToArrayAsync();
                    if (devicerules.Length > 0)
                    {
                        _context.DeviceRules.RemoveRange(devicerules);
                        await _context.SaveChangesAsync();
                    }
                    _context.Device.Remove(device);
                    await _context.SaveChangesAsync();
                    return new ApiResult<bool>(ApiCode.Success, "Ok", true);
                }
            }
            catch (Exception ex)
            {
                return new ApiResult<bool>(ApiCode.Exception, ex.Message, false);
            }
            return new ApiResult<bool>(ApiCode.NotFoundTenantOrCustomer, "Device or Gateway {id} not found", false);
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
                    if (dev.DeviceType == DeviceType.Device && !string.IsNullOrEmpty(dev.Owner?.Name))
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
                    _logger.LogInformation($"RPC  设备{dev.Name} 调用完成 {System.Text.Encoding.UTF8.GetString(response)}");
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
        public ActionResult<ApiResult<Dic>> Telemetry(string access_token, Dictionary<string, object> telemetrys)
        {
            Dic exceptions = new Dic();
            var (ok, device) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return Ok(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a device's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                _queue.PublishActive(device.Id, ActivityStatus.Activity);
                _queue.PublishTelemetryData(new PlayloadData() { DeviceId = device.Id, MsgBody = telemetrys, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.TelemetryData });
                return Ok(new ApiResult<Dic>(ApiCode.Success, "OK", null));
            }
        }

        /// <summary>
        /// 获取服务侧的设备属性
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
        [ProducesDefaultResponseType]
        public ActionResult<ApiResult> Attributes(string access_token, Dictionary<string, object> attributes)
        {
            var (ok, dev) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return Ok(new ApiResult(ApiCode.NotFoundDevice, $"{access_token} not a device's access token"));
            }
            else
            {
                _queue.PublishActive(dev.Id, ActivityStatus.Activity);
                _queue.PublishAttributeData(new PlayloadData() { DeviceId = dev.Id, MsgBody = attributes, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.AttributeData });
                return Ok(new ApiResult(ApiCode.Success, "OK"));
            }
        }

        /// <summary>
        /// 为网关的子设备或者普通设备上传告警信息
        /// </summary>
        /// <param name="access_token">token</param>
        /// <param name="alarm">警告内容</param>
        /// <returns></returns>
        /// <remarks>如果是网关设备，当OriginatorName为网关的名称或者ID时，则我们认为他是网关本身的警告，否则我们认为是设备的警告</remarks>
        [AllowAnonymous]
        [HttpPost("{access_token}/Alarm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<Dic>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<ApiResult> Alarm(string access_token, DeviceAlarmDto alarm)
        {
            var (ok, dev) = _context.GetDeviceByTokenWithTenantCustomer(access_token);

            if (ok)
            {
                return Ok(new ApiResult<Dic>(ApiCode.NotFoundDevice, $"{access_token} not a device's access token", new Dic(new DicKV[] { new DicKV("access_token", access_token) })));
            }
            else
            {
                try
                {
                    _queue.PublishActive(dev.Id, ActivityStatus.Activity);
                    var cad = new CreateAlarmDto()
                    {
                        AlarmDetail = alarm.AlarmDetail,
                        AlarmType = alarm.AlarmType,
                        OriginatorName = alarm.OriginatorName,
                        OriginatorType = dev.DeviceType == DeviceType.Gateway ? OriginatorType.Gateway : OriginatorType.Device,
                        Serverity = alarm.Serverity
                    };
                    _queue.PublishDeviceAlarm(cad);
                    return Ok(new ApiResult());
                }
                catch (Exception ex)
                {
                    return Ok(new ApiResult(ApiCode.Exception, $"检查参数是否为空{ex.Message}"));
                }
            }
        }

        /// <summary>
        /// Http方式调用RawDataGateway网关上传原始Json或者xml并通过规则链进行解析。
        /// </summary>
        /// <param name="access_token">Device 's access token </param>
        /// <param name="format"></param>
        /// <returns></returns>
        /// <remarks>需要在body里面填充数据</remarks>
        [AllowAnonymous]
        [HttpPost("{access_token}/PushDataToMap/{format}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult>> PushDataToMap(string access_token, [FromRoute] string format = "json")
        {
            var body = string.Empty;
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            var (ok, _dev) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return NotFound(new ApiResult(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token"));
            }
            else
            {
                using var sc = _scopeFactor.CreateScope();
                var hg = sc.ServiceProvider.GetService<RawDataGateway>();
                var result = await hg.ExecuteAsync(_dev, format, body);
                return result.Code == (int)ApiCode.Success ? Ok(result) : BadRequest(result);
            }
        }

        /// <summary>
        /// 上传原始Json或者xml 通过规则链进行解析。
        /// </summary>
        /// <param name="access_token">Device 's access token </param>
        /// <param name="format">只支持json和 xml， XML会转换为 Json。</param>
        /// <returns></returns>
        /// <remarks>需要在body里面填充数据</remarks>
        [AllowAnonymous]
        [HttpPost("{access_token}/PushDataToRuleChains/{fromat}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult>> PushDataToRuleChains(string access_token, string format = "json")
        {
            var body = string.Empty;
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            var (ok, _dev) = _context.GetDeviceByToken(access_token);
            if (ok)
            {
                return NotFound(new ApiResult(ApiCode.NotFoundDevice, $"{access_token} not a gateway's access token"));
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
                var rules = await _caching.GetAsync($"ruleid_{_dev.Id}_raw", async () =>
                {
                    var guids = await _context.GerDeviceRulesIdList(_dev.Id, EventType.RAW);
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

                            var result = await _flowRuleProcessor.RunFlowRules(g, Newtonsoft.Json.JsonConvert.DeserializeObject(body), _dev.Id, FlowRuleRunType.Normal, null);

                            //     _context.SaveFlowResult(_dev.Id,g, result);

                        });
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
                Type = attribute.DataType,
                DateTime = DateTime.UtcNow,
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
            var result2 = await _context.SaveAsync<AttributeLatest>(attributes.clientside, devid, DataSide.ClientSide);
            //如果保存时数据为空，则也认为保存成功
            if ((!attributes.anyside.Any()||(attributes.anyside.Any()&&result.ret > 0))
                && (!attributes.serverside.Any() || (attributes.serverside.Any() && result1.ret > 0))
                && (!attributes.clientside.Any() || (attributes.clientside.Any() && result2.ret > 0))
                )
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
            if (result2.ret == 0)
            {
                return new ApiResult<Dic>(ApiCode.AlreadyExists, "clientside attribute update failed", new Dic(result2.exceptions?.Select(f => new DicKV(f.Key, f.Value.Message)) ?? Array.Empty<DicKV>()));
            }

            return new ApiResult<Dic>(ApiCode.InValidData, " attributes update failed", null);
        }

        /// <summary>
        /// 属性删除
        /// </summary>
        /// <param name="input">要删除的属性。</param>
        [HttpDelete("[action]")]
        public async Task<ApiResult<bool>> RemoveAttribute(RemoveDeviceAttributeInput input)
        {
            var attribute = await _context.DataStorage.FirstOrDefaultAsync(c => c.DeviceId == input.DeviceId && c.KeyName == input.KeyName && c.DataSide == input.DataSide);
            if (attribute != null)
            {
                _context.DataStorage.Remove(attribute);
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "attribute has being removed", true);
            }
            else
            {
                return new ApiResult<bool>(ApiCode.CantFindObject, $"this attribute '{input.KeyName}' does not exist", false);
            }
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