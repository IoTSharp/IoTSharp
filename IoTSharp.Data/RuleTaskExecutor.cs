using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
   public class RuleTaskExecutor: IJustMy
    {
        [Key]
        public Guid ExecutorId { get; set; }
        public string ExecutorName { get; set; }
        public string ExecutorDesc { get; set; }
        public string Path { get; set; }
        public string TypeName { get; set; }
        public string DefaultConfig { get; set; }
        public string MataData { get; set; }
        public string Tag { get; set; }

  
        public int ExecutorStatus { get; set; }
        public DateTime AddDateTime { get; set; }
        public Guid Creator { get; set; }


        public int TestStatus { get; set; }
        public Guid Tester { get; set; }
        public DateTime TesterDateTime { get; set; }

        public Tenant Tenant { get; set; }

        public Customer Customer { get; set; }
    }
}
