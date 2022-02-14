using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Controllers.Models
{
    public class BaiduTranslateProfile
    {
        public string AppKey { get; set; }
        public string AppSecret { get; set; }

        public int? ApiInterval { get; set; }

        public string DefaultLang { get; set; }


        public List<MappingItem> LangFieldMapping { get; set; }
    }
    public class MappingItem
    {
        public string Source { get; set; }

        public string Target { get; set; }
    }
    public class BaiduTranslateResult
    {
        public string from { get; set; }
        public string to { get; set; }

        public TranslateResult[] trans_result { get; set; }
    }


    public class TranslateResult
    {
        public string src { get; set; }
        public string dst { get; set; }
    }

    public class AssetProfile
    {

        public string Value { get; set; }
    }
}
