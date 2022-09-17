using IoTSharp.Contracts;
using System;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
   

    public class Alarm: IJustMy
    {
        /// <summary>
        /// 告警ID
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// 警告类型
        /// </summary>
        public string AlarmType { get; set; }

        /// <summary>
        /// 警告细节描述
        /// </summary>
        public string AlarmDetail { get; set; }

        /// <summary>
        /// 警告创建时间
        /// </summary>
        public DateTime AckDateTime { get; set; }

        /// <summary>
        /// 手动清除时间
        /// </summary>
        public DateTime ClearDateTime { get; set; }

        /// <summary>
        /// 警告持续的开始时间
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// 结束时间， 当警告自动或者手动确实得到处理的时间
        /// </summary>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// 告警状态
        /// </summary>
        public AlarmStatus AlarmStatus { get; set; }

        /// <summary>
        /// 严重成都
        /// </summary>
        public ServerityLevel Serverity { get; set; }

        /// <summary>
        /// 如果严重等级变化了， 那么就触发传播， 否则不触发传播， 传播意思就是执行规则链， 通过规则链往后传播状态
        /// </summary>
        public bool Propagate { get; set; }

        /// <summary>
        /// 起因对象的Id
        /// </summary>
        public Guid OriginatorId { get; set; }

        /// <summary>
        /// 起因设备类型
        /// </summary>
        public OriginatorType OriginatorType { get; set; }

        /// <summary>
        /// 租户
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public Customer Customer { get; set; }
    }
}