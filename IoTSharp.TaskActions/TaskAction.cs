using System;
using System.Threading.Tasks;

namespace IoTSharp.TaskActions
{
    public abstract class TaskAction
    {
        public abstract Task<TaskActionOutput> ExecuteAsync(TaskActionInput _input);

        public IServiceProvider ServiceProvider { get; set; }
    }
}