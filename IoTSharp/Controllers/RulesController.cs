using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using IoTSharp.Models;
using IoTSharp.Models.Rule;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
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
        private readonly TaskExecutorHelper _helper;
        private UserManager<IdentityUser> _userManager;

        public RulesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, FlowRuleProcessor flowRuleProcessor, TaskExecutorHelper helper)
        {
            this._userManager = userManager;
            this._context = context;
            _flowRuleProcessor = flowRuleProcessor;
            _helper = helper;
        }

        /// <summary>
        /// 更新节点的条件表达式
        /// </summary>
        /// <returns> </returns>
        ///

        [HttpPost("[action]")]
        public async Task<ApiResult<bool>> UpdateFlowExpression(UpdateFlowExpression m)
        {
            var profile = this.GetUserProfile();
            var flow = await _context.Flows.SingleOrDefaultAsync(c => c.FlowId == m.FlowId && c.Tenant.Id == profile.Tenant);
            if (flow != null)
            {
                flow.Conditionexpression = m.Expression;
                _context.Flows.Update(flow);
                await _context.SaveChangesAsync();

                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            return new ApiResult<bool>(ApiCode.InValidData, "can't find this object", false);
        }

        [HttpPost("[action]")]
        public ApiResult<PagedData<FlowRule>> Index([FromBody] RulePageParam m)
        {
            var profile = this.GetUserProfile();

            Expression<Func<FlowRule, bool>> condition = x => x.RuleStatus > -1 && x.Tenant.Id == profile.Tenant;
            if (!string.IsNullOrEmpty(m.Name))
            {
                condition = condition.And(x => x.Name.Contains(m.Name));
            }

            if (m.CreatTime != null && m.CreatTime.Length == 2)
            {
                condition = condition.And(x => x.CreatTime > m.CreatTime[0] && x.CreatTime < m.CreatTime[1]);
            }

            if (!string.IsNullOrEmpty(m.Creator))
            {
                condition = condition.And(x => x.Creator == m.Creator);
            }

            return new ApiResult<PagedData<FlowRule>>(ApiCode.Success, "OK", new PagedData<FlowRule>
            {
                total = _context.FlowRules.Count(condition),
                rows = _context.FlowRules.OrderByDescending(c => c.CreatTime).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList()
            });
        }

        [HttpPost("[action]")]
        public async Task<ApiResult<bool>> Save(FlowRule m)
        {
            var profile = this.GetUserProfile();
            try
            {
                m.MountType = m.MountType;
                m.RuleStatus = 1;
                _context.JustFill(this, m);
                m.CreatTime = DateTime.Now;
                _context.FlowRules.Add(m);
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            catch (Exception ex)
            {
                return new ApiResult<bool>(ApiCode.Exception, ex.Message, false);
            }
        }

        [HttpPost("[action]")]
        public async Task<ApiResult<bool>> Update(FlowRule m)
        {
            var profile = this.GetUserProfile();
            var flowrule = _context.FlowRules.SingleOrDefault(c => c.RuleId == m.RuleId && c.Tenant.Id == profile.Tenant);
            if (flowrule != null)
            {
                try
                {
                    flowrule.MountType = m.MountType;
                    flowrule.Name = m.Name;
                    flowrule.RuleDesc = m.RuleDesc;
                    _context.FlowRules.Update(flowrule);
                    await _context.SaveChangesAsync();
                    return new ApiResult<bool>(ApiCode.Success, "OK", true);
                }
                catch (Exception ex)
                {
                    return new ApiResult<bool>(ApiCode.Exception, ex.Message, false);
                }
            }
            return new ApiResult<bool>(ApiCode.Success, "can't find this object", false);
        }

        [HttpGet("[action]")]
        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            var profile = this.GetUserProfile();
            var rule = _context.FlowRules.SingleOrDefault(c => c.RuleId == id && c.Tenant.Id == profile.Tenant);
            if (rule != null)
            {
                try
                {
                    rule.RuleStatus = -1;
                    _context.FlowRules.Update(rule);
                    await _context.SaveChangesAsync();
                    return new ApiResult<bool>(ApiCode.Success, "OK", true);
                }
                catch (Exception ex)
                {
                    return new ApiResult<bool>(ApiCode.Exception, ex.Message, false);
                }
            }

            return new ApiResult<bool>(ApiCode.Success, "can't find this object", false);
        }

        [HttpGet("[action]")]
        public ApiResult<FlowRule> Get(Guid id)
        {
            var profile = this.GetUserProfile();
            var rule = _context.FlowRules.SingleOrDefault(c => c.RuleId == id && c.Tenant.Id == profile.Tenant);
            if (rule != null)
            {
                return new ApiResult<FlowRule>(ApiCode.Success, "OK", rule);
            }

            return new ApiResult<FlowRule>(ApiCode.CantFindObject, "can't find this object", null);
        }

        /// <summary>
        /// 复制一个规则副本
        /// </summary>
        /// <param name="flowRule"></param>
        /// <returns></returns>

        [HttpPost("[action]")]
        public async Task<ApiResult<bool>> Fork(FlowRule flowRule)
        {
            var profile = this.GetUserProfile();
            var rule = await _context.FlowRules.SingleOrDefaultAsync(c => c.RuleId == flowRule.RuleId && c.Tenant.Id == profile.Tenant);
            if (rule != null)
            {
                var newrule = new FlowRule();
                newrule.DefinitionsXml = rule.DefinitionsXml;
                newrule.Describes = flowRule.Describes;
                //     newrule.Creator = profile.Id.ToString();
                newrule.Name = flowRule.Name;
                newrule.CreatTime = DateTime.Now;
                newrule.ExecutableCode = rule.ExecutableCode;
                newrule.RuleDesc = flowRule.RuleDesc;
                newrule.RuleStatus = 1;
                newrule.MountType = flowRule.MountType;
                newrule.ParentRuleId = rule.RuleId;
                newrule.CreateId = new Guid();
                newrule.SubVersion = rule.SubVersion + 0.01;
                newrule.Runner = rule.Runner;
                _context.FlowRules.Add(newrule);
                await _context.SaveChangesAsync();

                var flows = _context.Flows.Where(c => c.FlowRule.RuleId == rule.RuleId && c.CreateId == rule.CreateId).ToList();
                var newflows = flows.Select(c => new Flow()
                {
                    FlowRule = newrule,
                    Conditionexpression = c.Conditionexpression,
                    CreateDate = DateTime.Now,
                    FlowStatus = 1,
                    FlowType = c.FlowType,
                    Flowdesc = c.Flowdesc,
                    Incoming = c.Incoming,
                    Flowname = c.Flowname,
                    NodeProcessClass = c.NodeProcessClass,
                    NodeProcessMethod = c.NodeProcessMethod,
                    NodeProcessParams = c.NodeProcessParams,
                    NodeProcessScript = c.NodeProcessScript,
                    NodeProcessScriptType = c.NodeProcessScriptType,
                    NodeProcessType = c.NodeProcessType,
                    ObjectId = c.ObjectId,
                    Outgoing = c.Outgoing,
                    SourceId = c.SourceId,
                    TargetId = c.TargetId,

                    bpmnid = c.bpmnid,
                    CreateId = newrule.CreateId
                }).ToList();
                if (newflows.Count > 0)
                {
                    _context.Flows.AddRange(newflows);
                    await _context.SaveChangesAsync();
                }

                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            else
            {
            }

            return new ApiResult<bool>(ApiCode.CantFindObject, "can't find this object", false);
        }

        [HttpPost("[action]")]
        public ApiResult<bool> BindDevice(ModelRuleBind m)
        {
            var profile = this.GetUserProfile();
            if (m.dev != null)

            {
                foreach (var d in m.dev.ToList())
                {
                    if (!_context.DeviceRules.Any(c => c.FlowRule.RuleId == m.rule && c.Device.Id == d))
                    {
                        var dev = _context.Device.SingleOrDefault(c => c.Id == d && c.Tenant.Id == profile.Tenant);
                        var rule = _context.FlowRules.SingleOrDefault(c =>
                            c.RuleId == m.rule && c.Tenant.Id == profile.Tenant);
                        if (dev != null)
                        {
                            if (rule != null)
                            {
                                var dr = new DeviceRule();
                                dr.Device = dev;
                                dr.FlowRule = rule;
                                dr.ConfigDateTime = DateTime.Now;
                                dr.ConfigUser = profile.Id;
                                _context.DeviceRules.Add(dr);
                            }
                            else
                            {
                                return new ApiResult<bool>(ApiCode.CantFindObject, "can not found that rule:" + m.rule, false);
                            }
                        }
                        else
                        {
                            return new ApiResult<bool>(ApiCode.CantFindObject, "can not found that device:" + d, false);
                        }
                    }
                }
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.Success, "rule binding success", true);
            }

            return new ApiResult<bool>(ApiCode.CantFindObject, "No device found", false);
        }

        [HttpGet("[action]")]
        public async Task<ApiResult<bool>> DeleteDeviceRules(Guid deviceId, Guid ruleId)
        {
            var profile = this.GetUserProfile();
            var map = await _context.DeviceRules.Include(c => c.Device)
                .Include(c => c.FlowRule).FirstOrDefaultAsync(c => c.FlowRule.RuleId == ruleId && c.Device.Id == deviceId && c.Device.Tenant.Id == profile.Tenant && c.FlowRule.Tenant.Id == profile.Tenant);
            if (map != null)
            {
                _context.DeviceRules.Remove(map);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.Success, "rule has been removed", true);
            }
            return new ApiResult<bool>(ApiCode.CantFindObject, "this mapping was not found", true);
        }

        [HttpGet("[action]")]
        public ApiResult<List<FlowRule>> GetDeviceRules(Guid deviceId)
        {
            var profile = this.GetUserProfile();
            return new ApiResult<List<FlowRule>>(ApiCode.Success, "Ok", _context.DeviceRules.Include(c => c.Device).Where(c => c.Device.Id == deviceId && c.Device.Tenant.Id == profile.Tenant).Select(c => c.FlowRule).Select(c => new FlowRule() { RuleId = c.RuleId, CreatTime = c.CreatTime, Name = c.Name, RuleDesc = c.RuleDesc }).ToList());
        }

        [HttpGet("[action]")]
        public ApiResult<List<Device>> GetRuleDevices(Guid ruleId)
        {
            var profile = this.GetUserProfile();
            return new ApiResult<List<Device>>(ApiCode.Success, "Ok", _context.DeviceRules.Include(c => c.FlowRule).Where(c => c.FlowRule.RuleId == ruleId && c.FlowRule.Tenant.Id == profile.Tenant).Select(c => c.Device).ToList());
        }

        [HttpGet("[action]")]
        public ApiResult<List<Flow>> GetFlows(Guid ruleId)
        {
            var tid = User.GetTenantId();
            return new ApiResult<List<Flow>>(ApiCode.Success, "Ok",
                _context.Flows.Include(c => c.FlowRule)
                .Where(c => c.FlowRule.RuleId == ruleId && c.FlowStatus > 0 && c.Tenant.Id == tid).ToList());
        }

        [HttpPost("[action]")]
        public ApiResult<bool> SaveDiagram(ModelWorkFlow m)
        {
            var profile = this.GetUserProfile();
            var activity = JsonConvert.DeserializeObject<Activity>(m.Biz);
            var CreatorId = Guid.NewGuid();
            var CreateDate = DateTime.Now;
            var rule = _context.FlowRules.Include(c=>c.Customer).Include(c=>c.Tenant).FirstOrDefault(c => c.RuleId == activity.RuleId);
            rule.DefinitionsXml = m.Xml;
            rule.Creator = profile.Id.ToString();
            rule.CreateId = CreatorId;
            _context.Flows.Where(c => c.FlowRule.RuleId == rule.RuleId).ForEach(c =>
            {
                c.FlowStatus = -1;
            });
            _context.FlowRules.Update(rule);
            _context.SaveChanges();
            if (activity.BaseBpmnObjects != null)
            {
                var fw = activity.BaseBpmnObjects.Select(c => new Flow
                {
                    FlowRule = rule,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,
                    NodeProcessScript = c.BizObject.flowscript,
                    NodeProcessScriptType = c.BizObject.flowscripttype,
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Createor = profile.Id,
                    CreateDate = CreateDate,
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                });
      
                _context.Flows.AddRange(fw);
                _context.SaveChanges();
            }

            if (activity.StartEvents != null)
            {
                var fw = activity.StartEvents.Select(c => new Flow
                {
                    FlowRule = rule,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Createor = profile.Id,
                    CreateDate = CreateDate,
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                });
               
                _context.Flows.AddRange(fw);
                _context.SaveChanges();
            }

            if (activity.EndEvents != null)
            {
                var fw = activity.EndEvents.Select(c => new Flow
                {
                    FlowRule = rule,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Createor = profile.Id,
                    CreateDate = CreateDate,
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                });
           
                _context.Flows.AddRange(fw);
                _context.SaveChanges();
            }

            if (activity.SequenceFlows != null)
            {
                var fw = activity.SequenceFlows.Select(c => new Flow
                {
                    FlowRule = rule,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,
                    SourceId = c.sourceId,
                    TargetId = c.targetId,
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Conditionexpression = c.BizObject.conditionexpression,
                    NodeProcessParams = c.BizObject.NodeProcessParams,
                    Createor = profile.Id,
                    CreateDate = CreateDate,
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                });
         
                _context.Flows.AddRange(fw);
                _context.SaveChanges();
            }

            if (activity.Tasks != null)
            {
                var fw = activity.Tasks.Select(c => new Flow
                {
                    FlowRule = rule,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,
                    NodeProcessParams = c.BizObject.NodeProcessParams,
                    NodeProcessClass = c.BizObject.NodeProcessClass,
                    NodeProcessScript = c.BizObject.flowscript,
                    NodeProcessScriptType = c.BizObject.flowscripttype,
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Createor = profile.Id,
                    CreateDate = CreateDate,
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                });
           
                _context.Flows.AddRange(fw);
                _context.SaveChanges();
            }

            if (activity.DataInputAssociations != null)
            {
                var fw = activity.DataInputAssociations.Select(c => new Flow
                {
                    FlowRule = rule,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Createor = profile.Id,
                    CreateDate = CreateDate,
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                });
      
                _context.Flows.AddRange(fw);
                _context.SaveChanges();
            }

            if (activity.DataOutputAssociations != null)
            {
                var fw = activity.DataOutputAssociations.Select(c => new Flow
                {
                    FlowRule = rule,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Createor = profile.Id,
                    CreateDate = CreateDate,
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                });
            
                _context.Flows.AddRange(fw);
                _context.SaveChanges();
            }

            if (activity.TextAnnotations != null)
            {
                var fw = activity.TextAnnotations.Select(c => new Flow
                {
                    FlowRule = rule,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Createor = profile.Id,
                    CreateDate = CreateDate,
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                });
            
                _context.Flows.AddRange(fw);
                _context.SaveChanges();
            }

            if (activity.Containers != null)
            {
                var fw = activity.Containers.Select(c => new Flow
                {
                    FlowRule = rule,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Createor = profile.Id,
                    CreateDate = CreateDate,
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                });
           
                _context.Flows.AddRange(fw);
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
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Createor = profile.Id,
                    CreateDate = CreateDate,
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                }).ToList());
                _context.SaveChanges();
            }

            if (activity.DataStoreReferences != null)
            {
                var fw = activity.DataStoreReferences.Select(c => new Flow
                {
                    FlowRule = rule,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Createor = profile.Id,
                    CreateDate = CreateDate,
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                });
         
                _context.Flows.AddRange(fw);
                _context.SaveChanges();
            }

            if (activity.Lane != null)
            {
                var fw = activity.Lane.Select(c => new Flow
                {
                    FlowRule = rule,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Createor = profile.Id,
                    CreateDate = CreateDate,
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                });
             
                _context.Flows.AddRange(fw);
                _context.SaveChanges();
            }

            if (activity.LaneSet != null)
            {
                var fws = activity.LaneSet.Select(c => new Flow
                {
                    FlowRule = rule,
                    Flowname = c.BizObject.Flowname,
                    bpmnid = c.id,
                    FlowType = c.bpmntype,
                    FlowStatus = 1,
                    CreateId = CreatorId,
                    Createor = profile.Id,
                    CreateDate = CreateDate  ,    
                    Customer = rule.Customer,
                    Tenant = rule.Tenant
                });
             
                _context.Flows.AddRange(fws);
                _context.SaveChanges();
            }
            return new ApiResult<bool>(ApiCode.Success, "Ok", true);
        }

        [HttpGet("[action]")]
        public async Task<ApiResult<Activity>> GetDiagram(Guid id)
        {
            var profile = this.GetUserProfile();
            var ruleflow = await _context.FlowRules.FirstOrDefaultAsync(c => c.RuleId == id && c.Tenant.Id == profile.Tenant);
            Activity activity = new Activity();

            activity.SequenceFlows ??= new List<SequenceFlow>();
            activity.GateWays ??= new List<GateWay>();
            activity.Tasks ??= new List<IoTSharp.Models.Rule.BaseTask>();
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
            var flows = _context.Flows.Where(c => c.FlowRule.RuleId == id && c.FlowStatus > 0 && c.Tenant.Id == profile.Tenant).ToList();
            activity.Xml = ruleflow.DefinitionsXml?.Trim('\r');
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
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
                                    Flowname = item.Flowname,
                                    NodeProcessClass = item.NodeProcessClass
                                }
                            });
                        break;

                    case "bpmn:Task":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.BaseTask
                            {
                                id = item.bpmnid,
                                Flowname = item.Flowname,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType,
                                    Flowname = item.Flowname,
                                    flowscript = item.NodeProcessScript,
                                    flowscripttype = item.NodeProcessScriptType,
                                    NodeProcessClass = item.NodeProcessClass,
                                    NodeProcessParams = item.NodeProcessParams
                                }
                            });
                        break;

                    case "bpmn:BusinessRuleTask":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.BaseTask
                            {
                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType,
                                    Flowname = item.Flowname,
                                    flowscript = item.NodeProcessScript,
                                    flowscripttype = item.NodeProcessScriptType,
                                    NodeProcessClass = item.NodeProcessClass
                                }
                            });
                        break;

                    case "bpmn:ReceiveTask":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.BaseTask
                            {
                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType,
                                    Flowname = item.Flowname,
                                    NodeProcessClass = item.NodeProcessClass
                                }
                            });
                        break;

                    case "bpmn:UserTask":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.BaseTask
                            {
                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType,
                                    Flowname = item.Flowname,
                                    flowscript = item.NodeProcessScript,
                                    flowscripttype = item.NodeProcessScriptType,
                                    NodeProcessClass = item.NodeProcessClass
                                }
                            });
                        break;

                    case "bpmn:ServiceTask":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.BaseTask
                            {
                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType,
                                    Flowname = item.Flowname,
                                    flowscript = item.NodeProcessScript,
                                    flowscripttype = item.NodeProcessScriptType,
                                    NodeProcessClass = item.NodeProcessClass
                                }
                            });
                        break;

                    case "bpmn:ManualTask":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.BaseTask
                            {
                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType,
                                    Flowname = item.Flowname,
                                    flowscript = item.NodeProcessScript,
                                    flowscripttype = item.NodeProcessScriptType,
                                    NodeProcessClass = item.NodeProcessClass
                                }
                            });
                        break;

                    case "bpmn:SendTask":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.BaseTask
                            {
                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType,
                                    Flowname = item.Flowname,
                                    flowscript = item.NodeProcessScript,
                                    flowscripttype = item.NodeProcessScriptType,
                                    NodeProcessClass = item.NodeProcessClass
                                }
                            });
                        break;

                    case "bpmn:CallActivity":
                        activity.Tasks.Add(

                            new IoTSharp.Models.Rule.BaseTask
                            {
                                Flowname = item.Flowname,
                                id = item.bpmnid,
                                bpmntype = item.FlowType,
                                BizObject = new FormBpmnObject
                                {
                                    Flowid = item.FlowId.ToString(),
                                    Flowdesc = item.Flowdesc,
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
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
                                    Flowtype = item.FlowType,
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
                                Flowtype = item.FlowType,
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
            return new ApiResult<IoTSharp.Models.Rule.Activity>(ApiCode.Success, "rule has been removed", activity);
        }

        [HttpPost("[action]")]
        public async Task<ApiResult<dynamic>> Active([FromBody] JObject form)
        {
            var profile = this.GetUserProfile();
            var formdata = form.First.First;
            var extradata = form.First.Next;
            var obj = extradata.First.First.First.Value<JToken>();
            var __ruleid = obj.Value<string>();
            var ruleid = Guid.Parse(__ruleid);

            var d = formdata.Value<JToken>().ToObject(typeof(ExpandoObject));
            var testabizId = Guid.NewGuid().ToString(); //根据业务保存起来，用来查询执行事件和步骤
            var result = await _flowRuleProcessor.RunFlowRules(ruleid, d, Guid.Empty, EventType.TestPurpose, testabizId);


        
            var flowRule=_context.FlowRules.SingleOrDefault(c => c.RuleId == ruleid);

            var flows = _context.Flows.Where(c => c.FlowRule == flowRule).ToList();


            if (result.Count > 0)
            {
                await Task.Run(() =>
                {

                    var operevent =
                        _context.BaseEvents.SingleOrDefault(
                            c => c.EventId == result.FirstOrDefault().BaseEvent.EventId);

                    var list = result.Select(c => new FlowOperation
                    {
                        AddDate = c.AddDate,
                        BizId = c.BizId,
                        Data = c.Data, BaseEvent = operevent,
                        Flow = flows.SingleOrDefault(x=>x.FlowId==c.Flow.FlowId
                        ),
                        FlowRule = flowRule,
                        NodeStatus = c.NodeStatus,
                        OperationDesc = c.OperationDesc,
                        OperationId = new Guid(),
                        Step = c.Step,
                        Tag = c.Tag,
                        bpmnid = c.bpmnid
                    }).ToArray();
                    _context.FlowOperations.AddRange(list);
                    _context.SaveChanges();
                });
            }
            return new ApiResult<dynamic>(ApiCode.Success, "test complete", result.OrderBy(c => c.Step).
                Where(c => c.BaseEvent.Bizid == testabizId).ToList()
                .GroupBy(c => c.Step).Select(c => new
                {
                    Step = c.Key,
                    Nodes = c
                }).ToList());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>

        [HttpPost("[action]")]
        public async Task<ApiResult<PagedData<BaseEventDto>>> FlowEvents([FromBody] EventParam m)
        {
            var profile = this.GetUserProfile();
            Expression<Func<BaseEvent, bool>> condition = x => x.EventStaus > -1 && x.Tenant.Id == profile.Tenant;
            if (!string.IsNullOrEmpty(m.Name))
            {
                condition = condition.And(x => x.EventName.Contains(m.Name));
            }

            if (m.CreatTime != null && m.CreatTime.Length == 2)
            {
                condition = condition.And(x => x.CreaterDateTime > m.CreatTime[0] && x.CreaterDateTime < m.CreatTime[1]);
            }

            if (m.RuleId != null)
            {
                condition = condition.And(x => x.FlowRule.RuleId == m.RuleId);
            }

            if (m.Creator != null && m.Creator != Guid.Empty)
            {
                condition = condition.And(x => x.Creator == m.Creator.Value);
            }

            var result = _context.BaseEvents.OrderByDescending(c => c.CreaterDateTime).Where(condition)
                .Skip((m.offset) * m.limit).Take(m.limit).Select(c => new BaseEventDto
                {
                    Name = c.FlowRule.Name,
                    Bizid = c.Bizid,
                    CreaterDateTime = c.CreaterDateTime,
                    Creator = c.Creator,
                    EventDesc = c.EventDesc,
                    EventId = c.EventId,
                    EventStaus = c.EventStaus,
                    EventName = c.EventName,
                    MataData = c.MataData,
                    RuleId = c.FlowRule.RuleId,
                    Type = c.Type
                }).ToList();

            foreach (var item in result)
            {
                item.CreatorName = await GetCreatorName(item);
            }
            return new ApiResult<PagedData<BaseEventDto>>(ApiCode.Success, "OK", new PagedData<BaseEventDto>
            {
                total = _context.BaseEvents.Count(condition),
                rows = result
            });
        }

        private async Task<string> GetCreatorName(BaseEventDto dto)
        {
            if (dto.Type == EventType.Normal)
            {
                return _context.Device.SingleOrDefault(c => c.Id == dto.Creator)?.Name;
            }
            else
            {
                return (await _userManager.FindByIdAsync(dto.Creator.ToString()))?.UserName;
            }
        }

        [HttpGet("[action]")]
        public ApiResult<dynamic> GetFlowOperations(Guid eventId)
        {
            var profile = this.GetUserProfile();
            return new ApiResult<dynamic>(ApiCode.Success, "OK", _context.FlowOperations.Where(c => c.BaseEvent.EventId == eventId).ToList().OrderBy(c => c.Step).
              ToList()
                .GroupBy(c => c.Step).Select(c => new
                {
                    Step = c.Key,
                    Nodes = c
                }).ToList());
        }

        [HttpGet("[action]")]
        public ApiResult<dynamic> GetExecutors()
        {
            return new ApiResult<dynamic>(ApiCode.Success, "OK", _helper.GetTaskExecutorList().Select(c => new { label = c.Key, value = c.Value.FullName }).ToList());
        }

        [HttpPost("[action]")]
        public async Task<ApiResult<PagedData<RuleTaskExecutor>>> Executors(ExecutorParam m)
        {
            var profile = this.GetUserProfile();
            var rte = from x in _context.RuleTaskExecutors where x.ExecutorStatus > -1 && x.Tenant.Id == profile.Tenant orderby x.AddDateTime descending select x;
            var pd = new PagedData<RuleTaskExecutor>
            {
                total = await rte.CountAsync(),
                rows = await rte.Skip((m.offset) * m.limit).Take(m.limit).ToListAsync()
            };
            return new ApiResult<PagedData<RuleTaskExecutor>>(ApiCode.Success, "OK", pd);
        }

        [HttpGet("[action]")]
        public async Task<ApiResult<RuleTaskExecutor>> GetExecutor(Guid Id)
        {
            var profile = this.GetUserProfile();
            var executor = await _context.RuleTaskExecutors.SingleOrDefaultAsync(c => c.ExecutorId == Id && c.Tenant.Id == profile.Tenant);

            if (executor != null)
            {
                return new ApiResult<RuleTaskExecutor>(ApiCode.Success, "Ok", executor);
            }
            return new ApiResult<RuleTaskExecutor>(ApiCode.CantFindObject, "cant't find that object", null);
        }

        [HttpGet("[action]")]
        public async Task<ApiResult<bool>> DeleteExecutor(Guid Id)
        {
            var profile = this.GetUserProfile();
            var executor = await _context.RuleTaskExecutors.SingleOrDefaultAsync(c => c.ExecutorId == Id && c.Tenant.Id == profile.Tenant);

            if (executor != null)
            {
                executor.ExecutorStatus = -1;
                _context.RuleTaskExecutors.Update(executor);
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            return new ApiResult<bool>(ApiCode.CantFindObject, "cant't find that object", false);
        }

        [HttpPost("[action]")]
        public async Task<ApiResult<bool>> UpdateExecutor(RuleTaskExecutor m)
        {
            var profile = this.GetUserProfile();
            var executor = await _context.RuleTaskExecutors.SingleOrDefaultAsync(c => c.ExecutorId == m.ExecutorId && c.Tenant.Id == profile.Tenant);
            if (executor != null)
            {
                executor.DefaultConfig = m.DefaultConfig;
                executor.ExecutorDesc = m.ExecutorDesc;
                executor.ExecutorName = m.ExecutorName;
                executor.TypeName = m.ExecutorName;
                executor.Path = m.Path;
                executor.Tag = m.Tag;
                _context.RuleTaskExecutors.Update(executor);
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            return new ApiResult<bool>(ApiCode.CantFindObject, "cant't find that object", false);
        }

        [HttpPost("[action]")]
        public async Task<ApiResult<bool>> AddExecutor(RuleTaskExecutor m)
        {
            var profile = this.GetUserProfile();
            var executor = new RuleTaskExecutor();
            executor.DefaultConfig = m.DefaultConfig;
            executor.ExecutorDesc = m.ExecutorDesc;
            executor.ExecutorName = m.ExecutorName;
            executor.TypeName = m.ExecutorName;
            executor.Path = m.Path;
            executor.Tag = m.Tag;
            executor.AddDateTime = DateTime.Now;
            executor.Creator = User.GetUserId();
            executor.ExecutorStatus = 1;
            _context.JustFill(this, executor);
            _context.RuleTaskExecutors.Add(executor);
            var rest = await _context.SaveChangesAsync();
            return new ApiResult<bool>(ApiCode.Success, "Ok", rest > 0);
        }

        [HttpPost("[action]")]
        public async Task<ApiResult<RuleTaskExecutorTestResultDto>> TestTask(RuleTaskExecutorTestDto m)
        {
            var profile = this.GetUserProfile();
            var result = await this._flowRuleProcessor.TestScript(m.ruleId, m.flowId, m.Data);
            await _context.SaveChangesAsync();
            return new ApiResult<RuleTaskExecutorTestResultDto>(ApiCode.Success, "Ok", new RuleTaskExecutorTestResultDto() { Data = result.Data });
        }

        [HttpPost("RuleCondition")]
        public async Task<ApiResult<ConditionTestResult>> RuleCondition([FromBody] RuleTaskFlowTestResultDto m)
        {
            var profile = this.GetUserProfile();
            var data = JsonConvert.DeserializeObject(m.Data) as JObject;
            var d = data.ToObject(typeof(ExpandoObject));
            var result = await this._flowRuleProcessor.TestCondition(m.ruleId, m.flowId, d);
            return new ApiResult<ConditionTestResult>(ApiCode.Success, "Ok", result);
        }
    }
}