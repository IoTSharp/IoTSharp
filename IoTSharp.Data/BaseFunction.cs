using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
   public class BaseFunction
    {[Key]
        public long FunctionId { get; set; }
        public string FunctionName { get; set; }
        public string FunctionController { get; set; }
        public string FunctionAction { get; set; }
        public string FunctionDesc { get; set; }
        public string FunctionRoute { get; set; }
        public int FunctionType{ get; set; }
        public long FunctionParent { get; set; }
        public int FunctionStatus { get; set; }
    }
}
