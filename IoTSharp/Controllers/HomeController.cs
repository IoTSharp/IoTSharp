using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Authorization;
using IoTSharp.Contracts;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class HomeController :  ControllerBase
    {
        private ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public  ApiResult<HomeKanbanDto> KanBan()
        {
            HomeKanbanDto m = new HomeKanbanDto();
            m.DeviceCount= _context.Device.Count(c => !c.Deleted);
            m.EventCount = _context.BaseEvents.Count(c => c.EventStaus > -1);
            m.OnlineDeviceCount = -1;
            m.TelemetryDataCount = _context.TelemetryData.Count(c=>c.DateTime>DateTime.Today);
            return new ApiResult<HomeKanbanDto>(ApiCode.Success, "OK", m);
        }

        [HttpGet]
        public ApiResult<List<Device>> TopTenDevice()
        {
            return new ApiResult<List<Device>>(ApiCode.Success, "OK", _context.Device.Skip(0).Take(10).ToList());
        }
        [HttpGet]
        public ApiResult<List<BaseEvent>> TopTenEvents()
        {
            return new ApiResult<List<BaseEvent>>(ApiCode.Success, "OK", _context.BaseEvents.OrderByDescending(c => c.CreaterDateTime).Skip(0).Take(10).ToList());
        }

    }
}
