using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class DeviceModelDto
    {
        public Guid DeviceModelId { get; set; }
        public string ModelName { get; set; }
        public string ModelDesc { get; set; }


    }

    public class DeviceModelCommandDto
    {
        public Guid CommandId { get; set; }
        public string CommandTitle { get; set; }
        public string CommandI18N { get; set; }

        public int CommandType { get; set; }

        public string CommandParams { get; set; }

        public string CommandName { get; set; }
        public string CommandTemplate { get; set; }

        public Guid DeviceModelId { get; set; }


    }
}
