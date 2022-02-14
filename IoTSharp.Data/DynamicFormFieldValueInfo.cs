using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class DynamicFormFieldValueInfo :IJustMy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public long FieldValueId { get; set; }
        public long FieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public long FromId { get; set; }
        public Guid Creator { get; set; }
        public DateTime? FieldCreateDate { get; set; }
        public string FieldCode { get; set; }
        public string FieldUnit { get; set; }
        public long FieldValueType { get; set; }
        public long BizId { get; set; }
        public Tenant Tenant { get; set; }
        public Customer Customer { get; set; }
    }
}
