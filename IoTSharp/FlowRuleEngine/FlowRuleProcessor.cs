using Castle.Components.DictionaryAdapter;
using IoTSharp.Data;
using IoTSharp.Interpreter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.FlowRuleEngine
{
    public class FlowRuleProcessor
    {
        private readonly IServiceProvider _sp;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FlowRuleProcessor> _logger;
        private readonly AppSettings _setting;
        private List<Flow> _allFlows = new List<Flow>();
        public FlowRuleProcessor(ILogger<FlowRuleProcessor> logger, IServiceScopeFactory scopeFactor, IOptions<AppSettings> options)
        {
            _sp = scopeFactor.CreateScope().ServiceProvider;
            _context = _sp.GetRequiredService<ApplicationDbContext>();
            _logger = logger;
            _setting = options.Value;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruleid"> 规则Id</param>
        /// <param name="data">数据</param>
        /// <param name="creator">创建者(可以是模拟器(测试)，可以是设备，在EventType中区分一下)</param>
        /// <param name="type">类型</param>
            /// <param name="BizId">业务Id(第三方唯一Id，用于取回事件以及记录的标识)</param>
        /// <returns></returns>

        public async Task RunFlowRules(Guid ruleid, object data, Guid creator, EventType type, string BizId)
        {
            _allFlows.Clear();
            var rule = _context.FlowRules.SingleOrDefault(c => c.RuleId == ruleid);
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
                Type = EventType.TestPurpose,
                EventStaus = 1
            };
            _context.BaseEvents.Add(_event);
            _context.SaveChanges();
            var flows = _allFlows.Where(c => c.FlowType != "label").ToList();
            var start = flows.FirstOrDefault(c => c.FlowType == "bpmn:StartEvent");

            var startoperation = new FlowOperation()
            {
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
            _context.FlowOperations.Add(startoperation);
            await _context.SaveChangesAsync();
            var nextflows = await ProcessCondition(start.FlowId, data);
            if (nextflows != null)
            {

                foreach (var item in nextflows)
                {
                    var flowOperation = new FlowOperation()
                    {
                        AddDate = DateTime.Now,
                        FlowRule = item.FlowRule,
                        Flow = item,
                        Data = JsonConvert.SerializeObject(data),
                        NodeStatus = 1,
                        OperationDesc = "执行条件（" + (string.IsNullOrEmpty(item.Conditionexpression)
                            ? "空条件"
                            : item.Conditionexpression) + ")",
                        Step = startoperation.Step++,
                        bpmnid = item.bpmnid,
                        BaseEvent = _event
                    };
                    _context.FlowOperations.Add(flowOperation);
                    await _context.SaveChangesAsync();
                    await Process(flowOperation.OperationId, data);
                }

            }

        }

        private async Task<List<Flow>> ProcessCondition(Guid FlowId, dynamic data)
        {
            var flow = _allFlows.SingleOrDefault(c => c.FlowId == FlowId);
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
                SimpleFLowExcutor flowExcutor = new SimpleFLowExcutor();
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


        public async Task Process(Guid operationid, object data)
        {
            var peroperation = _context.FlowOperations.SingleOrDefault(c => c.OperationId == operationid);
            if (peroperation == null
            )
            {
                return;
            }
            var flow = _allFlows.SingleOrDefault(c => c.FlowId == peroperation.Flow.FlowId);
            switch (flow.FlowType)
            {
                case "bpmn:SequenceFlow":
                    var t = _allFlows.FirstOrDefault(c => c.bpmnid == flow.TargetId);
                    var operation = new FlowOperation()
                    {
                        AddDate = DateTime.Now,
                        FlowRule = t.FlowRule,
                        Flow = t,
                        Data = JsonConvert.SerializeObject(data),
                        NodeStatus = 1,
                        OperationDesc = "执行条件（" + (string.IsNullOrEmpty(flow.Conditionexpression)
                            ? "空条件"
                            : flow.Conditionexpression) + ")",
                        Step = peroperation.Step++,
                        bpmnid = t.bpmnid,
                        BaseEvent = peroperation.BaseEvent
                    };
                    _context.FlowOperations.Add(operation);
                    await _context.SaveChangesAsync();
                    await Process(operation.OperationId, data);
                    break;

                case "bpmn:Task":
                    {
                        var task = _allFlows.FirstOrDefault(c => c.bpmnid == flow.bpmnid);
                        var taskoperation = new FlowOperation()
                        {
                            bpmnid = task.bpmnid,
                            AddDate = DateTime.Now,
                            FlowRule = task.FlowRule,
                            Flow = task,
                            Data = JsonConvert.SerializeObject(data),
                            NodeStatus = 1,
                            OperationDesc = "执行任务" + flow.Flowname,
                            Step = peroperation.Step++,
                            BaseEvent = peroperation.BaseEvent
                        };
                        _context.FlowOperations.Add(taskoperation);
                        await _context.SaveChangesAsync();

                        //脚本处理
                        if (!string.IsNullOrEmpty(flow.NodeProcessScriptType) && !string.IsNullOrEmpty(flow.NodeProcessScript))
                        {
                            var scriptsrc = flow.NodeProcessScript;
                            switch (flow.NodeProcessScriptType)
                            {
                                case "csharp":

                                    //脚本处理逻辑
                                    break;
                                case "python":
                                    {
                                        dynamic obj = null;
                                        using (var pse = _sp.GetRequiredService<PythonScriptEngine>())
                                        {
                                            obj = pse.Do(scriptsrc, taskoperation.Data);
                                        }
                                        var next = ProcessCondition(taskoperation.Flow.FlowId, obj);
                                        foreach (var item in next)
                                        {

                                            var flowOperation = new FlowOperation()
                                            {
                                                AddDate = DateTime.Now,
                                                FlowRule = item.FlowRule,
                                                Flow = item,
                                                Data = JsonConvert.SerializeObject(data),
                                                NodeStatus = 1,
                                                OperationDesc = "执行条件（" + (string.IsNullOrEmpty(item.Conditionexpression)
                                                    ? "空条件"
                                                    : item.Conditionexpression) + ")",
                                                Step = taskoperation.Step++,
                                                bpmnid = item.bpmnid,
                                                BaseEvent = taskoperation.BaseEvent
                                            };
                                            _context.FlowOperations.Add(flowOperation);
                                            await _context.SaveChangesAsync();

                                            await Process(flowOperation.OperationId, obj);
                                        }
                                    }
                                    break;
                                case "sql":
                                    break;
                                case "javascript":
                                    {
                                        dynamic obj = null;
                                        using (var js = _sp.GetRequiredService<JavaScriptEngine>())
                                        {
                                            obj = js.Do(scriptsrc, taskoperation.Data);
                                        }

                                        var next = ProcessCondition(taskoperation.Flow.FlowId, obj);

                                        foreach (var item in next)
                                        {
                                            var flowOperation = new FlowOperation()
                                            {
                                                AddDate = DateTime.Now,
                                                FlowRule = item.FlowRule,
                                                Flow = item,
                                                Data = JsonConvert.SerializeObject(data),
                                                NodeStatus = 1,
                                                OperationDesc = "执行条件（" + (string.IsNullOrEmpty(item.Conditionexpression)
                                                    ? "空条件"
                                                    : item.Conditionexpression) + ")",
                                                Step = taskoperation.Step++,
                                                bpmnid = item.bpmnid,
                                                BaseEvent = taskoperation.BaseEvent
                                            };
                                            _context.FlowOperations.Add(flowOperation);
                                            await _context.SaveChangesAsync();
                                            await Process(flowOperation.OperationId, obj);
                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            var next =await ProcessCondition(taskoperation.Flow.FlowId, data);

                            foreach (var item in next)
                            {
                                var flowOperation = new FlowOperation()
                                {
                                    AddDate = DateTime.Now,
                                    FlowRule = item.FlowRule,
                                    Flow = item,
                                    Data = JsonConvert.SerializeObject(data),
                                    NodeStatus = 1,
                                    OperationDesc = "执行条件（" + (string.IsNullOrEmpty(item.Conditionexpression)
                                        ? "空条件"
                                        : item.Conditionexpression) + ")",
                                    Step = taskoperation.Step++,
                                    bpmnid = item.bpmnid,
                                    BaseEvent = taskoperation.BaseEvent
                                };
                                _context.FlowOperations.Add(flowOperation);
                                await _context.SaveChangesAsync();



                                await Process(flowOperation.OperationId, data);
                            }

                        }

                        // 执行任务，完成后

                        //如果是异步调用，可以在此终止，然后通过FlowOperation表中当前的taskoperation.OperationId找到当前挂起的FlowId 再次恢复处理

                        //   条件处理



                        //var flows = _allFlows.Where(c => c.SourceId == flow.bpmnid).ToList();
                        //var tasks = new BaseRuleTask()
                        //{
                        //    Name = flow.Flowname,
                        //    Eventid = flow.bpmnid,
                        //    id = flow.bpmnid,

                        //    outgoing = new EditableList<BaseRuleFlow>()
                        //};

                        //var emptyflow = flows.Where(c => c.Conditionexpression == string.Empty).ToList();

                        //foreach (var item in flows.Except(emptyflow))
                        //{
                        //    var rule = new BaseRuleFlow();
                        //    rule.Expression = item.Conditionexpression;
                        //    rule.id = item.bpmnid;
                        //    rule.Name = item.Flowname;
                        //    rule.Eventid = item.bpmnid;
                        //    tasks.outgoing.Add(rule);
                        //}

                        //if (tasks.outgoing.Count > 0)
                        //{
                        //    SimpleFLowExcutor flowExcutor = new SimpleFLowExcutor();
                        //    var result = await flowExcutor.Excute(new FlowExcuteEntity()
                        //    {
                        //        //    Action = null,
                        //        Params = data,
                        //        Task = tasks,
                        //        //   WaitTime = 0
                        //    });
                        //    var next = result.Where(c => c.IsSuccess).ToList();
                        //    foreach (var item in next)
                        //    {
                        //        var nextflow = _allFlows.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                        //        emptyflow.Add(nextflow);
                        //    }
                        //}

                        //foreach (var item in emptyflow)
                        //{
                        //    await Process(taskoperation.OperationId, data);
                        //}
                    }

                    break;

                case "bpmn:EndEvent":
                    // 合并结束
                    var end = _context.FlowOperations.FirstOrDefault(c => c.bpmnid == flow.bpmnid && c.BaseEvent.EventId == peroperation.BaseEvent.EventId);

                    if (end != null)
                    {
                        _context.FlowOperations.Update(end);
                    }
                    else
                    {
                        end.bpmnid = flow.bpmnid;
                        end.AddDate = DateTime.Now;
                        end.FlowRule = flow.FlowRule;
                        end.Flow = flow;
                        end.Data = JsonConvert.SerializeObject(data);
                        end.NodeStatus = 1;
                        end.OperationDesc = "处理完成";
                        end.Step = _context.FlowOperations.Where(c => c.BaseEvent.EventId == peroperation.BaseEvent.EventId).Max(c => c.Step) + 1;
                        end.BaseEvent = peroperation.BaseEvent;
                        _context.FlowOperations.Add(end);
                    }
                    await _context.SaveChangesAsync();

                    break;
                //case "bpmn:StartEvent":

                //    {
                //        var flows = allflow.Where(c => c.SourceId == flow.bpmnid).ToList();
                //        var tasks = new BaseRuleTask()
                //        {
                //            Name = flow.Flowname,
                //            Eventid = flow.bpmnid,
                //            id = flow.bpmnid,

                //            outgoing = new EditableList<BaseRuleFlow>()
                //        };
                //        _context.FlowOperations.Add(new FlowOperation()
                //        {
                //            bpmnid = flow.bpmnid,
                //            AddDate = DateTime.Now,
                //            RuleId = flow.RuleId,
                //            FlowId = flow.FlowId,
                //            Data = JsonConvert.SerializeObject(data),
                //            NodeStatus = 1,
                //            OperationDesc = "开始处理",
                //            Step = nextstep,
                //            EventId = _eventid
                //        });
                //        await _context.SaveChangesAsync();
                //        var emptyflow = flows.Where(c => c.Conditionexpression == string.Empty).ToList();

                //        foreach (var item in flows.Except(emptyflow))
                //        {
                //            var rule = new BaseRuleFlow();

                //            rule.id = item.bpmnid;
                //            rule.Name = item.bpmnid;
                //            rule.Eventid = item.bpmnid;
                //            rule.Expression = item.Conditionexpression;
                //            tasks.outgoing.Add(rule);
                //        }

                //        if (tasks.outgoing.Count > 0)
                //        {
                //            SimpleFLowExcutor flowExcutor = new SimpleFLowExcutor();
                //            var result = await flowExcutor.Excute(new FlowExcuteEntity()
                //            {
                //                //  Action = null,
                //                Params = data,  //也可以放自定义数据
                //                Task = tasks,
                //                //   WaitTime = 0

                //            });

                //            var next = result.Where(c => c.IsSuccess).ToList();

                //            foreach (var item in next)
                //            {
                //                var nextflow = allflow.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                //                emptyflow.Add(nextflow);

                //            }

                //        }
                //        nextstep++;
                //        foreach (var item in emptyflow)
                //        {
                //            await Process(item.FlowId, allflow, data, nextstep, _eventid);
                //        }
                //    }
                //    break;
                // //

                //没有终结点的节点必须
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
                        //var flows = allflow.Where(c => c.SourceId == flow.bpmnid).ToList();
                        //var tasks = new BaseRuleTask()
                        //{
                        //    Name = flow.Flowname,
                        //    Eventid = flow.bpmnid,
                        //    id = flow.bpmnid,

                        //    outgoing = new EditableList<BaseRuleFlow>()
                        //};
                        //_context.FlowOperations.Add(new FlowOperation()
                        //{
                        //    bpmnid = flow.bpmnid,
                        //    AddDate = DateTime.Now,
                        //    RuleId = flow.RuleId,
                        //    FlowId = flow.FlowId,
                        //    Data = JsonConvert.SerializeObject(data),
                        //    NodeStatus = 1,
                        //    OperationDesc = "执行任务" + flow.Flowname,
                        //    Step = nextstep,
                        //    EventId = _eventid
                        //});
                        //await _context.SaveChangesAsync();
                        //nextstep++;
                        //var emptyflow = flows.Where(c => c.Conditionexpression == string.Empty).ToList();
                        //foreach (var item in flows.Except(emptyflow))
                        //{
                        //    var rule = new BaseRuleFlow();
                        //    rule.Expression = item.Conditionexpression;
                        //    rule.id = item.bpmnid;
                        //    rule.Name = item.Flowname;
                        //    rule.Eventid = item.bpmnid;
                        //    tasks.outgoing.Add(rule);
                        //}

                        //if (tasks.outgoing.Count > 0)
                        //{
                        //    SimpleFLowExcutor flowExcutor = new SimpleFLowExcutor();
                        //    var result = await flowExcutor.Excute(new FlowExcuteEntity()
                        //    {
                        //        //  Action = null,
                        //        Params = data,
                        //        Task = tasks,
                        //        //   WaitTime = 0

                        //    });
                        //    var next = result.Where(c => c.IsSuccess).ToList();
                        //    foreach (var item in next)
                        //    {
                        //        var nextflow = allflow.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                        //        emptyflow.Add(nextflow);

                        //    }
                        //}

                        //foreach (var item in emptyflow)
                        //{
                        //    await Process(item.FlowId, allflow, data, nextstep, _eventid);
                        //}

                        break;
                    }
            }
        }
    }
}