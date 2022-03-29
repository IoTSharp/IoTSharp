using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IoTSharp.Data
{
    public class BaseI18N
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public Guid UserId { get; set; }
        public int Status { get; set; }
        public string KeyName { get; set; }
        public string ValueBG { get; set; }
        public string ValueCS { get; set; }
        public string ValueDA { get; set; }
        public string ValueDEDE { get; set; }
        public string ValueESES { get; set; }
        public string ValueENUS { get; set; }
        public string ValueENGR { get; set; }
        public string ValueELGR { get; set; }
        public string ValueFI { get; set; }
        public string ValueFRFR { get; set; }
        public string ValueHE { get; set; }
        public string ValueHRHR { get; set; }
        public string ValueHU { get; set; }
        public string ValueITIT { get; set; }
        public string ValueJAJP { get; set; }
        public string ValueKOKR { get; set; }
        public string ValueNL { get; set; }
        public string ValuePLPL { get; set; }
        public string ValuePT { get; set; }
        public string ValueSLSL { get; set; }
        public string ValueTRTR { get; set; }
        public string ValueSR { get; set; }
        public string ValueSV { get; set; }
        public string ValueUK { get; set; }
        public string ValueVI { get; set; }
        public string ValueZHCN { get; set; }
        public string ValueZHTW { get; set; }
        public int? ResourceType { get; set; }
        public long? ResourceId { get; set; }
        public string ResourceKey { get; set; }
        public string ResourceTag { get; set; }
        public string ResouceDesc { get; set; }
        public int? ResouceGroupId { get; set; }
        public DateTime? AddDate { get; set; }
    }
}