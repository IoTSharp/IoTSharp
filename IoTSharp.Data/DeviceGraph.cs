using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
   public class DeviceGraph
    {
        public Guid DeviceId { get; set; }
        public string GraphShape { get; set; }
        public decimal? GraphWidth { get; set; }
        public decimal? GraphHeight { get; set; }
        public decimal? GraphPostionX { get; set; }
        public decimal? GraphPostionY { get; set; }
        public string GraphElementId { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? Creator { get; set; }
        public long GraphId { get; set; }
        public Guid DiagramId { get; set; }
        public string GraphFill { get; set; }
        public string GraphStroke { get; set; }
        public decimal? GraphStrokeWidth { get; set; }
        public string GraphTextFill { get; set; }
        public decimal? GraphTextFontSize { get; set; }
        public decimal? GraphTextRefX { get; set; }
        public string GraphTextAnchor { get; set; }
        public string GraphTextVerticalAnchor { get; set; }
        public string GraphTextFontFamily { get; set; }
        public decimal? GraphTextRefY { get; set; }
    }
}
