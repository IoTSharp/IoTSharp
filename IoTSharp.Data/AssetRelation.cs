using IoTSharp.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class AssetRelation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// 所属资产。通过显式外键保持关系查询的租户和资产边界。
        /// </summary>
        // 历史迁移允许为空，保持旧数据和各 provider 快照兼容。
        public Guid? AssetId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Asset Asset { get; set; }
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
