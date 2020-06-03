using Newtonsoft.Json.Linq;

namespace IoTSharp.Sdk.Http
{
    public class InstallRresult
    {
        public string result { get; set; }
        public int count { get; set; }
    }

    public partial class ApiResult
    {
        public object Data { get; set; }

        public override string ToString()
        {
            string msg = "";
            if (Code != 0)
            {
                msg = $"{Code} {Msg}";
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