using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace IoTSharp.TaskAction
{
    public abstract class ITaskAction
    {
        public abstract Task<TaskActionOutput> ExecuteAsync(TaskActionInput _input);

        public IServiceProvider ServiceProvider { get; set; }
    }

    public class TaskActionOutput
    {
        private string _value;
        private dynamic _DynamicOutput;
        private readonly ExpandoObjectConverter expConverter = new();
        public Guid DeviceId { get; set; }
        public bool ExecutionStatus { get; set; }
        public string ExecutionInfo { get; set; }

        public dynamic DynamicOutput
        {
            get
            {
                return _DynamicOutput;
            }
            set
            {
                _DynamicOutput = value;
                _value = JsonConvert.SerializeObject(_DynamicOutput, expConverter);
            }
        }

        public string Output
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _DynamicOutput = JsonConvert.DeserializeObject<ExpandoObject>(_value, expConverter);
            }
        }
    }

    public class TaskActionInput
    {
        private dynamic _DynamicOutput;
        private string _value;
        private readonly ExpandoObjectConverter expConverter = new();
        public Guid DeviceId { get; set; }
        public String ExecutorConfig { get; set; }

        public dynamic DynamicInput
        {
            get
            {
                return _DynamicOutput;
            }
            set
            {
                _DynamicOutput = value;
                _value = JsonConvert.SerializeObject(_DynamicOutput, expConverter);
            }
        }

        public string Input
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _DynamicOutput = JsonConvert.DeserializeObject<object>(_value, expConverter);
            }
        }
    }
}