using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class HomeController :  ControllerBase
    {
        public new ApiResult<HomeDto> Index()
        {









            return new ApiResult<HomeDto>(ApiCode.Success, "OK", null);
        }
    }
}
