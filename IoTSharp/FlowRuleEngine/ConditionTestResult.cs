using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Data;

namespace IoTSharp.FlowRuleEngine
{
    public class ConditionTestResult
    {

        public List<Flow> Passed { get; set; }

        public List<Flow> Failed { get; set; }
    }



    public class ScriptTestResult
    {

        public dynamic Data { get; set; }

        public bool IsExecuted { get; set; }


    }
}
