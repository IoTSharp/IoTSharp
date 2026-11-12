using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IoTSharp.Data;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using IoTSharp.Controllers.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IoTSharp.Dtos;
using IoTSharp.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using IoTSharp.Contracts;
using IoTSharp.Extensions;
using System.Linq.Expressions;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// 租户管理
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private ApplicationDbContext _context;
        private ILogger _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public TenantsController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<TenantsController> logger, ApplicationDbContext context
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        [HttpGet]
        public async Task<ApiResult<PagedData<Tenant>>> GetTenant([FromQuery] QueryDto m)
        {
            var profile = this.GetUserProfile();
            var querym = _context.Tenant.Where(c => c.Deleted == false);
            var data = await m.Query(querym, c => c.Name);
            return new ApiResult<PagedData<Tenant>>(ApiCode.Success, "OK", data);
        }

        /// <summary>
        /// 读取当前账号可访问的未删除租户，系统管理员可跨租户查询。
        /// </summary>
        /// <param name="id">目标租户标识。</param>
        /// <returns>租户资料；不存在或超出授权范围时不返回数据。</returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<Tenant>> GetTenant(Guid id)
        {
            if (!HasTenantAccess(id))
            {
                return new ApiResult<Tenant>(ApiCode.CantFindObject, "can't find this object", null);
            }

            var tenant = await _context.Tenant
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.Deleted);
            if (tenant == null)
            {
                return new ApiResult<Tenant>(ApiCode.CantFindObject, "can't find this object", null);
            }

            return new ApiResult<Tenant>(ApiCode.Success, "OK", tenant);

        }
        /// <summary>
        /// 修改授权范围内租户的联系和地址资料，保留身份、删除状态和关联配置。
        /// </summary>
        /// <param name="id">目标租户标识，必须与请求体一致。</param>
        /// <param name="tenant">待更新的租户资料，仅复制允许编辑的字段。</param>
        /// <returns>更新后的租户；校验失败、越权或保存失败时不返回数据。</returns>
        [Authorize(Roles = nameof(UserRole.TenantAdmin))]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult<Tenant>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ApiResult<Tenant>> PutTenant(Guid id, Tenant tenant)
        {
            if (tenant == null || id == Guid.Empty || id != tenant.Id)
            {
                return new ApiResult<Tenant>(ApiCode.InValidData, "InValidData", null);
            }

            if (!HasTenantAccess(id))
            {
                return new ApiResult<Tenant>(ApiCode.CantFindObject, "can't find this object", null);
            }

            var existingTenant = await _context.Tenant
                .FirstOrDefaultAsync(c => c.Id == id && !c.Deleted);
            if (existingTenant == null)
            {
                return new ApiResult<Tenant>(ApiCode.CantFindObject, "can't find this object", null);
            }

            // 只更新租户资料字段，禁止请求体覆盖身份、删除标记和导航属性。
            existingTenant.Name = tenant.Name;
            existingTenant.Email = tenant.Email;
            existingTenant.Phone = tenant.Phone;
            existingTenant.Country = tenant.Country;
            existingTenant.Province = tenant.Province;
            existingTenant.City = tenant.City;
            existingTenant.Street = tenant.Street;
            existingTenant.Address = tenant.Address;
            existingTenant.ZipCode = tenant.ZipCode;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.Tenant.Any(c => c.Id == id && c.Deleted == false))
                {
                    return new ApiResult<Tenant>(ApiCode.CantFindObject, "cant't find this object", null);
                }
                else
                {

                    return new ApiResult<Tenant>(ApiCode.Exception, ex.Message, null);
                }
            }
            catch (Exception ex)
            {
                return new ApiResult<Tenant>(ApiCode.Exception, ex.Message, null);
            }

            return new ApiResult<Tenant>(ApiCode.Success, "Ok", existingTenant);
        }

        /// <summary>
        /// 验证当前用户是否可以访问指定租户，缺失或无效的租户声明默认拒绝。
        /// </summary>
        /// <param name="tenantId">请求访问的租户标识。</param>
        /// <returns>系统管理员或同租户用户返回 true，否则返回 false。</returns>
        private bool HasTenantAccess(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
            {
                return false;
            }

            if (User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                return true;
            }

            var tenantClaims = User.FindAll(IoTSharpClaimTypes.Tenant).Take(2).ToArray();
            return tenantClaims.Length == 1
                && Guid.TryParse(tenantClaims[0].Value, out var scopedTenantId)
                && scopedTenantId != Guid.Empty
                && scopedTenantId == tenantId;
        }
        /// <summary>
        /// 新增租户， 仅限系统管理员
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        // POST: api/Tenants
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResult<Tenant>), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<Tenant>> PostTenant(Tenant tenant)
        {
            try
            {
                _context.Tenant.Add(tenant);
                await _context.SaveChangesAsync();
                return new ApiResult<Tenant>(ApiCode.Success, "Ok", tenant);
            }
            catch (Exception ex)
            {

                return new ApiResult<Tenant>(ApiCode.Exception, ex.Message, tenant);
            }
        }
        /// <summary>
        /// 删除租户，仅限系统用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        // DELETE: api/Tenants/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<Tenant>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult<Tenant>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<Tenant>> DeleteTenant(Guid id)
        {
            var tenant = await _context.Tenant.FindAsync(id);
            if (tenant == null)
            {
                return new ApiResult<Tenant>(ApiCode.CantFindObject, "Not found tenant", tenant);
            }
            try
            {
                tenant.Deleted = true;
                _context.Tenant.Update(tenant);
                await _context.SaveChangesAsync();
                return new ApiResult<Tenant>(ApiCode.Success, "Ok", tenant);
            }
            catch (Exception ex)
            {
                return new ApiResult<Tenant>(ApiCode.Exception, ex.Message, tenant);

            }
        }


        /// <summary>
        /// 设置指定租户下的AI设置
        /// </summary>
        /// <returns></returns>
        [HttpPost("{id}/ai")]
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<AISettingsDto>> SetAISettings([FromRoute] Guid id, [FromBody] AISettingsSetDto dto)
        {
            var profile = this.GetUserProfile();
            if (id != Guid.Empty)
            {
                var querym = _context.Tenant.Include(s => s.AISettings).Where(c => !c.Deleted && c.Id == profile.Tenant && c.Id == id);
                if (querym.Any())
                {
                    var d = await querym.FirstOrDefaultAsync();

                    if (d.AISettings == null)
                    {
                        var dx = new AISettings();
                        dx.Id = Guid.NewGuid();
                        dx.Role = UserRole.TenantAdmin;
                        _context.AISettings.Add(dx);
                        d.AISettings = dx;
                    }
                    d.AISettings.Enable = dto.Enable;
                    d.AISettings.MCP_API_KEY = Guid.NewGuid().ToString();
                    d.AISettings.Name = dto.Name;
                    int ret = await _context.SaveChangesAsync();
                    var dtor = new AISettingsDto() { Enable = d.AISettings.Enable, MCP_API_KEY = d.AISettings.MCP_API_KEY, Name = d.AISettings.Name };
                    return new ApiResult<AISettingsDto>(ApiCode.Success, "OK", dtor);
                }
                else
                {
                    return new ApiResult<AISettingsDto>(ApiCode.NotFoundCustomer, "没有指定客户", new AISettingsDto());
                }
            }
            else
            {

                return new ApiResult<AISettingsDto>(ApiCode.InValidData, "传入的客户ID为空", new AISettingsDto());
            }

        }

        /// <summary>
        /// 获取指定租户下的AI设置
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}/ai")]
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<AISettingsDto>> GetAISettings([FromRoute] Guid id)
        {
            var profile = this.GetUserProfile();
            if (id != Guid.Empty)
            {
                var querym = _context.Tenant.Include(s => s.AISettings).Where(c => !c.Deleted && c.Id == profile.Tenant && c.Id == id);
                if (querym.Any())
                {
                    var d = await querym.FirstOrDefaultAsync();
                    var s = d?.AISettings ?? new AISettings() { Name = "None" };
                    var data = new AISettingsDto() { Enable = s.Enable, MCP_API_KEY = s.MCP_API_KEY, Name = s.Name };
                    return new ApiResult<AISettingsDto>(ApiCode.Success, "OK", data);
                }
                else
                {
                    return new ApiResult<AISettingsDto>(ApiCode.NotFoundCustomer, "没有指定租户", new AISettingsDto());
                }
            }
            else
            {

                return new ApiResult<AISettingsDto>(ApiCode.InValidData, "传入的租户ID为空", new AISettingsDto());
            }

        }

    }
}
