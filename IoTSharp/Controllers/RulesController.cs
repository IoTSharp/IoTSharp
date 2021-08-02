using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dynamitey;

using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Models;
using IoTSharp.Models.Rule;
using LinqKit;
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
            Expression<Func<FlowRule, bool>> expression = x => x.RuleStatus > -1;
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
                    rows = _context.FlowRules.OrderByDescending(c => c.CreatTime).Where(expression).Skip(m.offset * m.limit).Take(m.limit)
                        .ToList(),
                    totel = _context.FlowRules.Count(expression)
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
                FlowRule flowrule = new FlowRule();
                flowrule.Name = m.Name;
                flowrule.RuleDesc = m.RuleDesc;
                _context.FlowRules.Update(flowrule);
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
                _context.SaveChanges();
                return new AppMessage {ErrType = ErrType.正常返回,};
            }

            return new AppMessage {ErrType = ErrType.找不到对象,};
        }

        [HttpGet("[action]")]
        public AppMessage<FlowRule> Get(long id)
        {
            var rule = _context.FlowRules.FirstOrDefault(c => c.RuleId == id);
            if (rule != null)
            {
                return new AppMessage<FlowRule> {ErrType = ErrType.正常返回, Result = rule};
            }

            return new AppMessage<FlowRule> {ErrType = ErrType.找不到对象,};
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
                    FlowType = c.bpmntype,
                    SourceId = c.sourceId,
                    TargetId = c.targetId, Conditionexpression = c.BizObject.conditionexpression,
                    NodeProcessParams = c.BizObject.NodeProcessParams

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

            return new JsonResult(new AppMessage
                {ErrMessage = "操作成功", ErrType = ErrType.正常返回, IsVisble = true, ErrLevel = ErrLevel.Success});
        }



        [HttpGet("[action]")]
        public AppMessage GetDiagram(long id)
        {

            var ruleflow = _context.FlowRules.FirstOrDefault(c => c.RuleId == id);
            Activity activity = new Activity();

            activity.SequenceFlows ??= new List<SequenceFlow>();
            activity.GateWays ??= new List<GateWay>();
            activity.Tasks ??= new List<IoTSharp.Models.Rule.Task>();
            activity.LaneSet ??= new List<BpmnBaseObject>();
            activity.EndEvents ??= new List<BpmnBaseObject>();
            activity.StartEvents ??= new List<BpmnBaseObject>();
            activity.Containers ??= new List<BpmnBaseObject>();
            activity.BaseBpmnObjects ??= new List<BpmnBaseObject>();
            activity.DataStoreReferences ??= new List<BpmnBaseObject>();
            activity.SubProcesses ??= new List<BpmnBaseObject>();
            activity.DataOutputAssociations ??= new List<BpmnBaseObject>();
            activity.DataInputAssociations ??= new List<BpmnBaseObject>();
            activity.Lane ??= new List<BpmnBaseObject>();
            activity.TextAnnotations ??= new List<BpmnBaseObject>();
            activity.RuleId = id;
            var flows = _context.Flows.Where(c => c.RuleId == id).ToList();


            foreach (var item in flows)
            {

                switch (item.FlowType)
                {
                    case "bpmn:SequenceFlow":

                        activity.SequenceFlows.Add(
                            new SequenceFlow()
                            {
                                sourceId = item.SourceId,
                                targetId = item.TargetId,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                    conditionexpression = item.Conditionexpression

                                }
                            });
                        break;

                    case "bpmn:EndEvent":
                        activity.EndEvents.Add(

                            new GateWay
                            {
                                sourceId = item.SourceId,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                    NodeProcessClass = item.NodeProcessClass,
                                }
                            });
                        break;
                    case "bpmn:StartEvent":
                        activity.StartEvents.Add(

                            new GateWay
                            {
                                sourceId = item.SourceId,

                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });

                        break;
                    case "bpmn:ExclusiveGateway":
                        activity.GateWays.Add(

                            new GateWay
                            {
                                sourceId = item.SourceId,

                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                    NodeProcessClass = item.NodeProcessClass
                                }
                            });
                        break;
                    case "bpmn:ParallelGateway":
                        activity.GateWays.Add(

                            new GateWay
                            {
                                sourceId = item.SourceId,

                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                    NodeProcessClass = item.NodeProcessClass
                                }
                            });
                        break;

                    case "bpmn:InclusiveGateway":
                        activity.GateWays.Add(

                            new GateWay
                            {
                                sourceId = item.SourceId,

                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {

                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                    NodeProcessClass = item.NodeProcessClass
                                }
                            });
                        break;
                    case "bpmn:EventBasedGateway":
                        activity.GateWays.Add(

                            new GateWay
                            {
                                sourceId = item.SourceId,

                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                    NodeProcessClass = item.NodeProcessClass
                                }
                            });
                        break;

                    case "bpmn:ComplexGateway":
                        activity.GateWays.Add(

                            new GateWay
                            {
                                sourceId = item.SourceId,

                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                    NodeProcessClass = item.NodeProcessClass
                                }
                            });
                        break;
                    case "bpmn:Task":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.Task
                            {


                                id = item.bpmnid,
                                Flowname = item.Flowname,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });
                        break;
                    case "bpmn:BusinessRuleTask":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.Task
                            {
                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });
                        break;
                    case "bpmn:ReceiveTask":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.Task
                            {

                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,

                                }
                            });
                        break;
                    case "bpmn:UserTask":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.Task
                            {

                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });
                        break;

                    case "bpmn:ServiceTask":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.Task
                            {

                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });
                        break;
                    case "bpmn:ManualTask":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.Task
                            {

                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });
                        break;
                    case "bpmn:SendTask":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.Task
                            {

                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });
                        break;
                    case "bpmn:CallActivity":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.Task
                            {

                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });
                        break;
                    case "bpmn:IntermediateCatchEvent":
                        activity.EndEvents.Add(

                            new BpmnBaseObject
                            {
                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });
                        break;
                    case "bpmn:IntermediateThrowEvent":
                        activity.EndEvents.Add(

                            new BpmnBaseObject()
                            {
                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });
                        break;

                    case "bpmn:Lane":
                        activity.Containers.Add(

                            new BpmnBaseObject
                            {
                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });
                        break;
                    case "bpmn:Participant":
                        activity.Containers.Add(

                            new BpmnBaseObject
                            {


                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {

                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });
                        break;
                    case "bpmn:DataStoreReference":
                        activity.DataStoreReferences.Add(

                            new BpmnBaseObject
                            {


                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {

                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,
                                }
                            });
                        break;
                    case "bpmn:SubProcess":
                        activity.SubProcesses.Add(

                            new BpmnBaseObject
                            {
                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {

                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType.ToString(),
                                    Flowname = item.Flowname,


                                }
                            });
                        break;


                    default:
                        BpmnBaseObject node = new BpmnBaseObject
                        {
                            BizObject = new FormBpmnObject
                            {
                                Flowid = item.FlowId.ToString(),
                                Flowdesc = item.Flowdesc,
                                Flowtype = item.FlowType.ToString(),
                                Flowname = item.Flowname,
                            },
                            bpmntype = item.FlowType,
                            id = item.FlowId.ToString(),
                        };
                        activity.DefinitionsDesc = ruleflow.RuleDesc;
                        activity.RuleId = ruleflow.RuleId;
                        activity.DefinitionsName = ruleflow.Name;
                        activity.DefinitionsStatus = ruleflow.RuleStatus ?? 0;
                       
                        activity.BaseBpmnObjects ??= new List<BpmnBaseObject>();
                        activity.BaseBpmnObjects.Add(node);
                        break;
                }

            }

            return new AppMessage
            {
                Result = new
                {
                    Xml = ruleflow.DefinitionsXml?.Trim('\r'), Biz = activity
                }
            };
        }
        [HttpPost("[action]")]
        public AppMessage Process(FlowRule m)
        {

            return new AppMessage();
        }



        [HttpPost("[action]")]
        public AppMessage Active(string id)
        {
            return new AppMessage();
        }


        private void findNext()
        {



        }

    }



   
}
