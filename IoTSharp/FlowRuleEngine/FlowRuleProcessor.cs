using Castle.Components.DictionaryAdapter;
using EasyCaching.Core;
using IoTSharp.Data;
using IoTSharp.Interpreter;
using IoTSharp.TaskActions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using IoTSharp.Extensions;
using IoTSharp.Contracts;
using IoTSharp.Data.Extensions;

namespace IoTSharp.FlowRuleEngine
{
    public class FlowRuleProcessor
    {
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly ILogger<FlowRuleProcessor> _logger;
        private readonly AppSettings _setting;
        private readonly IEasyCachingProvider _caching;
        private readonly IServiceProvider _sp;
        private readonly TaskExecutorHelper _helper;
        private readonly int _maximumiteration = 1000;
  

        public FlowRuleProcessor(ILogger<FlowRuleProcessor> logger, IServiceScopeFactory scopeFactor, IOptions<AppSettings> options, TaskExecutorHelper helper, IEasyCachingProviderFactory factory)
        {
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(options.Value.CachingUseIn)}";
            _scopeFactor = scopeFactor;
            _logger = logger;
            _setting = options.Value;
            _caching = factory.GetCachingProvider(_hc_Caching);
            _sp = _scopeFactor.CreateScope().ServiceProvider;
            _helper = helper;
        }

