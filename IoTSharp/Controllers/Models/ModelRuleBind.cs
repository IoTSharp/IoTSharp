using System;

namespace IoTSharp.Controllers.Models
{
    public class ModelRuleBind
    {
        public Guid rule { get; set; }

        public Guid[] dev { get; set; }
    }
}
