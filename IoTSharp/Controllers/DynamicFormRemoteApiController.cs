using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Data;
using IoTSharp.Models;
using Microsoft.AspNetCore.Identity;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DynamicFormRemoteApiController : ControllerBase
    {


        private ApplicationDbContext _context;
        private UserManager<IdentityUser> _userManager;

        public DynamicFormRemoteApiController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            this._userManager = userManager;
            this._context = context;
        }

        [HttpGet("[action]")]
        public IActionResult GetDeviceListForComboxDataSouce(string? q )
        {
            return new JsonResult(new AppMessage{Result = _context.Device.ToList().Select(c => new { value = c.Id.ToString(), label = c.Name, title = c.Name }).ToArray() });
        }

    }
}
