using IoTSharp.Contracts;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Extensions;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// 设备模型
    /// </summary>
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class DeviceModelController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DeviceModelController(ApplicationDbContext context)
        {
            this._context = context;
        }

        [HttpPost]
        public ApiResult<PagedData<DeviceModel>> Index([FromQuery] QueryDto m)
        {
            try
            {
                Expression<Func<DeviceModel, bool>> condition = x => x.ModelStatus > -1;
                if (!string.IsNullOrEmpty(m.Name))
                {
                    condition.And(x => x.ModelName == m.Name);
                }
                return new ApiResult<PagedData<DeviceModel>>(ApiCode.Success, "OK", new PagedData<DeviceModel>() { rows = _context.DeviceModels.Where(condition).OrderBy(d=>d.ModelName).Skip((m.Offset) * m.Limit).Take(m.Limit).ToList(), total = _context.DeviceModels.Count(condition) });
            }
            catch (Exception e)
            {
                return new ApiResult<PagedData<DeviceModel>>(ApiCode.Exception, e.Message, null);
            }
        }

        [HttpGet]
        public ApiResult<DeviceModel> Get(Guid id)
        {
            var dm = _context.DeviceModels.SingleOrDefault(c => c.DeviceModelId == id);
            if (dm != null)
            {
                return new ApiResult<DeviceModel>(ApiCode.Success, "Ok", dm);
            }
            return new ApiResult<DeviceModel>(ApiCode.CantFindObject, "CantFindObject", null);
        }

        [HttpPost]
        public ApiResult<bool> Update(DeviceModelDto m)
        {
            var dm = _context.DeviceModels.SingleOrDefault(c => c.DeviceModelId == m.DeviceModelId);
            if (dm != null)
            {
                dm.ModelName = m.ModelName;
                dm.ModelDesc = m.ModelDesc;
                _context.DeviceModels.Update(dm);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.CantFindObject, "Ok", true);
            }
            return new ApiResult<bool>(ApiCode.CantFindObject, "CantFindObject", false);
        }

        [HttpPost]
        public ApiResult<bool> Save(DeviceModelDto m)
        {
            try
            {
                DeviceModel dm = new DeviceModel();
                dm.CreateDateTime = DateTime.UtcNow;
                dm.ModelStatus = 1;
                dm.ModelName = m.ModelName;
                dm.ModelDesc = m.ModelDesc;
                _context.DeviceModels.Update(dm);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.CantFindObject, "Ok", true);
            }
            catch (Exception ex)
            {
                return new ApiResult<bool>(ApiCode.Exception, ex.Message, false);
            }
        }

        [HttpGet]
        public ApiResult<bool> Delete(Guid id)
        {
            var dm = _context.DeviceModels.SingleOrDefault(c => c.DeviceModelId == id);
            if (dm != null)
            {
                dm.ModelStatus = -1;
                _context.DeviceModels.Update(dm);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.CantFindObject, "Ok", true);
            }
            return new ApiResult<bool>(ApiCode.CantFindObject, "CantFindObject", false);
        }

        [HttpGet]
        public ApiResult<List<DeviceModelCommand>> GetCommandsByDevice(Guid id)
        {
            var dev = _context.Device.SingleOrDefault(c => c.Id == id);
            if (dev != null && dev.DeviceModelId != null && dev.DeviceModelId != Guid.Empty)
            {
                return new ApiResult<List<DeviceModelCommand>>(ApiCode.Success, "Ok", _context.DeviceModelCommands.Where(c => c.DeviceModelId == dev.DeviceModelId && c.CommandStatus > -1).ToList());
            }

            return new ApiResult<List<DeviceModelCommand>>(ApiCode.Success, "Ok", null);
        }

        [HttpGet]
        public ApiResult<List<DeviceModelCommand>> GetCommands(Guid id)
        {
            return new ApiResult<List<DeviceModelCommand>>(ApiCode.Success, "Ok", _context.DeviceModelCommands.Where(c => c.DeviceModelId == id && c.CommandStatus > -1).ToList());
        }

        [HttpGet]
        public ApiResult<DeviceModelCommand> GetCommand(Guid id)
        {
            var dmc = _context.DeviceModelCommands.SingleOrDefault(c => c.CommandId == id);
            if (dmc != null)
            {
                return new ApiResult<DeviceModelCommand>(ApiCode.Success, "Ok", dmc);
            }
            return new ApiResult<DeviceModelCommand>(ApiCode.CantFindObject, "CantFindObject", null);
        }

        [HttpPost]
        public ApiResult<bool> SaveCommand(DeviceModelCommandDto m)
        {
            try
            {
                DeviceModelCommand dmc = new DeviceModelCommand
                {
                    CommandTitle = m.CommandTitle,
                    CommandI18N = m.CommandI18N,
                    CommandType = m.CommandType,
                    CommandParams = m.CommandParams,
                    CommandName = m.CommandName,
                    DeviceModelId = m.DeviceModelId,
                    CommandTemplate = m.CommandTemplate,
                    CreateDateTime = DateTime.UtcNow
                };

                _context.DeviceModelCommands.Add(dmc);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.CantFindObject, "Ok", true);
            }
            catch (Exception ex)
            {
                return new ApiResult<bool>(ApiCode.Exception, ex.Message, false);
            }
        }

        [HttpPost]
        public ApiResult<bool> UpdateCommand(DeviceModelCommandDto m)
        {
            var dmc = _context.DeviceModelCommands.SingleOrDefault(c => c.CommandId == m.CommandId);
            if (dmc != null)
            {
                dmc.CommandTitle = m.CommandTitle;
                dmc.CommandI18N = m.CommandI18N;
                dmc.CommandType = m.CommandType;
                dmc.CommandParams = m.CommandParams;
                dmc.CommandName = m.CommandName;
                dmc.DeviceModelId = m.DeviceModelId;
                dmc.CommandTemplate = m.CommandTemplate;
                _context.DeviceModelCommands.Update(dmc);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.CantFindObject, "Ok", true);
            }

            return new ApiResult<bool>(ApiCode.CantFindObject, "CantFindObject", false);
        }

        [HttpGet]
        public ApiResult<bool> DeleteCommand(Guid id)
        {
            var dmc = _context.DeviceModelCommands.SingleOrDefault(c => c.CommandId == id);
            if (dmc != null)
            {
                dmc.CommandStatus = -1;
                _context.DeviceModelCommands.Update(dmc);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.CantFindObject, "Ok", true);
            }

            return new ApiResult<bool>(ApiCode.CantFindObject, "CantFindObject", false);
        }
    }
}