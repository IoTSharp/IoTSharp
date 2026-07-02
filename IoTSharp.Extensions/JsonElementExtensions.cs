using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;

namespace IoTSharp.Extensions
{
    /// <summary>
    /// Converts JsonElement values into CLR-friendly objects used by legacy IoTSharp data paths.
    /// </summary>
    public static class JsonElementExtensions
    {
        /// <summary>
        /// Converts a JsonElement into ExpandoObject, List, scalar, or null based on its JSON kind.
        /// </summary>
        /// <param name="element">The JSON element to convert.</param>
        /// <returns>A CLR object that represents the JSON value.</returns>
        public static object ToClrObject(this JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => element.ToExpandoObject(),
                JsonValueKind.Array => element.EnumerateArray().Select(item => item.ToClrObject()).ToList(),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => ReadNumber(element),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                _ => element.GetRawText()
            };
        }

        /// <summary>
        /// Converts a JSON object element into an ExpandoObject with recursively converted property values.
        /// </summary>
        /// <param name="element">The JSON object element to convert.</param>
        /// <returns>An ExpandoObject containing the JSON object properties.</returns>
        public static ExpandoObject ToExpandoObject(this JsonElement element)
        {
            var expando = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)expando;

            foreach (var property in element.EnumerateObject())
            {
                dictionary[property.Name] = property.Value.ToClrObject();
            }

            return expando;
        }

        /// <summary>
        /// Preserves the narrowest practical numeric CLR type while reading a JSON number.
        /// </summary>
        /// <param name="element">The JSON number element.</param>
        /// <returns>An int, long, decimal, or double value.</returns>
        private static object ReadNumber(JsonElement element)
        {
            if (element.TryGetInt32(out var intValue))
            {
                return intValue;
            }

            if (element.TryGetInt64(out var longValue))
            {
                return longValue;
            }

            if (element.TryGetDecimal(out var decimalValue))
            {
                return decimalValue;
            }

            return element.GetDouble();
        }
    }
}
