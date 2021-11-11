using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using IoTSharp.Models;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class SubscriptionEventController : Controller
    {
        private ApplicationDbContext _context;
        private readonly FlowRuleProcessor _flowRuleProcessor;
        private readonly TaskExecutorHelper _helper;
        private UserManager<IdentityUser> _userManager;

        // GET: SubscriptionEventController
        public SubscriptionEventController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            this._userManager = userManager;
            this._context = context;
  
        }


        //[HttpPost("[action]")]
        //public async Task<ApiResult<PagedData<SubscriptionEvent>>> Index([FromBody] SubscriptionParam m)
        //{
        //    var profile = await this.GetUserProfile();

        //    Expression<Func<SubscriptionEvent, bool>> condition = x => x.EventStatus > -1;
        //    if (!string.IsNullOrEmpty(m.Name))
        //    {
        //        condition = condition.And(x => x.EventName.Contains(m.Name));
        //    }

        //    return new ApiResult<PagedData<SubscriptionEvent>>(ApiCode.Success, "OK", new PagedData<SubscriptionEvent>
        //    {
        //        total = _context.SubscriptionEvents.Count(condition),
        //        rows = _context.SubscriptionEvents.OrderByDescending(c => c.CreateDateTime).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList()
        //    });
        //}


        //[HttpGet("[action]")]
        //public async Task<ApiResult<SubscriptionEvent>> Get(Guid id)
        //{
        //    var subscriptionEvent = _context.SubscriptionEvents.SingleOrDefault(c => c.EventnId == id);
        //    if (subscriptionEvent != null)
        //    {
        //        return new ApiResult<SubscriptionEvent>(ApiCode.Success, "OK", subscriptionEvent);
        //    }
        //    return new ApiResult<SubscriptionEvent>(ApiCode.CantFindObject, "can't find this object", null);
        //}




        //[HttpPut("[action]")]
        //public async Task<ApiResult<PagedData<SubscriptionEvent>>> Update([FromBody] SubscriptionParam m)
        //{
        //    var profile = await this.GetUserProfile();

        //    Expression<Func<SubscriptionEvent, bool>> condition = x => x.EventStatus > -1;
        //    if (!string.IsNullOrEmpty(m.Name))
        //    {
        //        condition = condition.And(x => x.EventName.Contains(m.Name));
        //    }

        //    return new ApiResult<PagedData<SubscriptionEvent>>(ApiCode.Success, "OK", new PagedData<SubscriptionEvent>
        //    {
        //        total = _context.SubscriptionEvents.Count(condition),
        //        rows = _context.SubscriptionEvents.OrderByDescending(c => c.CreateDateTime).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList()
        //    });
        //}


        //[HttpPost("[action]")]
        //public async Task<ApiResult<PagedData<SubscriptionEvent>>> Save([FromBody] SubscriptionParam m)
        //{
        //    var profile = await this.GetUserProfile();

        //    Expression<Func<SubscriptionEvent, bool>> condition = x => x.EventStatus > -1;
        //    if (!string.IsNullOrEmpty(m.Name))
        //    {
        //        condition = condition.And(x => x.EventName.Contains(m.Name));
        //    }

        //    return new ApiResult<PagedData<SubscriptionEvent>>(ApiCode.Success, "OK", new PagedData<SubscriptionEvent>
        //    {
        //        total = _context.SubscriptionEvents.Count(condition),
        //        rows = _context.SubscriptionEvents.OrderByDescending(c => c.CreateDateTime).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList()
        //    });
        //}



        //[HttpGet("[action]")]
        //public async Task<ApiResult<PagedData<SubscriptionEvent>>> Delete([FromBody] SubscriptionParam m)
        //{
        //    var profile = await this.GetUserProfile();

        //    Expression<Func<SubscriptionEvent, bool>> condition = x => x.EventStatus > -1;
        //    if (!string.IsNullOrEmpty(m.Name))
        //    {
        //        condition = condition.And(x => x.EventName.Contains(m.Name));
        //    }

        //    return new ApiResult<PagedData<SubscriptionEvent>>(ApiCode.Success, "OK", new PagedData<SubscriptionEvent>
        //    {
        //        total = _context.SubscriptionEvents.Count(condition),
        //        rows = _context.SubscriptionEvents.OrderByDescending(c => c.CreateDateTime).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList()
        //    });
        //}


    }
}
