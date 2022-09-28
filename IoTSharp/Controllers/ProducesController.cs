using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        [HttpGet]
        public async Task<ApiResult<PagedData<ProduceDto>>> List([FromQuery] ProduceParam m)
        {
            var profile = this.GetUserProfile();
            Expression<Func<Produce, bool>> condition = x =>
                x.Customer.Id == profile.Comstomer && x.Tenant.Id == profile.Tenant;


            if (!string.IsNullOrEmpty(m.Name))
            {
                condition = condition.And(x => x.Name.Contains(m.Name));
            }

            return new ApiResult<PagedData<ProduceDto>>(ApiCode.Success, "OK", new PagedData<ProduceDto>
            {
                total = await _context.Produces.CountAsync(condition),
                rows = _context.Produces.Where(condition).Where(condition).Skip((m.offset) * m.limit).Take(m.limit)
                    .ToList().Select(c => new ProduceDto
                    { Id = c.Id, DefaultIdentityType = c.DefaultIdentityType, DefaultTimeout=c.DefaultTimeout, Description= c.Description, Name=c.Name}).ToList()

            });

        }
        [HttpGet]
        public async Task<ApiResult<List<ProduceData>>> ProduceDatas(Guid produceid)
        {
            var result =    await _context.ProduceDatas.Include(c => c.Owner).Where(p=>p.Owner.Id==produceid).AsSplitQuery().ToListAsync();
            return new ApiResult<List<ProduceData>>(ApiCode.Success, "OK", result);
        }
      


    }
} 
