using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShardingCore.Extensions;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AssetController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public AssetController(ApplicationDbContext context, ILogger<AssetController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ApiResult<PagedData<AssetDto>>> List([FromQuery] QueryDto m)
        {
            var profile = this.GetUserProfile();

            Expression<Func<Asset, bool>> condition = x =>
                x.Customer.Id == profile.Customer && x.Tenant.Id == profile.Tenant && x.Deleted==false;


            if (!string.IsNullOrEmpty(m.Name))
            {
                condition = condition.And(x => x.Name.Contains(m.Name));
            }
            var query = _context.Assets.Where(condition).OrderBy(k => k.Name).Skip((m.Offset) * m.Limit).Take(m.Limit)
                    .Select(c => new AssetDto
                    { Id = c.Id, AssetType = c.AssetType, Description = c.Description, Name = c.Name });
            return new ApiResult<PagedData<AssetDto>>(ApiCode.Success, "OK", new PagedData<AssetDto>
            {
                total = await _context.Assets.CountAsync(condition),
                rows = await query.ToListAsync()
            });

        }
        [HttpGet]
        public async Task<ApiResult<PagedData<AssetRelation>>> AssetRelations(Guid assetid)
        {
            var data =new  PagedData<AssetRelation>();
            var result = await _context.Assets.Include(c => c.OwnedAssets).SingleOrDefaultAsync(c => c.Id == assetid);
            data.rows = result.OwnedAssets;
            data.total = result.OwnedAssets.Count;
            return new ApiResult<PagedData<AssetRelation>>(ApiCode.Success, "OK",  data);
        }

        /// <summary>
        /// 获取资产的属性和遥测数据
        /// </summary>
        /// <param name="assetid"></param>
        /// <returns></returns>


        [HttpGet]
        public ApiResult<PagedData<AssetDeviceItem>> Relations(Guid assetid)
        {

            var profile = this.GetUserProfile();

            var result = _context.Assets.Include(c => c.OwnedAssets)
                .SingleOrDefault(x =>
                    x.Id == assetid && x.Customer.Id == profile.Customer && x.Tenant.Id == profile.Tenant && x.Deleted==false)?.OwnedAssets
                .ToList().GroupBy(c => c.DeviceId).Select(c => new
                    {
                        Device = c.Key,
                        Attrs = c.Where(c => c.DataCatalog == DataCatalog.AttributeLatest).ToList(),
                        Temps = c.Where(c => c.DataCatalog == DataCatalog.TelemetryLatest).ToList()
                    }
                ).ToList().Join(_context.Device, x => x.Device, y => y.Id, (x, y) => new AssetDeviceItem
                {
                    Id = x.Device,
                    Name = y.Name,
                    DeviceIdentity = y.DeviceIdentity,
                    DeviceType = y.DeviceType,
                    Timeout = y.Timeout,
                    Attrs = x.Attrs.Select(c => new ModelAssetAttrItem
                    {
                        dataSide = c.DataCatalog, keyName = c.KeyName, Name = c.Name, Description = c.Description,
                        Id = c.Id
                    }).ToArray(),
                    Temps = x.Temps.Select(c => new ModelAssetAttrItem
                    {
                        dataSide = c.DataCatalog, keyName = c.KeyName, Name = c.Name, Description = c.Description,
                        Id = c.Id
                    }).ToArray(),
                }).ToList();
            return new ApiResult<PagedData<AssetDeviceItem>>(ApiCode.Success, "OK",
                new PagedData<AssetDeviceItem>() {total = result?.Count ?? 0, rows = result}
            );

        }


        /// <summary>
        /// 根据资产Id获取资产信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<AssetDto>> Get(Guid id)
        {
            var profile = this.GetUserProfile();
            var asset = await _context.Assets.Include(c => c.Customer).Include(c => c.Tenant).SingleOrDefaultAsync(c =>
                c.Id == id && c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && c.Deleted==false);
            if (asset != null)
            {
                return new ApiResult<AssetDto>(ApiCode.Success, "OK",
                    new AssetDto()
                    {
                        AssetType = asset.AssetType,
                        Description = asset.Description,
                        Id = asset.Id,
                        Name = asset.Name
                    });
            }

            return new ApiResult<AssetDto>(ApiCode.CantFindObject, "Not found asset", null);

        }
        /// <summary>
        /// 修改资产信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult<bool>> Update([FromBody] AssetDto dto)
        {

            var profile = this.GetUserProfile();
            var asset = await _context.Assets.Include(c => c.Customer).Include(c => c.Tenant).SingleOrDefaultAsync(c =>
                c.Id == dto.Id && c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && c.Deleted==false);
            if (asset == null)
            {

                return new ApiResult<bool>(ApiCode.CantFindObject, "Not found asset", false);
            }

            try
            {
                asset.AssetType = dto.AssetType;
                asset.Name = dto.Name;
                asset.Description = dto.Description;
                _context.Assets.Update(asset);
                await _context.SaveChangesAsync();

                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return new ApiResult<bool>(ApiCode.Exception, "error", false);
            }

        }
        /// <summary>
        /// 保存资产信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<ApiResult<bool>> Save([FromBody] AssetAddDto dto)
        {
            try
            {

                var profile = this.GetUserProfile();
                Asset asset = new Asset();
                asset.Tenant = _context.Tenant.SingleOrDefault(c => c.Id == profile.Tenant);
                asset.Customer = _context.Customer.SingleOrDefault(c => c.Id == profile.Customer);
                asset.AssetType = dto.AssetType;
                asset.Name = dto.Name;
                asset.Description = dto.Description;
                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return new ApiResult<bool>(ApiCode.Exception, "error", false);
            }


        }

        [HttpDelete]
        public async Task<ApiResult<bool>> Delete(Guid id)
        {

            var profile = this.GetUserProfile();
            try
            {
                var asset = await _context.Assets.Include(c => c.Customer).Include(c => c.Tenant)
                    .Include(c => c.OwnedAssets).SingleOrDefaultAsync(c =>
                        c.Id == id && c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && c.Deleted==false);
                if (asset == null)
                {

                    return new ApiResult<bool>(ApiCode.CantFindObject, "Not found asset", false);
                }
                asset.Deleted = true;
                _context.Assets.Update(asset);
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return new ApiResult<bool>(ApiCode.Exception, "error", false);
            }

        }
        /// <summary>
        /// 增加资产
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<ApiResult<bool>> addDevice(ModelAddAssetDevice m)
        {

            var profile = this.GetUserProfile();
            try
            {
                var asset = await _context.Assets.Include(c => c.Customer).Include(c => c.Tenant)
                    .Include(c => c.OwnedAssets).SingleOrDefaultAsync(c =>
                        c.Id == m.AssetId && c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && c.Deleted==false);
                if (asset == null)
                {

                    return new ApiResult<bool>(ApiCode.CantFindObject, "Not found asset", false);
                }

                foreach (var item in m.Attrs)
                {
                    if (asset.OwnedAssets.All(c => c.KeyName != item.keyName))
                    {
                        asset.OwnedAssets.Add(new AssetRelation()
                        {
                            DeviceId = m.Deviceid,
                            DataCatalog = DataCatalog.AttributeLatest,
                            Description = "",
                            KeyName = item.keyName,
                            Name = item.keyName,
                        });
                    }

                }

                foreach (var item in m.Temps)
                {
                    if (asset.OwnedAssets.All(c => c.KeyName != item.keyName))
                    {
                        asset.OwnedAssets.Add(new AssetRelation()
                        {
                            DeviceId = m.Deviceid,
                            DataCatalog = DataCatalog.TelemetryLatest,
                            Description = "",
                            KeyName = item.keyName,
                            Name = item.keyName
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return new ApiResult<bool>(ApiCode.Exception, "error", false);
            }


        }




        [HttpDelete]
        public async Task<ApiResult<bool>> RemoveDevice(ModelAssetDevice m)
        {

            var profile = this.GetUserProfile();
            try
            {
                var asset = await _context.Assets.Include(c => c.Customer).Include(c => c.Tenant)
                    .Include(c => c.OwnedAssets).SingleOrDefaultAsync(c =>
                        c.Id == m.AssetId && c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && c.Deleted==false);

                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return new ApiResult<bool>(ApiCode.Exception, "error", false);
            }

        }
        /// <summary>
        /// 根据Id移除资产的属性或者遥测
        /// </summary>
        /// <param name="relationId"></param>
        /// <returns></returns>
        [HttpDelete]

        public async Task<ApiResult<bool>> RemoveAssetRaletions(Guid relationId)
        {
            var profile = this.GetUserProfile();
            try
            {
                var attr = _context.AssetRelations.SingleOrDefault(c => c.Id == relationId);
                if (attr != null)
                {
                    _context.AssetRelations.Remove(attr);
                    await _context.SaveChangesAsync();
                    return new ApiResult<bool>(ApiCode.Success, "Ok", true);
                }

                return new ApiResult<bool>(ApiCode.Success, "can't find this raletion", false);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return new ApiResult<bool>(ApiCode.Exception, "error", false);
            }
        }

        /// <summary>
        ///  修改资产和设备关联属性或者遥测信息
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>

        [HttpPost]

        public async Task<ApiResult<bool>> EditRelation(ModelEditAssetAttrItem m)
        {
            var profile = this.GetUserProfile();
            try
            {
                var attr = _context.AssetRelations.SingleOrDefault(c => c.Id == m.Id);
                if (attr != null)
                {
                    attr.Description = m.Description;
                    attr.Name = m.Name;
                    _context.AssetRelations.Update(attr);
                    await _context.SaveChangesAsync();
                    return new ApiResult<bool>(ApiCode.Success, "Ok", true);
                }
                return new ApiResult<bool>(ApiCode.Success, "can't find this raletion", false);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return new ApiResult<bool>(ApiCode.Exception, "error", false);
            }

        }
    }
} 
