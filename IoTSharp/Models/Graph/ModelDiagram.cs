using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Models.Graph
{
    public class ModelDiagram
    {
        public long DiagramId { get; set; }
        public Mapping[] mappings { get; set; }
        public Shape[] shapes { get; set; }


        public string DiagramName { get; set; }
        public string DiagramDesc { get; set; }
        public string DiagramImage { get; set; }
    }
}
