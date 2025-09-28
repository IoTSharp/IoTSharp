using IoTSharp.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class AISettings
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string MCP_API_KEY { get; set; }=string.Empty;

        public UserRole Role { get; set; } = UserRole.Anonymous;

        public bool Enable { get; set; } = true;
    }
}
