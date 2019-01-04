using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Sharp.Sdk.CSharp
{
    public class ApiResult
    {
        public int code { get; set; }
        public string msg { get; set; }
        public JObject data { get; set; }
    }

    public partial class AccountClient
    {
        public AccountClient(string url) : this(new System.Net.Http.HttpClient())
        {
            BaseUrl = url;
        }
    }

}
