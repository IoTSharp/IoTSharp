using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using IoTSharp.Interpreter;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using CSScriptLib;
using Newtonsoft.Json;

using System.Linq;
using System.Reflection;
using IronPython.Runtime;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace IoTSharp.Interpreter
{
    public class CSharpScriptEngine : ScriptEngineBase, IDisposable
    {
         
        private bool disposedValue;
        private IMemoryCache _cache;

        public CSharpScriptEngine(ILogger<CSharpScriptEngine> logger, IOptions<EngineSetting> _opt, IMemoryCache cache) :base(logger,_opt.Value, Task.Factory.CancellationToken)
        {
            _cache = cache;
        }

        public  override string    Do(string _source,string input)
        {
            var runscript = _cache.GetOrCreate(_source, c =>
            {
                 var  src = _source.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.TrimEntries);
                StringBuilder _using = new StringBuilder();
                StringBuilder _body = new StringBuilder();
                src.ToList().ForEach(l =>
               {
                   if (l.StartsWith("using "))
                   {
                       _using.AppendLine(l);
                   }
                   else
                   {
                       _body.AppendLine(l);
                   }

               });
               return  CSScript.Evaluator
                      .CreateDelegate(@$"
                                    {_using}    
                                    dynamic  runscript(dynamic   input)
                                    {{
                                      {_body}
                                    }}");
            });
            var expConverter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(input, expConverter);
            dynamic result =    runscript(obj);
            var json= System.Text.Json.JsonSerializer.Serialize(result);
            _logger.LogDebug($"source:{Environment.NewLine}{ _source}{Environment.NewLine}{Environment.NewLine}input:{Environment.NewLine}{ input}{Environment.NewLine}{Environment.NewLine} ouput:{Environment.NewLine}{ json}{Environment.NewLine}{Environment.NewLine}");
            return json;
        }

        protected virtual void Dispose(bool disposing)
        {
           
            if (!disposedValue)
            {
                if (disposing)
                {
                 
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

