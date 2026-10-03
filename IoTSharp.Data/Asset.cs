using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class Asset
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public string AssetType { get; set; }
        /// <summary>
        /// 资产关联的设备数据点。关系记录保存设备运行数据映射，资产本身不承载设备运行状态。
        /// </summary>
        public List<AssetRelation> OwnedAssets { get; set; } = new();
        public Customer Customer { get; set; }
        public Tenant Tenant { get; set; }
        public bool Deleted { get; set; }
    }
}
