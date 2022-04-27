using IoTSharp.Data;
using System;
using Avro.Util;

namespace IoTSharp.Dtos
{
    public class DeviceAlarmDto
    {
        /// <summary>
        /// 起因设备的名称或者GUID
        /// </summary>
        [Newtonsoft.Json.JsonRequired]
        public string OriginatorName { get; set; }

        /// <summary>
        /// 警告类型
        /// </summary>
        [Newtonsoft.Json.JsonRequired]
        public string AlarmType { get; set; }

        /// <summary>
        /// 警告细节描述
        /// </summary>
        public string AlarmDetail { get; set; }

        /// <summary>
        /// 严重成都
        /// </summary>
        public ServerityLevel Serverity { get; set; } = ServerityLevel.Indeterminate;


        public Guid warnDataId { get; set; }
    }

    public class CreateAlarmDto : DeviceAlarmDto
    {

        public DateTime CreateDateTime { get; set; }
        /// <summary>
        /// 起因设备类型
        /// </summary>
        public OriginatorType OriginatorType { get; set; }
    }

    public class AlarmDto
    {
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
        /// 传播
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

        public object Originator { get; set; }
    }


    public class AlarmStatusDto
    {
        public Guid Id { get; set; }

        public AlarmStatus AlarmStatus { get; set; }

    }

    public class AssetDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? AssetType { get; set; }
    }

    public class AssetRelationDto
    {
        public Guid Id { get; set; }

        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 设备Id
        /// </summary>
        public Guid DeviceId { get; set; }

        /// <summary>
        /// 数据类型， 是遥测， 还是属性
        /// </summary>
        public DataCatalog DataCatalog { get; set; }

        /// <summary>
        /// 对应的键名称
        /// </summary>
        public string KeyName { get; set; }
    }
}