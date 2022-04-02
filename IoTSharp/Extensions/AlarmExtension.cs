using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Extensions
{
    public static class AlarmExtension
    {
        public static async Task<ApiResult> OccurredAlarm(this  ControllerBase  @this,   ApplicationDbContext _context,   CreateAlarmDto dto,Action<Alarm> action)
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
                };
                action?.Invoke(alarm);
                var isone = from a in _context.Alarms where a.OriginatorId == alarm.OriginatorId && a.AlarmType == alarm.AlarmType && a.Serverity == alarm.Serverity select a;
                if (isone.Any())
                {
                    var old = isone.First();
                    old.AlarmDetail = alarm.AlarmDetail;
                    old.EndDateTime = DateTime.Now;
                }
                else
                {
                    _context.Alarms.Add(alarm);
                }
                int ret = await _context.SaveChangesAsync();
                result = new ApiResult(ret > 0 ? ApiCode.Success : ApiCode.NothingToDo, ret > 0 ? "OK" : "No data");
            }
            catch (Exception ex)
            {
                result = new ApiResult(ApiCode.Exception, ex.Message);
            }
            return result;
        }
    }
}
