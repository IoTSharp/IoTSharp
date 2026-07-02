using IoTSharp.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoonSharp.Interpreter;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;

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
            script.Globals["input"] = ToDynValue(script, JsonNodeParser.ParseNode(input));

            var result = script.DoString(_source);
            if (result.Type is DataType.Nil or DataType.Void)
            {
                var outputValue = script.Globals.Get("output");
                if (outputValue.Type is not DataType.Nil and not DataType.Void)
                {
                    result = outputValue;
                }
            }

            var outputjson = ToJsonNode(result)?.ToJsonString(JsonOptions.Default) ?? "null";
            _logger.LogDebug("source:{NewLine}{Source}{NewLine}{NewLine}input:{NewLine}{Input}{NewLine}{NewLine}output:{NewLine}{Output}{NewLine}{NewLine}",
                Environment.NewLine,
                _source,
                input,
                outputjson);

            return outputjson;
        }

        private static DynValue ToDynValue(Script script, JsonNode token)
        {
            return token switch
            {
                null => DynValue.Nil,
                JsonObject obj => DynValue.NewTable(ToTable(script, obj)),
                JsonArray array => DynValue.NewTable(ToArrayTable(script, array)),
                JsonValue value when value.TryGetValue<bool>(out var boolValue) => DynValue.NewBoolean(boolValue),
                JsonValue value when value.TryGetValue<double>(out var numberValue) => DynValue.NewNumber(numberValue),
                JsonValue value when value.TryGetValue<string>(out var textValue) => DynValue.NewString(textValue ?? string.Empty),
                _ => DynValue.NewString(token.ToJsonString(JsonOptions.Default))
            };
        }

        private static Table ToTable(Script script, JsonObject value)
        {
            var table = new Table(script);
            foreach (var property in value)
            {
                table[property.Key] = ToDynValue(script, property.Value);
            }

            return table;
        }

        private static Table ToArrayTable(Script script, JsonArray value)
        {
            var table = new Table(script);
            var index = 1;
            foreach (var item in value)
            {
                table[index++] = ToDynValue(script, item);
            }

            return table;
        }

        private static JsonNode ToJsonNode(DynValue value)
        {
            return value.Type switch
            {
                DataType.Void => null,
                DataType.Nil => null,
                DataType.Boolean => JsonValue.Create(value.Boolean),
                DataType.Number => JsonValue.Create(value.Number),
                DataType.String => JsonValue.Create(value.String),
                DataType.Table => ToJsonNode(value.Table),
                DataType.Tuple => CreateArray(value.Tuple.Select(ToJsonNode)),
                _ => value.ToObject() is { } obj ? JsonObjectSerializer.SerializeToNode(obj) : null
            };
        }

        private static JsonNode ToJsonNode(Table table)
        {
            var pairs = table.Pairs.ToList();
            if (pairs.Count == 0)
            {
                return new JsonObject();
            }

            if (TryConvertToArray(pairs, out var array))
            {
                return array;
            }

            var obj = new JsonObject();
            foreach (var pair in pairs)
            {
                obj[GetKeyName(pair.Key)] = ToJsonNode(pair.Value);
            }

            return obj;
        }

        private static bool TryConvertToArray(System.Collections.Generic.IReadOnlyCollection<TablePair> pairs, out JsonArray array)
        {
            array = new JsonArray();
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

                array.Add(ToJsonNode(ordered[expected - 1].Value));
            }

            return true;
        }

        private static JsonArray CreateArray(System.Collections.Generic.IEnumerable<JsonNode> values)
        {
            var array = new JsonArray();
            foreach (var value in values)
            {
                array.Add(value);
            }

            return array;
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
