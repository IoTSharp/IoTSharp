using Jint;
using Jint.Native;
using Jint.Native.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using IoTSharp.Interpreter;
namespace IoTSharp.Interpreter.JavaScript
{
    public class JavaScriptEngine
    {
        private readonly CancellationToken _cancellationToken;
        private readonly Engine _engine;
        private readonly JsonParser _parser;
        private readonly ILogger _logger;
        private readonly JsonSerializer   _serializer;
        private string _source;

        public JavaScriptEngine(ILogger<JavaScriptEngine> logger, EngineSetting setting, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            var engine = new Engine(options =>
            {

                // Limit memory allocations to MB
                options.LimitMemory(4_000_000);

                // Set a timeout to 4 seconds.
                options.TimeoutInterval(TimeSpan.FromSeconds(setting.Timeout));

                // Set limit of 1000 executed statements.
                // options.MaxStatements(1000);
                // Use a cancellation token.
                options.CancellationToken(_cancellationToken);
            });
            _engine = engine;
            _parser = new JsonParser(_engine);
            _logger = logger;
        }
 
        public   string  Do(string _source,string input)
        {
            var js = _engine.SetValue("input",_parser.Parse(input)).Evaluate(_source).ToObject();
            var json= System.Text.Json.JsonSerializer.Serialize(js);
            _logger.LogDebug($"source:{Environment.NewLine}{ _source}{Environment.NewLine}{Environment.NewLine}input:{Environment.NewLine}{ input}{Environment.NewLine}{Environment.NewLine} ouput:{Environment.NewLine}{ json}{Environment.NewLine}{Environment.NewLine}");
            return json;
        }
    }
}

