using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.FlowRuleEngine
{
    public interface ITaskExcutor
    {

        public TaskExcutorResult Excute(TaskExcutorParam param);


    }


    public interface IExcutEntity
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

    }

    public class TaskExcutorResult
    {
    
        public dynamic Result { get; set; }
    }

    public class TaskExcutorParam
    {
        public IExcutEntity ExcutEntity { get; set; }
        public string Param { get; set; }

    }
}
