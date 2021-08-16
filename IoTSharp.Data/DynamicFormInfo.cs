using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class DynamicFormInfo
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]

        public long FormId { get; set; }
        public long? BizId { get; set; }
        public long? FormCreator { get; set; }
        public string FormName { get; set; }
        public string FormDesc { get; set; }
        public int? FormStatus { get; set; }
        public string FormSchame { get; set; }
        public string ModelClass { get; set; }
        public string Url { get; set; }
        public Guid Creator { get; set; }
        public DateTime? FromCreateDate { get; set; }
        public string FormLayout { get; set; } //horizontal  vertical inline

        public bool IsCompact { get; set; }
    }
}
