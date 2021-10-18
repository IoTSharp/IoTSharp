using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using IoTSharp.Models;
using IoTSharp.Models.Rule;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class RulesController : ControllerBase
    {
        private ApplicationDbContext _context;
        private readonly FlowRuleProcessor _flowRuleProcessor;
        private UserManager<IdentityUser> _userManager;

        public RulesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, FlowRuleProcessor flowRuleProcessor)
        {
            this._userManager = userManager;
            this._context = context;
            _flowRuleProcessor = flowRuleProcessor;
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
        public AppMessage Delete(Guid id)
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
        public AppMessage<FlowRule> Get(Guid id)
        {
            var rule = _context.FlowRules.FirstOrDefault(c => c.RuleId == id);
            if (rule != null)
            {
                return new AppMessage<FlowRule> { ErrType = ErrType.正常返回, Result = rule };
            }

            return new AppMessage<FlowRule> { ErrType = ErrType.找不到对象, };
        }

        [HttpPost("[action]")]
        public async Task<AppMessage<FlowRule>> BindDevice(ModelRuleBind m)
        {
            var profile = await this.GetUserProfile();
            if (m.dev != null)

            {
                m.dev.ToList().ForEach(d =>
                {
                    if (!_context.DeviceRules.Any(c => c.FlowRule.RuleId == m.rule && c.Device.Id == d))
                    {
                        var dr = new DeviceRule();
                        dr.Device = _context.Device.SingleOrDefault(c => c.Id == d);
                        dr.FlowRule = _context.FlowRules.SingleOrDefault(c => c.RuleId == m.rule);
                        dr.ConfigDateTime = DateTime.Now;
                        dr.ConfigUser = profile.Id;
                        _context.DeviceRules.Add(dr);
                        _context.SaveChanges();
                    }
                });
                return new AppMessage<FlowRule> { ErrType = ErrType.正常返回, ErrMessage = "规则已下发" };
            }
            //var rule = _context.FlowRules.FirstOrDefault(c => c.RuleId == id);
            //if (rule != null)
            //{
            //    return new AppMessage<FlowRule> { ErrType = ErrType.正常返回, Result = rule };
            //}

            return new AppMessage<FlowRule> { ErrType = ErrType.参数错误, ErrMessage = "请选择下发设备" };
        }

        [HttpGet("[action]")]
        public async Task<AppMessage<FlowRule>> DeleteDeviceRules(Guid deviceId, Guid ruleId)
        {
            var profile = await this.GetUserProfile();

            var map = _context.DeviceRules.FirstOrDefault(c => c.FlowRule.RuleId == ruleId && c.Device.Id == deviceId);

            if (map != null)

            {
                _context.DeviceRules.Remove(map);
                _context.SaveChanges();
                return new AppMessage<FlowRule> { ErrType = ErrType.正常返回, ErrMessage = "规则绑定已删除" };
            }
            //var rule = _context.FlowRules.FirstOrDefault(c => c.RuleId == id);
            //if (rule != null)
            //{
            //    return new AppMessage<FlowRule> { ErrType = ErrType.正常返回, Result = rule };
            //}

            return new AppMessage<FlowRule> { ErrType = ErrType.参数错误, ErrMessage = "规则绑定不存在或已删除" };
        }

        [HttpGet("[action]")]
        public async Task<AppMessage<List<FlowRule>>> GetDeviceRules(Guid deviceId)
        {
            return new AppMessage<List<FlowRule>> { ErrType = ErrType.正常返回, Result = await _context.DeviceRules.Where(c => c.Device.Id == deviceId).Select(c => c.FlowRule).ToListAsync() };
        }

        [HttpGet("[action]")]
        public async Task<AppMessage<List<Device>>> GetRuleDevices(Guid ruleId)
        {
            return new AppMessage<List<Device>> { ErrType = ErrType.正常返回, Result = await _context.DeviceRules.Where(c => c.FlowRule.RuleId == ruleId).Select(c => c.Device).ToListAsync() };
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
            _context.Flows.RemoveRange(_context.Flows.Where(c => c.FlowRule == rule).ToList());
            _context.SaveChanges();

            if (activity.BaseBpmnObjects != null)
            {
                _context.Flows.AddRange(activity.BaseBpmnObjects.Select(c => new Flow
                {
                    FlowRule = rule,
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
                    FlowRule = rule,
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
                    FlowRule = rule,
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
                    FlowRule = rule,
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
                    FlowRule = rule,
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
                    FlowRule = rule,
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
                    FlowRule = rule,
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
                    FlowRule = rule,
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
                    FlowRule = rule,
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
                    FlowRule = rule,
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
                    FlowRule = rule,
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
                    FlowRule = rule,
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
                    FlowRule = rule,
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
        public AppMessage GetDiagram(Guid id)
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
            var flows = _context.Flows.Where(c => c.FlowRule.RuleId == id).ToList();

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

        //模拟处理

        [HttpPost("[action]")]
        public async Task<AppMessage> Active([FromBody] JObject form)
        {
            var profile = await this.GetUserProfile();
            var formdata = form.First.First;
            var extradata = form.First.Next;
            var obj = extradata.First.First.First.Value<JToken>();
            var obj1 = extradata.First.First.Next.First.Value<JToken>();
            var formid = obj.Value<int>();
            var ruleid = obj1.Value<Guid>();
            //wrong way
            //   var _form = _context.DynamicFormInfos.FirstOrDefault(c => c.FormId == formid);
            //   object data = formdata.ToObject(BuildPocoObject(_form.ModelClass, "FormData"+ _form.FormId));
            //  var _params = _context.DynamicFormFieldInfos.Where(c => c.FormId == formid).ToList();
            var d = formdata.ToObject(typeof(ExpandoObject));

            var rule = _context.FlowRules.SingleOrDefault(c => c.RuleId == ruleid);
            var _event = new BaseEvent()
            {
                CreaterDateTime = DateTime.Now,
                Creator = profile.Id,
                EventDesc = "测试",
                EventName = "测试",
                MataData = JsonConvert.SerializeObject(d),
                //   FlowRule = rule,
                Bizid = formid.ToString(),
                Type = EventType.TestPurpose,
                EventStaus = 1
            };
            _context.BaseEvents.Add(_event);
            _context.SaveChanges();
            // 非测试环境事件产生完，此时应当返回事件，剩余处理下一步进行异步处理。

            await _flowRuleProcessor.RunFlowRules(ruleid, d, Guid.Parse(this._userManager.GetUserId(this.User)), null);

            //应该由事件总线去通知
            return new AppMessage
            {
                ErrType = ErrType.正常返回,
                Result = _context.FlowOperations.OrderBy(c => c.Step).
                Where(c => c.BaseEvent == _event).ToList()
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