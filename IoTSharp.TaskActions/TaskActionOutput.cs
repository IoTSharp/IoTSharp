using IoTSharp.Extensions;
using System;
using System.Dynamic;

namespace IoTSharp.TaskActions
{
    public class TaskActionOutput
    {
        private string _value;
        private dynamic _DynamicOutput;
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
                _value = JsonObjectSerializer.Serialize(_DynamicOutput);
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
                _DynamicOutput = JsonObjectSerializer.DeserializeExpando(_value);
            }
        }
    }
}
