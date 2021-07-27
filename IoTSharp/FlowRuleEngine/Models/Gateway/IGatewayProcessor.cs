using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.FlowRuleEngine.Models.Gateway
{
    public interface IGatewayProcessor
    {
        public void Process();
    }

    public abstract class InclusiveGatewayProcess
    {
        public abstract string[] Process(string EventId, int ProjectId, int DefinitionsId, string[] TargetId);
        public abstract int[] Process(int FlowId);

    }


    public abstract class ExclusiveGatewayProcess
    {
        public abstract string Process(string EventId, int ProjectId, int DefinitionsId, string[] TargetId);

        public abstract int Process(int FlowId);
    }
}


