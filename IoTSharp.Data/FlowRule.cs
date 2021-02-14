using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class FlowRule
    {
        public Guid Id { get; set; }
        public RuleType RuleType { get; set; }
        public string Name { get; set; }

        public string Describes { get; set; }

        public string Runner { get; set; }

        public string ExecutableCode {get;set;}

    }
}
