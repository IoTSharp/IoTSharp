using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{

    public class ProduceData : DataStorage
    {
      public  Produce Owner { get; set; }
    }
}
