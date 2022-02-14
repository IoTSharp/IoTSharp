using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DictionaryController : ControllerBase
    {
        private ApplicationDbContext _context;

        public DictionaryController(ApplicationDbContext context)
        {
            this._context = context;
        }

        [HttpPost("[action]")]
        public ApiResult<PagedData<BaseDictionary>> Index([FromBody] DictionaryParam m)
        {
            Expression<Func<BaseDictionary, bool>> condition = x => x.DictionaryStatus > -1;
            if (!string.IsNullOrEmpty(m.DictionaryName))
            {
                condition = condition.And(x => x.DictionaryName.Contains(m.DictionaryName));
            }

            if (m.DictionaryGroupId > 0)
            {
                condition = condition.And(x => x.DictionaryGroupId == m.DictionaryGroupId);
            }

            var rows = _context.BaseDictionaries.OrderByDescending(c => c.DictionaryId).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList();
            var total = _context.BaseDictionaries.Count(condition);

            return new ApiResult<PagedData<BaseDictionary>>(ApiCode.Success, "OK", new PagedData<BaseDictionary>
            {
                total = total,
                rows = rows
            });
        }

        [HttpGet("[action]")]
        public ApiResult<BaseDictionary> Get(int id)
        {
            var Dictionary = _context.BaseDictionaries.FirstOrDefault(c => c.DictionaryId == id);
            if (Dictionary != null)
            {
                return new ApiResult<BaseDictionary>(ApiCode.Success, "OK", Dictionary);
            }
            return new ApiResult<BaseDictionary>(ApiCode.CantFindObject, "can't find this object", null);
        }

        [HttpPost("[action]")]
        public ApiResult<bool> Save(BaseDictionary m)
        {
            var dict = new BaseDictionary()
            {
                DictionaryStatus = m.DictionaryStatus = 1,
                DictionaryName = m.DictionaryName,
                DictionaryValueType = m.DictionaryValueType,
                DictionaryValue = m.DictionaryValue,
                DictionaryValueTypeName = m.DictionaryValueTypeName,
                Dictionary18NKeyName = m.Dictionary18NKeyName,
                DictionaryPattern = m.DictionaryPattern,
                DictionaryDesc = m.DictionaryDesc,
                DictionaryGroupId = m.DictionaryGroupId,
                DictionaryColor = m.DictionaryColor,
                DictionaryIcon = m.DictionaryIcon,
            };
            _context.BaseDictionaries.Add(dict);
            _context.SaveChanges();

            return new ApiResult<bool>(ApiCode.Success, "OK", true);
        }

        [HttpPost("[action]")]
        public ApiResult<bool> Update(BaseDictionary m)
        {
            var dictionary = _context.BaseDictionaries.FirstOrDefault(c => c.DictionaryId == m.DictionaryId);
            if (dictionary != null)
            {
                dictionary.DictionaryName = m.DictionaryName;
                dictionary.DictionaryGroupId = m.DictionaryGroupId;
                dictionary.DictionaryValue = m.DictionaryValue;
                dictionary.DictionaryValueType = m.DictionaryValueType;
                dictionary.DictionaryValueTypeName = m.DictionaryValueTypeName;
                dictionary.Dictionary18NKeyName = m.Dictionary18NKeyName;
                dictionary.DictionaryPattern = m.DictionaryPattern;
                dictionary.DictionaryDesc = m.DictionaryDesc;
                dictionary.DictionaryColor = m.DictionaryColor;
                dictionary.DictionaryIcon = m.DictionaryIcon;
                _context.BaseDictionaries.Update(dictionary);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            return new ApiResult<bool>(ApiCode.Success, "can't find this object", false);
        }

        [HttpGet("[action]")]
        public ApiResult<bool> SetStatus(int id)
        {
            var obj = _context.BaseDictionaries.FirstOrDefault(c => c.DictionaryId == id);
            if (obj != null)
            {
                obj.DictionaryStatus = obj.DictionaryStatus == 1 ? 0 : 1;
                _context.BaseDictionaries.Update(obj);
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
            var Dictionary = _context.BaseDictionaries.FirstOrDefault(c => c.DictionaryId == id);
            if (Dictionary != null)
            {
                Dictionary.DictionaryStatus = -1;
                _context.BaseDictionaries.Update(Dictionary);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            return new ApiResult<bool>(ApiCode.Success, "can't find this object", false);
        }
    }
}