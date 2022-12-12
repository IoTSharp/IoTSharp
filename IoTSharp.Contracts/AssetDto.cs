 
namespace IoTSharp.Contracts
{
    public class AssetDto
    {
        /// <summary>
        /// 资产Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 资产名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 资产描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 资产类型
        /// </summary>
        public string AssetType { get; set; }
    }
    public class AssetAddDto
    {
        /// <summary>
        /// 资产名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 资产描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 资产类型
        /// </summary>
        public string AssetType { get; set; }
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