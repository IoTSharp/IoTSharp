using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Models;
using Microsoft.AspNetCore.Identity;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public AppMessage Index([FromBody] IPageParam m)
        {

            Expression<Func<BaseDictionaryGroup, bool>> condition = x => x.DictionaryGroupStatus > -1;



            var result = _context.BaseDictionaryGroups
                .OrderByDescending(c => c.DictionaryGroupId).Skip((m.offset) * m.limit).Take(m.limit).ToList();
            return new AppMessage
            {
                Result = new 
                {
                    rows = result,
                    total = _context.BaseDictionaryGroups.Count()
                }
            };

        }

        [HttpGet("[action]")]
        public AppMessage Get(int id)
        {
            var dictionaryGroup = _context.BaseDictionaryGroups.FirstOrDefault(c => c.DictionaryGroupId == id);

            if (dictionaryGroup != null)
            {
                return new AppMessage
                {
                    Result = dictionaryGroup
                };
            }
            return new AppMessage
            {
                ErrType = ErrType.找不到对象
            };


        }


        [HttpGet("[action]")]
        public AppMessage SetStatus(int id)
        {
            var obj = _context.BaseDictionaryGroups.FirstOrDefault(c => c.DictionaryGroupId == id);
            if (obj != null)
            {
                obj.DictionaryGroupStatus = obj.DictionaryGroupStatus == 1 ? 0 : 1;
                _context.BaseDictionaryGroups.Update(obj);
                _context.SaveChanges();
                return new AppMessage
                {
                    ErrType = ErrType.正常返回
                };
            }
            return new AppMessage
            {
                ErrType = ErrType.找不到对象
            };
        }







        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public AppMessage Save(BaseDictionaryGroup m)
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
            return new AppMessage
            {
                ErrType = ErrType.正常返回
            };
        }
        [HttpPost("[action]")]
        public AppMessage Update(BaseDictionaryGroup m)
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

                return new AppMessage
                {
                    ErrType = ErrType.正常返回
                };

            }
            return new AppMessage
            {
                ErrType = ErrType.找不到对象
            }
            ;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public AppMessage Delete(int id)
        {
            var dictionaryGroup = _context.BaseDictionaryGroups.FirstOrDefault(c => c.DictionaryGroupId == id);

            if (dictionaryGroup != null)
            {

                dictionaryGroup.DictionaryGroupStatus = -1;
                _context.BaseDictionaryGroups.Update(dictionaryGroup);
                _context.SaveChanges();
                return new AppMessage
                {
                    ErrType = ErrType.正常返回
                };
            }
            return new AppMessage
            {
                ErrType = ErrType.找不到对象
            };
        }


    }
}
