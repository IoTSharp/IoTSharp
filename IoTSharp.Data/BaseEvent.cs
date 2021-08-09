using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
   public class BaseEvent
    {

        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]

        public long EventId { get; set; }
        public string EventName { get; set; }
        public string EventDesc { get; set; }
        public int EventStaus { get; set; }
        public int Type { get; set; }
        public string MataData { get; set; }
        public Guid Creator { get; set; }
        public DateTime CreaterDateTime { get; set; }
    }
}
