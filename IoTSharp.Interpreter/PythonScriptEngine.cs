using Microsoft.Extensions.Logging;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Dynamic;
using System.Threading;

namespace IoTSharp.Interpreter
{
    public class PythonScriptEngine : ScriptEngineBase
    {
        private ScriptEngine _engine;

        public PythonScriptEngine(ILogger<PythonScriptEngine> logger, EngineSetting setting) : base(logger, setting,System.Threading.Tasks.Task.Factory.CancellationToken)
        {
            _engine = IronPython.Hosting.Python.CreateEngine();
        }
        public override string Do(string _source, string input)
        {
            var expConverter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(input, expConverter);
            var scope = _engine.CreateScope();
            scope.SetVariable("input", obj);
            _engine.Execute(_source , scope);
            dynamic _output = scope.GetVariable("output");
           var outputjson=   JsonConvert.SerializeObject(_output);
            return outputjson;
        }
    }
}
