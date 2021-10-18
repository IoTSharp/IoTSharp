using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Scripting.Hosting;
using Neo.IronLua;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Dynamic;
using System.Threading;

namespace IoTSharp.Interpreter
{
    public class LuaScriptEngine : ScriptEngineBase
    {
        private Lua _engine;
        private LuaGlobal env;

        public LuaScriptEngine(ILogger<LuaScriptEngine> logger, IOptions<EngineSetting> _opt) : base(logger, _opt.Value, System.Threading.Tasks.Task.Factory.CancellationToken)
        {
            _engine = new Lua();
             env = _engine.CreateEnvironment(); // Create a environment
        }
        public override string Do(string _source, string input)
        {
            var expConverter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(input, expConverter);
            env["input"] = obj;
              var  res = env.DoChunk(_source,"lua_iotsharp");
            var outputjson = res.ToString();
            return outputjson;
        }
    }
}
