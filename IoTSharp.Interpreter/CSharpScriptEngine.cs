using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using IoTSharp.Interpreter;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using CSScriptLib;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using IronPython.Runtime;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace IoTSharp.Interpreter
{
    public class CSharpScriptEngine : ScriptEngineBase, IDisposable
    {
         
        private bool disposedValue;


        public CSharpScriptEngine(ILogger<CSharpScriptEngine> logger, IOptions<EngineSetting> _opt):base(logger,_opt.Value, Task.Factory.CancellationToken)
        {
            //CSScript.EvaluatorConfig


            CSScript.Evaluator.ReferenceAssembly(Assembly.GetAssembly(typeof(JsonConvert)))
                .ReferenceAssembly(Assembly.GetAssembly(typeof(JObject)))
                .ReferenceAssembly(Assembly.GetAssembly(typeof(JProperty)))
                .ReferenceAssembly(Assembly.GetAssembly(typeof(ExpandoObject)));
        }


        public  override string    Do(string _source,string input)
        {
  
            var requirednamespace =new Dictionary<string, String>();
            requirednamespace.Add(typeof(JsonConvert).Namespace, "Newtonsoft.Json");
            requirednamespace.Add(typeof(JObject).Namespace, "Newtonsoft.Json.Linq");
            requirednamespace.Add(typeof(ExpandoObject).Namespace, "ExpandoObject ");
            requirednamespace.Add(typeof(ICSAction).Namespace, "ICSAction");
            string ClassTemplate = @$"public class Script:ICSAction
                                        {{
                                            public dynamic Run(string input)
                                            {{
                                                  {_source} 
                                            }}
                                        }}";


            var runcode = requirednamespace.Aggregate("", (x, y) => x + "using " + y.Key + ";") + ClassTemplate;
             ICSAction csa = CSScript.Evaluator.LoadCode<ICSAction>(runcode);
          //   var expConverter = new ExpandoObjectConverter();
            //dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(input, expConverter);

            //var runscript = CSScript.Evaluator
            //      .CreateDelegate(@$"dynamic  runscript(dynamic   input)
            //                        {{
            //                          {_source} 
            //                        }}");

            dynamic result = csa.Run(input);
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


    public interface ICSAction
    {
        dynamic Run(string input);

   
          


    } 



}

