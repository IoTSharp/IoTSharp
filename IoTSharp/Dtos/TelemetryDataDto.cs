using IoTSharp.Contracts;
using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{

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
