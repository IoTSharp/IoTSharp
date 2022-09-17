namespace IoTSharp.Contracts
{
    public class TelemetryDataDto
    {
        public string KeyName { get; set; }

        public DateTime DateTime { get; set; }
        public  DataType DataType { get; set; }
        public object Value { get; set; }
    }
}