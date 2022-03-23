using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class TelemetryDataDto
    {
        public string KeyName { get; set; }

        public DateTime DateTime { get; set; }
        public Data.DataType DataType { get; set; }
        public object Value { get; set; }
    }

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
    public class TimeSpanConverter : System.Text.Json.Serialization.JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    /// <summary>
    /// 查询历史遥测数据请求结构体
    /// </summary>
    public class TelemetryDataQueryDto
    {
        /// <summary>
        /// 要获取的键值， 如果为空， 则为全部
        /// </summary>
        public string keys { get; set; }= string.Empty;  
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime begin { get; set; }
        /// <summary>
        /// 截止时间， 默认为现在。 
        /// </summary>
        public DateTime end { get; set; } = DateTime.Now;
        /// <summary>
        /// 数据截面聚合间隔
        /// </summary>
        /// <example>1.03:14:56:166</example>
        /// <remarks>d.hh:mm:ss:FFF</remarks>
        [Newtonsoft.Json. JsonConverter(typeof(TimespanConverterNewtonsoft))]
        [Newtonsoft.Json.JsonProperty(TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All)]
        [System.Text.Json.Serialization.JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan every { get; set; }= TimeSpan.Zero;
        /// <summary>
        /// 数据截面计算方式， 
        /// </summary>
        public Aggregate aggregate { get; set; } = Aggregate.None;
    }
}
