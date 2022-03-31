using System;
using IoTSharp.Dtos;

namespace IoTSharp.Models
{
    public class ModelAssetDevice
    {
        public Guid AssetId { get; set; }

        public AssetRelationDto[] Relations { get; set; }
    }



}
