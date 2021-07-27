using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Models.DeviceMapping
{
    public class Edge
    {
        public string Id { get; set; }

        public string source { get; set; }

        public string target { get; set; }
        public double[] vertices { get; set; }
        public string[] labels { get; set; }
    }
}
