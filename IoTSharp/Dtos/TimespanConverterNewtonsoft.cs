using System;

namespace IoTSharp.Dtos
{
    public class TimespanConverterNewtonsoft : Newtonsoft.Json.JsonConverter<TimeSpan>
    {
        /// <summary>
        /// Format: Days.Hours:Minutes:Seconds:Milliseconds
        /// </summary>
        public const string TimeSpanFormatString = @"d\.hh\:mm\:ss\:FFF";

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, TimeSpan value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var timespanFormatted = $"{value.ToString(TimeSpanFormatString)}";
            writer.WriteValue(timespanFormatted);
        }

        public override TimeSpan ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            TimeSpan parsedTimeSpan;
            TimeSpan.TryParseExact((string)reader.Value, TimeSpanFormatString, null, out parsedTimeSpan);
            return parsedTimeSpan;
        }
    }
}
