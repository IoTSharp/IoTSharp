using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using IoTSharp.Data;
using Newtonsoft.Json;

namespace IoTSharp.FlowRuleEngine.Models.Task
{


    interface IFlowProcessor
    {


    }

    public class SimpleProcessor
    {


        public SimpleProcessor(ApplicationDbContext context)
        {
            _context = context;
        }

        private ApplicationDbContext _context;



        public async System.Threading.Tasks.Task Start(long ruleid, object? data, Guid creator, string BizId)
        {
            var _event = new BaseEvent()
            {
                CreaterDateTime = DateTime.Now,
                Creator = creator,
                EventDesc = "测试",
                EventName = "测试",
                MataData = JsonConvert.SerializeObject(data),
                RuleId = ruleid,
                Bizid = BizId,
                Type = EventType.TestPurpose,
                EventStaus = 1
            };
            _context.BaseEvents.Add(_event);
            _context.SaveChanges();
            var flows = _context.Flows.Where(c => c.RuleId == ruleid && c.FlowType != "label").ToList();
            var start = flows.FirstOrDefault(c => c.FlowType == "bpmn:StartEvent");


            var startoperation = new FlowOperation()
            {
                bpmnid = start.bpmnid,
                AddDate = DateTime.Now,
                RuleId = start.RuleId,
                FlowId = start.FlowId,
                Data = JsonConvert.SerializeObject(data),
                NodeStatus = 1,
                OperationDesc = "开始处理",
                Step = 1,
                EventId = _event.EventId
            };
            _context.FlowOperations.Add(startoperation);
            await _context.SaveChangesAsync();
            var nextflows = await ProcessCondition(start.FlowId, data);

            foreach (var item in nextflows)
            {
                var flowOperation = new FlowOperation()
                {
                    AddDate = DateTime.Now,
                    RuleId = item.RuleId,
                    FlowId = item.FlowId,
                    Data = JsonConvert.SerializeObject(data),
                    NodeStatus = 1,
                    OperationDesc = "执行条件（" + (string.IsNullOrEmpty(item.Conditionexpression)
                        ? "空条件"
                        : item.Conditionexpression) + ")",
                    Step = startoperation.Step++,
                    bpmnid = item.bpmnid,
                    EventId = _event.EventId
                };
                _context.FlowOperations.Add(flowOperation);
                await _context.SaveChangesAsync();
                await Process(flowOperation.OperationId, data);
            }





            //       await Process(start, data,1, _event.EventId);


        }

        private async Task<List<Flow>> ProcessCondition(long FlowId, dynamic data)
        {
            var flow = _context.Flows.SingleOrDefault(c => c.FlowId == FlowId);
            var flows = _context.Flows.Where(c => c.SourceId == flow.bpmnid).ToList();
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
                    Params = data,  //也可以放自定义数据
                    Task = tasks,
                    //   WaitTime = 0

                });
                var next = result.Where(c => c.IsSuccess).ToList();
                foreach (var item in next)
                {
                    var nextflow = flows.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                    emptyflow.Add(nextflow);

                }

