using System;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using MongoDB.Bson.Serialization.Serializers;

namespace IoTSharp.Models
{
    public class ModelAssetDevice
    {
        public Guid AssetId { get; set; }


        public Guid Deviceid { get; set; }


        public string Devicename { get; set; }
        public ModelAssetAttrItem[] Attrs { get; set; }

        public ModelAssetAttrItem[] Temps { get; set; }
    }


    public class ModelAddAssetDevice
    {

        /// <summary>
        /// 资产Id
        /// </summary>
        public Guid AssetId { get; set; }
        /// <summary>
        /// 要导入的设备Id
        /// </summary>

        public Guid Deviceid { get; set; }

        /// <summary>
        /// 要导入的设备属性数据列表
        /// </summary>
        public ModelAddAssetDeviceItem[] Attrs { get; set; }


        /// <summary>
        /// 要导入的设备遥测数据列表
        /// </summary>
        public ModelAddAssetDeviceItem[] Temps { get; set; }




        public class ModelAddAssetDeviceItem
        {
            /// <summary>
            /// 属性或者遥测的描述
            /// </summary>
            public string Description { get; set; }


            /// <summary>
            ///属性或者遥测的KeyName
            /// </summary>
            public string keyName { get; set; }

            /// <summary>
            ///数据侧
            /// </summary>
            public DataCatalog dataSide { get; set; }


            /// <summary>
            /// 属性或者遥测的KeyName的别名
            /// </summary>
            public string Name { get; set; }



        }

    }



    public class ModelEditAssetAttrItem
    {/// <summary>
     ///  关联关系的id 
     /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///  属性或者遥测的描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///  属性或者遥测的KeyName的别名
        /// </summary>
        public string Name { get; set; }

    }



    public class ModelAssetAttrItem
    {/// <summary>
     ///  关联关系的id 
     /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///  属性或者遥测的描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///  属性或者遥测的KeyName
        /// </summary>
        public string keyName { get; set; }


        /// <summary>
        ///  属性或者遥测的数据侧
        /// </summary>
        public DataCatalog dataSide { get; set; }


        /// <summary>
        ///  属性或者遥测的别名
        /// </summary>
        public string Name { get; set; }

    }

    public class AssetDeviceItem
    {/// <summary>
     /// 设备名称
     /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 设备Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public DeviceType DeviceType { get; set; }


        /// <summary>
        /// 超时
        /// </summary>

        public int Timeout { get; set; }
        /// <summary>
        /// 设备认证方式
        /// </summary>
        public DeviceIdentity DeviceIdentity { get; set; }
        /// <summary>
        /// 属性数据
        /// </summary>
        public ModelAssetAttrItem[] Attrs { get; set; }
        /// <summary>
        /// 遥测数据
        /// </summary>
        public ModelAssetAttrItem[] Temps { get; set; }
    }
}
