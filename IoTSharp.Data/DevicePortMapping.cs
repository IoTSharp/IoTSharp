using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{


    //mapping 对应Edge
  public  class DevicePortMapping
    {
        [System.ComponentModel.DataAnnotations.Key]
        public Guid MappingId { get; set; }
        public string SourceId { get; set; }
        public string TargeId { get; set; }
        public string SourceElementId { get; set; }
        public string TargetElementId { get; set; }
        public DateTime? CreateDate { get; set; }
        public Guid Creator { get; set; }
        public int? MappingStatus { get; set; }
        public decimal? MappingIndex { get; set; }
        public Guid SourceDeviceId { get; set; }
        public Guid TargetDeviceId { get; set; }
    }
}
