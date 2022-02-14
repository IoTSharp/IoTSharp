using System;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
    public class Flow:IJustMy
    {
        [Key]
        public Guid FlowId { get; set; }

        public string bpmnid { get; set; }
        public string Flowname { get; set; }
        public FlowRule FlowRule { get; set; }
        public string Flowdesc { get; set; }
        public string ObjectId { get; set; }
        public string FlowType { get; set; }
        public string SourceId { get; set; }
        public string TargetId { get; set; }
        public string NodeProcessClass { get; set; }
        public string Conditionexpression { get; set; }
        public string NodeProcessMethod { get; set; }
        public string NodeProcessParams { get; set; }
        public string NodeProcessType { get; set; }
        public string NodeProcessScriptType { get; set; }
        public string NodeProcessScript { get; set; }
        public string Incoming { get; set; }
        public string Outgoing { get; set; }
        public int FlowStatus { get; set; }
        public int TestStatus { get; set; }
        public Guid Tester { get; set; }
        public DateTime TesterDateTime { get; set; }
        public Guid CreateId { get; set; }

        public DateTime CreateDate { get; set; }

        public Guid Createor { get; set; }

        public RuleTaskExecutor Executor { get; set; }


        public Tenant Tenant { get; set; }

        public Customer Customer { get; set; }

    }
}