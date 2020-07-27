using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.FlowRuleEngine
{
    public interface IRuleNode
    {
        string name { get; set; }
        string InputData { get; set; }
        public void Run();
        string OuputData { get; set; }
    }
}
