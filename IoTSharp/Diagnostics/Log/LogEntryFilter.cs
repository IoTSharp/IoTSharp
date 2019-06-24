namespace IoTSharp.Diagnostics.Log
{
    public class LogEntryFilter
    {
        public bool IncludeInformations { get; set; }

        public bool IncludeWarnings { get; set; }

        public bool IncludeErrors { get; set; }

        public int TakeCount { get; set; }
    }
}