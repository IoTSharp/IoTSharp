using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DynamicFormInfoController : ControllerBase
    {
        private ApplicationDbContext _context;
        private UserManager<IdentityUser> _userManager;

        public DynamicFormInfoController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            this._userManager = userManager;
            this._context = context;
        }

        [HttpPost("[action]")]
        public ApiResult<PagedData<DynamicFormInfo>> Index([FromBody] IPageParam m)
        {
            Expression<Func<DynamicFormInfo, bool>> condition = x => x.FormStatus > -1;
            var result = _context
                .DynamicFormInfos.OrderByDescending(c => c.FormId).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList();

            return new ApiResult<PagedData<DynamicFormInfo>>(ApiCode.Success, "OK", new PagedData<DynamicFormInfo>
            {
                total = _context.DynamicFormInfos.Count(condition),
                rows = _context.DynamicFormInfos.OrderByDescending(c => c.FormId).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList()
            });
        }

        [HttpGet("[action]")]
        public ApiResult<DynamicFormInfo> Get(int id)
        {
            var dynamicFormInfo = _context.DynamicFormInfos.SingleOrDefault(c => c.FormId == id);
            if (dynamicFormInfo != null)
            {
                return new ApiResult<DynamicFormInfo>(ApiCode.Success, "OK", dynamicFormInfo);
            }
            else
            {
                return new ApiResult<DynamicFormInfo>(ApiCode.CantFindObject, "can't find this object", null);
            }
        }

        [HttpGet("[action]")]
        public ApiResult<List<DynamicFormInfo>> GetFields(int id)
        {
            var dynamicFormInfo = _context.DynamicFormInfos.Where(c => c.FormId == id).ToList();
            return new ApiResult<List<DynamicFormInfo>>(ApiCode.Success, "OK", dynamicFormInfo);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public ApiResult<bool> Save(DynamicFormInfo m)
        {
            var route = new DynamicFormInfo()
            {
                FormName = m.FormName,
                FormDesc = m.FormDesc,
                FromCreateDate = DateTime.Now,

                FormStatus = 1,
            };

            _context.DynamicFormInfos.Add(route);
            _context.SaveChanges();

            return new ApiResult<bool>(ApiCode.Success, "OK", true);
        }

        [HttpPost("[action]")]
        public ApiResult<bool> Update(DynamicFormInfo m)
        {
            var route = _context.DynamicFormInfos.FirstOrDefault(c => c.FormId == m.FormId);
            if (route != null)
            {
                route.FormName = m.FormName;
                route.FormDesc = m.FormDesc;
                _context.DynamicFormInfos.Update(route);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            return new ApiResult<bool>(ApiCode.CantFindObject, "can't find this object", false);
        }

        [HttpGet("[action]")]
        public ApiResult<bool> Delete(int id)
        {
            var route = _context.DynamicFormInfos.FirstOrDefault(c => c.FormId == id);
            if (route != null)
            {
                route.FormStatus = -1;
                _context.DynamicFormInfos.Update(route);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            return new ApiResult<bool>(ApiCode.CantFindObject, "can't find this object", false);
        }

        [HttpGet("[action]")]
        public ApiResult<bool> SetStatus(int id)
        {
            var obj = _context.DynamicFormInfos.FirstOrDefault(c => c.FormId == id);
            if (obj != null)
            {
                obj.FormStatus = obj.FormStatus == 1 ? 0 : 1;
                _context.DynamicFormInfos.Update(obj);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            return new ApiResult<bool>(ApiCode.CantFindObject, "can't find this object", false);
        }

        [HttpPost("[action]")]
        public ApiResult<bool> SaveParams(FormFieldData model)
        {
            var fields = _context.DynamicFormFieldInfos.
                Where(c => c.FormId == model.Id && c.FieldStatus > -1).ToList();

            if (fields != null)
            {
                var typeinfo = _context.BaseDictionaries.Where(c => c.DictionaryGroupId == 2 && c.DictionaryStatus > 0)
                    .ToList();
                _context.DynamicFormFieldInfos.Where(c => c.FormId == model.Id &&
                                                          c.FieldStatus > -1).ToList()
                    .ForEach(x =>
                    {
                        if (model.propdata.All(c => c.FieldId != x.FieldId))
                        {
                            x.FieldStatus = -1;
                            _context.DynamicFormFieldInfos.Update(x);
                            _context.SaveChanges();
                        }
                    });
                foreach (var item in model.propdata)
                {
                    var field =
                        fields.FirstOrDefault(c => c.FieldId == item.FieldId);
                    if (field != null)
                    {
                        field.FieldStatus = 1;
                        field.FieldPocoTypeName = typeinfo
                            .FirstOrDefault(c => c.DictionaryValue == item.FieldValueType.ToString())?.DictionaryTag;
                        field.FieldCode = item.FieldCode;
                        field.FieldName = item.FieldName;
                        field.IsRequired = item.IsRequired;
                        field.FieldValue = item.FieldValue;
                        field.FieldValueType = item.FieldValueType;
                        field.FieldMaxLength = item.FieldMaxLength;
                        field.FieldI18nKey = item.FieldI18nKey;
                        field.FieldPattern = item.FieldPattern;
                        field.FieldValueDataSource = item.FieldValueDataSource;
                        field.FieldUIElementSchema = item.FieldUIElementSchema;
                        field.FieldUIElement = item.FieldUIElement;
                        field.FieldUnit = item.FieldUnit;

                        field.FieldCreateDate = DateTime.Now;

                        switch (item.FieldUIElement)
                        {
                            case 1:
                                field.FieldValueTypeName = "number";
                                break;

                            case 5:
                                field.FieldValueTypeName = "boolean";
                                break;

                            case 16:
                                field.FieldValueType = 14;
                                field.FieldValueTypeName = "number";
                                break;

                            case 19:
                                field.FieldValueTypeName = "number";
                                break;

                            default:
                                field.FieldValueTypeName = "string";
                                break;
                        }

                        _context.DynamicFormFieldInfos.Update(field);
                        _context.SaveChanges();
                    }
                    else
                    {
                        field = new DynamicFormFieldInfo()
                        {
                            FieldCode = item.FieldCode,
                            FieldId = item.FieldId,
                            FieldStatus = 1,
                            IsRequired = item.IsRequired,
                            FormId = model.Id,
                            FieldPocoTypeName = typeinfo
                                .FirstOrDefault(c => c.DictionaryValue == item.FieldValueType.ToString())?.DictionaryTag,
                            FieldName = item.FieldName,
                            FieldValue = item.FieldValue,
                            FieldValueType = item.FieldValueType,
                            FieldMaxLength = item.FieldMaxLength,
                            FieldI18nKey = item.FieldI18nKey,
                            FieldPattern = item.FieldPattern,
                            FieldValueDataSource = item.FieldValueDataSource,
                            FieldUIElementSchema = item.FieldUIElementSchema,
                            FieldUIElement = item.FieldUIElement,
                            FieldUnit = item.FieldUnit
                        };
                        switch (item.FieldUIElement)
                        {
                            case 1:
                                field.FieldValueTypeName = "number";
                                break;

                            case 5:
                                field.FieldValueTypeName = "boolean";
                                break;

                            case 19:
                                field.FieldValueTypeName = "number";
                                break;

                            default:
                                field.FieldValueTypeName = "string";
                                break;
                        }
                        _context.DynamicFormFieldInfos.Add(field);
                        _context.SaveChanges();
                    }
                }
                var allfields = _context.DynamicFormFieldInfos.Where(c => c.FormId == model.Id &&
                                                                          c.FieldStatus > -1).ToList();

                // 土味代码生成
                StringBuilder builder = new StringBuilder("public class FormData" + model.Id + "{\n");
                allfields.ForEach(x =>
                {
                    builder.Append("public ").Append(Type.GetType(x.FieldPocoTypeName)?.Name).Append(" ").Append(x.FieldCode)
                        .Append("{ get; set;}\n");
                });
                builder.Append("}");
                var form = _context.DynamicFormInfos.FirstOrDefault(c => c.FormId == model.Id);
                form.ModelClass = builder.ToString();
                _context.DynamicFormInfos.Update(form);
                _context.SaveChanges();

                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            return new ApiResult<bool>(ApiCode.CantFindObject, "can't find any fields", false);
        }

        [HttpGet("[action]")]
        public ApiResult<dynamic> GetParams(int id)
        {
            var deviceTypefields = _context.DynamicFormFieldInfos.Where(c => c.FormId == id && c.FieldStatus > -1).ToList();

            return new ApiResult<dynamic>(ApiCode.Success, "OK", new
            {
                Id = id,
                propdata = deviceTypefields.Select(c => new
                {
                    c.FormId,
                    c.FieldId,
                    c.FieldName,
                    c.FieldValue,
                    FieldValueType = c.FieldValueType.ToString(),
                    c.FieldStatus,
                    c.FieldPattern,
                    c.FieldMaxLength,
                    c.FieldI18nKey,
                    c.FieldValueDataSource,
                    c.FieldValueTypeName,
                    c.FieldUIElement,
                    c.FieldUIElementSchema,
                    c.FieldUnit,
                    c.FieldCode
                }).ToArray()
            });
        }

        [HttpGet("[action]")]
        public ApiResult<List<DynamicFormFieldInfo>> GetFormFieldValue(int BizId, int FormId)
        {
            var Fields = _context.DynamicFormFieldInfos.Where(c => c.FormId == FormId && c.FieldStatus > 0)
                .ToList();
            var FieldValues =
                _context.DynamicFormFieldValueInfos.Where(c => c.FromId == FormId && c.BizId == BizId);

            foreach (var item in Fields)
            {
                var _item = FieldValues.FirstOrDefault(c => c.FieldId == item.FieldId);
                if (_item != null)
                {
                    item.FieldValue = _item.FieldValue;
                }
            }

            return new ApiResult<List<DynamicFormFieldInfo>>(ApiCode.Success, "OK", Fields);
        }
    }
}