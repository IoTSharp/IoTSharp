using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using IoTSharp.Dtos;
using IoTSharp.EventBus;
using IoTSharp.Extensions;
using IoTSharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Scripting.Utils;
using ShardingCore.Extensions;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProducesController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly IPublisher _queue;

        public ProducesController(ApplicationDbContext context, ILogger<ProducesController> logger, IPublisher queue)
        {
            _context = context;
            _logger = logger;
            _queue = queue;
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<PagedData<ProduceDto>>> List([FromQuery] QueryDto m)
        {
            var profile = this.GetUserProfile();
            Expression<Func<Produce, bool>> condition = x =>
                x.Customer.Id == profile.Customer && x.Tenant.Id == profile.Tenant && x.Deleted == false;
            var querym = _context.Produces.Include(c => c.Devices.Where(c => c.Deleted == false));
            var data = await m.Query(querym, condition, c => c.Name, c => new ProduceDto
            {
                Id = c.Id,
                DefaultIdentityType = c.DefaultIdentityType,
                DefaultTimeout = c.DefaultTimeout,
                Description = c.Description,
                Name = c.Name,
                Devices = c.Devices,
                DefaultDeviceType = c.DefaultDeviceType
            });
            return new ApiResult<PagedData<ProduceDto>>(ApiCode.Success, "OK", data);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="produceid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<ProduceData>>> ProduceDatas(Guid produceid)
        {
            var result = await _context.ProduceDatas.Include(c => c.Owner).Where(p => p.Owner.Id == produceid)
                .ToListAsync();
            return new ApiResult<List<ProduceData>>(ApiCode.Success, "OK", result);
        }

        /// <summary>
        /// 获取产品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<ApiResult<ProduceAddDto>> Get(Guid id)
        {
            var result = await _context.Produces.SingleOrDefaultAsync(c => c.Id == id && c.Deleted == false);
            if (result != null)
            {
                return new ApiResult<ProduceAddDto>(ApiCode.Success, "OK", new ProduceAddDto
                {
                    Id = result.Id,
                    Icon = result.Icon,
                    DefaultDeviceType = result.DefaultDeviceType,
                    DefaultIdentityType = result.DefaultIdentityType,
                    DefaultTimeout = result.DefaultTimeout,
                    Description = result.Description,
                    GatewayConfiguration = result.GatewayConfiguration,
                    GatewayType = result.GatewayType,
                    Name = result.Name

                });
            }
            else
            {
                return new ApiResult<ProduceAddDto>(ApiCode.CantFindObject, "Produce is  not found", null);
            }

        }

        /// <summary>
        /// 删除产品
        /// </summary>
        /// <param name="produceid">产品ID</param>
        /// <returns></returns>

        [HttpGet]
        public async Task<ApiResult<bool>> Delete(Guid produceid)
        {

            try
            {
                var produce = await _context.Produces.FindAsync(produceid);
                if (produce != null)
                {
                    produce.Deleted = true;
                    _context.Produces.Update(produce);
                    await _context.SaveChangesAsync();
                    return new ApiResult<bool>(ApiCode.Success, "OK", true);
                }

                return new ApiResult<bool>(ApiCode.CantFindObject, "Produce is  not found", false);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }

        }

        /// <summary>
        /// 新增产品
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> Save(ProduceAddDto dto)
        {

            try
            {
                var profile = this.GetUserProfile();
                var produce = new Produce();
                produce.Customer = _context.Customer.Find(profile.Customer);
                produce.Tenant = _context.Tenant.Find(profile.Tenant);
                produce.DefaultDeviceType = dto.DefaultDeviceType;
                produce.DefaultIdentityType = dto.DefaultIdentityType;
                produce.DefaultTimeout = dto.DefaultTimeout;
                produce.Description = dto.Description;
                produce.Name =
                    dto.Name;
                produce.GatewayConfiguration = dto.GatewayConfiguration;
                produce.Icon = dto.Icon;
                produce.Deleted = false;
                produce.GatewayType = dto.GatewayType;
                produce.ProduceToken = Guid.NewGuid().ToString().Replace("-", "");
                _context.Produces.Add(produce);
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }

        }

        /// <summary>
        /// 修改产品
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult<bool>> Update(ProduceAddDto dto)
        {

            try
            {
                var produce = await _context.Produces.SingleOrDefaultAsync(c => c.Id == dto.Id && c.Deleted == false);
                if (produce != null)
                {
                    produce.DefaultIdentityType = dto.DefaultIdentityType;
                    produce.DefaultTimeout = dto.DefaultTimeout;
                    produce.Description = dto.Description;
                    produce.GatewayType = dto.GatewayType;
                    produce.GatewayConfiguration = dto.GatewayConfiguration;
                    produce.Name = dto.Name;
                    produce.Icon = dto.Icon;
                    _context.Produces.Update(produce);
                    await _context.SaveChangesAsync();


                    return new ApiResult<bool>(ApiCode.Success, "Ok", true);
                }

                return new ApiResult<bool>(ApiCode.CantFindObject, "Produce is  not found", false);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }

        }

        /// <summary>
        /// 获取产品属性
        /// </summary>
        /// <param name="produceId">产品ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<ProduceDataItemDto>>> GetProduceData(Guid produceId)
        {
            var produce = await _context.Produces.Include(c => c.DefaultAttributes)
                .SingleOrDefaultAsync(c => c.Id == produceId && c.Deleted == false);
            if (produce != null)
            {
                var result = produce.DefaultAttributes.Select(c =>
                    new ProduceDataItemDto
                    { KeyName = c.KeyName, DataSide = c.DataSide, Type = c.Type }).ToList();
                return new ApiResult<List<ProduceDataItemDto>>(ApiCode.Success, "Ok", result);
            }

            return new ApiResult<List<ProduceDataItemDto>>(ApiCode.CantFindObject, "Produce is  not found", null);
        }




        /// <summary>
        /// 修改属性
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> EditProduceData(ProduceDataEditDto dto)
        {

            try
            {
                var produce = await _context.Produces.Include(c => c.DefaultAttributes)
                    .SingleOrDefaultAsync(c => c.Id == dto.produceId && c.Deleted == false);
                if (produce != null)
                {

                    var d = _context.ProduceDatas.ToList();
                    var pds = _context.ProduceDatas.Include(c => c.Owner).Where(c => c.Owner.Id == dto.produceId).ToList();
                    if (dto.ProduceData != null && dto.ProduceData.Length > 0)
                    {
                        var data = dto.ProduceData.Select(c => new ProduceData
                        {
                            KeyName = c.KeyName,
                            DataSide = c.DataSide,
                            Type = c.Type,
                            Owner = produce,
                            DateTime = DateTime.UtcNow
                        }).ToList();

                        var delete_keynames = pds.Select(c => c.KeyName.ToLower())
                            .Except(dto.ProduceData.Select(c => c.KeyName.ToLower())).ToArray();
                        foreach (var item in data)
                        {
                            var pd = pds.FirstOrDefault(c => c.KeyName.ToLower() == item.KeyName.ToLower());
                            if (pd != null)
                            {
                                pd.DataSide = item.DataSide;
                                pd.Type = item.Type;
                                _context.ProduceDatas.Update(pd);
                            }
                            else
                            {
                                produce.DefaultAttributes.Add(item);
                            }
                        }
                        await _context.SaveChangesAsync();
                        if (delete_keynames.Length > 0)
                        {
                            var deleteattr = pds.Join(delete_keynames, x => x.KeyName.ToLower(), y => y, (x, y) => x)
                                .ToArray();
                            _context.ProduceDatas.RemoveRange(deleteattr);
                            await _context.SaveChangesAsync();

                        }
                        return new ApiResult<bool>(ApiCode.Success, "Ok", true);
                    }

                    return new ApiResult<bool>(ApiCode.CantFindObject, "Produce data is  not found", false);
                }

                return new ApiResult<bool>(ApiCode.CantFindObject, "Produce is  not found", false);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }

        }




        /// <summary>
        /// 获取产品字典
        /// </summary>
        /// <param name="produceId">产品ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<ProduceDictionary>>> GetProduceDictionary(Guid produceId)
        {
            var produce = await _context.Produces.Include(c => c.Dictionaries)
                .SingleOrDefaultAsync(c => c.Id == produceId && c.Deleted == false);
            if (produce != null)
            {


                var dic = produce.Dictionaries.Where(d => d.Deleted == false).ToList();
                return new ApiResult<List<ProduceDictionary>>(ApiCode.Success, "Ok", dic);
            }

            return new ApiResult<List<ProduceDictionary>>(ApiCode.CantFindObject, "Produce is  not found", null);
        }


        /// <summary>
        /// 修改字典
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> EditProduceDictionary(ProduceDictionaryEditDto dto)
        {
            var profile = this.GetUserProfile();
            try
            {
                var produce = await _context.Produces.Include(c => c.Dictionaries)
                    .SingleOrDefaultAsync(c => c.Id == dto.produceId && c.Deleted == false);
                if (produce != null)
                {
                    var deletedic = produce.Dictionaries.Select(c => c.Id)
                        .Except(dto.ProduceDictionaryData.Where(c => c.Id != Guid.Empty).Select(c => c.Id)).ToList();

                    foreach (var item in dto.ProduceDictionaryData)
                    {

                        if (item.Id == Guid.Empty)
                        {

                            var produceDictionary = new ProduceDictionary();
                            produceDictionary.KeyName = item.KeyName;
                            produceDictionary.DataType = item.DataType;
                            produceDictionary.Customer = profile.Customer;
                            produceDictionary.DefaultValue = item.DefaultValue;
                            produceDictionary.Display = item.Display;
                            produceDictionary.DisplayName = item.DisplayName;
                            produceDictionary.KeyDesc = item.KeyDesc;
                            produceDictionary.Tag = item.Tag;
                            produceDictionary.Deleted = false;

                            produceDictionary.UnitConvert = item.UnitConvert;
                            produceDictionary.Unit = item.Unit;
                            produceDictionary.UnitExpression = item.UnitExpression;
                            produce.Dictionaries.Add(produceDictionary);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            var produceDictionary =
                                await _context.ProduceDictionaries.SingleOrDefaultAsync(c => c.Id == item.Id);
                            if (produceDictionary != null)
                            {
                                produceDictionary.KeyName = item.KeyName;
                                produceDictionary.DataType = item.DataType;
                                produceDictionary.Customer = profile.Customer;
                                produceDictionary.DefaultValue = item.DefaultValue;
                                produceDictionary.Display = item.Display;
                                produceDictionary.DisplayName = item.DisplayName;
                                produceDictionary.KeyDesc = item.KeyDesc;
                                produceDictionary.Tag = item.Tag;
                                produceDictionary.Deleted = false;
                                produceDictionary.UnitConvert = item.UnitConvert;
                                produceDictionary.Unit = item.Unit;
                                produceDictionary.UnitExpression = item.UnitExpression;
                                _context.ProduceDictionaries.Update(produceDictionary);
                                await _context.SaveChangesAsync();
                            }

                        }
                    }
                    var sc = produce.Dictionaries.Where(c => deletedic.Any(p => p == c.Id));
                    sc.ForEach(c => c.Deleted = true);
                    _context.ProduceDictionaries.UpdateRange(sc);
                    await _context.SaveChangesAsync();

                    return new ApiResult<bool>(ApiCode.Success, "Ok", true);
                }

                return new ApiResult<bool>(ApiCode.CantFindObject, "Produce is  not found", false);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }

        }

        /// <summary>
        /// 获取产品的数据映射关系（产品字段 ↔ 设备字段）
        /// </summary>
        /// <param name="produceId">产品ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<ProduceDataMappingDto>>> GetDataMappings(Guid produceId)
        {
            if (produceId == Guid.Empty)
            {
                return new ApiResult<List<ProduceDataMappingDto>>(ApiCode.NotFoundProduce, "Produce not found", new List<ProduceDataMappingDto>());
            }

            var produceExists = await _context.Produces.AnyAsync(p => p.Id == produceId && !p.Deleted);
            if (!produceExists)
            {
                return new ApiResult<List<ProduceDataMappingDto>>(ApiCode.NotFoundProduce, "Produce not found", new List<ProduceDataMappingDto>());
            }

            var mappings = await _context.ProduceDataMappings
                .Include(m => m.Produce)
                .Where(m => m.Produce.Id == produceId && !m.Deleted)
                .Select(m => new ProduceDataMappingDto
                {
                    Id = m.Id,
                    ProduceKeyName = m.ProduceKeyName,
                    DataCatalog = m.DataCatalog,
                    DeviceId = m.DeviceId,
                    DeviceKeyName = m.DeviceKeyName,
                    Description = m.Description
                })
                .ToListAsync();
            return new ApiResult<List<ProduceDataMappingDto>>(ApiCode.Success, "Ok", mappings);
        }

        /// <summary>
        /// 保存产品的数据映射关系（全量替换）
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> SaveDataMappings([FromBody] SaveProduceDataMappingsDto dto)
        {
            try
            {
                var produce = await _context.Produces.Include(p => p.DefaultAttributes)
                    .SingleOrDefaultAsync(p => p.Id == dto.ProduceId && !p.Deleted);
                if (produce == null)
                    return new ApiResult<bool>(ApiCode.CantFindObject, "Produce not found", false);

                // Remove old mappings for this produce
                var existing = await _context.ProduceDataMappings
                    .Include(m => m.Produce)
                    .Where(m => m.Produce.Id == dto.ProduceId)
                    .ToListAsync();
                _context.ProduceDataMappings.RemoveRange(existing);

                // Add new mappings
                if (dto.Mappings != null)
                {
                    foreach (var item in dto.Mappings)
                    {
                        _context.ProduceDataMappings.Add(new ProduceDataMapping
                        {
                            Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                            Produce = produce,
                            ProduceKeyName = item.ProduceKeyName,
                            DataCatalog = item.DataCatalog,
                            DeviceId = item.DeviceId,
                            DeviceKeyName = item.DeviceKeyName,
                            Description = item.Description,
                            Deleted = false
                        });
                    }
                }
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }
        }

        /// <summary>
        /// HTTP方式上传遥测数据到产品。数据将按映射关系路由到对应的设备键。
        /// </summary>
        /// <param name="produce_token">Product's ProduceToken</param>
        /// <param name="telemetrys">Telemetry key-value pairs using product key names</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api/Produces/{produce_token}/Telemetry")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult>> ProduceTelemetry(string produce_token, [FromBody] Dictionary<string, object> telemetrys)
        {
            var (ok, produce) = _context.GetProduceByToken(produce_token);
            if (ok)
                return Ok(new ApiResult(ApiCode.CantFindObject, $"{produce_token} is not a valid produce token"));
            try
            {
                var mappings = await _context.ProduceDataMappings
                    .Include(m => m.Produce)
                    .Where(m => m.Produce.Id == produce.Id && !m.Deleted && m.DataCatalog == DataCatalog.TelemetryData)
                    .ToListAsync();

                // Group by device and route each mapped key
                var byDevice = mappings
                    .Where(m => telemetrys.ContainsKey(m.ProduceKeyName))
                    .GroupBy(m => m.DeviceId);

                foreach (var grp in byDevice)
                {
                    var devicePayload = grp.ToDictionary(
                        m => m.DeviceKeyName,
                        m => telemetrys[m.ProduceKeyName]);
                    _queue.PublishActive(grp.Key, ActivityStatus.Activity);
                    _queue.PublishTelemetryData(new PlayloadData
                    {
                        DeviceId = grp.Key,
                        MsgBody = devicePayload,
                        DataSide = DataSide.ClientSide,
                        DataCatalog = DataCatalog.TelemetryData
                    });
                }
                return Ok(new ApiResult(ApiCode.Success, "OK"));
            }
            catch (Exception ex)
            {
                return Ok(new ApiResult(ApiCode.Exception, ex.Message));
            }
        }

        /// <summary>
        /// 获取产品的属性数据（通过映射关系从关联设备聚合）
        /// </summary>
        /// <param name="produce_token">Product's ProduceToken</param>
        /// <param name="dataSide">Specifying data side</param>
        /// <param name="keys">Comma-separated product key names (optional filter)</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("/api/Produces/{produce_token}/Attributes/{dataSide}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult>> ProduceAttributes(string produce_token, DataSide dataSide, string keys)
        {
            var (ok, produce) = _context.GetProduceByToken(produce_token);
            if (ok)
                return Ok(new ApiResult(ApiCode.CantFindObject, $"{produce_token} is not a valid produce token"));
            try
            {
                var mappings = await _context.ProduceDataMappings
                    .Include(m => m.Produce)
                    .Where(m => m.Produce.Id == produce.Id && !m.Deleted && m.DataCatalog == DataCatalog.AttributeData)
                    .ToListAsync();

                string[] keyFilter = string.IsNullOrEmpty(keys)
                    ? Array.Empty<string>()
                    : keys.Split(',', StringSplitOptions.RemoveEmptyEntries);

                // Filter by requested keys
                if (keyFilter.Length > 0)
                    mappings = mappings.Where(m => keyFilter.Contains(m.ProduceKeyName)).ToList();

                // Collect device keys needed
                var deviceIds = mappings.Select(m => m.DeviceId).Distinct().ToList();
                var deviceAttrs = await _context.AttributeLatest
                    .Where(a => deviceIds.Contains(a.DeviceId) && a.DataSide == dataSide)
                    .ToListAsync();

                // Build result: map device key values back to produce key names
                var result = mappings
                    .Select(m =>
                    {
                        var attr = deviceAttrs.FirstOrDefault(a =>
                            a.DeviceId == m.DeviceId && a.KeyName == m.DeviceKeyName);
                        return new
                        {
                            ProduceKeyName = m.ProduceKeyName,
                            DeviceId = m.DeviceId,
                            DeviceKeyName = m.DeviceKeyName,
                            DataCatalog = m.DataCatalog,
                            DataSide = attr?.DataSide,
                            DateTime = attr?.DateTime,
                            Type = attr?.Type,
                            Value = attr?.ToObject()
                        };
                    })
                    .ToArray();
                return Ok(new ApiResult<object[]>(ApiCode.Success, "Ok", result));
            }
            catch (Exception ex)
            {
                return Ok(new ApiResult(ApiCode.Exception, ex.Message));
            }
        }

        /// <summary>
        /// HTTP方式上传属性数据到产品。数据将按映射关系路由到对应的设备键。
        /// </summary>
        /// <param name="produce_token">Product's ProduceToken</param>
        /// <param name="attributes">Attribute key-value pairs using product key names</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api/Produces/{produce_token}/Attributes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult>> ProduceAttributesUpload(string produce_token, [FromBody] Dictionary<string, object> attributes)
        {
            var (ok, produce) = _context.GetProduceByToken(produce_token);
            if (ok)
                return Ok(new ApiResult(ApiCode.CantFindObject, $"{produce_token} is not a valid produce token"));
            try
            {
                var mappings = await _context.ProduceDataMappings
                    .Include(m => m.Produce)
                    .Where(m => m.Produce.Id == produce.Id && !m.Deleted && m.DataCatalog == DataCatalog.AttributeData)
                    .ToListAsync();

                // Group by device and route each mapped key
                var byDevice = mappings
                    .Where(m => attributes.ContainsKey(m.ProduceKeyName))
                    .GroupBy(m => m.DeviceId);

                foreach (var grp in byDevice)
                {
                    var devicePayload = grp.ToDictionary(
                        m => m.DeviceKeyName,
                        m => attributes[m.ProduceKeyName]);
                    _queue.PublishActive(grp.Key, ActivityStatus.Activity);
                    _queue.PublishAttributeData(new PlayloadData
                    {
                        DeviceId = grp.Key,
                        MsgBody = devicePayload,
                        DataSide = DataSide.ClientSide,
                        DataCatalog = DataCatalog.AttributeData
                    });
                }
                return Ok(new ApiResult(ApiCode.Success, "OK"));
            }
            catch (Exception ex)
            {
                return Ok(new ApiResult(ApiCode.Exception, ex.Message));
            }
        }
    }
}

