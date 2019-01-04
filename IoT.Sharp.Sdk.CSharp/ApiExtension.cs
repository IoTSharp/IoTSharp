using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Sharp.Sdk.CSharp
{
    public static class ApiExtension
    {
       public  static ApiResult ToResult(this SwaggerException swaggerException)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResult>(swaggerException.Response);
        }
    }
}
