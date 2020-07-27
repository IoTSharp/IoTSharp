using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.FlowRuleEngine
{
    public class IoTSharpRoot : ISwitcher
    {
        public IoTSharpRoot(string json)
        {

        }
        public string Switcher => nameof(IoTSharpRoot);
        public IRuleNode InputNode { get; set; }
        public List<IRuleNode> OuputNodes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
