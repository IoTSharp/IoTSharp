using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Authorization;
using IoTSharp.Contracts;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using IoTSharp.Extensions;
using EasyCaching.Core;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

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
            var tid = profile.Tenant;
            var data = _caching.Get($"{nameof(HomeKanbanDto)}{tid}", () =>
             {
                 HomeKanbanDto m = new();
                 m.DeviceCount = _context.Device.Count(c =>  c.Tenant.Id==tid &&  !c.Deleted);
                 m.EventCount = _context.BaseEvents.Count(c =>  c.Tenant.Id==tid &&  c.CreaterDateTime>DateTime.Today &&  c.EventStaus > -1);
                 var query = from c in _context.Device where c.Tenant.Id == profile.Tenant && !c.Deleted select c;
                 var al = from a in _context.AttributeLatest where a.KeyName == Constants._Active && a.Value_Boolean == true select a.DeviceId;
                 var devquery = from x in query where al.Contains(x.Id) select x;
                 m.OnlineDeviceCount = devquery.Count();
                 m.AttributesDataCount = _context.AttributeLatest.Count(c => c.DateTime > DateTime.Today);
                 m.AlarmsCount = _context.Alarms.Count(c => c.Tenant.Id == tid && c.StartDateTime > DateTime.Today);
                 var tuc = from t in _context.UserClaims where t.ClaimType == IoTSharpClaimTypes.Tenant select t;
                 var uq = from u in _context.Users where tuc.Any(c => c.UserId == u.Id) select u;
                 m.UserCount = uq.Count();
                 m.ProduceCount = _context.Produces.Count(c => c.Tenant.Id == tid && !c.Deleted);
                 m.RulesCount = _context.FlowRules.Count(c => c.Tenant.Id == tid && c.RuleStatus>-1);
                 return m;
             },TimeSpan.FromMinutes(5));
            return new ApiResult<HomeKanbanDto>(ApiCode.Success, "OK", data.Value);
        }

        [HttpGet]
        public ApiResult<List<Device>> TopTenDevice()
        {
            var tid = this.User.GetTenantId();
            var data = _caching.Get($"toptendevice{tid}", () => _context.Device.Where(c => c.Tenant.Id == tid && !c.Deleted).Skip(0).Take(10).ToList(), TimeSpan.FromMinutes(1));
            return new ApiResult<List<Device>>(ApiCode.Success, "OK", data.Value);
        }
        [HttpGet]
        public ApiResult<List<BaseEvent>> TopTenEvents()
        {
            var tid = this.User.GetTenantId();
            var data = _caching.Get($"top_ten_events{tid}", () =>
                        _context.BaseEvents.Where(c => c.Tenant.Id == tid && c.EventStaus > -1).OrderByDescending(c => c.CreaterDateTime).Skip(0).Take(10).ToList(),
                         TimeSpan.FromSeconds(30));
            return new ApiResult<List<BaseEvent>>(ApiCode.Success, "OK", data.Value);
        }

    }
}
