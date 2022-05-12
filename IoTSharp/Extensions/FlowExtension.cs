using IoTSharp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace IoTSharp.Extensions
{
    public static class FlowExtension
    {

        public static void SaveFlowResult(this ApplicationDbContext _context, Guid DeviceId, Guid RuleId, List<FlowOperation> result)
        {

            if (_context.DeviceRules.Any(c =>
                                c.Device.Id == DeviceId && c.FlowRule.RuleId == RuleId && c.EnableTrace == 1))
            {
                if (result.Count > 0)
                {

                    var flowRule = _context.FlowRules.SingleOrDefault(c => c.RuleId == RuleId);

                    var flows = _context.Flows.Where(c => c.FlowRule == flowRule).ToList();

                    var operevent =
                        _context.BaseEvents.SingleOrDefault(
                            c => c.EventId == result.FirstOrDefault().BaseEvent.EventId);

                    var list = result.Select(c => new FlowOperation
                    {
                        AddDate = c.AddDate,
                        BizId = c.BizId,
                        Data = c.Data,
                        BaseEvent = operevent,
                        Flow = flows.SingleOrDefault(x => x.FlowId == c.Flow.FlowId
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

                }


            }
            else
            {
                if (DeviceId == Guid.Empty)
                {
                    if (result.Count > 0)
                    {

                        var flowRule = _context.FlowRules.SingleOrDefault(c => c.RuleId == RuleId);

                        var flows = _context.Flows.Where(c => c.FlowRule == flowRule).ToList();

                        var operevent =
                            _context.BaseEvents.SingleOrDefault(
                                c => c.EventId == result.FirstOrDefault().BaseEvent.EventId);

                        var list = result.Select(c => new FlowOperation
                        {
                            AddDate = c.AddDate,
                            BizId = c.BizId,
                            Data = c.Data,
                            BaseEvent = operevent,
                            Flow = flows.SingleOrDefault(x => x.FlowId == c.Flow.FlowId
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

                    }

                }


            }
        }
    }
}
