using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
   public class DeviceGraph
    {
        [Key]
        public Guid GraphId { get; set; }
        public Guid DeviceId { get; set; }
        public string GraphShape { get; set; }
        public int  GraphWidth { get; set; }
        public int GraphHeight { get; set; }
        public int GraphPostionX { get; set; }
        public int  GraphPostionY { get; set; }
        public string GraphElementId { get; set; }
        public DateTime? CreateDate { get; set; }
        public Guid Creator { get; set; }

        public DeviceDiagram DeviceDiagram { get; set; }
        public string GraphFill { get; set; }
        public string GraphStroke { get; set; }
        public int GraphStrokeWidth { get; set; }
        public string GraphTextFill { get; set; }
        public int GraphTextFontSize { get; set; }
        public int GraphTextRefX { get; set; }
        public string GraphTextAnchor { get; set; }
        public string GraphTextVerticalAnchor { get; set; }
        public string GraphTextFontFamily { get; set; }
        public int GraphTextRefY { get; set; }

        public Tenant Tenant { get; set; }
        public Customer Customer { get; set; }
    }
}
