using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.FlowRuleEngine.Models;
using IoTSharp.FlowRuleEngine.Models.Task;
using IoTSharp.Models;
using IoTSharp.Models.Rule;
using IoTSharp.Models.Rule.Params;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Reflection;
using IoTSharp.App_Code.Util.Extensions;
using IoTSharp.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.EntityFrameworkCore;


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
        public async Task<AppMessage> Index([FromBody] RulePageParam m)
        {


            var profile = await this.GetUserProfile();

            Expression<Func<FlowRule, bool>> expression = x => x.RuleStatus > -1;
            if (!string.IsNullOrEmpty(m.Name))
            {
                expression = expression.And(x => x.Name.Contains(m.Name));
            }

            if (m.CreatTime != null && m.CreatTime.Length == 2)
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
                    rows = await _context.FlowRules.OrderByDescending(c => c.RuleId).Where(expression).Skip(m.offset * m.limit).Take(m.limit)
                        .ToListAsync(),
                    totel = await _context.FlowRules.CountAsync(expression)
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
                return new AppMessage { ErrType = ErrType.正常返回, };
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




        [HttpPost("[action]")]
        public async Task<AppMessage> SaveDiagram(ModelWorkFlow m)
        {
            //    var user = _userManager.GetUserId(User);
            var profile = await this.GetUserProfile();
            var activity = JsonConvert.DeserializeObject<IoTSharp.Models.Rule.Activity>(m.Biz);

            var rule = _context.FlowRules.FirstOrDefault(c => c.RuleId == activity.RuleId);

            rule.DefinitionsXml = m.Xml;
            rule.Creator = profile.Id.ToString();
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
                    NodeProcessScript = c.BizObject.flowscript,
                    NodeProcessScriptType = c.BizObject.flowscripttype

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
                    TargetId = c.targetId,
                    Conditionexpression = c.BizObject.conditionexpression,
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
                    NodeProcessClass = c.BizObject.NodeProcessClass,
                    NodeProcessScript = c.BizObject.flowscript,
                    NodeProcessScriptType = c.BizObject.flowscripttype
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

            return new AppMessage
            { ErrMessage = "操作成功", ErrType = ErrType.正常返回, IsVisble = true, ErrLevel = ErrLevel.Success };
        }



        [HttpGet("[action]")]
        public AppMessage GetDiagram(long id)
        {

            var ruleflow = _context.FlowRules.FirstOrDefault(c => c.RuleId == id);
            IoTSharp.Models.Rule.Activity activity = new IoTSharp.Models.Rule.Activity();

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
                                    flowscript = item.NodeProcessScript,
                                    flowscripttype = item.NodeProcessScriptType
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
                                    flowscript = item.NodeProcessScript,
                                    flowscripttype = item.NodeProcessScriptType
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
                                    flowscript = item.NodeProcessScript,
                                    flowscripttype = item.NodeProcessScriptType
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
                                    flowscript = item.NodeProcessScript,
                                    flowscripttype = item.NodeProcessScriptType
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
                                    flowscript = item.NodeProcessScript,
                                    flowscripttype = item.NodeProcessScriptType
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
                                    flowscript = item.NodeProcessScript,
                                    flowscripttype = item.NodeProcessScriptType
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
                    Xml = ruleflow.DefinitionsXml?.Trim('\r'),
                    Biz = activity
                }
            };
        }

        private async System.Threading.Tasks.Task Process(Flow flow, List<Flow> allflow, object data, int nextstep, long _eventid)
        {

            switch (flow.FlowType)
            {
                case "bpmn:SequenceFlow":
                    var t = allflow.FirstOrDefault(c => c.bpmnid == flow.TargetId);
                    _context.FlowOperations.Add(new FlowOperation()
                    {
                        AddDate = DateTime.Now,
                        RuleId = flow.RuleId,
                        FlowId = flow.FlowId,
                        Data = JsonConvert.SerializeObject(data),
                        NodeStatus = 1,
                        OperationDesc = "执行条件（" + (string.IsNullOrEmpty(flow.Conditionexpression)?"空条件": flow.Conditionexpression)+")",
                        Step = nextstep,
                        bpmnid = flow.bpmnid,
                        EventId = _eventid
                    });
                    await _context.SaveChangesAsync();
                    await Process(t, allflow, data, ++nextstep, _eventid);
                    break;
                case "bpmn:Task":
                    {
                        var flows = allflow.Where(c => c.SourceId == flow.bpmnid).ToList();
                        var tasks = new BaseRuleTask()
                        {
                            Name = flow.Flowname,
                            Eventid = flow.bpmnid,
                            id = flow.bpmnid,

                            outgoing = new EditableList<BaseRuleFlow>()
                        };
                        _context.FlowOperations.Add(new FlowOperation()
                        {
                            bpmnid = flow.bpmnid,
                            AddDate = DateTime.Now,
                            RuleId = flow.RuleId,
                            FlowId = flow.FlowId,
                            Data = JsonConvert.SerializeObject(data),
                            NodeStatus = 1,
                            OperationDesc = "执行任务" + flow.Flowname,
                            Step = nextstep,
                            EventId = _eventid
                        });
                        await _context.SaveChangesAsync();
                        nextstep++;

                        var emptyflow = flows.Where(c => c.Conditionexpression == string.Empty).ToList();

                        foreach (var item in flows.Except(emptyflow))
                        {
                            var rule = new BaseRuleFlow();
                            rule.Expression = item.Conditionexpression;
                            rule.id = item.bpmnid;
                            rule.Name = item.Flowname;
                            rule.Eventid = item.bpmnid;
                            tasks.outgoing.Add(rule);
                        }


                        if (tasks.outgoing.Count > 0)
                        {
                            SimpleFLowExcutor fLowExcutor = new SimpleFLowExcutor();
                            var result = await fLowExcutor.Excute(new FlowExcuteEntity()
                            {
                            //    Action = null,
                                Params = data,
                                Task = tasks,
                             //   WaitTime = 0

                            });
                            var next = result.Where(c => c.IsSuccess).ToList();
                            foreach (var item in next)
                            {

                                var nextflow = allflow.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                                emptyflow.Add(nextflow);
                            }
                        }
                       

                        foreach (var item in emptyflow)
                        {
                            await Process(item, allflow, data, nextstep, _eventid);
                        }
                    }

                    break;
                case "bpmn:EndEvent":
                    // 合并结束
                  var end=  _context.FlowOperations.FirstOrDefault(c => c.bpmnid == flow.bpmnid&&c.EventId==_eventid)??new FlowOperation();


                    end.bpmnid = flow.bpmnid;
                    end.AddDate = DateTime.Now;
                    end.RuleId = flow.RuleId;
                    end.FlowId = flow.FlowId;
                    end.Data = JsonConvert.SerializeObject(data);
                    end.NodeStatus = 1;
                    end.OperationDesc = "处理完成";
                    end.Step = _context.FlowOperations.Where(c =>  c.EventId == _eventid).Max(c=>c.Step)+1;
                    end.EventId = _eventid;

                    if (end.OperationId > 0)
                    {
                        _context.FlowOperations.Update(end);
                    }
                    else
                    {
                        _context.FlowOperations.Add(end);
                    }
                    await _context.SaveChangesAsync();

                    break;
                case "bpmn:StartEvent":

                    {
                        var flows = allflow.Where(c => c.SourceId == flow.bpmnid).ToList();
                        var tasks = new BaseRuleTask()
                        {
                            Name = flow.Flowname,
                            Eventid = flow.bpmnid,
                            id = flow.bpmnid,

                            outgoing = new EditableList<BaseRuleFlow>()
                        };
                        _context.FlowOperations.Add(new FlowOperation()
                        {
                            bpmnid = flow.bpmnid,
                            AddDate = DateTime.Now,
                            RuleId = flow.RuleId,
                            FlowId = flow.FlowId,
                            Data = JsonConvert.SerializeObject(data),
                            NodeStatus = 1,
                            OperationDesc = "开始处理",
                            Step = nextstep,
                            EventId = _eventid
                        });
                        await _context.SaveChangesAsync(); 
                        var emptyflow = flows.Where(c => c.Conditionexpression == string.Empty).ToList();

                        foreach (var item in flows.Except(emptyflow))
                        {
                            var rule = new BaseRuleFlow();

                            rule.id = item.bpmnid;
                            rule.Name = item.bpmnid;
                            rule.Eventid = item.bpmnid;
                            rule.Expression = item.Conditionexpression;
                            tasks.outgoing.Add(rule);
                        }

                        if (tasks.outgoing.Count > 0)
                        {
                            SimpleFLowExcutor fLowExcutor = new SimpleFLowExcutor();
                            var result = await fLowExcutor.Excute(new FlowExcuteEntity()
                            {
                              //  Action = null,
                                Params = data,
                                Task = tasks,
                             //   WaitTime = 0

                            });

                            var next = result.Where(c => c.IsSuccess).ToList();


                    
                            foreach (var item in next)
                            {

                                var nextflow = allflow.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                                emptyflow.Add(nextflow);

                            }

                          
                        }
                        nextstep++;
                        foreach (var item in emptyflow)
                        {
                            await Process(item, allflow, data, nextstep, _eventid);
                        }
                    }
                    break;
                case "label":

                    break;
                case "bpmn:Lane":

                    break;

                case "bpmn:Participant":

                    break;

                case "bpmn:DataStoreReference":

                    break;

                case "bpmn:SubProcess":

                    break;
                default:
                    {
                        var flows = allflow.Where(c => c.SourceId == flow.bpmnid).ToList();
                        var tasks = new BaseRuleTask()
                        {
                            Name = flow.Flowname,
                            Eventid = flow.bpmnid,
                            id = flow.bpmnid,

                            outgoing = new EditableList<BaseRuleFlow>()
                        };
                        _context.FlowOperations.Add(new FlowOperation()
                        {
                            bpmnid = flow.bpmnid,
                            AddDate = DateTime.Now,
                            RuleId = flow.RuleId,
                            FlowId = flow.FlowId,
                            Data = JsonConvert.SerializeObject(data),
                            NodeStatus = 1,
                            OperationDesc = "执行任务" + flow.Flowname,
                            Step = nextstep,
                            EventId = _eventid
                        });
                        await _context.SaveChangesAsync();
                        nextstep++;
                        var emptyflow = flows.Where(c => c.Conditionexpression == string.Empty).ToList();
                        foreach (var item in flows.Except(emptyflow))
                        {
                            var rule = new BaseRuleFlow();
                            rule.Expression = item.Conditionexpression;
                            rule.id = item.bpmnid;
                            rule.Name = item.Flowname;
                            rule.Eventid = item.bpmnid;
                            tasks.outgoing.Add(rule);
                        }


                        if (tasks.outgoing.Count > 0)
                        {
                            SimpleFLowExcutor fLowExcutor = new SimpleFLowExcutor();
                            var result = await fLowExcutor.Excute(new FlowExcuteEntity()
                            {
                                //  Action = null,
                                Params = data,
                                Task = tasks,
                             //   WaitTime = 0

                            });
                            var next = result.Where(c => c.IsSuccess).ToList();
                            foreach (var item in next)
                            {

                                var nextflow = allflow.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                                emptyflow.Add(nextflow);


                            }
                        }

                        foreach (var item in emptyflow)
                        {
                            await Process(item, allflow, data, nextstep, _eventid);
                        }

                        break;
                    }



            }

        }



        [HttpPost("[action]")]
        public async Task<AppMessage> Active([FromBody] JObject form)
        {

            var profile = await this.GetUserProfile();
            var formdata = form.First.First;
            var extradata = form.First.Next;
            var obj = extradata.First.First.First.Value<JToken>();
            var obj1 = extradata.First.First.Next.First.Value<JToken>();
            var formid = obj.Value<int>();
            var ruleid = obj1.Value<int>();
            //wrong way
            //   var _form = _context.DynamicFormInfos.FirstOrDefault(c => c.FormId == formid);
            //   object data = formdata.ToObject(BuildPocoObject(_form.ModelClass, "FormData"+ _form.FormId));
            //  var _params = _context.DynamicFormFieldInfos.Where(c => c.FormId == formid).ToList();
            var d = formdata.ToObject(typeof(ExpandoObject));

            var _event = new BaseEvent()
            {
                CreaterDateTime = DateTime.Now,
                Creator = profile.Id,
                EventDesc = "测试",
                EventName = "测试",
                MataData = JsonConvert.SerializeObject(d),
                RuleId = ruleid,
                Bizid = formid.ToString(),
                Type = EventType.TestPurpose,
                EventStaus = 1
            };
            _context.BaseEvents.Add(_event);
            _context.SaveChanges();
            // 非测试环境事件产生完，此时应当返回事件，剩余处理下一步进行异步处理。




            var flows = _context.Flows.Where(c => c.RuleId == ruleid && c.FlowType != "label").ToList();
            var start = flows.FirstOrDefault(c => c.FlowType == "bpmn:StartEvent");
            //    var end = flows.FirstOrDefault(c => c.FlowType == "bpmn:EndEvent");
            await Process(start, flows, d, 1, _event.EventId);



            //应该由事件总线去通知
            return new AppMessage
            {
                ErrType = ErrType.正常返回,
                Result = _context.FlowOperations.OrderBy(c => c.Step).
                Where(c => c.EventId == _event.EventId).ToList()
                .GroupBy(c => c.Step).Select(c => new
                {
                    Step = c.Key,
                    Nodes = c
                }).ToList()
            };
        }


     

        private Type BuildPocoObject(string classtext, string typename)
        {
            MetadataReference[] references = {


            };


            var tree = SyntaxFactory.ParseSyntaxTree(classtext);
            CSharpCompilation compilation = CSharpCompilation.Create(typename, new[] { tree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var ms = new MemoryStream();
            EmitResult result = compilation.Emit(ms);
            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);
                foreach (Diagnostic diagnostic in failures)
                {

                }
                return null;
            }
            else
            {
                ms.Seek(0, SeekOrigin.Begin);
                return Assembly.Load(ms.ToArray()).DefinedTypes.FirstOrDefault();
            }

        }

    }




}