                return emptyflow;
            }

            return null;
        }

        // 由operationid唤起处理流程
        public async System.Threading.Tasks.Task Process(long operationid, object data)
        {

            var peroperation = _context.FlowOperations.SingleOrDefault(c => c.OperationId == operationid);

            if (peroperation == null
            )
            {
                return;
            }

            var flow = _context.Flows.SingleOrDefault(c => c.FlowId == peroperation.FlowId);
            var allflow = _context.Flows.Where(c => c.RuleId == flow.RuleId).ToList();
            switch (flow.FlowType)
            {
                case "bpmn:SequenceFlow":
                    var t = allflow.FirstOrDefault(c => c.bpmnid == flow.TargetId);
                    var operation = new FlowOperation()
                    {
                        AddDate = DateTime.Now,
                        RuleId = flow.RuleId,
                        FlowId = flow.FlowId,
                        Data = JsonConvert.SerializeObject(data),
                        NodeStatus = 1,
                        OperationDesc = "执行条件（" + (string.IsNullOrEmpty(flow.Conditionexpression)
                            ? "空条件"
                            : flow.Conditionexpression) + ")",
                        Step = peroperation.Step++,
                        bpmnid = flow.bpmnid,
                        EventId = peroperation.EventId
                    };
                    _context.FlowOperations.Add(operation);
                    await _context.SaveChangesAsync();
                    await Process(operation.OperationId, data);
                    break;
                case "bpmn:Task":
                    {
                        var taskoperation = new FlowOperation()
                        {
                            bpmnid = flow.bpmnid,
                            AddDate = DateTime.Now,
                            RuleId = flow.RuleId,
                            FlowId = flow.FlowId,
                            Data = JsonConvert.SerializeObject(data),
                            NodeStatus = 1,
                            OperationDesc = "执行任务" + flow.Flowname,
                            Step = peroperation.Step++,
                            EventId = peroperation.EventId
                        };
                        _context.FlowOperations.Add(taskoperation);
                        await _context.SaveChangesAsync();




                        //脚本处理
                        if (!string.IsNullOrEmpty(flow.NodeProcessScriptType) && !string.IsNullOrEmpty(flow.NodeProcessScript))
                        {
                            switch (flow.NodeProcessScriptType)
                            {
                                case "csharp":
                                    var scripts = flow.NodeProcessScript;

                                    //脚本处理逻辑
                                    break;
                                case "python":
                                    break;
                                case "xml":
                                    break;
                                case "json":
                                    break;
                                case "bat":
                                    break;
                                case "sql":
                                    break;
                            }
                        }

                        // 执行任务，完成后


                        //如果是异步调用，可以在此终止，然后通过FlowOperation表中当前的taskoperation.OperationId找到当前挂起的FlowId 再次恢复处理

                        //条件处理
                        //    var flows = allflow.Where(c => c.SourceId == flow.bpmnid).ToList();
                        //    var tasks = new BaseRuleTask()
                        //    {
                        //        Name = flow.Flowname,
                        //        Eventid = flow.bpmnid,
                        //        id = flow.bpmnid,

                        //        outgoing = new EditableList<BaseRuleFlow>()
                        //    };



                        //    var emptyflow = flows.Where(c => c.Conditionexpression == string.Empty).ToList();

                        //    foreach (var item in flows.Except(emptyflow))
                        //    {
                        //        var rule = new BaseRuleFlow();
                        //        rule.Expression = item.Conditionexpression;
                        //        rule.id = item.bpmnid;
                        //        rule.Name = item.Flowname;
                        //        rule.Eventid = item.bpmnid;
                        //        tasks.outgoing.Add(rule);
                        //    }
                        //    nextstep++;

                        //    if (tasks.outgoing.Count > 0)
                        //    {
                        //        SimpleFLowExcutor flowExcutor = new SimpleFLowExcutor();
                        //        var result = await flowExcutor.Excute(new FlowExcuteEntity()
                        //        {
                        //            //    Action = null,
                        //            Params = data,
                        //            Task = tasks,
                        //            //   WaitTime = 0

                        //        });
                        //        var next = result.Where(c => c.IsSuccess).ToList();
                        //        foreach (var item in next)
                        //        {

                        //            var nextflow = allflow.FirstOrDefault(a => a.bpmnid == item.Rule.SuccessEvent);
                        //            emptyflow.Add(nextflow);
                        //        }
                        //    }


                        //    foreach (var item in emptyflow)
                        //    {
                        //        await Process(item, allflow, data, nextstep, _eventid);
                        //    }
                    }

                    break;
                case "bpmn:EndEvent":
                    // 合并结束
                    var end = _context.FlowOperations.FirstOrDefault(c => c.bpmnid == flow.bpmnid && c.EventId == peroperation.EventId) ?? new FlowOperation();


                    end.bpmnid = flow.bpmnid;
                    end.AddDate = DateTime.Now;
                    end.RuleId = flow.RuleId;
                    end.FlowId = flow.FlowId;
                    end.Data = JsonConvert.SerializeObject(data);
                    end.NodeStatus = 1;
                    end.OperationDesc = "处理完成";
                    end.Step = _context.FlowOperations.Where(c => c.EventId == peroperation.EventId).Max(c => c.Step) + 1;
                    end.EventId = peroperation.EventId;

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
