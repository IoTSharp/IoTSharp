using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.Models;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class SubscriptionEventController : Controller
    {
        private ApplicationDbContext _context;
        private UserManager<IdentityUser> _userManager;

        // GET: SubscriptionEventController
        public SubscriptionEventController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            this._userManager = userManager;
            this._context = context;
        }

        [HttpPost("[action]")]
        public ApiResult<PagedData<SubscriptionEvent>> Index([FromBody] SubscriptionParam m)
        {
            var profile = this.GetUserProfile();
            Expression<Func<SubscriptionEvent, bool>> condition = x => x.EventStatus > -1 && x.Tenant.Id == profile.Tenant;
            if (!string.IsNullOrEmpty(m.Name))
            {
                condition = condition.And(x => x.EventName.Contains(m.Name));
            }
            return new ApiResult<PagedData<SubscriptionEvent>>(ApiCode.Success, "OK", new PagedData<SubscriptionEvent>
            {
                total = _context.SubscriptionEvents.Count(condition),
                rows = _context.SubscriptionEvents.OrderByDescending(c => c.CreateDateTime).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList()
            });
        }

        [HttpGet("[action]")]
        public async Task<ApiResult<SubscriptionEvent>> Get(Guid id)
        {
            var profile = this.GetUserProfile();
            var subscriptionEvent = await _context.SubscriptionEvents.SingleOrDefaultAsync(c => c.EventId == id);
            if (subscriptionEvent != null)
            {
                return new ApiResult<SubscriptionEvent>(ApiCode.Success, "OK", subscriptionEvent);
            }
            return new ApiResult<SubscriptionEvent>(ApiCode.CantFindObject, "can't find this object", null);
        }

        [HttpPut("[action]")]
        public async Task<ApiResult<bool>> Update(SubscriptionEvent m)
        {
            var profile = this.GetUserProfile();
            var se = _context.SubscriptionEvents.SingleOrDefault(c => c.EventId == m.EventId);
            if (se != null)
            {
                se.Creator = profile.Id;
                se.EventDesc = m.EventDesc;
                se.EventName = m.EventName;
                se.Type = m.Type;
                se.EventNameSpace = m.EventNameSpace;
                se.EventParam = m.EventParam;
                se.EventTag = m.EventTag;
                _context.SubscriptionEvents.Update(se);
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            else
            {
                return new ApiResult<bool>(ApiCode.CantFindObject, "can't find object", false);
            }
        }

        [HttpPost("[action]")]
        public async Task<ApiResult<bool>> Save(SubscriptionEvent m)
        {
            try
            {
                SubscriptionEvent se = new()
                {
                    Creator =  User.GetUserId(),
                    EventName = m.EventName,
                    EventDesc = m.EventDesc,
                    EventNameSpace = m.EventNameSpace,
                    EventParam = m.EventParam,
                    EventTag = m.EventTag,
                    Type = m.Type,
                    CreateDateTime = DateTime.Now,
                    EventStatus = 1
                };
                _context.JustFill(this,se);
                _context.SubscriptionEvents.Add(se);
                await this._context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }
        }

        [HttpGet("[action]")]
        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            var se = _context.SubscriptionEvents.SingleOrDefault(c => c.EventId == id);
            if (se != null)
            {
                se.EventStatus = -1;
                _context.SubscriptionEvents.Update(se);
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            else
            {
                return new ApiResult<bool>(ApiCode.CantFindObject, "can't find object", false);
            }
        }
    }
}