using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
  public  class DeviceGraphToolBox
    {
        [Key]
        public Guid ToolBoxId { get; set; }
        public string ToolBoxName { get; set; }
        public string ToolBoxIcon { get; set; }
        public int? ToolBoxStatus { get; set; }
        public string ToolBoxRequestUri { get; set; }
        public string ToolBoxType { get; set; }
        public long? DeviceId { get; set; }
        public decimal? ToolBoxOffsetX { get; set; }
        public decimal? ToolBoxOffsetY { get; set; }
        public decimal? ToolBoxOffsetTopPer { get; set; }
        public decimal? ToolBoxOffsetLeftPer { get; set; }
        public DateTime? CreateDate { get; set; }
        public Guid Creator { get; set; }
        public string CommondParam { get; set; }
        public string CommondType { get; set; }
    }
}
