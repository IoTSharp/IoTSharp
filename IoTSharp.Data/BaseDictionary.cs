using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class BaseDictionary
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]

        public long DictionaryId { get; set; }
        public string DictionaryName { get; set; }
        public string DictionaryValue { get; set; }
        public string Dictionary18NKeyName { get; set; }
        public int? DictionaryStatus { get; set; }
        public int? DictionaryValueType { get; set; }
        public string DictionaryValueTypeName { get; set; }
        public long? DictionaryGroupId { get; set; }
        public string DictionaryPattern { get; set; }
        public string DictionaryDesc { get; set; }
        public string DictionaryColor { get; set; }
        public string DictionaryIcon { get; set; }
        public string DictionaryTag { get; set; }
    }
}
