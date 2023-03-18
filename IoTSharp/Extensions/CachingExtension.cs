using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using System;
using System.Linq;

namespace IoTSharp.Extensions
{
    public static class CachingExtension
    {

        public static  HomeKanbanDto  GetKanBanCache(this IEasyCachingProvider _caching,  Guid tid, ApplicationDbContext _context)
        {
            var kbc= _caching.Get($"{nameof(HomeKanbanDto)}{tid}", () =>
            {
                HomeKanbanDto m = new();
                m.DeviceCount = _context.Device.Count(c => c.Tenant.Id == tid && !c.Deleted);
                m.EventCount = _context.BaseEvents.Count(c => c.Tenant.Id == tid && c.CreaterDateTime > DateTime.Today && c.EventStaus > -1);
                var query = from c in _context.Device where c.Tenant.Id == tid && !c.Deleted select c;
                var al = from a in _context.AttributeLatest where a.KeyName == Constants._Active && a.Value_Boolean == true select a.DeviceId;
                var devquery = from x in query where al.Contains(x.Id) select x;
                m.OnlineDeviceCount = devquery.Count();
                m.AttributesDataCount = _context.AttributeLatest.Count(c => c.DateTime > DateTime.Today);
                m.AlarmsCount = _context.Alarms.Count(c => c.Tenant.Id == tid && c.StartDateTime > DateTime.Today);
                var tuc = from t in _context.UserClaims where t.ClaimType == IoTSharpClaimTypes.Tenant select t;
                var uq = from u in _context.Users where tuc.Any(c => c.UserId == u.Id) select u;
                m.UserCount = uq.Count();
                m.ProduceCount = _context.Produces.Count(c => c.Tenant.Id == tid && !c.Deleted);
                m.RulesCount = _context.FlowRules.Count(c => c.Tenant.Id == tid && c.RuleStatus > -1);
                return m;
            }, TimeSpan.FromMinutes(5));
            return kbc.Value;
        }
    }
}
