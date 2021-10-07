using CLanguage;
using Microsoft.Extensions.Logging;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Dynamic;
using System.Threading;

namespace IoTSharp.Interpreter
{
    public class CScriptEngine : ScriptEngineBase
    {
      
        public CScriptEngine(ILogger<CScriptEngine> logger, EngineSetting setting, CancellationToken cancellationToken) : base(logger, setting, cancellationToken)
        {
           
        }
        public override string Do(string _source, string input)
        {
           var obj=  CLanguageService.Eval(_source, $"char* input=\"{input}\";");
            var outputjson = JsonConvert.SerializeObject(obj);
            return outputjson;
        }
    }
}
