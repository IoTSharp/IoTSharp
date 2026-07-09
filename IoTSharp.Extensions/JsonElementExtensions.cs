using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace IoTSharp.Extensions
{
    /// <summary>
    /// 将 JsonElement 转换为 IoTSharp 旧数据路径更容易处理的 CLR 对象。
    /// </summary>
    public static class JsonElementExtensions
    {
        /// <summary>
        /// 根据 JSON 类型将 JsonElement 转换为 ExpandoObject、列表、标量值或 null。
        /// </summary>
        /// <param name="element">需要转换的 JSON 元素。</param>
        /// <returns>表示该 JSON 值的 CLR 对象。</returns>
        public static object ToClrObject(this JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => element.ToExpandoObject(),
                JsonValueKind.Array => element.ToClrObjectList(),
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
        /// 将 JSON 对象元素转换为 ExpandoObject，并递归转换属性值。
        /// </summary>
        /// <param name="element">需要转换的 JSON 对象元素。</param>
        /// <returns>包含 JSON 对象属性的 ExpandoObject。</returns>
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
        /// 将 JSON 数组元素转换为列表，并按数组长度预分配容量。
        /// </summary>
        /// <param name="element">JSON 数组元素。</param>
        /// <returns>包含转换后 CLR 值的列表。</returns>
        public static List<object> ToClrObjectList(this JsonElement element)
        {
            var list = new List<object>(element.GetArrayLength());
            foreach (var item in element.EnumerateArray())
            {
                list.Add(item.ToClrObject());
            }

            return list;
        }

        /// <summary>
        /// 读取 JSON 数字时尽量保留更贴近的 CLR 数值类型。
        /// </summary>
        /// <param name="element">JSON 数字元素。</param>
        /// <returns>int、long、decimal 或 double 数值。</returns>
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
