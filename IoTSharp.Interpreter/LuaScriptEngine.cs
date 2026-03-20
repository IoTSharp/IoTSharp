using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;

namespace IoTSharp.Interpreter
{
    public class LuaScriptEngine : ScriptEngineBase, IDisposable
    {
        private bool disposedValue;

        public LuaScriptEngine(ILogger<LuaScriptEngine> logger, IOptions<EngineSetting> _opt) : base(logger, _opt.Value, System.Threading.Tasks.Task.Factory.CancellationToken)
        {
        }

        public override string Do(string _source, string input)
        {
            var script = new Script(CoreModules.Preset_Default);
            script.Globals["input"] = ToDynValue(script, string.IsNullOrWhiteSpace(input) ? JValue.CreateNull() : JToken.Parse(input));

            var result = script.DoString(_source);
            if (result.Type is DataType.Nil or DataType.Void)
            {
                var outputValue = script.Globals.Get("output");
                if (outputValue.Type is not DataType.Nil and not DataType.Void)
                {
                    result = outputValue;
                }
            }

            var outputjson = ToJsonToken(result).ToString(Formatting.None);
            _logger.LogDebug("source:{NewLine}{Source}{NewLine}{NewLine}input:{NewLine}{Input}{NewLine}{NewLine}output:{NewLine}{Output}{NewLine}{NewLine}",
                Environment.NewLine,
                _source,
                input,
                outputjson);

            return outputjson;
        }

        private static DynValue ToDynValue(Script script, JToken token)
        {
            return token.Type switch
            {
                JTokenType.Object => DynValue.NewTable(ToTable(script, (JObject)token)),
                JTokenType.Array => DynValue.NewTable(ToArrayTable(script, (JArray)token)),
                JTokenType.Integer => DynValue.NewNumber(token.Value<double>()),
                JTokenType.Float => DynValue.NewNumber(token.Value<double>()),
                JTokenType.Boolean => DynValue.NewBoolean(token.Value<bool>()),
                JTokenType.String => DynValue.NewString(token.Value<string>() ?? string.Empty),
                JTokenType.Null => DynValue.Nil,
                JTokenType.Undefined => DynValue.Nil,
                _ => DynValue.NewString(token.ToString(Formatting.None))
            };
        }

        private static Table ToTable(Script script, JObject value)
        {
            var table = new Table(script);
            foreach (var property in value.Properties())
            {
                table[property.Name] = ToDynValue(script, property.Value);
            }

            return table;
        }

        private static Table ToArrayTable(Script script, JArray value)
        {
            var table = new Table(script);
            var index = 1;
            foreach (var item in value)
            {
                table[index++] = ToDynValue(script, item);
            }

            return table;
        }

        private static JToken ToJsonToken(DynValue value)
        {
            return value.Type switch
            {
                DataType.Void => JValue.CreateNull(),
                DataType.Nil => JValue.CreateNull(),
                DataType.Boolean => new JValue(value.Boolean),
                DataType.Number => new JValue(value.Number),
                DataType.String => new JValue(value.String),
                DataType.Table => ToJsonToken(value.Table),
                DataType.Tuple => new JArray(value.Tuple.Select(ToJsonToken)),
                _ => value.ToObject() is { } obj ? JToken.FromObject(obj) : JValue.CreateNull()
            };
        }

        private static JToken ToJsonToken(Table table)
        {
            var pairs = table.Pairs.ToList();
            if (pairs.Count == 0)
            {
                return new JObject();
            }

            if (TryConvertToArray(pairs, out var array))
            {
                return array;
            }

            var obj = new JObject();
            foreach (var pair in pairs)
            {
                obj[GetKeyName(pair.Key)] = ToJsonToken(pair.Value);
            }

            return obj;
        }

        private static bool TryConvertToArray(System.Collections.Generic.IReadOnlyCollection<TablePair> pairs, out JArray array)
        {
            array = new JArray();
            var indexedValues = pairs
                .Select(pair => (Success: TryGetArrayIndex(pair.Key, out var index), Index: index, pair.Value))
                .ToList();

            if (indexedValues.Any(item => !item.Success))
            {
                return false;
            }

            var ordered = indexedValues.OrderBy(item => item.Index).ToList();
            for (var expected = 1; expected <= ordered.Count; expected++)
            {
                if (ordered[expected - 1].Index != expected)
                {
                    return false;
                }

                array.Add(ToJsonToken(ordered[expected - 1].Value));
            }

            return true;
        }

        private static bool TryGetArrayIndex(DynValue key, out int index)
        {
            index = 0;
            if (key.Type != DataType.Number)
            {
                return false;
            }

            var number = key.Number;
            if (number < 1 || Math.Abs(number % 1) > double.Epsilon)
            {
                return false;
            }

            index = Convert.ToInt32(number, CultureInfo.InvariantCulture);
            return true;
        }

        private static string GetKeyName(DynValue key)
        {
            return key.Type switch
            {
                DataType.String => key.String,
                DataType.Number => key.Number.ToString(CultureInfo.InvariantCulture),
                DataType.Boolean => key.Boolean.ToString(),
                _ => key.ToDebugPrintString()
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
