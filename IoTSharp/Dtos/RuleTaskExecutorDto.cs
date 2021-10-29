using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class RuleTaskExecutorTestDto
    {

        public Guid ExecutorId { get; set; }
        public dynamic Data { get; set; }
    }


    public class RuleTaskExecutorTestResultDto
    {

        public Guid ExecutorId { get; set; }
        public dynamic Data { get; set; }

        public dynamic Params { get; set; }
    }

}
