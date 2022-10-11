using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShardingCore.Extensions;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProducesController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ProducesController(ApplicationDbContext context, ILogger<ProducesController> logger)
        {
            _context = context;
            _logger = logger;
        }
        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<PagedData<ProduceDto>>> List([FromQuery] ProduceParam m)
        {
            var profile = this.GetUserProfile();
            Expression<Func<Produce, bool>> condition = x =>
                x.Customer.Id == profile.Customer && x.Tenant.Id == profile.Tenant;


            if (!string.IsNullOrEmpty(m.Name))
            {
                condition = condition.And(x => x.Name.Contains(m.Name));
            }

            return new ApiResult<PagedData<ProduceDto>>(ApiCode.Success, "OK", new PagedData<ProduceDto>
            {
                total = await _context.Produces.CountAsync(condition),
                rows = _context.Produces.Where(condition).Where(condition).Skip((m.offset) * m.limit).Take(m.limit)
                    .ToList().Select(c => new ProduceDto
                    { Id = c.Id, DefaultIdentityType = c.DefaultIdentityType, DefaultTimeout = c.DefaultTimeout, Description = c.Description, Name = c.Name }).ToList()

            });

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="produceid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<ProduceData>>> ProduceDatas(Guid produceid)
        {
            var result = await _context.ProduceDatas.Include(c => c.Owner).Where(p => p.Owner.Id == produceid).AsSplitQuery().ToListAsync();
            return new ApiResult<List<ProduceData>>(ApiCode.Success, "OK", result);
        }
        /// <summary>
        /// 获取产品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<ApiResult<ProduceAddDto>> Get(Guid id)
        {
            var result = await _context.Produces.SingleOrDefaultAsync(c => c.Id == id);
            if (result != null)
            {
                return new ApiResult<ProduceAddDto>(ApiCode.Success, "OK", new ProduceAddDto
                {

                    Id = result.Id,
                    Customer = result.Customer.Id,
                    Tenant = result.Tenant.Id,
                    Icon = result.Icon,
                    DefaultDeviceType = result.DefaultDeviceType,
                    DefaultIdentityType = result.DefaultIdentityType,
                    DefaultTimeout = result.DefaultTimeout,
                    Description = result.Description,
                    GatewayConfiguration = result.GatewayConfiguration,
                    GatewayType = result.GatewayType,
                    Name = result.Name

                });
            }
            else
            {
                return new ApiResult<ProduceAddDto>(ApiCode.CantFindObject, "Produce is  not found", null);
            }

        }
        /// <summary>
        /// 删除产品
        /// </summary>
        /// <param name="produceid"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<ApiResult<bool>> Delete(Guid produceid)
        {

            try
            {
                var produce = await _context.Produces.SingleOrDefaultAsync(c => c.Id == produceid);
                if (produce != null)
                {
                    _context.ProduceDatas.RemoveRange(_context.ProduceDatas.Where(c => c.Owner == produce));
                    await _context.SaveChangesAsync();
                    _context.Produces.Remove(produce);
                    await _context.SaveChangesAsync();
                    return new ApiResult<bool>(ApiCode.Success, "OK", true);
                }
                return new ApiResult<bool>(ApiCode.CantFindObject, "Produce is  not found", false);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }

        }

        /// <summary>
        /// 新增产品
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<bool>> Save(ProduceAddDto dto)
        {

            try
            {
                var profile = this.GetUserProfile();
                var produce = new Produce();
                produce.Customer = _context.Customer.Find(profile.Customer);
                produce.Tenant = _context.Tenant.Find(profile.Tenant);
                produce.DefaultDeviceType = dto.DefaultDeviceType;
                produce.DefaultIdentityType=dto.DefaultIdentityType;
                produce.DefaultTimeout=dto.DefaultTimeout;
                produce.Description=dto.Description;
                produce.Name=   
                    dto.Name;
                produce.GatewayConfiguration=dto.GatewayConfiguration;
                produce.Icon=dto.Icon;
                produce.GatewayType=dto.GatewayType;
                _context.Produces.Add(produce);
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }

        }

        /// <summary>
        /// 修改产品
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<bool>> Update(ProduceAddDto dto)
        {

            try
            {
                var produce = await _context.Produces.SingleOrDefaultAsync(c => c.Id == dto.Id);
                if (produce != null)
                {
                    produce.DefaultIdentityType = dto.DefaultIdentityType;
                    produce.DefaultIdentityType = dto.DefaultIdentityType;
                    produce.DefaultTimeout = dto.DefaultTimeout;
                    produce.Description = dto.Description;
                    produce.GatewayConfiguration = dto.GatewayConfiguration;
                    produce.Name = dto.Name;
                    produce.Icon = dto.Icon;
                    _context.Produces.Update(produce);
                    await _context.SaveChangesAsync();
                }
                return new ApiResult<bool>(ApiCode.CantFindObject, "Produce is  not found", false);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }
         
        }
    }
}
