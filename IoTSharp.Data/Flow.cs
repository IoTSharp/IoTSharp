using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
   public class Flow
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public long FlowId { get; set; }
        public string bpmnid { get; set; }
        public string Flowname { get; set; }
        public long RuleId { get; set; }
        public string Flowdesc { get; set; }
        public string ObjectId { get; set; }
        public string FlowType { get; set; }
        public string SourceId { get; set; }
        public string TargetId { get; set; }
        public string NodeProcessClass { get; set; }
        public string Conditionexpression { get; set; }
        public string NodeProcessMethod { get; set; }
        public string NodeProcessParams { get; set; }
        public string NodeProcessType{ get; set; }
        public string NodeProcessScriptType { get; set; }
        public string NodeProcessScript { get; set; }
        public string Incoming { get; set; }
        public string Outgoing { get; set; }
    }
}
