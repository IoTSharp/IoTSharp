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
    public class PythonScriptEngine : ScriptEngineBase,IDisposable
    {
        private ScriptEngine _engine;
        private bool disposedValue;

        public PythonScriptEngine(ILogger<PythonScriptEngine> logger, IOptions< EngineSetting> setting) : base(logger, setting.Value,System.Threading.Tasks.Task.Factory.CancellationToken)
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _engine = null;
                    // TODO: 释放托管状态(托管对象)
                }
                
                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
