using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IoTSharp.Data
{
    public class DynamicFormFieldInfo : IJustMy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FieldId { get; set; }

        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public int FieldValueType { get; set; }
        public long FormId { get; set; }
        public Guid Creator { get; set; }
        public DateTime? FieldCreateDate { get; set; }
        public DateTime? FieldEditDate { get; set; }
        public string FieldCode { get; set; }
        public string FieldUnit { get; set; }
        public bool IsRequired { get; set; }
        public bool IsEnabled { get; set; }
        public int FieldStatus { get; set; }
        public string FieldI18nKey { get; set; }
        public string FieldValueDataSource { get; set; }
        public string FieldValueLocalDataSource { get; set; }
        public string FieldPattern { get; set; }
        public int FieldMaxLength { get; set; }
        public string FieldValueTypeName { get; set; }
        public long FieldUIElement { get; set; }
        public string FieldUIElementSchema { get; set; }
        public string FieldPocoTypeName { get; set; }
        public Tenant Tenant { get; set; }
        public Customer Customer { get; set; }
    }
}