using System;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;

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

    public class ModelAssetAttrItem
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string keyName { get; set; }
        public DataCatalog dataSide { get; set; }
        public string Name { get; set; }
        
    }

    public class AssetDeviceItem
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public DeviceType DeviceType { get; set; }



        public int Timeout { get; set; }

        public DeviceIdentity DeviceIdentity { get; set; }

        public ModelAssetAttrItem[] Attrs { get; set; }

        public ModelAssetAttrItem[] Temps { get; set; }
    }
}
