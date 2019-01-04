using Newtonsoft.Json.Linq;

namespace IoT.Sharp.Sdk.CSharp
{
    public class InstallRresult
    {
        public string result { get; set; }
        public int count { get; set; }
    }

    public class ApiResult
    {
        public int code { get; set; }
        public string msg { get; set; }
        public JObject data { get; set; }

        public override string ToString()
        {
            string msg = "";
            if (code != 0)
            {
                msg = $"{code} {msg}";
            }
            return msg;
        }
    }

    public partial class AccountClient
    {
        public AccountClient(string url) : this(new System.Net.Http.HttpClient())
        {
            BaseUrl = url;
        }
    }

    public partial class InstallerClient
    {
        public InstallerClient(string url) : this(new System.Net.Http.HttpClient())
        {
            BaseUrl = url;
        }
    }
}