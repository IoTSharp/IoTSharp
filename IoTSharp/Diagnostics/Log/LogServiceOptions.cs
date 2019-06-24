namespace IoTSharp.Diagnostics.Log
{
    public class LogServiceOptions
    {
        public const string Filename = "LogServiceConfiguration.json";

        public int MessageCount { get; set; } = 1000;
    }
}
