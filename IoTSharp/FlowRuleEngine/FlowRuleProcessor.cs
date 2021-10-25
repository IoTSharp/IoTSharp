using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using IoTSharp.Data;
using IoTSharp.Interpreter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace IoTSharp.FlowRuleEngine
{
    public class FlowRuleProcessor
    {
        private readonly IServiceProvider _sp;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FlowRuleProcessor> _logger;
        private readonly AppSettings _setting;
        private List<Flow> _allFlows ;
        private List<FlowOperation> _allflowoperation ;
        public FlowRuleProcessor(

            ApplicationDbContext context, ILogger<FlowRuleProcessor> logger, IServiceScopeFactory scopeFactor, IOptions<AppSettings> options)
        {
            _sp = scopeFactor.CreateScope().ServiceProvider;
            _context = context;
            _logger = logger;
            _setting = options.Value;
            _allFlows = new List<Flow>();
            _allflowoperation = new List<FlowOperation>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruleid"> 规则Id</param>
        /// <param name="data">数据</param>
        /// <param name="creator">创建者(可以是模拟器(测试)，可以是设备，在EventType中区分一下)</param>
        /// <param name="type">类型</param>
        /// <param name="BizId">业务Id(第三方唯一Id，用于取回事件以及记录的标识)</param>
        /// <returns> 返回所有节点的记录信息，需要保存则保存</returns>

        public async Task<List<FlowOperation>> RunFlowRules(Guid ruleid, object data, Guid creator, EventType type, string BizId)
        {
            var rule = _context.FlowRules.FirstOrDefault(c => c.RuleId == ruleid);
            _allFlows = _context.Flows.Where(c => c.FlowRule == rule).ToList();
            var _event = new BaseEvent()
            {
                CreaterDateTime = DateTime.Now,
                Creator = creator,
                EventDesc = "测试",
                EventName = "测试",
                MataData = JsonConvert.SerializeObject(data),
                FlowRule = rule,
                Bizid = BizId,
                Type = type,
                EventStaus = 1
            };
            _context.BaseEvents.Add(_event);
            _context.SaveChanges();
            var flows = _allFlows.Where(c => c.FlowType != "label").ToList();
            var start = flows.FirstOrDefault(c => c.FlowType == "bpmn:StartEvent");

            var startoperation = new FlowOperation()
            {

                OperationId = Guid.NewGuid(),
                bpmnid = start.bpmnid,
                AddDate = DateTime.Now,
                FlowRule = start.FlowRule,
                Flow = start,
                Data = JsonConvert.SerializeObject(data),
                NodeStatus = 1,
                OperationDesc = "开始处理",
                Step = 1,
                BaseEvent = _event
            };
            _allflowoperation.Add(startoperation);
            var nextflows = await ProcessCondition(start.FlowId, data);
            if (nextflows != null)
            {

                foreach (var item in nextflows)
                {
                    var flowOperation = new FlowOperation()
                    {
                        OperationId = Guid.NewGuid(),
                        AddDate = DateTime.Now,
                        FlowRule = item.FlowRule,
                        Flow = item,
                        Data = JsonConvert.SerializeObject(data),
                        NodeStatus = 1,
                        OperationDesc = "执行条件（" + (string.IsNullOrEmpty(item.Conditionexpression)
                            ? "空条件"
                            : item.Conditionexpression) + ")",
                        Step = ++startoperation.Step,
                        bpmnid = item.bpmnid,
                        BaseEvent = _event
                    };
                    _allflowoperation.Add(flowOperation);
                    await Process(flowOperation.OperationId, data);
                }
                return _allflowoperation;
            }
            return null;
        }



        public async Task Process(Guid operationid, object data)
        {
            var peroperation = _allflowoperation.FirstOrDefault(c => c.OperationId == operationid);
            if (peroperation == null
            )
            {
                return;
            }
            var flow = _allFlows.FirstOrDefault(c => c.bpmnid == peroperation.Flow.TargetId);
            switch (flow.FlowType)
            {
                case "bpmn:SequenceFlow":

                    var operation = new FlowOperation()
                    {
                        OperationId = Guid.NewGuid(),
                        AddDate = DateTime.Now,
                        FlowRule = flow.FlowRule,
                        Flow = flow,
                        Data = JsonConvert.SerializeObject(data),
                        NodeStatus = 1,
                        OperationDesc = "执行条件（" + (string.IsNullOrEmpty(flow.Conditionexpression)
                            ? "空条件"
                            : flow.Conditionexpression) + ")",
                        Step = ++peroperation.Step,
                        bpmnid = flow.bpmnid,
                        BaseEvent = peroperation.BaseEvent
                    };
                    _allflowoperation.Add(operation);
                    await Process(operation.OperationId, data);
                    break;

                case "bpmn:Task":
                    {

                        var taskoperation = new FlowOperation()
                        {
                            OperationId = Guid.NewGuid(),
                            bpmnid = flow.bpmnid,
                            AddDate = DateTime.Now,
                            FlowRule = flow.FlowRule,
                            Flow = flow,
                            Data = JsonConvert.SerializeObject(data),
                            NodeStatus = 1,
                            OperationDesc = "执行任务" + flow.Flowname,
                            Step = ++peroperation.Step,
                            BaseEvent = peroperation.BaseEvent
                        };
                        _allflowoperation.Add(taskoperation);


                        //脚本处理
                        if (!string.IsNullOrEmpty(flow.NodeProcessScriptType) && !string.IsNullOrEmpty(flow.NodeProcessScript))
                        {
                            var scriptsrc = flow.NodeProcessScript;


                            dynamic obj = null;
                            switch (flow.NodeProcessScriptType)
                            {
                                case "csharp":

                                    //脚本处理逻辑
                                    break;
                                case "python":
                                    {
                                        using (var pse = _sp.GetRequiredService<PythonScriptEngine>())
                                        {
                                            string result = pse.Do(scriptsrc, taskoperation.Data);
                                            obj = JsonConvert.DeserializeObject<ExpandoObject>(result);
                                        }
                                    }
                                    break;
                                case "sql":
                                    {
                               
                                        using (var pse = _sp.GetRequiredService<SQLEngine>())
                                        {
                                            string result = pse.Do(scriptsrc, taskoperation.Data);
                                            obj = JsonConvert.DeserializeObject<ExpandoObject>(result);
                                        }
                                    }

                                    break;
                                case "lua":
                                    {
                                        using (var lua = _sp.GetRequiredService<LuaScriptEngine>())
                                        {
                                            string result = lua.Do(scriptsrc, taskoperation.Data);
                                            obj = JsonConvert.DeserializeObject<ExpandoObject>(result);
                                        }
                                    }
                                    break;


                                case "javascript":
                                    {
                                        using (var js = _sp.GetRequiredService<JavaScriptEngine>())
                                        {
                                            string result = js.Do(scriptsrc, taskoperation.Data);
                                            obj = JsonConvert.DeserializeObject<ExpandoObject>(result);
                                        }
                                    }
                                    break;
                            }

                            if (obj != null)
                            {
                                var next = await ProcessCondition(taskoperation.Flow.FlowId, obj);
                                foreach (var item in next)
                                {
                                    var flowOperation = new FlowOperation()
                                    {
                                        OperationId = Guid.NewGuid(),
                                        AddDate = DateTime.Now,
                                        FlowRule = item.FlowRule,
                                        Flow = item,
                                        Data = JsonConvert.SerializeObject(obj),
                                        NodeStatus = 1,
                                        OperationDesc = "执行条件（" +
                                                        (string.IsNullOrEmpty(item.Conditionexpression)
                                                            ? "空条件"
                                                            : item.Conditionexpression) + ")",
                                        Step = ++taskoperation.Step,
                                        bpmnid = item.bpmnid,
                                        BaseEvent = taskoperation.BaseEvent
                                    };
                                    _allflowoperation.Add(flowOperation);
                                    await Process(flowOperation.OperationId, obj);
                                }
                            }
                            else
                            {
                                taskoperation.OperationDesc = "脚本执行异常,未能获取到结果";
                                taskoperation.NodeStatus = 2;
                            }
                        }
                        else
                        {
                            var next = await ProcessCondition(taskoperation.Flow.FlowId, data);
                            foreach (var item in next)
                            {
                                var flowOperation = new FlowOperation()
                                {
                                    OperationId = Guid.NewGuid(),
                                    AddDate = DateTime.Now,
                                    FlowRule = item.FlowRule,
                                    Flow = item,
                                    Data = JsonConvert.SerializeObject(data),
                                    NodeStatus = 1,
                                    OperationDesc = "执行条件（" + (string.IsNullOrEmpty(item.Conditionexpression)
                                        ? "空条件"
                                        : item.Conditionexpression) + ")",
                                    Step = ++taskoperation.Step,
                                    bpmnid = item.bpmnid,
                                    BaseEvent = taskoperation.BaseEvent
                                };
                                _allflowoperation.Add(flowOperation);
                                await Process(flowOperation.OperationId, data);
                            }

                        }
                    }

                    break;

                case "bpmn:EndEvent":
             
                    var end = _allflowoperation.FirstOrDefault(c => c.bpmnid == flow.bpmnid );

                    if (end != null)
                    {
                        end.bpmnid = flow.bpmnid;
                        end.AddDate = DateTime.Now;
                        end.FlowRule = flow.FlowRule;
                        end.Flow = flow;
                        end.Data = JsonConvert.SerializeObject(data);
                        end.NodeStatus = 1;
                        end.OperationDesc = "处理完成";
                        end.Step = 1+_allflowoperation.Max(c => c.Step) ;
                        end.BaseEvent = peroperation.BaseEvent;
                    }
                    else
                    {
                        end = new FlowOperation();
                        end.OperationId = Guid.NewGuid();
                        end.bpmnid = flow.bpmnid;
                        end.AddDate = DateTime.Now;
                        end.FlowRule = flow.FlowRule;
                        end.Flow = flow;
                        end.Data = JsonConvert.SerializeObject(data);
                        end.NodeStatus = 1;
                        end.OperationDesc = "处理完成";
                        end.Step = 1+_allflowoperation.Max(c => c.Step) ;
                        end.BaseEvent = peroperation.BaseEvent;
                        _allflowoperation.Add(end);
                    }


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



        private async Task<List<Flow>> ProcessCondition(Guid FlowId, dynamic data)
        {
            var flow = _allFlows.FirstOrDefault(c => c.FlowId == FlowId);
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
                    //  Action = null,
                    Params = data,  //交给下一个节点的义数据
                    Task = tasks,
                    //   WaitTime = 0
                });
                var next = result.Where(c => c.IsSuccess).ToList();
                foreach (var item in next)
                {
                    var nextflow = flows.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                    emptyflow.Add(nextflow);
                }


            }

            return emptyflow;
        }
    }
}
