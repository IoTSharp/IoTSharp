using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.FlowRuleEngine
{
    public interface ISwitcher
    {
        string Switcher { get;  }
        IRuleNode InputNode { get; set; }
        List<IRuleNode> OuputNodes { get; set; }
    }
}
