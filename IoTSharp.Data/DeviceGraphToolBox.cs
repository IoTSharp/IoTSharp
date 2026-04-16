using System;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
    public class DeviceGraphToolBox : IJustMy
    {
        [Key]
        public Guid ToolBoxId { get; set; }

        public string ToolBoxName { get; set; }
        public string ToolBoxIcon { get; set; }
        public int ToolBoxStatus { get; set; }
        public string ToolBoxRequestUri { get; set; }
        public string ToolBoxType { get; set; }
        public long DeviceId { get; set; }
        public int ToolBoxOffsetX { get; set; }
        public int ToolBoxOffsetY { get; set; }
        public int ToolBoxOffsetTopPer { get; set; }
        public int ToolBoxOffsetLeftPer { get; set; }
        public DateTime? CreateDate { get; set; }
        public Guid Creator { get; set; }
        public string CommondParam { get; set; }
        public string CommondType { get; set; }
        public Tenant Tenant { get; set; }
        public Customer Customer { get; set; }
    }
}