using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class AISettingsSetDto
    {
        public string Name { get; set; } = string.Empty;

        public bool Enable { get; set; } = true;
    }
    public class AISettingsDto
    {
 
        public string Name { get; set; } = string.Empty;
        public string MCP_API_KEY { get; set; }=string.Empty;

        public bool Enable { get; set; } = true;
    }
}
