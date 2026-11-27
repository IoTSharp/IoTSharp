using Microsoft.AspNetCore.Mvc;
using System;

using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Authorization;
using IoTSharp.Contracts;
using EasyCaching.Core;
using Microsoft.Extensions.Options;
using IoTSharp.Extensions;
using System.Threading.Tasks;
using System.Linq;
using IoTSharp.Services;
using Microsoft.EntityFrameworkCore;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IEasyCachingProvider _caching;
        private readonly AppSettings _setting;
        private ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context, IEasyCachingProviderFactory factory, IOptions<AppSettings> options)
        {
            _context = context;
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(options.Value.CachingUseIn)}";
            _caching = factory.GetCachingProvider(_hc_Caching);
            _setting = options.Value;
        }
        [HttpGet]
        public async Task<ApiResult<HomeKanbanDto>> KanBan([FromServices] OnlineDemoSettings demo = null)
        {
            var profile = this.GetUserProfile();
            if (demo?.IsDemo(User) == true)
            {
                var tenantWide = User.IsInRole(nameof(UserRole.TenantAdmin));
                var token = HttpContext.RequestAborted;
                var devices = _context.Device.Where(d => d.TenantId == profile.Tenant && !d.Deleted
                    && (tenantWide || d.CustomerId == profile.Customer)).Select(d => d.Id);
                var ids = await devices.ToArrayAsync(token);
                var demoData = new HomeKanbanDto
                {
                    DeviceCount = ids.Length,
                    OnlineDeviceCount = await _context.AttributeLatest.CountAsync(a => ids.Contains(a.DeviceId)
                        && a.KeyName == Constants._Active && a.Value_Boolean == true, token),
                    AttributesDataCount = await _context.AttributeLatest.CountAsync(a => ids.Contains(a.DeviceId), token),
                    ProductCount = await _context.Products.CountAsync(p => p.Tenant.Id == profile.Tenant && !p.Deleted
                        && (tenantWide || p.Customer.Id == profile.Customer), token),
                    AlarmsCount = await _context.Alarms.CountAsync(a => a.Tenant.Id == profile.Tenant
                        && (tenantWide || a.Customer.Id == profile.Customer), token),
                    RulesCount = await _context.FlowRules.CountAsync(r => r.Tenant.Id == profile.Tenant && r.RuleStatus > -1
                        && (tenantWide || r.Customer.Id == profile.Customer), token),
                    UserCount = await _context.Relationship.CountAsync(r => r.Tenant.Id == profile.Tenant
                        && (tenantWide || r.Customer.Id == profile.Customer), token)
                };
                return new ApiResult<HomeKanbanDto>(ApiCode.Success, "OK", demoData);
            }
            var data = await _caching.GetKanBanCacheAsync(profile.Tenant, _context, HttpContext.RequestAborted);
            return new ApiResult<HomeKanbanDto>(ApiCode.Success, "OK", data);
        }
        [AllowAnonymous]
        [HttpGet]
        public ApiResult<HomeNowDto> Now()
        {
            return new ApiResult<HomeNowDto>(ApiCode.Success, "OK", new HomeNowDto() { NowString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        }

    }
}
