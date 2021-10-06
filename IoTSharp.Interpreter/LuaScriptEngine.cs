using Microsoft.Extensions.Logging;
using Microsoft.Scripting.Hosting;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Dynamic;
using System.Threading;

namespace IoTSharp.Interpreter
{
    public class LuaScriptEngine : ScriptEngineBase
    {
        private Script _engine;

        public LuaScriptEngine(ILogger<LuaScriptEngine> logger, EngineSetting setting, CancellationToken cancellationToken) : base(logger, setting, cancellationToken)
        {
            _engine = new Script();
        }
        public override string Do(string _source, string input)
        {
            var expConverter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(input, expConverter);
            UserData.RegisterExtensionType(obj.GetType());
            UserData.RegisterType(obj.GetType());
            DynValue dvobj = UserData.Create(obj);
            _engine.Globals.Set("input", dvobj);
             DynValue res = _engine.DoString(_source);
            var outputjson = res.ToObject().ToString();
            return outputjson;
        }
    }
}
