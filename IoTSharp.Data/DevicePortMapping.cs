using System;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
    public class DevicePortMapping
    {
        [Key]
        public Guid MappingId { get; set; }

        public string SourceId { get; set; }
        public string TargeId { get; set; }
        public string SourceElementId { get; set; }
        public string TargetElementId { get; set; }
        public DateTime? CreateDate { get; set; }
        public Guid Creator { get; set; }
        public int MappingStatus { get; set; }
        public int MappingIndex { get; set; }
        public Guid SourceDeviceId { get; set; }
        public Guid TargetDeviceId { get; set; }
    }
}