        public async Task RunRules(Guid devid, object obj, EventType mountType)
        {
            try
            {
                var rules = await _caching.GetAsync($"ruleid_{devid}_{Enum.GetName(mountType)}", async () =>
                {
                    using (var scope = _scopeFactor.CreateScope())
                    using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        return await _dbContext.GerDeviceRulesIdList(devid, mountType);
                    }
                }, TimeSpan.FromSeconds(_setting.RuleCachingExpiration));
                if (rules.HasValue)
                {
                    rules.Value.ToList().ForEach(async g =>
                    {
                        try
                        {
                          await RunFlowRules(g, obj, devid, FlowRuleRunType.Normal, null);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"为设备{devid}执行规则链{g}时遇到错误{ex.Message}");
                        }
                    });
                }
                else
                {
                    _logger.LogDebug($"{devid}的数据无相关规则链处理。");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{devid}处理规则链时遇到异常:{ex.Message}");

            }
        }

        /// <summary>
        ///运行指定规则链的规则
        /// </summary>
        /// <param name="ruleid"> 规则Id</param>
        /// <param name="data">数据</param>
        /// <param name="deviceId">创建者(可以是模拟器(测试)，可以是设备，在EventType中区分一下)</param>
        /// <param name="type">类型</param>
        /// <param name="bizId">业务Id(第三方唯一Id，用于取回事件以及记录的标识)</param>
        /// <returns> 返回所有节点的记录信息，需要保存则保存</returns>

        public async Task<List<FlowOperation>> RunFlowRules(Guid ruleid, object data, Guid deviceId, FlowRuleRunType type, string bizId)
        {
            var _allflowoperation = new List<FlowOperation>();
             var  cacheRule = await GetFlowRule(ruleid);
            if (cacheRule.HasValue)
            {
                FlowRule rule = cacheRule.Value.rule;
                var _allFlows = cacheRule.Value._allFlows;
                _logger.LogInformation($"开始执行规则链{rule?.Name}({ruleid})");
                var @event = new BaseEvent()
                {
                    CreaterDateTime = DateTime.UtcNow,
                    Creator = deviceId,
                    EventDesc = $"Event Rule:{rule?.Name}({ruleid}) device is {deviceId}",
                    EventName = $"开始执行规则链{rule?.Name}({ruleid})",
                    MataData = JsonConvert.SerializeObject(data),
                    FlowRule = rule,
                    Bizid = bizId,
                    Type = type,
                    EventStaus = 1
                };
                using (var sp = _scopeFactor.CreateScope())
                {
                    using (var context = sp.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        var r = context.FlowRules.Include(c => c.Customer).Include(c => c.Tenant).FirstOrDefault(c => c.RuleId == rule.RuleId);
                        if (r != null)
                        {
                            @event.FlowRule = r;
                            @event.Tenant = r.Tenant;
                            @event.Customer = r.Customer;
                            context.BaseEvents.Add(@event);
                            context.SaveChanges();
                        }
                    }
                }
                var flows = _allFlows.Where(c => c.FlowType != "label").ToList();
                var start = flows.FirstOrDefault(c => c.FlowType == "bpmn:StartEvent");

                if (start == null)
                {
                    _allflowoperation.Add(new FlowOperation()
                    {
                        OperationId = Guid.NewGuid(),
                        bpmnid = "",
                        AddDate = DateTime.UtcNow,
                        FlowRule = rule,
                        Flow = start,
                        Data = JsonConvert.SerializeObject(data),
                        NodeStatus = 1,
                        OperationDesc = "未能找到启动节点",
                        Step = 1,
                        BaseEvent = @event
                    });

                    return _allflowoperation;
                }
                var startoperation = new FlowOperation()
                {
                    OperationId = Guid.NewGuid(),
                    bpmnid = start.bpmnid,
                    AddDate = DateTime.UtcNow,
                    FlowRule = rule,
                    Flow = start,
                    Data = JsonConvert.SerializeObject(data),
                    NodeStatus = 1,
                    OperationDesc = "进入开始节点",
                    Step = 1,
                    BaseEvent = @event
                };

                _allflowoperation.Add(startoperation);
                //从“开始节点”上链接的线节点对象进行规则判断，通过线对象上的规则才能进行后续逻辑
                var nextflows = await ProcessCondition(_allFlows, start.FlowId, data);
                //获取到的通过规则判断的后续节点列表
                if (nextflows != null)
                {
                    var step = startoperation.Step + 1;
                    foreach (var item in nextflows)
                    {
                        var flowOperation = new FlowOperation()
                        {
                            OperationId = Guid.NewGuid(),
                            AddDate = DateTime.UtcNow,
                            FlowRule = rule,
                            BaseEvent = @event,
                            Flow = item,
                            Data = JsonConvert.SerializeObject(data),
                            NodeStatus = 1,
                            OperationDesc = "Condition（" + (string.IsNullOrEmpty(item.Conditionexpression)
                                ? "Empty Condition"
                                : item.Conditionexpression) + ")",
                            Step = step,
                            bpmnid = item.bpmnid,
                        };

                        _allflowoperation.Add(flowOperation);
                        //执行节点逻辑
                        await Process(_allFlows, _allflowoperation, flowOperation.OperationId, data, deviceId);
                    }
                    return _allflowoperation;
                }
            }
            return null;
        }

        private async Task<CacheValue<(FlowRule rule, List<Flow> _allFlows)>> GetFlowRule(Guid ruleid)
        {
            return await _caching.GetAsync($"RunFlowRules_{ruleid}", async () =>
            {
                FlowRule rule;
                List<Flow> allFlows;
                using (var sp = _scopeFactor.CreateScope())
                {
                    using (var context = sp.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        rule = await context.FlowRules.AsNoTracking().FirstOrDefaultAsync(c => c.RuleId == ruleid);
                        allFlows = await context.Flows.AsNoTracking().Where(c => c.FlowRule == rule && c.FlowStatus > 0).ToListAsync();
                        _logger.LogInformation($"读取规则链{rule?.Name}({ruleid}),子流程共计:{allFlows.Count}");
                    }
                }
                return (rule, _allFlows: allFlows);
            }, TimeSpan.FromSeconds(_setting.RuleCachingExpiration));
        }

        public async Task Process(List<Flow> _allFlows, List<FlowOperation> _allflowoperation  , Guid operationid, object data, Guid deviceId)
        {
            var peroperation = _allflowoperation.FirstOrDefault(c => c.OperationId == operationid);
            if (peroperation != null)
            {
                if (peroperation.Step > this._maximumiteration)
                {
                    peroperation.OperationDesc = "Maximum iteration depth has been reached";
                    peroperation.NodeStatus = 3;
                    return;
                }

                var flow = _allFlows.FirstOrDefault(c => c.bpmnid == peroperation.Flow.TargetId && c.FlowType != "label");
                switch (flow.FlowType)
                {
                    //线节点对象
                    case "bpmn:SequenceFlow":
                        {
                            var step = peroperation.Step + 1;
                            var operation = new FlowOperation()
                            {
                                OperationId = Guid.NewGuid(),
                                AddDate = DateTime.UtcNow,
                                FlowRule = peroperation.BaseEvent.FlowRule,
                                Flow = flow,
                                Data = JsonConvert.SerializeObject(data),
                                NodeStatus = 1,
                                OperationDesc = "Condition（" + (string.IsNullOrEmpty(flow.Conditionexpression)
                                    ? "Empty Condition"
                                    : flow.Conditionexpression) + ")",
                                Step = step,
                                bpmnid = flow.bpmnid,
                                BaseEvent = peroperation.BaseEvent
                            };
                            _allflowoperation.Add(operation);
                            await Process(_allFlows, _allflowoperation, operation.OperationId, data, deviceId);

                        }

                        break;
                    //中间执行器和脚本节点
                    case "bpmn:Task":
                        {
                            var step = peroperation.Step + 1;
                            var taskoperation = new FlowOperation()
                            {
                                OperationId = Guid.NewGuid(),
                                bpmnid = flow.bpmnid,
                                AddDate = DateTime.UtcNow,
                                FlowRule = peroperation.BaseEvent.FlowRule,
                                Flow = flow,
                                Data = JsonConvert.SerializeObject(data),
                                NodeStatus = 1,
                                OperationDesc = "Run" + flow.NodeProcessScriptType + "Task:" + flow.Flowname,
                                Step = step,
                                BaseEvent = peroperation.BaseEvent
                            };
                            _allflowoperation.Add(taskoperation);

                            //脚本处理
                            if (!string.IsNullOrEmpty(flow.NodeProcessScriptType) && (!string.IsNullOrEmpty(flow.NodeProcessScript) || !string.IsNullOrEmpty(flow.NodeProcessClass)))
                            {
                                var scriptsrc = flow.NodeProcessScript;

                                dynamic obj = null;
                                switch (flow.NodeProcessScriptType)
                                {
                                    case "executor":

                                        if (!string.IsNullOrEmpty(flow.NodeProcessClass))
                                        {
                                            TaskAction executor = _helper.CreateInstanceByTypeName(flow.NodeProcessClass);
                                            if (executor != null)
                                            {
                                                try
                                                {
                                                    //执行器入口 Input上一个节点向当前节点的传参，DeviceId设备编号，ExecutorConfig当前节点在设计时的配置内容
                                                    var result = await executor.ExecuteAsync(new TaskActionInput()
                                                    {
                                                        Input = taskoperation.Data,
                                                        DeviceId = deviceId,
                                                        ExecutorConfig = flow.NodeProcessParams,
                                                    }
                                                    );

                                                    _logger.Log(LogLevel.Information, "执行器" + flow.NodeProcessClass + "已完成处理");
                                                    obj = result.DynamicOutput;
                                                    taskoperation.OperationDesc += "\r\n" + result.ExecutionInfo;
                                                    if (!result.ExecutionStatus)
                                                    {
                                                        taskoperation.NodeStatus = 2;
                                                        string info = JsonConvert.SerializeObject(result.DynamicOutput);
                                                        _logger.Log(LogLevel.Information, "执行器执行失败："+ result.ExecutionInfo +"\r\n"+ flow.NodeProcessClass + "未能正确处理:" + info);
                                                        return;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.Log(LogLevel.Information, "执行器" + flow.NodeProcessClass + "未能正确处理:" + ex.Source);

                                                    taskoperation.OperationDesc += "\r\n" + ex.Message;
                                                    taskoperation.NodeStatus = 2;
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                _logger.Log(LogLevel.Warning, "脚本执行异常,未能实例化执行器");
                                                taskoperation.OperationDesc += "\r\n" + "脚本执行异常,未能实例化执行器";
                                                taskoperation.NodeStatus = 2;
                                                return;
                                            }
                                        }
                                        break;

                                    case "python":
                                        {
                                            using (var pse = _sp.GetRequiredService<PythonScriptEngine>())
                                            {
                                                try
                                                {
                                                    string result = pse.Do(scriptsrc, taskoperation.Data);
                                                    obj = JsonConvert.DeserializeObject<object>(result);
                                                }
                                                catch (Exception ex)
                                                {

                                                    _logger.Log(LogLevel.Warning, "python脚本执行异常");
                                                    taskoperation.OperationDesc += ex.Message;
                                                    taskoperation.NodeStatus = 2;
                                                }
                                            }
                                        }
                                        break;

                                    case "sql":
                                        {
                                            using (var pse = _sp.GetRequiredService<SQLEngine>())
                                            {
                                                try
                                                {
                                                    string result = pse.Do(scriptsrc, taskoperation.Data);
                                                    obj = JsonConvert.DeserializeObject<object>(result);
                                                }
                                                catch (Exception ex)
                                                {

                                                    _logger.Log(LogLevel.Warning, "sql脚本执行异常");
                                                    taskoperation.OperationDesc += ex.Message;
                                                    taskoperation.NodeStatus = 2;
                                                }
                                            }
                                        }

                                        break;

                                    case "lua":
                                        {

                                            using (var lua = _sp.GetRequiredService<LuaScriptEngine>())
                                            {
                                                try
                                                {
                                                    string result = lua.Do(scriptsrc, taskoperation.Data);
                                                    obj = JsonConvert.DeserializeObject<object>(result);
                                                }
                                                catch (Exception ex)
                                                {

                                                    _logger.Log(LogLevel.Warning, "lua脚本执行异常");
                                                    taskoperation.OperationDesc += ex.Message;
                                                    taskoperation.NodeStatus = 2;
                                                }
                                            }

                                        }
                                        break;

                                    case "javascript":
                                        {

                                            using (var js = _sp.GetRequiredService<JavaScriptEngine>())
                                            {
                                                try
                                                {
                                                    string result = js.Do(scriptsrc, taskoperation.Data);
                                                    obj = JsonConvert.DeserializeObject<object>(result);

                                                }
                                                catch (Exception ex)
                                                {

                                                    _logger.Log(LogLevel.Warning, "javascript脚本执行异常");
                                                    taskoperation.OperationDesc += ex.Message;
                                                    taskoperation.NodeStatus = 2;
                                                }

                                            }
                                        }
                                        break;

                                    case "csharp":
                                        {
                                            using (var js = _sp.GetRequiredService<CSharpScriptEngine>())
                                            {

                                                try
                                                {

                                                    string result = js.Do(scriptsrc, taskoperation.Data);
                                                    obj = JsonConvert.DeserializeObject<object>(result);
                                                }
                                                catch (Exception ex)
                                                {

                                                    _logger.Log(LogLevel.Warning, "csharp脚本执行异常");
                                                    _logger.Log(LogLevel.Warning, ex.Message);
                                                    taskoperation.OperationDesc += ex.Message;
                                                    taskoperation.NodeStatus = 2;
                                                }


                                            }
                                        }
                                        break;
                                }

                                if (obj != null)
                                {
                                    var next = await ProcessCondition(_allFlows, taskoperation.Flow.FlowId, obj);
                                    var cstep = taskoperation.Step + 1;
                                    foreach (var item in next)
                                    {
                                        var flowOperation = new FlowOperation()
                                        {
                                            OperationId = Guid.NewGuid(),
                                            AddDate = DateTime.UtcNow,
                                            FlowRule = peroperation.BaseEvent.FlowRule,
                                            Flow = item,
                                            Data = JsonConvert.SerializeObject(obj),
                                            NodeStatus = 1,
                                            OperationDesc = "Execute（" +
                                                            (string.IsNullOrEmpty(item.Conditionexpression)
                                                                ? "Empty Condition"
                                                                : item.Conditionexpression) + ")",
                                            Step = cstep,
                                            bpmnid = item.bpmnid,
                                            BaseEvent = taskoperation.BaseEvent
                                        };
                                        _allflowoperation.Add(flowOperation);
                                        await Process(_allFlows, _allflowoperation, flowOperation.OperationId, obj, deviceId);
                                    }
                                }
                                else
                                {

                                    taskoperation.NodeStatus = 2;
                                    _logger.Log(LogLevel.Warning, "脚本未能顺利执行");
                                }
                            }
                            else
                            {
                                var next = await ProcessCondition(_allFlows, taskoperation.Flow.FlowId, data);
                                var cstep = taskoperation.Step + 1;
                                foreach (var item in next)
                                {
                                    var flowOperation = new FlowOperation()
                                    {
                                        OperationId = Guid.NewGuid(),
                                        AddDate = DateTime.UtcNow,
                                        FlowRule = peroperation.BaseEvent.FlowRule,
                                        Flow = item,
                                        Data = JsonConvert.SerializeObject(data),
                                        NodeStatus = 1,
                                        OperationDesc = "Execute（" + (string.IsNullOrEmpty(item.Conditionexpression)
                                            ? "Empty Condition"
                                            : item.Conditionexpression) + ")",
                                        Step = cstep,
                                        bpmnid = item.bpmnid,
                                        BaseEvent = taskoperation.BaseEvent
                                    };
                                    _allflowoperation.Add(flowOperation);
                                    await Process(_allFlows, _allflowoperation, flowOperation.OperationId, data, deviceId);
                                }
                            }
                        }

                        break;
                    //结束节点
                    case "bpmn:EndEvent":


                        var end = new FlowOperation();
                        end.BuildFlowOperation(peroperation, flow);
                        end.OperationId = Guid.NewGuid();
                        end.bpmnid = flow.bpmnid;
                        end.AddDate = DateTime.UtcNow;
                        end.FlowRule = peroperation.BaseEvent.FlowRule;
                        end.Flow = flow;
                        end.Data = JsonConvert.SerializeObject(data);
                        end.NodeStatus = 1;
                        end.OperationDesc = "处理完成";
                        end.Step = 1 + _allflowoperation.Max(c => c.Step);
                        end.BaseEvent = peroperation.BaseEvent;
                        _allflowoperation.Add(end);

                        _logger.Log(LogLevel.Warning, "规则链执行完成");

                        break;

                    //没有终结点的节点必须留个空标签
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
                            break;
                        }
                }
            }
        }
        /// <summary>
        /// 调用规则引擎，判断当前节点后的连线中的规则是否通过校验，如果验证为真则返回满足条件的线对应的目标节点
        /// </summary>
        /// <param name="_allFlows">所有的节点</param>
        /// <param name="flowId">当前节点</param>
        /// <param name="data">进入到当前节点的数据传参</param>
        /// <returns></returns>
        public async Task<List<Flow>> ProcessCondition(List<Flow> _allFlows, Guid flowId, object data)
        {

            var tt = data.GetType();
            var emptyflow = new List<Flow>();
            //根据节点Id获取节点信息
            var flow = _allFlows.FirstOrDefault(c => c.FlowId == flowId);
            if (flow != null)
            {
                //根据节点Id获取到当前节点与以后节点的线对象列表（一个节点可以存在很多线对象关联到下一级节点）
                var flows = _allFlows.Where(c => c.SourceId == flow?.bpmnid).ToList();
                //没有逻辑的线节点对象
                emptyflow = flows.Where(c => c.Conditionexpression == string.Empty||  c.Conditionexpression==null).ToList() ?? new List<Flow>();
                var tasks = new BaseRuleTask()
                {
                    Name = flow.Flowname,
                    Eventid = flow.bpmnid,
                    id = flow.bpmnid,
                    outgoing = new EditableList<BaseRuleFlow>()
                };
                foreach (var item in flows.Except(emptyflow))//排除掉没有逻辑的线节点
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
                    SimpleFlowExcutor flowExcutor = new SimpleFlowExcutor();
                    if (data != null)
                    {
                        if (data.GetType() == typeof(JObject))
                        {
                            var t = data as JObject;
                            var d = t?.ToObject<ExpandoObject>();
                            if (d != null)
                            {
                                //执行判断，利用规则引擎执行线对象中的规则
                                var result = await flowExcutor.Excute(new FlowExcuteEntity()
                                {
                                    Params = d,
                                    Task = tasks,
                                });
                                //筛选规则通过的线对象
                                var next = result.Where(c => c.IsSuccess).ToList();
                                foreach (var item in next)
                                {
                                    var nextflow = flows.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                                    emptyflow.Add(nextflow);
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"执行 {flowId}的规则链时遇到data为空。");
                            }
                        } else 
                        if (data.GetType() == typeof(JArray))
                        {
                            var result = await flowExcutor.Excute(new FlowExcuteEntity()
                            {
                                Params = data,
                                Task = tasks,
                            });
                            var next = result.Where(c => c.IsSuccess).ToList();
                            foreach (var item in next)
                            {
                                var nextflow = flows.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                                emptyflow.Add(nextflow);
                            }
                        }
                        else
                        if (data is ExpandoObject)
                        {
                            var result = await flowExcutor.Excute(new FlowExcuteEntity()
                            {
                                Params = data,
                                Task = tasks,
                            });
                            var next = result.Where(c => c.IsSuccess).ToList();
                            foreach (var item in next)
                            {
                                var nextflow = flows.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                                emptyflow.Add(nextflow);
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"执行 {flowId}的规则链时遇到未预期的数据类型:{data.GetType()}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"执行 {flowId}的规则链时遇到data为空。");
                        var result = await flowExcutor.Excute(new FlowExcuteEntity()
                        {
                            Params = null,
                            Task = tasks,
                        });
                        foreach (var item in result.Where(c => c.IsSuccess).ToList())
                        {
                            var nextflow = flows.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                            emptyflow.Add(nextflow);
                        }

                    }


                }
            }
            else
            {
                _logger.LogWarning($"ProcessCondition flowId={flowId}");
            }
            return emptyflow;
        }

        public async Task<ScriptTestResult> TestScript(Guid ruleid, Guid flowId, string data)
        {
            var cacheRule =await GetFlowRule(ruleid);
            if (cacheRule.HasValue)
            {
                var flow = cacheRule.Value._allFlows.FirstOrDefault(c => c.FlowId == flowId);

                if (!string.IsNullOrEmpty(flow?.NodeProcessScriptType) &&
                    (!string.IsNullOrEmpty(flow.NodeProcessScript) || !string.IsNullOrEmpty(flow.NodeProcessClass)))
                {
                    var scriptsrc = flow.NodeProcessScript;

                    dynamic obj = null;

                    switch (flow.NodeProcessScriptType)
                    {
                        case "executor":

                            if (!string.IsNullOrEmpty(flow.NodeProcessClass))
                            {
                                TaskAction executor = _helper.CreateInstanceByTypeName(flow.NodeProcessClass);
                                if (executor != null)
                                {
                                    try
                                    {
                                        var result = await executor.ExecuteAsync(new TaskActionInput()
                                        {
                                            Input = data,
                                            ExecutorConfig = flow.NodeProcessParams,
                                            DeviceId = Guid.Empty
                                        });
                                        obj = result.DynamicOutput;
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning($"执行{flow.NodeProcessClass}失败， {ex.Message}");
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning($"{flow.NodeProcessClass},未找到类型 ");
                                }
                            }
                            break;

                        case "python":
                            {
                                using (var pse = _sp.GetRequiredService<PythonScriptEngine>())
                                {
                                    string result = pse.Do(scriptsrc, data);
                                    obj = JsonConvert.DeserializeObject<object>(result);
                                }
                            }
                            break;

                        case "sql":
                            {
                                using (var pse = _sp.GetRequiredService<SQLEngine>())
                                {
                                    string result = pse.Do(scriptsrc, data);
                                    obj = JsonConvert.DeserializeObject<object>(result);
                                }
                            }

                            break;

                        case "lua":
                            {
                                using (var lua = _sp.GetRequiredService<LuaScriptEngine>())
                                {
                                    string result = lua.Do(scriptsrc, data);
                                    obj = JsonConvert.DeserializeObject<object>(result);
                                }
                            }
                            break;

                        case "javascript":
                            {
                                using (var js = _sp.GetRequiredService<JavaScriptEngine>())
                                {
                                    string result = js.Do(scriptsrc, data);
                                    obj = JsonConvert.DeserializeObject<object>(result);
                                }
                            }
                            break;

                        case "csharp":
                            {
                                using (var js = _sp.GetRequiredService<CSharpScriptEngine>())
                                {
                                    string result = js.Do(scriptsrc, data);
                                    obj = JsonConvert.DeserializeObject<object>(result);
                                }
                            }
                            break;
                    }

                    if (obj != null)
                    {
                        return new ScriptTestResult() { Data = obj, IsExecuted = true };
                    }
                }
            }

            return new ScriptTestResult() { Data = null, IsExecuted = false }; ;
        }

        public async Task<ConditionTestResult> TestCondition(Guid ruleid, Guid flowId, dynamic data)
        {
            var cacheRule = await GetFlowRule(ruleid);
            if (cacheRule.HasValue)
            {
               var  _allFlows = cacheRule.Value._allFlows;
                var flow = _allFlows.FirstOrDefault(c => c.FlowId == flowId);
                var flows = _allFlows.Where(c => c.SourceId == flow.bpmnid).ToList();
                var emptyflow = flows.Where(c => c.Conditionexpression == string.Empty).ToList() ?? new List<Flow>();
                var tasks = new BaseRuleTask()
                {
                    Name = flow.Flowname,
                    Eventid = flow.bpmnid,
                    id = flow.bpmnid,
                    outgoing = new EditableList<BaseRuleFlow>()
                };
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
                    SimpleFlowExcutor flowExcutor = new SimpleFlowExcutor();
                    var result = await flowExcutor.Excute(new FlowExcuteEntity()
                    {
                        Params = data,
                        Task = tasks,
                    });
                    var next = result.Where(c => c.IsSuccess).ToList();
                    foreach (var item in next)
                    {
                        var nextflow = flows.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                        emptyflow.Add(nextflow);
                    }
                }
                return new ConditionTestResult { Failed = flows.Except(emptyflow).ToList(), Passed = emptyflow };
            }
            else
            {
                return null;
            }
        }
    }
}