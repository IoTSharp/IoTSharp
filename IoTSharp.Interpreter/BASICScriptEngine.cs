using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Dynamic;
using System.Threading;

namespace IoTSharp.Interpreter
{
    public class BASICScriptEngine : ScriptEngineBase
    {
      
        public BASICScriptEngine(ILogger<PythonScriptEngine> logger  , IOptions<EngineSetting> _opt) : base(logger, _opt.Value, System.Threading.Tasks.Task.Factory.CancellationToken)
        {

        }
        public override string Do(string _source, string input)
        {
            var expConverter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(input, expConverter);
            //https://github.com/Timu5/BasicSharp
           var outputjson=   JsonConvert.SerializeObject(obj);
            return outputjson;
        }
    }
}
