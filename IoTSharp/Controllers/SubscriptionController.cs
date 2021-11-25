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


        [HttpPost("[action]")]
        public async Task<ApiResult<PagedData<SubscriptionEvent>>> Index([FromBody] SubscriptionParam m)
        {
            var profile = await this.GetUserProfile();

            Expression<Func<SubscriptionEvent, bool>> condition = x => x.EventStatus > -1;
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
        public ApiResult<SubscriptionEvent> Get(Guid id)
        {
            var subscriptionEvent = _context.SubscriptionEvents.SingleOrDefault(c => c.EventId == id);
            if (subscriptionEvent != null)
            { 
                return new ApiResult<SubscriptionEvent>(ApiCode.Success, "OK", subscriptionEvent);
            }
            return new ApiResult<SubscriptionEvent>(ApiCode.CantFindObject, "can't find this object", null);
        }




        [HttpPut("[action]")]
        public async Task<ApiResult<bool>> Update(SubscriptionEvent m)
        {
            var profile = await this.GetUserProfile();
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
            return new ApiResult<bool>(ApiCode.CantFindObject, "can't find object", false);
        }


        [HttpPost("[action]")]
        public async Task<ApiResult<bool>> Save(SubscriptionEvent m)
        {

            try
            {
                var profile = await this.GetUserProfile();
                SubscriptionEvent se = new SubscriptionEvent();
                se.Creator = profile.Id; se.EventName = m.EventName;
                se.EventDesc = m.EventDesc;
                se.EventNameSpace = m.EventNameSpace;
                se.EventParam = m.EventParam;
                se.EventTag = m.EventTag;
                se.Type = m.Type;
                se.CreateDateTime = DateTime.Now;
                se.EventStatus = 1;

                this._context.SubscriptionEvents.Add(se);
                await this._context.SaveChangesAsync(); return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }
          
         
        }


        //public async Task<ApiResult<bool>> Subscript(Guid id)
        //{



        //}




        //public async Task<ApiResult<bool>> GetSubscriptionCustomer(Guid id)
        //{

      

        //}


        [HttpGet("[action]")]
        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            var profile = await this.GetUserProfile();

            var se = this._context.SubscriptionEvents.SingleOrDefault(c => c.EventId == id);
            if (se != null)
            {
                se.EventStatus = -1;
                this._context.SubscriptionEvents.Update(se);
                await this._context.SaveChangesAsync(); return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }

            return new ApiResult<bool>(ApiCode.CantFindObject, "can't find object", false);
        }


    }
}
