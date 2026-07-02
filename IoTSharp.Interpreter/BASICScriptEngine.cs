using IoTSharp.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Scripting.Hosting;
using System;
using System.Dynamic;
using System.Threading;

namespace IoTSharp.Interpreter
{
    public class BASICScriptEngine : ScriptEngineBase
    {

        public BASICScriptEngine(ILogger<BASICScriptEngine> logger, IOptions<EngineSetting> _opt) : base(logger, _opt.Value, System.Threading.Tasks.Task.Factory.CancellationToken)
        {

        }
        public override string Do(string _source, string input)
        {
            dynamic obj = JsonObjectSerializer.DeserializeExpando(input);
            //https://github.com/Timu5/BasicSharp
            var outputjson = JsonObjectSerializer.Serialize(obj);
            return outputjson;
        }
    }
}
