using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IoTSharp.Hub.Data;
using Microsoft.AspNetCore.Authorization;
using IoTSharp.Hub.Dtos;

namespace IoTSharp.Hub.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DevicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Devices
        [HttpGet("Customers/{customerId}")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices(Guid customerId)
        {
            var f = from c in _context.Device where c.Customer.Id == customerId select c;
            if (!f.Any())
            {
                return NotFound(new ApiResult<Guid>(ApiCode.NotFoundCustomer, $"Customer {customerId} not found ", customerId));
            }
            else
            {
                return await f.ToArrayAsync();
            }
        }

        // GET: api/Devices/5
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> GetDevice(Guid id)
        {
            var device = await _context.Device.FindAsync(id);

            if (device == null)
            {
                return NotFound(new ApiResult<Guid>(ApiCode.NotFoundDevice, $"Device {id} not found ", id));
            }

            return device;
        }

        // PUT: api/Devices/5
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDevice(Guid id, Device device)
        {
            if (id != device.Id)
            {
                return BadRequest();
            }

            _context.Entry(device).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(id))
                {
                    return NotFound(new ApiResult<Guid>(ApiCode.NotFoundDevice, $"Device {id} not found ", id));
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Devices
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPost]
        public async Task<ActionResult<Device>> PostDevice(Device device)
        {
            device.Tenant = _context.Tenant.Find(device.Tenant.Id);
            device.Customer = _context.Customer.Find(device.Customer.Id);
            if (device.Tenant == null || device.Customer == null)
            {
                return NotFound(new ApiResult<Device>(ApiCode.NotFoundTenantOrCustomer, $"Not found Tenant or Customer ", device));
            }
            _context.Device.Add(device);
            await _context.SaveChangesAsync();
            return await GetDevice(device.Id);
        }

        // DELETE: api/Devices/5
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Device>> DeleteDevice(Guid id)
        {
            var device = await _context.Device.FindAsync(id);
            if (device == null)
            {
                return NotFound(new ApiResult<Guid>(ApiCode.NotFoundDevice, $"Device {id} not found ", id));
            }

            _context.Device.Remove(device);
            await _context.SaveChangesAsync();

            return device;
        }

        private bool DeviceExists(Guid id)
        {
            return _context.Device.Any(e => e.Id == id);
        }
    }
}