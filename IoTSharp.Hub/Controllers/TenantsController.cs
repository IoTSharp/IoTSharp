using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IoTSharp.Hub.Data;
using Microsoft.AspNetCore.Authorization;

namespace IoTSharp.Hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TenantsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles =nameof(UserRole.TenantAdmin))]
    //[AllowAnonymous]
        // GET: api/Tenants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tenant>>> GetTenant()
        {
            return await   _context.Tenant.ToListAsync();
        }

        [Authorize(Roles = nameof(UserRole.NormalUser))]
        // GET: api/Tenants/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tenant>> GetTenant(Guid id)
        {
            var tenant = await _context.Tenant.FindAsync(id);

            if (tenant == null)
            {
                return NotFound();
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
            catch (DbUpdateConcurrencyException)
            {
                if (!TenantExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        // POST: api/Tenants
        [HttpPost]
        public async Task<ActionResult<Tenant>> PostTenant(Tenant tenant)
        {
            _context.Tenant.Add(tenant);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTenant", new { id = tenant.Id }, tenant);
        }
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        // DELETE: api/Tenants/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Tenant>> DeleteTenant(Guid id)
        {
            var tenant = await _context.Tenant.FindAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }

            _context.Tenant.Remove(tenant);
            await _context.SaveChangesAsync();

            return tenant;
        }

        private bool TenantExists(Guid id)
        {
            return _context.Tenant.Any(e => e.Id == id);
        }
    }
}
