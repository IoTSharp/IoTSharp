using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IoTSharp.App_Code.Util;
using IoTSharp.Data;
using IoTSharp.Models;
using IoTSharp.Models.Rule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class RulesController : ControllerBase
    {

        private ApplicationDbContext _context;
        private UserManager<IdentityUser> _userManager;
        public RulesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            this._userManager = userManager;
            this._context = context;
        }
        [HttpPost("[action]")]
        public AppMessage Index([FromBody] RulePageParam m)
        {
            Expression<Func<FlowRule, bool>> expression = x => x.RuleStatus>-1;
            if (!string.IsNullOrEmpty(m.Name))
            {
                expression = expression.And(x => x.Name.Contains(m.Name));
            }

            if (m.CreatTime != null)
            {
                expression = expression.And(x => x.CreatTime > m.CreatTime[0] && x.CreatTime < m.CreatTime[1]);
            }
            if (!string.IsNullOrEmpty(m.Creator))
            {
                expression = expression.And(x => x.Creator == m.Creator);
            }

         

            return new AppMessage()
            {
                ErrType = ErrType.正常返回,
                Result = new
                {
                    rows = _context.FlowRules.OrderByDescending(c => c.CreatTime).Skip(m.offset* m.limit).Take(m.limit).ToList(),
                    totel = _context.FlowRules.Count()
                }

            };
        }
        [HttpPost("[action]")]
        public AppMessage Save(FlowRule m)
        {
            if (ModelState.IsValid)
            {
                m.RuleStatus = 1;
                _context.FlowRules.Add(m);
                _context.SaveChanges();

                return new AppMessage
                {
                    ErrType = ErrType.正常返回,
                    Result = m
                };

            }

            return new AppMessage
            {
                ErrType = ErrType.参数错误,
                ErrMessage = ModelState.Values.SelectMany(c => c.Errors.FirstOrDefault()?.ErrorMessage).Aggregate("",
                (x, y) => x + "," + y)
            };
        }

        [HttpPost("[action]")]
        public AppMessage Update(FlowRule m)
        {
            if (ModelState.IsValid)
            {
                _context.FlowRules.Update(m);
                _context.SaveChanges();
                return new AppMessage
                {
                    ErrType = ErrType.正常返回,
                    Result = m
                };

            }


            return new AppMessage
            {
                ErrType = ErrType.参数错误,
                ErrMessage = ModelState.Values.SelectMany(c => c.Errors.FirstOrDefault()?.ErrorMessage).Aggregate("",
                    (x, y) => x + "," + y)
            };
        }

        [HttpGet("[action]")]
        public AppMessage Delete(long id)
        {
            var rule = _context.FlowRules.FirstOrDefault(c => c.RuleId == id);
            if (rule != null)
            {
                rule.RuleStatus = -1;
                _context.FlowRules.Update(rule);
                _context.SaveChanges(); return new AppMessage { ErrType = ErrType.正常返回, };
            }

            return new AppMessage { ErrType = ErrType.找不到对象, };
        }

        [HttpGet("[action]")]
        public AppMessage<FlowRule> Get(long id)
        {
            var rule = _context.FlowRules.FirstOrDefault(c => c.RuleId == id);
            if (rule != null)
            {
                return new AppMessage<FlowRule> { ErrType = ErrType.正常返回, Result = rule };
            }
            return new AppMessage<FlowRule> { ErrType = ErrType.找不到对象, };
        }

        [HttpGet("[action]")]
        public AppMessage Active(string id)
        {
            return new AppMessage();
        }



        [HttpPost("[action]")]
        public ActionResult SaveDiagram(ModelWorkFlow m)
        {
            var user = _userManager.GetUserId(User);
            var activity = JsonConvert.DeserializeObject<Activity>(m.Biz);

             var rule = _context.FlowRules.FirstOrDefault(c => c.RuleId == activity.RuleId);
             rule.DefinitionsXml = m.Xml;
             _context.FlowRules.Update(rule);
             _context.SaveChanges();
             _context.Flows.RemoveRange(_context.Flows.Where(c => c.RuleId == activity.RuleId).ToList());
            _context.SaveChanges();

            if (activity.BaseBpmnObjects != null)
            {
         
                _context.Flows.AddRange(activity.BaseBpmnObjects.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,

                }).ToList());
                _context.SaveChanges();
    
            }

            if (activity.StartEvents != null)
            {
                _context.Flows.AddRange(activity.StartEvents.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,

                }).ToList());
                _context.SaveChanges();
            }
            if (activity.EndEvents != null)
            {
                _context.Flows.AddRange(activity.EndEvents.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,

                }).ToList());
                _context.SaveChanges();

            }
            if (activity.SequenceFlows != null)
            {
                _context.Flows.AddRange(activity.SequenceFlows.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype, SourceId = c.sourceId, TargetId = c.targetId

                }).ToList());
                _context.SaveChanges();

            }
            if (activity.Tasks != null)
            {
                _context.Flows.AddRange(activity.Tasks.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype, 
                    NodeProcessParams = c.BizObject.NodeProcessParams,
                    NodeProcessClass = c.BizObject.NodeProcessClass
                }).ToList());
                _context.SaveChanges();
            }
            if (activity.DataInputAssociations != null)
            {
                _context.Flows.AddRange(activity.DataInputAssociations.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype, 

                }).ToList());
                _context.SaveChanges();
            }
            if (activity.DataOutputAssociations != null)
            {
                _context.Flows.AddRange(activity.DataOutputAssociations.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,

                }).ToList());
                _context.SaveChanges();
            }
            if (activity.TextAnnotations != null)
            {
                _context.Flows.AddRange(activity.TextAnnotations.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,

                }).ToList());
                _context.SaveChanges();
            }
            if (activity.Containers != null)
            {
                _context.Flows.AddRange(activity.Containers.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,

                }).ToList());
                _context.SaveChanges();
            }
            if (activity.GateWays != null)
            {
                _context.Flows.AddRange(activity.GateWays.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,

                }).ToList());
                _context.SaveChanges();
            }
            if (activity.DataStoreReferences != null)
            {
                _context.Flows.AddRange(activity.DataStoreReferences.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,

                }).ToList());
                _context.SaveChanges();
            }
            if (activity.Lane != null)
            {
                _context.Flows.AddRange(activity.Lane.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,

                }).ToList());
                _context.SaveChanges();

            }
            if (activity.LaneSet != null)
            {
                _context.Flows.AddRange(activity.LaneSet.Select(c => new Flow
                {

                    RuleId = activity.RuleId,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,

                }).ToList());
                _context.SaveChanges();

            }
            return new JsonResult(new AppMessage { ErrMessage = "操作成功", ErrType = ErrType.正常返回, IsVisble = true, ErrLevel = ErrLevel.Success });
        }
    }


}
