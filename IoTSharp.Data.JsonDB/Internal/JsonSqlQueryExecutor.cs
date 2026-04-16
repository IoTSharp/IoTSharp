#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IoTSharp.Data.JsonDB.Internal
{
    /// <summary>
    /// Core SQL-over-JSON execution engine. This class is the internal implementation
    /// shared by all ADO.NET wrapper types.
    /// </summary>
    internal static class JsonSqlQueryExecutor
    {
        /// <summary>
        /// Executes a SQL statement against a JSON input string and returns the result as a JSON string.
        /// </summary>
        public static string Execute(
            string sql,
            string input,
            IReadOnlyDictionary<string, Func<IReadOnlyList<object?>, object?>>? methods = null)
        {
            var root = JsonNode.Parse(input) ?? throw new InvalidOperationException("JSON input cannot be null.");
            return ExecuteOnNode(sql, root, methods);
        }

        /// <summary>
        /// Executes a SQL statement against a JsonNode and returns the result as a JSON string.
        /// </summary>
        public static string ExecuteOnNode(
            string sql,
            JsonNode root,
            IReadOnlyDictionary<string, Func<IReadOnlyList<object?>, object?>>? methods = null)
        {
            var statement = SqlStatementParser.Parse(sql);
            var context = new SqlExecutionContext(root, methods ?? new Dictionary<string, Func<IReadOnlyList<object?>, object?>>());
            var result = ExecuteStatement(statement, context);

            return result switch
            {
                null => "null",
                JsonNode node => node.ToJsonString(),
                _ => JsonSerializer.Serialize(result)
            };
        }

        /// <summary>
        /// Executes a SQL statement against a JsonNode, modifying the node in-place for
        /// mutating statements, and returning the result object.
        /// </summary>
        internal static object? ExecuteStatementOnNode(
            string sql,
            JsonNode root,
            IReadOnlyDictionary<string, Func<IReadOnlyList<object?>, object?>> methods)
        {
            var statement = SqlStatementParser.Parse(sql);
            var context = new SqlExecutionContext(root, methods);
            return ExecuteStatement(statement, context);
        }

        internal static object? ExecuteStatement(SqlStatement statement, SqlExecutionContext context)
        {
            return statement switch
            {
                SqlSelectStatement selectStatement => ExecuteSelect(selectStatement, context),
                SqlUpdateStatement updateStatement => ExecuteUpdate(updateStatement, context),
                SqlDeleteStatement deleteStatement => ExecuteDelete(deleteStatement, context),
                SqlInsertStatement insertStatement => ExecuteInsert(insertStatement, context),
                _ => throw new InvalidOperationException("Unsupported SQL statement.")
            };
        }

        private static JsonArray ExecuteSelect(SqlSelectStatement statement, SqlExecutionContext context)
        {
            var rows = FilterAndSortRows(statement.Where, statement.OrderBy, statement.Limit, context);
            var result = new JsonArray();

            foreach (var row in rows)
            {
                result.Add(ProjectRow(row, statement.Items, context));
            }

            return result;
        }

        private static JsonNode ExecuteUpdate(SqlUpdateStatement statement, SqlExecutionContext context)
        {
            var rows = FilterAndSortRows(statement.Where, statement.OrderBy, statement.Limit, context);
            foreach (var row in rows)
            {
                if (row.Node is not JsonObject jsonObject)
                {
                    throw new InvalidOperationException("Update statements require JSON object rows.");
                }

                foreach (var assignment in statement.Assignments)
                {
                    var value = SqlExpressionEvaluator.Evaluate(assignment.Expression, row, context);
                    SetPathValue(jsonObject, assignment.Path, ConvertToJsonNode(value));
                }
            }

            return context.Root;
        }

        private static object? ExecuteDelete(SqlDeleteStatement statement, SqlExecutionContext context)
        {
            var rows = FilterAndSortRows(statement.Where, statement.OrderBy, statement.Limit, context);
            if (context.Root is JsonArray jsonArray)
            {
                foreach (var index in rows.Where(static row => row.ArrayIndex >= 0).Select(static row => row.ArrayIndex).Distinct().OrderByDescending(static index => index))
                {
                    jsonArray.RemoveAt(index);
                }

                return context.Root;
            }

            return rows.Count > 0 ? null : context.Root;
        }

        private static JsonNode ExecuteInsert(SqlInsertStatement statement, SqlExecutionContext context)
        {
            if (context.Root is not JsonArray jsonArray)
            {
                throw new InvalidOperationException("Insert statements require the JSON input root to be an array.");
            }

            var row = new JsonObject();
            var rowContext = new SqlRowContext(row, -1);
            foreach (var assignment in statement.Assignments)
            {
                var value = SqlExpressionEvaluator.Evaluate(assignment.Expression, rowContext, context);
                SetPathValue(row, assignment.Path, ConvertToJsonNode(value));
            }

            jsonArray.Add(row);
            return context.Root;
        }

        internal static List<SqlRowContext> FilterAndSortRows(
            SqlExpression? where,
            IReadOnlyList<SqlOrderByItem> orderBy,
            SqlLimit? limit,
            SqlExecutionContext context)
        {
            var rows = EnumerateRows(context.Root)
                .Where(row => where is null || EvaluateBoolean(where, row, context))
                .ToList();

            if (orderBy.Count > 0)
            {
                rows.Sort((left, right) => CompareRows(left, right, orderBy, context));
            }

            return ApplyLimit(rows, limit);
        }

        private static List<SqlRowContext> EnumerateRows(JsonNode root)
        {
            if (root is JsonArray array)
            {
                var rows = new List<SqlRowContext>(array.Count);
                for (var i = 0; i < array.Count; i++)
                {
                    if (array[i] is not null)
                    {
                        rows.Add(new SqlRowContext(array[i]!, i));
                    }
                }

                return rows;
            }

            return new List<SqlRowContext> { new(root, -1) };
        }

        private static JsonNode ProjectRow(SqlRowContext row, IReadOnlyList<SqlSelectItem> items, SqlExecutionContext context)
        {
            if (items.Count == 1 && items[0].IsWildcard)
            {
                return row.Node.DeepClone();
            }

            JsonObject result = new();
            foreach (var item in items)
            {
                if (item.IsWildcard)
                {
                    if (row.Node is not JsonObject sourceObject)
                    {
                        continue;
                    }

                    foreach (var property in sourceObject)
                    {
                        result[property.Key] = property.Value?.DeepClone();
                    }

                    continue;
                }

                var value = SqlExpressionEvaluator.Evaluate(item.Expression!, row, context);
                result[item.Alias] = ConvertToJsonNode(value);
            }

            return result;
        }

        private static List<SqlRowContext> ApplyLimit(List<SqlRowContext> rows, SqlLimit? limit)
        {
            if (limit is null)
            {
                return rows;
            }

            return limit.Count.HasValue
                ? rows.Skip(limit.Offset).Take(limit.Count.Value).ToList()
                : rows.Take(limit.Offset).ToList();
        }

        private static int CompareRows(SqlRowContext left, SqlRowContext right, IReadOnlyList<SqlOrderByItem> orderBy, SqlExecutionContext context)
        {
            foreach (var item in orderBy)
            {
                var leftValue = SqlExpressionEvaluator.Evaluate(item.Expression, left, context);
                var rightValue = SqlExpressionEvaluator.Evaluate(item.Expression, right, context);
                var comparison = CompareValues(leftValue, rightValue, item.Numeric);
                if (comparison == 0)
                {
                    continue;
                }

                return item.Descending ? -comparison : comparison;
            }

            return 0;
        }

        internal static bool EvaluateBoolean(SqlExpression expression, SqlRowContext row, SqlExecutionContext context)
        {
            return ToBoolean(SqlExpressionEvaluator.Evaluate(expression, row, context));
        }

        internal static bool ToBoolean(object? value)
        {
            if (value is null)
            {
                return false;
            }

            return value switch
            {
                bool booleanValue => booleanValue,
                string stringValue when bool.TryParse(stringValue, out var booleanValue) => booleanValue,
                string stringValue => !string.IsNullOrWhiteSpace(stringValue),
                JsonNode jsonNode => jsonNode switch
                {
                    JsonValue jsonValue when jsonValue.TryGetValue<bool>(out var jsonBoolean) => jsonBoolean,
                    JsonValue jsonValue when jsonValue.TryGetValue<string>(out var jsonString) => !string.IsNullOrWhiteSpace(jsonString),
                    JsonArray jsonArray => jsonArray.Count > 0,
                    JsonObject jsonObject => jsonObject.Count > 0,
                    _ => true
                },
                _ when TryConvertToDecimal(value, out var number) => number != 0,
                _ => true
            };
        }

        internal static int CompareValues(object? left, object? right, bool numericOnly)
        {
            if (TryConvertToDecimal(left, out var leftNumber) && TryConvertToDecimal(right, out var rightNumber))
            {
                return leftNumber.CompareTo(rightNumber);
            }

            if (numericOnly)
            {
                return 0;
            }

            if (left is null && right is null)
            {
                return 0;
            }

            if (left is null)
            {
                return -1;
            }

            if (right is null)
            {
                return 1;
            }

            if (TryConvertToBoolean(left, out var leftBoolean) && TryConvertToBoolean(right, out var rightBoolean))
            {
                return leftBoolean.CompareTo(rightBoolean);
            }

            return string.CompareOrdinal(ToComparableString(left), ToComparableString(right));
        }

        internal static bool ValuesEqual(object? left, object? right)
        {
            if (left is null || right is null)
            {
                return left is null && right is null;
            }

            if (TryConvertToDecimal(left, out var leftNumber) && TryConvertToDecimal(right, out var rightNumber))
            {
                return leftNumber == rightNumber;
            }

            if (TryConvertToBoolean(left, out var leftBoolean) && TryConvertToBoolean(right, out var rightBoolean))
            {
                return leftBoolean == rightBoolean;
            }

            return string.Equals(ToComparableString(left), ToComparableString(right), StringComparison.Ordinal);
        }

        internal static bool TryConvertToDecimal(object? value, out decimal result)
        {
            switch (value)
            {
                case byte byteValue:
                    result = byteValue;
                    return true;
                case short int16Value:
                    result = int16Value;
                    return true;
                case int int32Value:
                    result = int32Value;
                    return true;
                case long int64Value:
                    result = int64Value;
                    return true;
                case float singleValue:
                    result = (decimal)singleValue;
                    return true;
                case double doubleValue:
                    result = (decimal)doubleValue;
                    return true;
                case decimal decimalValue:
                    result = decimalValue;
                    return true;
                case JsonValue jsonValue when jsonValue.TryGetValue<decimal>(out result):
                    return true;
                case JsonValue jsonValue when jsonValue.TryGetValue<double>(out var jsonDoubleValue):
                    result = (decimal)jsonDoubleValue;
                    return true;
                case string stringValue when decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result):
                    return true;
                default:
                    result = default;
                    return false;
            }
        }

        internal static bool TryConvertToBoolean(object? value, out bool result)
        {
            switch (value)
            {
                case bool booleanValue:
                    result = booleanValue;
                    return true;
                case JsonValue jsonValue when jsonValue.TryGetValue<bool>(out result):
                    return true;
                case string stringValue when bool.TryParse(stringValue, out result):
                    return true;
                default:
                    result = default;
                    return false;
            }
        }

        internal static object? ResolvePathValue(JsonNode? node, string path)
        {
            if (!TryGetPathNode(node, path, out var value))
            {
                return null;
            }

            return ConvertNodeToValue(value);
        }

        internal static bool TryGetPathNode(JsonNode? node, string path, out JsonNode? value)
        {
            value = node;
            foreach (var segment in SplitPath(path))
            {
                if (value is not JsonObject jsonObject || !jsonObject.TryGetPropertyValue(segment, out value))
                {
                    value = null;
                    return false;
                }
            }

            return true;
        }

        internal static void SetPathValue(JsonObject root, string path, JsonNode? value)
        {
            var segments = SplitPath(path).ToArray();
            if (segments.Length == 0)
            {
                throw new InvalidOperationException("Assignment path is required.");
            }

            JsonObject current = root;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                if (current[segments[i]] is not JsonObject next)
                {
                    next = new JsonObject();
                    current[segments[i]] = next;
                }

                current = next;
            }

            current[segments[^1]] = value;
        }

        internal static IEnumerable<string> SplitPath(string path)
        {
            return path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(static segment => segment.Replace("`", string.Empty));
        }

        internal static object? ConvertNodeToValue(JsonNode? node)
        {
            if (node is null)
            {
                return null;
            }

            if (node is not JsonValue)
            {
                return node.DeepClone();
            }

            // Use if/else instead of a switch expression to prevent the C# compiler from
            // inferring JsonNode? as the common type (which would implicitly convert string
            // back to JsonValue<string> via JsonNode's implicit operator).
            using var document = JsonDocument.Parse(node.ToJsonString());
            var kind = document.RootElement.ValueKind;

            if (kind == JsonValueKind.String)
            {
                return document.RootElement.GetString();
            }

            if (kind == JsonValueKind.Number)
            {
                if (document.RootElement.TryGetInt64(out var int64Value)) return int64Value;
                if (document.RootElement.TryGetDecimal(out var decimalValue)) return decimalValue;
                return document.RootElement.GetDouble();
            }

            if (kind == JsonValueKind.True) return true;
            if (kind == JsonValueKind.False) return false;
            if (kind is JsonValueKind.Null or JsonValueKind.Undefined) return null;

            return node.DeepClone();
        }

        internal static JsonNode? ConvertToJsonNode(object? value)
        {
            return value switch
            {
                null => null,
                JsonNode jsonNode => jsonNode.DeepClone(),
                string stringValue => JsonValue.Create(stringValue),
                bool boolValue => JsonValue.Create(boolValue),
                byte byteValue => JsonValue.Create(byteValue),
                short int16Value => JsonValue.Create(int16Value),
                int int32Value => JsonValue.Create(int32Value),
                long int64Value => JsonValue.Create(int64Value),
                float singleValue => JsonValue.Create(singleValue),
                double doubleValue => JsonValue.Create(doubleValue),
                decimal decimalValue => JsonValue.Create(decimalValue),
                _ => JsonSerializer.SerializeToNode(value)
            };
        }

        private static string ToComparableString(object value)
        {
            return value switch
            {
                JsonNode jsonNode => jsonNode.ToJsonString(),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
            };
        }
    }
}
