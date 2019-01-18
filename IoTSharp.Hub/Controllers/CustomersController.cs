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
    [Authorize]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Tenants
        [HttpGet("Tenant/{tenantId}")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers(Guid tenantId)
        {
            var f = from c in _context.Customer where c.Tenant.Id == tenantId select c;
            if (!f.Any())
            {
                return NotFound();
            }
            else
            {
                return await f.ToArrayAsync();
            }
        }

        // GET: api/Customers/5
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(Guid id)
        {
            var customer = await _context.Customer.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        // PUT: api/Customers/5
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(Guid id, Customer customer)
        {
            if (id != customer.Id)
            {
                return BadRequest();
            }
            customer.Tenant = _context.Tenant.Find(customer.Tenant.Id);
            _context.Entry(customer).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
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

        // POST: api/Customers
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            customer.Tenant = _context.Tenant.Find(customer.Tenant.Id);
            _context.Customer.Add(customer);
            await _context.SaveChangesAsync();
            return await GetCustomer(customer.Id);
        }

        // DELETE: api/Customers/5
        [Authorize(Roles = nameof(UserRole.TenantAdmin))]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Customer>> DeleteCustomer(Guid id)
        {
            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customer.Remove(customer);
            await _context.SaveChangesAsync();

            return customer;
        }

        private bool CustomerExists(Guid id)
        {
            return _context.Customer.Any(e => e.Id == id);
        }

        public class TenantDto
        {
            public Guid Id { get; set; }
        }
    }
}