using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class BaseDictionaryGroup
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]

        public long DictionaryGroupId { get; set; }
        public string DictionaryGroupName { get; set; }
        public string DictionaryGroupKey { get; set; }
        public int? DictionaryGroupValueType { get; set; }
        public int? DictionaryGroupStatus { get; set; }
        public string DictionaryGroupValueTypeName { get; set; }
        public string DictionaryGroupDesc { get; set; }
        public string DictionaryGroup18NKeyName { get; set; }
    }
}
