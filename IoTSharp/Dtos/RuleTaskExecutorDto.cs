using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class RuleTaskExecutorTestDto
    {
        public Guid ruleId { get; set; }
        public Guid flowId { get; set; }
        public string Data { get; set; }

        public string Params { get; set; }
    }


    public class RuleTaskExecutorTestResultDto
    {

     
        public dynamic Data { get; set; }

    }



    public class RuleTaskFlowTestResultDto
    {
        public Guid ruleId { get; set; }
        public Guid flowId { get; set; }
        public string Data { get; set; }


    }



    public class RuleTaskScriptTestResultDto
    {

        public Guid ExecutorId { get; set; }
        public dynamic Data { get; set; }

        public dynamic Params { get; set; }
    }


    public class UpdateFlowExpression
    {
        public Guid FlowId { get; set; }
        public string Expression { get; set; }
    }
}
