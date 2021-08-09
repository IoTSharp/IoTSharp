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
        public int? NodeStatus { get; set; }
        public string OperationDesc { get; set; }
        public string Data  { get; set; }
        public string BizId { get; set; }
        public long FlowId { get; set; }
        public long RuleId { get; set; }
        public int Step { get; set; }
        public string Tag { get; set; }
    }
}
