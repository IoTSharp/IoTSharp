using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models.FormFieldTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DynamicFormRemoteApiController : ControllerBase
    {
        private ApplicationDbContext _context;

        public DynamicFormRemoteApiController(ApplicationDbContext context)
        {
            this._context = context;
        }

        //测试动态表单的远程数据源
        [HttpGet("[action]")]
        public ApiResult<List<NzOption>> GetDeviceListForComboxDataSouce(string q)
        {
            return new ApiResult<List<NzOption>>(ApiCode.Success, "Ok",
                _context.Device.OrderByDescending(c => c.LastActive).Skip(0).Take(10).ToList()
                    .Select(c => new NzOption { value = c.Id.ToString(), label = c.Name, title = c.Name }).ToList());
        }
    }
}