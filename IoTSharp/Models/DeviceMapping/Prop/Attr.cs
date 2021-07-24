using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Models.DeviceMapping.Prop
{
    
    public class Attr
    {
        public Body portBody { get; set; }
        public Text text { get; set; }
    }
    public class Position
    {
        public string name { get; set; }

    }
    public class Text {
        public string text { get; set; } }
    public class Label
    {
        public string position { get; set; }
    }

    
}
