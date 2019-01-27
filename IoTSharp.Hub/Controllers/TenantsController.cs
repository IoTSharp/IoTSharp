using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IoTSharp.Hub.Data;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IoTSharp.Hub.Dtos;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IoTSharp.Hub.Controllers
{
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
        /// Only for SystemAdmin
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tenant>>> GetTenant()
        {
            try
            {
                return await _context.Tenant.ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResult(ApiCode.Exception, ex.Message));
            }
        }

        /// <summary>
        /// Normal user can use
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{id}")]
        public async Task<ActionResult<Tenant>> GetTenant(Guid id)
        {
            var tenant = await _context.Tenant.FindAsync(id);
            if (tenant == null)
            {
                return NotFound(new ApiResult(ApiCode.NotFoundTenant, "Not found tenant"));
            }
            return tenant;
        }

        [Authorize(Roles = nameof(UserRole.TenantAdmin))]
        // PUT: api/Tenants/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTenant(Guid id, Tenant tenant)
        {
            if (id != tenant.Id)
            {
                return BadRequest();
            }

            _context.Entry(tenant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!TenantExists(id))
                {
                    return NotFound(new ApiResult<Tenant>(ApiCode.NotFoundTenant, "Not found tenant", tenant));
                }
                else
                {
                    return BadRequest(new ApiResult<EntityEntry[]>(ApiCode.Exception, ex.Message, ex.Entries.ToArray()));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResult<Tenant>(ApiCode.Exception, ex.Message, tenant));
            }

            return NoContent();
        }

        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        // POST: api/Tenants
        [HttpPost]
        public async Task<ActionResult<Tenant>> PostTenant(Tenant tenant)
        {
            try
            {
                _context.Tenant.Add(tenant);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetTenant", new { id = tenant.Id }, tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResult<Tenant>(ApiCode.Exception, ex.Message, tenant));
            }
        }

        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        // DELETE: api/Tenants/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Tenant>> DeleteTenant(Guid id)
        {
            var tenant = await _context.Tenant.FindAsync(id);
            if (tenant == null)
            {
                return NotFound(new ApiResult<Tenant>(ApiCode.NotFoundTenant, "Not found tenant", tenant));
            }
            try
            {
                _context.Tenant.Remove(tenant);
                await _context.SaveChangesAsync();
                return tenant;
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResult<Tenant>(ApiCode.Exception, ex.Message, tenant));
            }
        }

        private bool TenantExists(Guid id)
        {
            return _context.Tenant.Any(e => e.Id == id);
        }
    }
}