using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DictionaryGroupController : ControllerBase
    {

        private ApplicationDbContext _context;
        private UserManager<IdentityUser> _userManager;

        public DictionaryGroupController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            this._userManager = userManager;
            this._context = context;
        }


        [HttpPost("[action]")]
        public ApiResult<PagedData<BaseDictionaryGroup>> Index([FromBody] IPageParam m)
        {

            Expression<Func<BaseDictionaryGroup, bool>> condition = x => x.DictionaryGroupStatus > -1;
            var result = _context.BaseDictionaryGroups.Where(condition)
                .OrderByDescending(c => c.DictionaryGroupId).Skip((m.offset) * m.limit).Take(m.limit).ToList();

            return new ApiResult<PagedData<BaseDictionaryGroup>>(ApiCode.Success, "OK", new PagedData<BaseDictionaryGroup>
            {
                total = _context.BaseDictionaryGroups.Count(condition),
                rows = result
            });
        }

        [HttpGet("[action]")]
        public ApiResult<BaseDictionaryGroup> Get(int id)
        {
            var dictionaryGroup = _context.BaseDictionaryGroups.FirstOrDefault(c => c.DictionaryGroupId == id);
            if (dictionaryGroup != null)
            {
                return new ApiResult<BaseDictionaryGroup>(ApiCode.Success, "OK", dictionaryGroup);
            }
            return new ApiResult<BaseDictionaryGroup>(ApiCode.Success, "can't find this object", null);


        }


        [HttpGet("[action]")]
        public ApiResult<bool> SetStatus(int id)
        {
            var obj = _context.BaseDictionaryGroups.FirstOrDefault(c => c.DictionaryGroupId == id);
            if (obj != null)
            {
                obj.DictionaryGroupStatus = obj.DictionaryGroupStatus == 1 ? 0 : 1;
                _context.BaseDictionaryGroups.Update(obj);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            return new ApiResult<bool>(ApiCode.Success, "can't find this object", false);
        }







        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public ApiResult<bool> Save(BaseDictionaryGroup m)
        {
            var dictionaryGroup = new BaseDictionaryGroup()
            {
                DictionaryGroupKey = m.DictionaryGroupKey,
                DictionaryGroupName = m.DictionaryGroupName,
                DictionaryGroupValueType = m.DictionaryGroupValueType,
                DictionaryGroupStatus = 1,
                DictionaryGroupValueTypeName = m.DictionaryGroupValueTypeName,
                DictionaryGroupDesc = m.DictionaryGroupDesc,
                DictionaryGroupId = m.DictionaryGroupId

            };

            _context.BaseDictionaryGroups.Add(dictionaryGroup);
            _context.SaveChanges();
            return new ApiResult<bool>(ApiCode.Success, "OK", true);
        }
        [HttpPost("[action]")]
        public ApiResult<bool> Update(BaseDictionaryGroup m)
        {
            var dictionaryGroup = _context.BaseDictionaryGroups.FirstOrDefault(c => c.DictionaryGroupId == m.DictionaryGroupId);

            if (dictionaryGroup != null)
            {
                dictionaryGroup.DictionaryGroupName = m.DictionaryGroupName;
                dictionaryGroup.DictionaryGroupId = m.DictionaryGroupId;
                dictionaryGroup.DictionaryGroupKey = m.DictionaryGroupKey;
                dictionaryGroup.DictionaryGroupValueType = m.DictionaryGroupValueType;
                dictionaryGroup.DictionaryGroupValueTypeName = m.DictionaryGroupValueTypeName;
                dictionaryGroup.DictionaryGroupDesc = m.DictionaryGroupDesc;
                _context.BaseDictionaryGroups.Update(dictionaryGroup);
                _context.SaveChanges();

                return new ApiResult<bool>(ApiCode.Success, "OK", true);

            }
            return new ApiResult<bool>(ApiCode.Success, "can't find this object", false);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public ApiResult<bool> Delete(int id)
        {
            var dictionaryGroup = _context.BaseDictionaryGroups.FirstOrDefault(c => c.DictionaryGroupId == id);

            if (dictionaryGroup != null)
            {

                dictionaryGroup.DictionaryGroupStatus = -1;
                _context.BaseDictionaryGroups.Update(dictionaryGroup);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            return new ApiResult<bool>(ApiCode.Success, "can't find this object", false);
        }


    }
}
