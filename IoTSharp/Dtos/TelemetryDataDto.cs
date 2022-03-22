using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

    /// <summary>
    /// 查询历史遥测数据请求结构体
    /// </summary>
    public class TelemetryDataQueryDto
    {
        /// <summary>
        /// 要获取的键值， 如果为空， 则为全部
        /// </summary>
        public string keys { get; set; }=String.Empty;  
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
        public TimeSpan every { get; set; }= TimeSpan.Zero;
        /// <summary>
        /// 数据截面计算方式， 
        /// </summary>
        public Aggregate aggregate { get; set; } = Aggregate.None;
    }
}
