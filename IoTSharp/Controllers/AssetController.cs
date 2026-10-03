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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShardingCore.Extensions;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
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
                x.Customer.Id == profile.Customer && x.Tenant.Id == profile.Tenant && x.Deleted == false;


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
            var profile = this.GetUserProfile();
            var result = await _context.Assets
                .Include(c => c.OwnedAssets)
                .SingleOrDefaultAsync(c => c.Id == assetid
                    && c.Customer.Id == profile.Customer
                    && c.Tenant.Id == profile.Tenant
                    && !c.Deleted);
            if (result is null)
            {
                return new ApiResult<PagedData<AssetRelation>>(ApiCode.CantFindObject, "can't find that asset", new PagedData<AssetRelation>
                {
                    rows = new List<AssetRelation>(),
                    total = 0
                });
            }

            var data = new PagedData<AssetRelation>
            {
                rows = result.OwnedAssets,
                total = result.OwnedAssets.Count
            };

            return new ApiResult<PagedData<AssetRelation>>(ApiCode.Success, "OK", data);
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
            var asset = _context.Assets
                .Include(c => c.OwnedAssets)
                .SingleOrDefault(x => x.Id == assetid
                    && x.Customer.Id == profile.Customer
                    && x.Tenant.Id == profile.Tenant
                    && !x.Deleted);
            if (asset == null)
            {
                return new ApiResult<PagedData<AssetDeviceItem>>(ApiCode.CantFindObject, "can't find that asset", new PagedData<AssetDeviceItem>
                {
                    total = 0,
                    rows = new List<AssetDeviceItem>()
                });
            }

            var grouped = asset.OwnedAssets
                .GroupBy(c => c.DeviceId)
                .ToList();
            var deviceIds = grouped.Select(c => c.Key).ToList();
            var devices = _context.Device
                .Include(c => c.DeviceIdentity)
                .Where(c => deviceIds.Contains(c.Id)
                    && c.CustomerId == profile.Customer
                    && c.TenantId == profile.Tenant
                    && !c.Deleted)
                .ToDictionary(c => c.Id);
            var result = grouped
                .Where(c => devices.ContainsKey(c.Key))
                .Select(c =>
                {
                    var device = devices[c.Key];
                    return new AssetDeviceItem
                    {
                        Id = device.Id,
                        Name = device.Name,
                        DeviceIdentity = device.DeviceIdentity,
                        DeviceType = device.DeviceType,
                        Timeout = device.Timeout,
                        Attrs = c.Where(r => r.DataCatalog == DataCatalog.AttributeLatest).Select(r => new ModelAssetAttrItem
                        {
                            dataSide = r.DataCatalog,
                            keyName = r.KeyName,
                            Name = r.Name,
                            Description = r.Description,
                            Id = r.Id
                        }).ToArray(),
                        Temps = c.Where(r => r.DataCatalog == DataCatalog.TelemetryLatest).Select(r => new ModelAssetAttrItem
                        {
                            dataSide = r.DataCatalog,
                            keyName = r.KeyName,
                            Name = r.Name,
                            Description = r.Description,
                            Id = r.Id
                        }).ToArray()
                    };
                })
                .ToList();
            return new ApiResult<PagedData<AssetDeviceItem>>(ApiCode.Success, "OK",
                new PagedData<AssetDeviceItem>() { total = result?.Count ?? 0, rows = result }
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
                c.Id == id && c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && c.Deleted == false);
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
                c.Id == dto.Id && c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && c.Deleted == false);
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
                        c.Id == id && c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && c.Deleted == false);
                if (asset == null)
                {

                    return new ApiResult<bool>(ApiCode.CantFindObject, "Not found asset", false);
                }
                asset.Deleted = true;
                if (asset.OwnedAssets.Count > 0)
                {
                    _context.AssetRelations.RemoveRange(asset.OwnedAssets);
                }
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
        /// 将设备的有效属性或遥测点位关联到当前租户和客户的资产。
        /// </summary>
        /// <param name="m">资产、设备标识及待关联点位；至少提供一个非空点位键。</param>
        /// <returns>关联结果；无有效点位时拒绝请求，避免报告没有实际关系的绑定成功。</returns>

        [HttpPost]
        public async Task<ApiResult<bool>> addDevice(ModelAddAssetDevice m)
        {

            var profile = this.GetUserProfile();
            try
            {
                if (m == null || m.AssetId == Guid.Empty || m.Deviceid == Guid.Empty)
                {
                    return new ApiResult<bool>(ApiCode.InValidData, "asset and device are required", false);
                }
                if (!(m.Attrs?.Any(item => !string.IsNullOrWhiteSpace(item?.keyName)) ?? false)
                    && !(m.Temps?.Any(item => !string.IsNullOrWhiteSpace(item?.keyName)) ?? false))
                {
                    return new ApiResult<bool>(ApiCode.InValidData, "at least one attribute or telemetry point key is required", false);
                }
                var asset = await _context.Assets.Include(c => c.Customer).Include(c => c.Tenant)
                    .Include(c => c.OwnedAssets).SingleOrDefaultAsync(c =>
                        c.Id == m.AssetId && c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && c.Deleted == false);
                if (asset == null)
                {

                    return new ApiResult<bool>(ApiCode.CantFindObject, "Not found asset", false);
                }

                var deviceExists = await _context.Device.AnyAsync(c => c.Id == m.Deviceid
                    && c.CustomerId == profile.Customer
                    && c.TenantId == profile.Tenant
                    && !c.Deleted);
                if (!deviceExists)
                {
                    return new ApiResult<bool>(ApiCode.NotFoundDevice, "Not found device", false);
                }

                foreach (var item in m.Attrs ?? Array.Empty<ModelAddAssetDevice.ModelAddAssetDeviceItem>())
                {
                    var keyName = item?.keyName?.Trim();
                    if (string.IsNullOrWhiteSpace(keyName))
                    {
                        continue;
                    }
                    if (asset.OwnedAssets.All(c => c.DeviceId != m.Deviceid
                        || c.DataCatalog != DataCatalog.AttributeLatest
                        || !string.Equals(c.KeyName, keyName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var relation = new AssetRelation()
                        {
                            AssetId = asset.Id,
                            DeviceId = m.Deviceid,
                            DataCatalog = DataCatalog.AttributeLatest,
                            Description = "",
                            KeyName = keyName,
                            Name = string.IsNullOrWhiteSpace(item.Name) ? keyName : item.Name.Trim(),
                        };
                        asset.OwnedAssets.Add(relation);
                        // 关系使用预生成的 Guid，显式标记新增，避免 EF 将新关系识别为已有记录的更新。
                        _context.AssetRelations.Add(relation);
                    }

                }

                foreach (var item in m.Temps ?? Array.Empty<ModelAddAssetDevice.ModelAddAssetDeviceItem>())
                {
                    var keyName = item?.keyName?.Trim();
                    if (string.IsNullOrWhiteSpace(keyName))
                    {
                        continue;
                    }
                    if (asset.OwnedAssets.All(c => c.DeviceId != m.Deviceid
                        || c.DataCatalog != DataCatalog.TelemetryLatest
                        || !string.Equals(c.KeyName, keyName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var relation = new AssetRelation()
                        {
                            AssetId = asset.Id,
                            DeviceId = m.Deviceid,
                            DataCatalog = DataCatalog.TelemetryLatest,
                            Description = "",
                            KeyName = keyName,
                            Name = string.IsNullOrWhiteSpace(item.Name) ? keyName : item.Name.Trim()
                        };
                        asset.OwnedAssets.Add(relation);
                        _context.AssetRelations.Add(relation);
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
                if (m == null || m.AssetId == Guid.Empty || m.Deviceid == Guid.Empty)
                {
                    return new ApiResult<bool>(ApiCode.InValidData, "asset and device are required", false);
                }
                var asset = await _context.Assets.Include(c => c.Customer).Include(c => c.Tenant)
                    .Include(c => c.OwnedAssets).SingleOrDefaultAsync(c =>
                        c.Id == m.AssetId && c.Customer.Id == profile.Customer && c.Tenant.Id == profile.Tenant && c.Deleted == false);

                if (asset == null)
                {
                    return new ApiResult<bool>(ApiCode.CantFindObject, "Not found asset", false);
                }

                var relations = asset.OwnedAssets.Where(c => c.DeviceId == m.Deviceid).ToList();
                if (relations.Count == 0)
                {
                    return new ApiResult<bool>(ApiCode.CantFindObject, "Device is not related to this asset", false);
                }
                _context.AssetRelations.RemoveRange(relations);
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
                var attr = await _context.AssetRelations
                    .Include(c => c.Asset)
                    .ThenInclude(c => c.Customer)
                    .Include(c => c.Asset)
                    .ThenInclude(c => c.Tenant)
                    .SingleOrDefaultAsync(c => c.Id == relationId
                        && c.Asset.Customer.Id == profile.Customer
                        && c.Asset.Tenant.Id == profile.Tenant
                        && !c.Asset.Deleted);
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
                var attr = await _context.AssetRelations
                    .Include(c => c.Asset)
                    .ThenInclude(c => c.Customer)
                    .Include(c => c.Asset)
                    .ThenInclude(c => c.Tenant)
                    .SingleOrDefaultAsync(c => c.Id == m.Id
                        && c.Asset.Customer.Id == profile.Customer
                        && c.Asset.Tenant.Id == profile.Tenant
                        && !c.Asset.Deleted);
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
