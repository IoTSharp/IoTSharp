using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.FlowRuleEngine.Models.Task
{

    public interface ITaskExcutor
    {

       void DoWork(ExcuteEntity entity);

    }

    public class ExcuteEntity
    {

       

    }
}
