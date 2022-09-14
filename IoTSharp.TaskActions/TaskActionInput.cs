using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace IoTSharp.TaskActions
{
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