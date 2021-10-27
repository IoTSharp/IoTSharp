using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.FlowRuleEngine
{
    public interface ITaskExecutor
    {

        public TaskExecutorResult Execute(TaskExecutorParam param);


    }


    public interface IExecutEntity
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

    }

    public class TaskExecutorResult
    {
    
        public dynamic Result { get; set; }
    }

    public class TaskExecutorParam
    {
        public IExecutEntity ExecutEntity { get; set; }
        public string Param { get; set; }

    }
}
