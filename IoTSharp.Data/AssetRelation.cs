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
        public Guid Id { get; set; }
        /// <summary>
        /// 设备Id
        /// </summary>
        public Guid DeviceId { get; set; }
        /// <summary>
        /// 数据类型， 是遥测， 还是属性
        /// </summary>
        public DataCatalog  DataCatalog { get; set; }
        /// <summary>
        /// 对应的键名称
        /// </summary>
        public string KeyName { get; set; }

    }
}
