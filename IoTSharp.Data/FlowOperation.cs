using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
   public class FlowOperation
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public long OperationId { get; set; } 
        public DateTime? AddDate { get; set; }
        /// <summary>
        /// 节点处理状态，0 创建完
        /// </summary>
        public int? NodeStatus { get; set; }  
        public string OperationDesc { get; set; }
        public string Data  { get; set; }
        public string BizId { get; set; }
        public string bpmnid { get; set; }
        public long FlowId { get; set; }
        public long RuleId { get; set; }
        public long EventId { get; set; }
        public int Step { get; set; }
        public string Tag { get; set; }
    }
}
