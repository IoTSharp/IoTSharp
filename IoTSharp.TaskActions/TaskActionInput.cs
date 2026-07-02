using IoTSharp.Extensions;
using System;

namespace IoTSharp.TaskActions
{
    public class TaskActionInput
    {
        private dynamic _DynamicOutput;
        private string _value;
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
                _value = JsonObjectSerializer.Serialize(_DynamicOutput);
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
                _DynamicOutput = JsonObjectSerializer.DeserializeUntyped(_value);
            }
        }
    }
}
