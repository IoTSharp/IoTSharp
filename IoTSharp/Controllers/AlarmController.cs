using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AlarmController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly UserProfile profile;

        public AlarmController(ApplicationDbContext context)
        {
            _context = context;
             profile = this.GetUserProfile();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Occurred([FromBody] CreateAlarmDto dto)
        {
            var result = new ApiResult();
            try
            {
                var alarm = new Alarm
                {
                    Id = Guid.NewGuid(),
                    AckDateTime = DateTime.Now,
                    AlarmDetail = dto.AlarmDetail,
                    AlarmStatus = AlarmStatus.Active_UnAck,
                    AlarmType = dto.AlarmType,
                    ClearDateTime = new DateTime(1970, 1, 1),
                    EndDateTime = new DateTime(1970, 1, 1),
                    Propagate = true,
                    Serverity = dto.Serverity,
                    StartDateTime = DateTime.Now,
                    OriginatorType = dto.OriginatorType
                };
                var oname = dto.OriginatorName;
                Guid originator = Guid.Empty;
                switch (dto.OriginatorType)
                {
                    case OriginatorType.Device:
                        originator = _context.Device.FirstOrDefault(d => d.Id.ToString() == oname || d.Name == oname)?.Id ?? Guid.Empty;
                        break;
                    case OriginatorType.Gateway:
                        originator = _context.Gateway.FirstOrDefault(d => d.Id.ToString() == oname || d.Name == oname)?.Id ?? Guid.Empty;
                        break;
                    case OriginatorType.Asset:
                        originator = _context.Assets.FirstOrDefault(d => d.Id.ToString() == oname || d.Name == oname)?.Id ?? Guid.Empty;
                        break;
                    case OriginatorType.Unknow:
                    default:
                        break;
                }
                alarm.OriginatorId = originator;
                _context.JustFill(this, alarm);
                _context.Alarms.Add(alarm);
                int ret = await _context.SaveChangesAsync();
                result = new ApiResult(ret > 0 ? ApiCode.Success : ApiCode.NothingToDo, ret > 0 ? "OK" : "No data");
            }
            catch (Exception ex)
            {
                result = new ApiResult(ApiCode.Exception, ex.Message);
            }
            return result;
        }

        [HttpPost]
        public async Task<ApiResult<PagedData<AlarmDto>>> List([FromBody] AlarmParam m)
        {


       

            Expression<Func<Alarm, bool>> condition = x =>
                x.Customer.Id == profile.Comstomer && x.Tenant.Id == profile.Tenant;

            if (m.AckDateTime != null && m.AckDateTime.Length == 2)
            {

                condition = condition.And(x => x.AckDateTime > m.AckDateTime[0] && x.AckDateTime < m.AckDateTime[1]);
            }

            if (m.ClearDateTime != null && m.ClearDateTime.Length == 2)
            {

                condition = condition.And(x =>
                    x.ClearDateTime > m.ClearDateTime[0] && x.ClearDateTime < m.ClearDateTime[1]);
            }

            if (m.StartDateTime != null && m.StartDateTime.Length == 2)
            {
                condition = condition.And(x =>
                    x.StartDateTime > m.StartDateTime[0] && x.EndDateTime < m.StartDateTime[1]);
            }

            if (m.EndDateTime != null && m.EndDateTime.Length == 2)
            {
                condition = condition.And(x => x.EndDateTime > m.EndDateTime[0] && x.EndDateTime < m.EndDateTime[1]);
            }

            if (!string.IsNullOrEmpty(m.AlarmType))
            {
                condition = condition.And(x => x.AlarmType == m.AlarmType);
            }

            if (m.AlarmStatus != -1)
            {
                condition = condition.And(x => x.AlarmStatus == (AlarmStatus) m.AlarmStatus);
            }

            if (m.Serverity != -1)
            {
                condition = condition.And(x => x.Serverity == (ServerityLevel) m.Serverity);
            }


            if (m.OriginatorType != -1)
            {
                condition = condition.And(x => x.OriginatorType == (OriginatorType) m.OriginatorType);
            }
            else
            {

                if (m.OriginatorId != Guid.Empty)
                {
                    condition = condition.And(x => x.OriginatorId == m.OriginatorId);
                }
            }



            return new ApiResult<PagedData<AlarmDto>>(ApiCode.Success, "OK", new PagedData<AlarmDto>
            {
                total = await _context.Alarms.CountAsync(condition),
                rows = _context.Alarms.Where(condition).Where(condition).Skip((m.offset) * m.limit).Take(m.limit)
                    .ToList().Select(c => new AlarmDto
                    {
                        ClearDateTime = c.ClearDateTime,
                        AckDateTime = c.AckDateTime,
                        EndDateTime = c.EndDateTime,
                        AlarmDetail = c.AlarmDetail,
                        AlarmType = c.AlarmType,
                        AlarmStatus = c.AlarmStatus,
                        Id = c.Id,
                        OriginatorId = c.OriginatorId,
                        OriginatorType = c.OriginatorType,
                        Propagate = c.Propagate,
                        Serverity = c.Serverity,
                        StartDateTime = c.StartDateTime
                    }).ToList()

            });

        }

        [HttpPost]
        public async Task<ApiResult<List<ModelOriginatorItem>>> Originators([FromBody] ModelOriginatorSearch m)
        {
            var profile = this.GetUserProfile();
            switch ((OriginatorType)m.OriginatorType)
            {
                case OriginatorType.Unknow:
                default:
                case OriginatorType.Device:
                    return new ApiResult<List<ModelOriginatorItem>>(ApiCode.Success, "OK", await _context.Device.Where(c => c.Name.Contains(m.OriginatorName) && c.DeviceType == DeviceType.Device && c.Customer.Id == profile.Comstomer && c.Tenant.Id == profile.Tenant)
                        .Select(c => new ModelOriginatorItem { Id = c.Id, Name = c.Name }).ToListAsync());
                case OriginatorType.Gateway:
                    return new ApiResult<List<ModelOriginatorItem>>(ApiCode.Success, "OK", await _context.Device.Where(c => c.Name.Contains(m.OriginatorName) && c.DeviceType == DeviceType.Gateway && c.DeviceType == DeviceType.Device && c.Customer.Id == profile.Comstomer && c.Tenant.Id == profile.Tenant)
                        .Select(c => new ModelOriginatorItem { Id = c.Id, Name = c.Name }).ToListAsync()); ;
                case OriginatorType.Asset:
                    return new ApiResult<List<ModelOriginatorItem>>(ApiCode.Success, "OK", await _context.Assets.Where(c => c.Name.Contains(m.OriginatorName) && c.Customer.Id == profile.Comstomer && c.Tenant.Id == profile.Tenant).Select(c => new ModelOriginatorItem { Id = c.Id, Name = c.Name }).ToListAsync());
             
            }
        }
    }
}
