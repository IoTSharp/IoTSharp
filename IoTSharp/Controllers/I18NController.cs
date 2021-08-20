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
using LinqKit;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class I18NController : ControllerBase
    {


        private ApplicationDbContext _context;


        public I18NController(ApplicationDbContext context)
        {

            this._context = context;
        }


        [HttpPost("[action]")]
        public AppMessage Index([FromBody] I18NParam m)
        {

            Expression<Func<BaseI18N, bool>> condition = x => x.Status > -1;
            if (!string.IsNullOrEmpty(m.KeyName))
            {
                condition = condition.And(x => x.KeyName.Contains(m.KeyName));
            }
            var rows = _context.BaseI18Ns.OrderByDescending(c => c.Id).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList();
            var total = _context.BaseI18Ns.Count(condition);
            return new AppMessage
            {
                ErrType = ErrType.正常返回,
                Result = new
                {
                    rows,
                    total
                }

            };
        }
        [HttpGet("[action]")]
        public AppMessage Get(int id)
        {
            return new AppMessage
            {
                ErrType = ErrType.正常返回,
                Result = _context.BaseI18Ns.SingleOrDefault(c=>c.Id==id)

            };

        }
        [HttpPost("[action]")]
        public AppMessage Save(BaseI18N m )
        {
            var i18n = new BaseI18N()
            {
                AddDate = DateTime.Now,

                Status = 1,
                KeyName = m.KeyName,
                ValueDEDE = m.ValueDEDE,
                ValueESES = m.ValueESES,
                ValueENUS = m.ValueENUS,
                ValueENGR = m.ValueENGR,
                ValueELGR = m.ValueELGR,
                ValueFRFR = m.ValueFRFR,
                ValueHRHR = m.ValueHRHR,
                ValueITIT = m.ValueITIT,
                ValueJAJP = m.ValueJAJP,
                ValueKOKR = m.ValueKOKR,
                ValuePLPL = m.ValuePLPL,
                ValueSLSL = m.ValueSLSL,
                ValueTRTR = m.ValueTRTR,
                ValueZHCN = m.ValueZHCN,
                ValueZHTW = m.ValueZHTW,
                ValueBG = m.ValueBG,
                ValueCS = m.ValueCS,
                ValueDA = m.ValueDA,
                ValueFI = m.ValueFI,
                ValueHE = m.ValueHE,
                ValueHU = m.ValueHU,
                ValueNL = m.ValueNL,
                ValueSR = m.ValueSR,
                ValueSV = m.ValueSV,
                ValueUK = m.ValueUK,
                ValueVI = m.ValueVI,

            };

            _context.BaseI18Ns.Add(i18n);
            _context.SaveChanges();
            var i18ns = _context.BaseI18Ns.Where(c => c.Status > -1).ToArray();

            return new AppMessage
            {
                ErrType = ErrType.正常返回,

            };

        }

        [HttpGet("[action]")]
        public AppMessage Delete(long id)
        {
            var i18n = _context.BaseI18Ns.FirstOrDefault(c => c.Id == id);

            if (i18n != null)
            {
                i18n.Status = -1;
                _context.BaseI18Ns.Update(i18n);
                _context.SaveChanges();
                return new AppMessage
                {
                    ErrType = ErrType.正常返回,

                };
            }
            return new AppMessage
            {
                ErrType = ErrType.正常返回,

            };

        }

        [HttpGet("[action]")]
        public AppMessage SetStatus(int id)
        {
            var obj = _context.BaseI18Ns.FirstOrDefault(c => c.Id == id);
            if (obj != null)
            {
                obj.Status = obj.Status == 1 ? 0 : 1;
                _context.BaseI18Ns.Update(obj);
                _context.SaveChanges();
                return new AppMessage
                {
                    ErrType = ErrType.正常返回,
                    Result = new
                    {

                    }

                };
            }
            return new AppMessage
            {
                ErrType = ErrType.找不到对象,
                Result = new
                {

                }

            };

        }
        [HttpPost("[action]")]
        public AppMessage Update(BaseI18N m)
        {


            var i18n = _context.BaseI18Ns.FirstOrDefault(c => c.Id == m.Id);
            if (i18n != null)
            {
                i18n.KeyName = m.KeyName;
                i18n.ValueDEDE = m.ValueDEDE;
                i18n.ValueESES = m.ValueESES;
                i18n.ValueENUS = m.ValueENUS;
                i18n.ValueENGR = m.ValueENGR;
                i18n.ValueELGR = m.ValueELGR;
                i18n.ValueFRFR = m.ValueFRFR;
                i18n.ValueHRHR = m.ValueHRHR;
                i18n.ValueITIT = m.ValueITIT;
                i18n.ValueJAJP = m.ValueJAJP;
                i18n.ValueKOKR = m.ValueKOKR;
                i18n.ValuePLPL = m.ValuePLPL;
                i18n.ValueSLSL = m.ValueSLSL;
                i18n.ValueTRTR = m.ValueTRTR;
                i18n.ValueZHCN = m.ValueZHCN;
                i18n.ValueZHTW = m.ValueZHTW;
                i18n.ValueBG = m.ValueBG;
                i18n.ValueCS = m.ValueCS;
                i18n.ValueDA = m.ValueDA;
                i18n.ValueFI = m.ValueFI;
                i18n.ValueHE = m.ValueHE;
                i18n.ValueHU = m.ValueHU;
                i18n.ValueNL = m.ValueNL;
                i18n.ValueSR = m.ValueSR;
                i18n.ValueSV = m.ValueSV;
                i18n.ValueUK = m.ValueUK;
                i18n.ValueVI = m.ValueVI;
                _context.BaseI18Ns.Update(i18n);
                _context.SaveChanges();

                return new AppMessage
                {
                    ErrType = ErrType.正常返回,
                    Result = new
                    {

                    }

                };
            }

            return new AppMessage
            {
                ErrType = ErrType.找不到对象,
                Result = new
                {

                }

            };

        }
    }
}
