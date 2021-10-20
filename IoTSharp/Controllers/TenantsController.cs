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
        /// 系统管理员用来获取全部租户列表
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<List<Tenant>>> GetTenant()
        {
            try
            {
               // return await _context.Tenant.ToListAsync();
             
                return Ok(new ApiResult<PagedData<Tenant>>(ApiCode.Exception, "Ok", new PagedData<Tenant>() { rows = await _context.Tenant.ToListAsync(), total = await _context.Tenant.CountAsync() }));
            }
            catch (Exception ex)
            {
                return Ok(new ApiResult(ApiCode.Exception, ex.Message));
            }
        }

        /// <summary>
        /// 普通用户用于活的自身的租户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<Tenant>> GetTenant(Guid id)
        {
            var tenant = await _context.Tenant.FindAsync(id);
            if (tenant == null)
            {
                return new ApiResult<Tenant>(ApiCode.Success, "can't find this object", null);
            }

            return new ApiResult<Tenant>(ApiCode.Success, "OK", tenant);
     
        }
        /// <summary>
        /// 修改指定的租户信息， 仅限租户管理员
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.TenantAdmin))]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult<Tenant>), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ApiResult<Tenant>> PutTenant(Guid id, Tenant tenant)
        {
            if (id != tenant.Id)
            {
                return new ApiResult<Tenant>(ApiCode.InValidData, "InValidData", tenant);
            }

            _context.Entry(tenant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.Tenant.Any(c=>c.Id==id))
                {
                    return new ApiResult<Tenant>(ApiCode.CantFindObject, "cant't find this object", tenant);
                }
                else
                {

                    return new ApiResult<Tenant>(ApiCode.InValidData, ex.Message, tenant);
                 //  return BadRequest(new ApiResult<EntityEntry[]>(ApiCode.Exception, ex.Message, ex.Entries.ToArray()));
                }
            }
            catch (Exception ex)
            {
                return new ApiResult<Tenant>(ApiCode.InValidData, ex.Message, tenant);
             //   return BadRequest(new ApiResult<Tenant>(ApiCode.Exception, ex.Message, tenant));
            }

            return new ApiResult<Tenant>(ApiCode.Success, "Ok", tenant);
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
             //   return CreatedAtAction("GetTenant", new { id = tenant.Id }, tenant);
            }
            catch (Exception ex)
            {

                return new ApiResult<Tenant>(ApiCode.InValidData, ex.Message, tenant);
            //    return BadRequest(new ApiResult<Tenant>(ApiCode.Exception, ex.Message, tenant));
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
             //   return NotFound(new ApiResult<Tenant>(ApiCode.NotFoundTenant, "Not found tenant", tenant));
            }
            try
            {
                _context.Tenant.Remove(tenant);
                await _context.SaveChangesAsync();
              //  return tenant;

                return new ApiResult<Tenant>(ApiCode.Success, "Ok", tenant);
            }
            catch (Exception ex)
            {
                return new ApiResult<Tenant>(ApiCode.InValidData, ex.Message, tenant);
            
            }
        }

        private ApiResult<bool> TenantExists(Guid id)
        {
            return new ApiResult<bool>(ApiCode.InValidData, "Ok", _context.Tenant.Any(e => e.Id == id));


        }
    }
}