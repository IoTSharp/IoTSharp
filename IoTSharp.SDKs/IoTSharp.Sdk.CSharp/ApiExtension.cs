using System;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Sdk.Http
{
    public static class ApiExtension
    {
        public static ApiResult ToResult(this SwaggerException swaggerException)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResult>(swaggerException.Response);
        }

        public static T ToData<T>(this ApiResult _apiResult) => (T)_apiResult.Data;

     
        public static string ToDisplayText(this DataStorage storage)
        {
            string _display = "";
            if (storage != null)
            {
                switch (storage.Type)
                {
                    case DataType.Binary:
                        if (storage.Value_Binary != null)
                        {
                            _display = string.Join(" ", (from tx in storage.Value_Binary.ToArray() select tx.ToString("X2")).ToArray());
                        }
                        break;
                    case DataType.Boolean:
                        _display = storage.Value_Boolean.ToString();
                        break;
                    case DataType.DateTime:
                     if (storage.Value_DateTime!=DateTime.MinValue)   _display = storage.Value_DateTime?.ToString("yyyy-MM-dd HH:mm:ss");
                        break;
                    case DataType.Double:
                        _display = storage.Value_Double.ToString();
                        break;
                    case DataType.Json:
                        _display = storage.Value_Json;
                        break;
                    case DataType.Long:
                        _display = storage.Value_Long.ToString();
                        break;
                    case DataType.String:
                        _display = storage.Value_String;
                        break;
                    case DataType.XML:
                        _display = storage.Value_XML;
                        break;
                    default:
                        break;
                }
            }
            return _display;
        }
    }
}