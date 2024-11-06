using Microsoft.AspNetCore.Mvc;
using System;
 
using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Authorization;
using IoTSharp.Contracts;
using EasyCaching.Core;
using Microsoft.Extensions.Options;
using IoTSharp.Extensions;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class HomeController :  ControllerBase
    {
        private readonly IEasyCachingProvider _caching;
        private readonly AppSettings _setting;
        private ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context,IEasyCachingProviderFactory factory,IOptions<AppSettings> options)
        {
            _context = context;
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(options.Value.CachingUseIn)}";
            _caching = factory.GetCachingProvider(_hc_Caching);
            _setting = options.Value;
        }
        [HttpGet]
        public  ApiResult<HomeKanbanDto> KanBan()
        {
            var profile = this.GetUserProfile();
            var  data = _caching.GetKanBanCache(profile.Tenant, _context);
            return new ApiResult<HomeKanbanDto>(ApiCode.Success, "OK", data);
        }
        [HttpGet]
        public ApiResult<HomeNowDto> Now()
        {
            return new ApiResult<HomeNowDto>(ApiCode.Success, "OK",  new HomeNowDto() { NowString=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        }

    }
}
