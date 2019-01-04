using System;
using System.Threading.Tasks;

namespace IoT.Sharp.Sdk.CSharp
{
    public static class ApiExtension
    {
        public static ApiResult ToResult(this SwaggerException swaggerException)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResult>(swaggerException.Response);
        }

        public static T ToData<T>(this ApiResult _apiResult) => _apiResult.data.ToObject<T>();

        public static async Task<T> ToResultAsync<T>(this FileResponse fr)
        {
            T result = default(T);
            try
            {
                using (System.Net.Http.StreamContent stream = new System.Net.Http.StreamContent(fr.Stream))
                {
                    result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(await stream.ReadAsStringAsync());
                }
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
}