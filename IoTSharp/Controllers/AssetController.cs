using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.Models;
using LinqKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        public async Task<ApiResult<PagedData<AssetDto>>> List([FromQuery] AssetParam m)
        {
            var profile = this.GetUserProfile();
            Expression<Func<Asset, bool>> condition = x => x.Customer.Id == profile.Comstomer && x.Tenant.Id == profile.Tenant;


            if (!string.IsNullOrEmpty(m.Name))
            {
                condition = condition.And(x => x.Name.Contains(m.Name));
            }
            return new ApiResult<PagedData<AssetDto>>(ApiCode.Success, "OK", new PagedData<AssetDto>
            {
                total = await _context.Assets.CountAsync(condition),
                rows = _context.Assets.Where(condition).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList().Select(c => new AssetDto { Id = c.Id, AssetType = c.AssetType, Description = c.Description, Name = c.Name}).ToList()

            });

        }

        [HttpGet]
        public async Task<ApiResult<List<AssetRelation>>> Relations(Guid assetid)
        {

            var profile = this.GetUserProfile();

            var result= await _context.Assets.Include(c => c.OwnedAssets)
                .SingleOrDefaultAsync(c => c.Id == assetid && c.Customer.Id == profile.Comstomer && c.Tenant.Id == profile.Tenant);
            return new ApiResult<List<AssetRelation>>(ApiCode.Success, "OK",
                new List<AssetRelation>(result?.OwnedAssets??new List<AssetRelation>()));

        }



        [HttpGet]
        public async Task<ApiResult<AssetDto>> Get(Guid id)
        {
            var profile = this.GetUserProfile();
            var asset = await _context.Assets.Include(c => c.Customer).Include(c => c.Tenant).SingleOrDefaultAsync(c => c.Id == id && c.Customer.Id == profile.Comstomer && c.Tenant.Id == profile.Tenant);
            if (asset != null)
            {
                return new ApiResult<AssetDto>(ApiCode.Success, "OK", new AssetDto() { AssetType = asset.AssetType, Description = asset.Description, Id = asset.Id, Name = asset.Name });
            }
            return new ApiResult<AssetDto>(ApiCode.CantFindObject, "Not found asset", null);

        }

        [HttpPut]
        public async Task<ApiResult<bool>> Update([FromBody] AssetDto dto)
        {

            var profile = this.GetUserProfile();
            var asset = await _context.Assets.Include(c => c.Customer).Include(c => c.Tenant).SingleOrDefaultAsync(c => c.Id == dto.Id && c.Customer.Id == profile.Comstomer && c.Tenant.Id == profile.Tenant);
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


        [HttpPost]
        public async Task<ApiResult<bool>> Save([FromBody]AssetDto dto)
        {
            try
            {

                var profile = this.GetUserProfile();
                Asset asset = new Asset();
                asset.Tenant = _context.Tenant.SingleOrDefault(c => c.Id == profile.Tenant);
                asset.Customer = _context.Customer.SingleOrDefault(c => c.Id == profile.Comstomer);
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
                var asset = await _context.Assets.Include(c => c.Customer).Include(c => c.Tenant).Include(c => c.OwnedAssets).SingleOrDefaultAsync(c => c.Id == id && c.Customer.Id == profile.Comstomer && c.Tenant.Id == profile.Tenant);
                if (asset == null)
                {

                    return new ApiResult<bool>(ApiCode.CantFindObject, "Not found asset", false);
                }
                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return new ApiResult<bool>(ApiCode.Exception, "error", false);
            }

        }


        [HttpPost]
        public async Task<ApiResult<bool>> addDevice(ModelAssetDevice m)
        {
            var profile = this.GetUserProfile();
            try
            {
                var asset = await _context.Assets.Include(c => c.Customer).Include(c => c.Tenant).Include(c => c.OwnedAssets).SingleOrDefaultAsync(c => c.Id == m.AssetId && c.Customer.Id == profile.Comstomer && c.Tenant.Id == profile.Tenant);
                if (asset == null)
                {

                    return new ApiResult<bool>(ApiCode.CantFindObject, "Not found asset", false);
                }
                foreach (var item in m.Relations)
                {
                    asset.OwnedAssets.Add(new AssetRelation() { DeviceId = item.DeviceId, DataCatalog = item.DataCatalog, Description = item.Description, KeyName = item.KeyName, Name = item.Name });
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

        [HttpPost]
        public async Task<ApiResult<bool>> RemoveDevice(ModelAssetDevice m)
        {

            var profile = this.GetUserProfile();
            try
            {
                var asset = await _context.Assets.Include(c => c.Customer).Include(c => c.Tenant).Include(c => c.OwnedAssets).SingleOrDefaultAsync(c => c.Id == m.AssetId && c.Customer.Id == profile.Comstomer && c.Tenant.Id == profile.Tenant);
                if (asset == null)
                {

                    return new ApiResult<bool>(ApiCode.CantFindObject, "Not found asset", false);
                }
                foreach (var item in m.Relations)
                {
                    var ar = _context.AssetRelations.FirstOrDefault(c => c.Id == item.Id);
                    if (ar != null) _context.AssetRelations.Remove(ar);
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

    }
}
