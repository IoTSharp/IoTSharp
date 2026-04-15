#nullable enable

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IoTSharp.Interpreter
{
    public sealed class SQLEngine : IDisposable
    {
        private readonly ILogger<SQLEngine> _logger;
        private readonly Dictionary<string, Func<IReadOnlyList<object?>, object?>> _methods = new(StringComparer.OrdinalIgnoreCase);

        public SQLEngine(ILogger<SQLEngine> logger, IOptions<EngineSetting> opt)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(opt);
            ArgumentNullException.ThrowIfNull(opt.Value);

            _logger = logger;
        }

        /// <summary>
        /// Registers an external method that can be invoked from SQL expressions.
        /// </summary>
        public void RegisterMethod(string name, Func<IReadOnlyList<object?>, object?> method)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Method name is required.", nameof(name));
            }

            ArgumentNullException.ThrowIfNull(method);
            _methods[name] = method;
        }

        public string Do(string source, string input)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException("SQL source is required.", nameof(source));
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("JSON input is required.", nameof(input));
            }

            var json = JsonSqlQueryExecutor.Execute(source, input, _methods);
            _logger.LogDebug($"source:{Environment.NewLine}{source}{Environment.NewLine}{Environment.NewLine}input:{Environment.NewLine}{input}{Environment.NewLine}{Environment.NewLine} ouput:{Environment.NewLine}{json}{Environment.NewLine}{Environment.NewLine}");
            return json;
        }

        public void Dispose()
        {
        }
    }

    internal static class JsonSqlQueryExecutor
    {
        public static string Execute(string sql, string input, IReadOnlyDictionary<string, Func<IReadOnlyList<object?>, object?>> methods)
        {
            var statement = SqlStatementParser.Parse(sql);
            var root = JsonNode.Parse(input) ?? throw new InvalidOperationException("JSON input cannot be null.");
            var context = new SqlExecutionContext(root, methods);
            var result = ExecuteStatement(statement, context);

            return result switch
            {
                null => "null",
                JsonNode node => node.ToJsonString(),
                _ => JsonSerializer.Serialize(result)
            };
        }

        private static object? ExecuteStatement(SqlStatement statement, SqlExecutionContext context)
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

        private static List<SqlRowContext> FilterAndSortRows(SqlExpression? where, IReadOnlyList<SqlOrderByItem> orderBy, SqlLimit? limit, SqlExecutionContext context)
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

        private static object? ConvertNodeToValue(JsonNode? node)
        {
            if (node is null)
            {
                return null;
            }

            if (node is not JsonValue)
            {
                return node.DeepClone();
            }

            using var document = JsonDocument.Parse(node.ToJsonString());
            return document.RootElement.ValueKind switch
            {
                JsonValueKind.String => document.RootElement.GetString(),
                JsonValueKind.Number when document.RootElement.TryGetInt64(out var int64Value) => int64Value,
                JsonValueKind.Number when document.RootElement.TryGetDecimal(out var decimalValue) => decimalValue,
                JsonValueKind.Number => document.RootElement.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                _ => node.DeepClone()
            };
        }

        private static JsonNode? ConvertToJsonNode(object? value)
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

    internal sealed class SqlExecutionContext
    {
        public SqlExecutionContext(JsonNode root, IReadOnlyDictionary<string, Func<IReadOnlyList<object?>, object?>> methods)
        {
            Root = root;
            Methods = methods;
        }

        public JsonNode Root { get; }

        public IReadOnlyDictionary<string, Func<IReadOnlyList<object?>, object?>> Methods { get; }
    }

    internal readonly record struct SqlRowContext(JsonNode Node, int ArrayIndex);

    internal abstract record SqlStatement(string Table);

    internal sealed record SqlSelectStatement(
        string Table,
        IReadOnlyList<SqlSelectItem> Items,
        SqlExpression? Where,
        IReadOnlyList<SqlOrderByItem> OrderBy,
        SqlLimit? Limit) : SqlStatement(Table);

    internal sealed record SqlUpdateStatement(
        string Table,
        IReadOnlyList<SqlAssignment> Assignments,
        SqlExpression? Where,
        IReadOnlyList<SqlOrderByItem> OrderBy,
        SqlLimit? Limit) : SqlStatement(Table);

    internal sealed record SqlDeleteStatement(
        string Table,
        SqlExpression? Where,
        IReadOnlyList<SqlOrderByItem> OrderBy,
        SqlLimit? Limit) : SqlStatement(Table);

    internal sealed record SqlInsertStatement(string Table, IReadOnlyList<SqlAssignment> Assignments) : SqlStatement(Table);

    internal sealed record SqlSelectItem(SqlExpression? Expression, string Alias, bool IsWildcard);

    internal sealed record SqlOrderByItem(SqlExpression Expression, bool Descending, bool Numeric);

    internal sealed record SqlAssignment(string Path, SqlExpression Expression);

    internal sealed record SqlLimit(int Offset, int? Count);

    internal abstract record SqlExpression;

    internal sealed record SqlLiteralExpression(object? Value) : SqlExpression;

    internal sealed record SqlIdentifierExpression(string Path) : SqlExpression;

    internal sealed record SqlFunctionCallExpression(string Name, IReadOnlyList<SqlExpression> Arguments) : SqlExpression;

    internal sealed record SqlUnaryExpression(SqlUnaryOperator Operator, SqlExpression Operand) : SqlExpression;

    internal sealed record SqlBinaryExpression(SqlExpression Left, SqlBinaryOperator Operator, SqlExpression Right) : SqlExpression;

    internal enum SqlUnaryOperator
    {
        Plus,
        Minus,
        Not,
    }

    internal enum SqlBinaryOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        And,
        Or,
    }

    internal static class SqlStatementParser
    {
        public static SqlStatement Parse(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("SQL source is required.", nameof(sql));
            }

            var trimmed = sql.Trim();
            if (trimmed.StartsWith("select ", StringComparison.OrdinalIgnoreCase))
            {
                return ParseSelect(trimmed);
            }

            if (trimmed.StartsWith("update ", StringComparison.OrdinalIgnoreCase))
            {
                return ParseUpdate(trimmed);
            }

            if (trimmed.StartsWith("delete from ", StringComparison.OrdinalIgnoreCase))
            {
                return ParseDelete(trimmed);
            }

            if (trimmed.StartsWith("insert into ", StringComparison.OrdinalIgnoreCase))
            {
                return ParseInsert(trimmed);
            }

            throw new ArgumentException("Unsupported SQL statement.", nameof(sql));
        }

        private static SqlSelectStatement ParseSelect(string sql)
        {
            var body = sql["select".Length..].TrimStart();
            var fromIndex = FindClauseIndex(body, "from");
            if (fromIndex < 0)
            {
                throw new ArgumentException("Select statements require a FROM clause.", nameof(sql));
            }

            var fieldsText = body[..fromIndex].Trim();
            var tail = body[(fromIndex + "from".Length)..].TrimStart();
            var clauses = ParseCommonTail(tail);

            return new SqlSelectStatement(
                clauses.Table,
                ParseSelectItems(fieldsText),
                ParseOptionalExpression(clauses.Where),
                ParseOrderBy(clauses.OrderBy),
                ParseLimit(clauses.Limit));
        }

        private static SqlUpdateStatement ParseUpdate(string sql)
        {
            var body = sql["update".Length..].TrimStart();
            var setIndex = FindClauseIndex(body, "set");
            if (setIndex < 0)
            {
                throw new ArgumentException("Update statements require a SET clause.", nameof(sql));
            }

            var table = body[..setIndex].Trim();
            var tail = body[(setIndex + "set".Length)..].TrimStart();
            var clauses = ParseClauseSegments(table, tail);

            return new SqlUpdateStatement(
                clauses.Table,
                ParseAssignments(clauses.MainBody),
                ParseOptionalExpression(clauses.Where),
                ParseOrderBy(clauses.OrderBy),
                ParseLimit(clauses.Limit));
        }

        private static SqlDeleteStatement ParseDelete(string sql)
        {
            var tail = sql["delete from".Length..].TrimStart();
            var clauses = ParseCommonTail(tail);

            return new SqlDeleteStatement(
                clauses.Table,
                ParseOptionalExpression(clauses.Where),
                ParseOrderBy(clauses.OrderBy),
                ParseLimit(clauses.Limit));
        }

        private static SqlInsertStatement ParseInsert(string sql)
        {
            var body = sql["insert into".Length..].TrimStart();
            var setIndex = FindClauseIndex(body, "set");
            if (setIndex < 0)
            {
                throw new ArgumentException("Insert statements require a SET clause.", nameof(sql));
            }

            var table = body[..setIndex].Trim();
            var assignments = body[(setIndex + "set".Length)..].Trim();
            return new SqlInsertStatement(table, ParseAssignments(assignments));
        }

        private static SqlTail ParseCommonTail(string tail)
        {
            return ParseClauseSegments(null, tail);
        }

        private static SqlTail ParseClauseSegments(string? table, string tail)
        {
            var whereIndex = FindClauseIndex(tail, "where");
            var orderIndex = FindClauseIndex(tail, "order by");
            var limitIndex = FindClauseIndex(tail, "limit");
            var firstClauseIndex = MinPositive(whereIndex, orderIndex, limitIndex);

            var resolvedTable = string.IsNullOrWhiteSpace(table)
                ? (firstClauseIndex >= 0 ? tail[..firstClauseIndex].Trim() : tail.Trim())
                : table.Trim();

            var mainBody = string.IsNullOrWhiteSpace(table)
                ? string.Empty
                : (firstClauseIndex >= 0 ? tail[..firstClauseIndex].Trim() : tail.Trim());

            var where = SliceClause(tail, whereIndex, "where", orderIndex, limitIndex);
            var orderBy = SliceClause(tail, orderIndex, "order by", limitIndex);
            var limit = SliceClause(tail, limitIndex, "limit");

            return new SqlTail(resolvedTable, mainBody, where, orderBy, limit);
        }

        private static IReadOnlyList<SqlSelectItem> ParseSelectItems(string text)
        {
            var items = new List<SqlSelectItem>();
            foreach (var segment in SplitTopLevel(text, ','))
            {
                var trimmed = segment.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }

                if (trimmed == "*")
                {
                    items.Add(new SqlSelectItem(null, "*", true));
                    continue;
                }

                var aliasIndex = FindClauseIndex(trimmed, "as");
                var expressionText = aliasIndex >= 0 ? trimmed[..aliasIndex].Trim() : trimmed;
                var alias = aliasIndex >= 0 ? trimmed[(aliasIndex + "as".Length)..].Trim() : GetDefaultAlias(expressionText);
                items.Add(new SqlSelectItem(SqlExpressionParser.Parse(expressionText), alias, false));
            }

            return items;
        }

        private static IReadOnlyList<SqlAssignment> ParseAssignments(string text)
        {
            var assignments = new List<SqlAssignment>();
            foreach (var segment in SplitTopLevel(text, ','))
            {
                var equalsIndex = FindAssignmentOperator(segment);
                if (equalsIndex < 0)
                {
                    throw new ArgumentException($"Invalid assignment '{segment}'.", nameof(text));
                }

                var path = segment[..equalsIndex].Trim();
                var expressionText = segment[(equalsIndex + 1)..].Trim();
                if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(expressionText))
                {
                    throw new ArgumentException($"Invalid assignment '{segment}'.", nameof(text));
                }

                assignments.Add(new SqlAssignment(path, SqlExpressionParser.Parse(expressionText)));
            }

            return assignments;
        }

        private static IReadOnlyList<SqlOrderByItem> ParseOrderBy(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Array.Empty<SqlOrderByItem>();
            }

            var items = new List<SqlOrderByItem>();
            foreach (var segment in SplitTopLevel(text, ','))
            {
                var trimmed = segment.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }

                var expressionText = trimmed;
                var descending = false;
                var numeric = false;
                foreach (var suffix in new[] { " descnum", " ascnum", " desc", " asc" })
                {
                    if (trimmed.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        expressionText = trimmed[..^suffix.Length].TrimEnd();
                        descending = suffix.Contains("desc", StringComparison.OrdinalIgnoreCase);
                        numeric = suffix.Contains("num", StringComparison.OrdinalIgnoreCase);
                        break;
                    }
                }

                items.Add(new SqlOrderByItem(SqlExpressionParser.Parse(expressionText), descending, numeric));
            }

            return items;
        }

        private static SqlLimit? ParseLimit(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length switch
            {
                1 => new SqlLimit(int.Parse(parts[0], CultureInfo.InvariantCulture), null),
                2 => new SqlLimit(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture)),
                _ => throw new ArgumentException("Invalid LIMIT clause.", nameof(text))
            };
        }

        private static SqlExpression? ParseOptionalExpression(string? text)
        {
            return string.IsNullOrWhiteSpace(text) ? null : SqlExpressionParser.Parse(text);
        }

        private static string GetDefaultAlias(string expressionText)
        {
            return expressionText;
        }

        private static int FindAssignmentOperator(string text)
        {
            var quote = '\0';
            var depth = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var current = text[i];
                if (quote != '\0')
                {
                    if (current == '\\')
                    {
                        i++;
                        continue;
                    }

                    if (current == quote)
                    {
                        quote = '\0';
                    }

                    continue;
                }

                if (current is '\'' or '"')
                {
                    quote = current;
                    continue;
                }

                if (current == '(')
                {
                    depth++;
                    continue;
                }

                if (current == ')')
                {
                    depth--;
                    continue;
                }

                if (depth == 0 && current == '=' && (i == 0 || text[i - 1] is not '!' and not '<' and not '>') && (i == text.Length - 1 || text[i + 1] != '='))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string? SliceClause(string text, int clauseIndex, string clauseKeyword, params int[] nextClauseIndexes)
        {
            if (clauseIndex < 0)
            {
                return null;
            }

            var start = clauseIndex + clauseKeyword.Length;
            var end = nextClauseIndexes.Where(index => index > clauseIndex).DefaultIfEmpty(text.Length).Min();
            return text[start..end].Trim();
        }

        private static int MinPositive(params int[] values)
        {
            return values.Where(static value => value >= 0).DefaultIfEmpty(-1).Min();
        }

        private static IEnumerable<string> SplitTopLevel(string text, char separator)
        {
            var quote = '\0';
            var depth = 0;
            var lastIndex = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var current = text[i];
                if (quote != '\0')
                {
                    if (current == '\\')
                    {
                        i++;
                        continue;
                    }

                    if (current == quote)
                    {
                        quote = '\0';
                    }

                    continue;
                }

                if (current is '\'' or '"')
                {
                    quote = current;
                    continue;
                }

                if (current == '(')
                {
                    depth++;
                    continue;
                }

                if (current == ')')
                {
                    depth--;
                    continue;
                }

                if (depth == 0 && current == separator)
                {
                    yield return text[lastIndex..i];
                    lastIndex = i + 1;
                }
            }

            yield return text[lastIndex..];
        }

        private static int FindClauseIndex(string text, string clause)
        {
            var quote = '\0';
            var depth = 0;
            for (var i = 0; i <= text.Length - clause.Length; i++)
            {
                var current = text[i];
                if (quote != '\0')
                {
                    if (current == '\\')
                    {
                        i++;
                        continue;
                    }

                    if (current == quote)
                    {
                        quote = '\0';
                    }

                    continue;
                }

                if (current is '\'' or '"')
                {
                    quote = current;
                    continue;
                }

                if (current == '(')
                {
                    depth++;
                    continue;
                }

                if (current == ')')
                {
                    depth--;
                    continue;
                }

                if (depth == 0 && text.AsSpan(i).StartsWith(clause, StringComparison.OrdinalIgnoreCase) && IsBoundary(text, i - 1) && IsBoundary(text, i + clause.Length))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsBoundary(string text, int index)
        {
            if (index < 0 || index >= text.Length)
            {
                return true;
            }

            return char.IsWhiteSpace(text[index]);
        }

        private sealed record SqlTail(string Table, string MainBody, string? Where, string? OrderBy, string? Limit);
    }

    internal static class SqlExpressionEvaluator
    {
        public static object? Evaluate(SqlExpression expression, SqlRowContext row, SqlExecutionContext context)
        {
            return expression switch
            {
                SqlLiteralExpression literalExpression => literalExpression.Value,
                SqlIdentifierExpression identifierExpression => JsonSqlQueryExecutor.ResolvePathValue(row.Node, identifierExpression.Path),
                SqlFunctionCallExpression functionCallExpression => InvokeFunction(functionCallExpression, row, context),
                SqlUnaryExpression unaryExpression => EvaluateUnary(unaryExpression, row, context),
                SqlBinaryExpression binaryExpression => EvaluateBinary(binaryExpression, row, context),
                _ => throw new InvalidOperationException("Unsupported SQL expression.")
            };
        }

        private static object? InvokeFunction(SqlFunctionCallExpression expression, SqlRowContext row, SqlExecutionContext context)
        {
            if (!context.Methods.TryGetValue(expression.Name, out var method))
            {
                throw new InvalidOperationException($"SQL method '{expression.Name}' is not registered.");
            }

            var arguments = expression.Arguments.Select(argument => Evaluate(argument, row, context)).ToArray();
            return method(arguments);
        }

        private static object? EvaluateUnary(SqlUnaryExpression expression, SqlRowContext row, SqlExecutionContext context)
        {
            var operand = Evaluate(expression.Operand, row, context);
            return expression.Operator switch
            {
                SqlUnaryOperator.Plus => operand,
                SqlUnaryOperator.Minus when JsonSqlQueryExecutor.TryConvertToDecimal(operand, out var number) => -number,
                SqlUnaryOperator.Not => !JsonSqlQueryExecutor.ToBoolean(operand),
                _ => throw new InvalidOperationException("Unsupported unary SQL expression.")
            };
        }

        private static object? EvaluateBinary(SqlBinaryExpression expression, SqlRowContext row, SqlExecutionContext context)
        {
            var left = Evaluate(expression.Left, row, context);
            if (expression.Operator == SqlBinaryOperator.And)
            {
                return JsonSqlQueryExecutor.ToBoolean(left) && JsonSqlQueryExecutor.ToBoolean(Evaluate(expression.Right, row, context));
            }

            if (expression.Operator == SqlBinaryOperator.Or)
            {
                return JsonSqlQueryExecutor.ToBoolean(left) || JsonSqlQueryExecutor.ToBoolean(Evaluate(expression.Right, row, context));
            }

            var right = Evaluate(expression.Right, row, context);
            return expression.Operator switch
            {
                SqlBinaryOperator.Add => Add(left, right),
                SqlBinaryOperator.Subtract => Calculate(left, right, static (l, r) => l - r),
                SqlBinaryOperator.Multiply => Calculate(left, right, static (l, r) => l * r),
                SqlBinaryOperator.Divide => Calculate(left, right, static (l, r) => l / r),
                SqlBinaryOperator.Modulo => Calculate(left, right, static (l, r) => l % r),
                SqlBinaryOperator.Equal => JsonSqlQueryExecutor.ValuesEqual(left, right),
                SqlBinaryOperator.NotEqual => !JsonSqlQueryExecutor.ValuesEqual(left, right),
                SqlBinaryOperator.GreaterThan => JsonSqlQueryExecutor.CompareValues(left, right, numericOnly: false) > 0,
                SqlBinaryOperator.GreaterThanOrEqual => JsonSqlQueryExecutor.CompareValues(left, right, numericOnly: false) >= 0,
                SqlBinaryOperator.LessThan => JsonSqlQueryExecutor.CompareValues(left, right, numericOnly: false) < 0,
                SqlBinaryOperator.LessThanOrEqual => JsonSqlQueryExecutor.CompareValues(left, right, numericOnly: false) <= 0,
                _ => throw new InvalidOperationException("Unsupported binary SQL expression.")
            };
        }

        private static object Add(object? left, object? right)
        {
            if (JsonSqlQueryExecutor.TryConvertToDecimal(left, out var leftNumber) && JsonSqlQueryExecutor.TryConvertToDecimal(right, out var rightNumber))
            {
                return leftNumber + rightNumber;
            }

            return $"{Convert.ToString(left, CultureInfo.InvariantCulture)}{Convert.ToString(right, CultureInfo.InvariantCulture)}";
        }

        private static decimal Calculate(object? left, object? right, Func<decimal, decimal, decimal> operation)
        {
            if (!JsonSqlQueryExecutor.TryConvertToDecimal(left, out var leftNumber) || !JsonSqlQueryExecutor.TryConvertToDecimal(right, out var rightNumber))
            {
                throw new InvalidOperationException("Arithmetic SQL expressions require numeric values.");
            }

            return operation(leftNumber, rightNumber);
        }
    }

    internal static class SqlExpressionParser
    {
        public static SqlExpression Parse(string text)
        {
            var parser = new Parser(SqlTokenizer.Tokenize(text));
            return parser.Parse();
        }

        private sealed class Parser
        {
            private readonly IReadOnlyList<SqlToken> _tokens;
            private int _index;

            public Parser(IReadOnlyList<SqlToken> tokens)
            {
                _tokens = tokens;
            }

            public SqlExpression Parse()
            {
                var expression = ParseOrExpression();
                Expect(SqlTokenKind.End);
                return expression;
            }

            private SqlExpression ParseOrExpression()
            {
                var expression = ParseAndExpression();
                while (Match(SqlTokenKind.Or))
                {
                    expression = new SqlBinaryExpression(expression, SqlBinaryOperator.Or, ParseAndExpression());
                }

                return expression;
            }

            private SqlExpression ParseAndExpression()
            {
                var expression = ParseComparisonExpression();
                while (Match(SqlTokenKind.And))
                {
                    expression = new SqlBinaryExpression(expression, SqlBinaryOperator.And, ParseComparisonExpression());
                }

                return expression;
            }

            private SqlExpression ParseComparisonExpression()
            {
                var expression = ParseAdditiveExpression();
                while (Current.Kind is SqlTokenKind.Equal or SqlTokenKind.NotEqual or SqlTokenKind.GreaterThan or SqlTokenKind.GreaterThanOrEqual or SqlTokenKind.LessThan or SqlTokenKind.LessThanOrEqual)
                {
                    var operatorKind = ReadBinaryOperator();
                    expression = new SqlBinaryExpression(expression, operatorKind, ParseAdditiveExpression());
                }

                return expression;
            }

            private SqlExpression ParseAdditiveExpression()
            {
                var expression = ParseMultiplicativeExpression();
                while (Current.Kind is SqlTokenKind.Plus or SqlTokenKind.Minus)
                {
                    var operatorKind = Current.Kind == SqlTokenKind.Plus ? SqlBinaryOperator.Add : SqlBinaryOperator.Subtract;
                    _index++;
                    expression = new SqlBinaryExpression(expression, operatorKind, ParseMultiplicativeExpression());
                }

                return expression;
            }

            private SqlExpression ParseMultiplicativeExpression()
            {
                var expression = ParseUnaryExpression();
                while (Current.Kind is SqlTokenKind.Asterisk or SqlTokenKind.Slash or SqlTokenKind.Percent)
                {
                    var operatorKind = Current.Kind switch
                    {
                        SqlTokenKind.Asterisk => SqlBinaryOperator.Multiply,
                        SqlTokenKind.Slash => SqlBinaryOperator.Divide,
                        _ => SqlBinaryOperator.Modulo
                    };

                    _index++;
                    expression = new SqlBinaryExpression(expression, operatorKind, ParseUnaryExpression());
                }

                return expression;
            }

            private SqlExpression ParseUnaryExpression()
            {
                if (Match(SqlTokenKind.Plus))
                {
                    return new SqlUnaryExpression(SqlUnaryOperator.Plus, ParseUnaryExpression());
                }

                if (Match(SqlTokenKind.Minus))
                {
                    return new SqlUnaryExpression(SqlUnaryOperator.Minus, ParseUnaryExpression());
                }

                if (Match(SqlTokenKind.Not))
                {
                    return new SqlUnaryExpression(SqlUnaryOperator.Not, ParseUnaryExpression());
                }

                return ParsePrimaryExpression();
            }

            private SqlExpression ParsePrimaryExpression()
            {
                if (Match(SqlTokenKind.LeftParen))
                {
                    var expression = ParseOrExpression();
                    Expect(SqlTokenKind.RightParen);
                    return expression;
                }

                var token = Current;
                _index++;
                return token.Kind switch
                {
                    SqlTokenKind.Identifier => ParseIdentifier(token.Value),
                    SqlTokenKind.String => new SqlLiteralExpression(token.Value),
                    SqlTokenKind.Number => new SqlLiteralExpression(decimal.Parse(token.Value, CultureInfo.InvariantCulture)),
                    SqlTokenKind.Boolean => new SqlLiteralExpression(bool.Parse(token.Value)),
                    SqlTokenKind.Null => new SqlLiteralExpression(null),
                    _ => throw new ArgumentException($"Unsupported token '{token.Value}' in SQL expression.", nameof(_tokens))
                };
            }

            private SqlExpression ParseIdentifier(string value)
            {
                if (!Match(SqlTokenKind.LeftParen))
                {
                    return new SqlIdentifierExpression(value);
                }

                var arguments = new List<SqlExpression>();
                if (!Match(SqlTokenKind.RightParen))
                {
                    do
                    {
                        arguments.Add(ParseOrExpression());
                    }
                    while (Match(SqlTokenKind.Comma));

                    Expect(SqlTokenKind.RightParen);
                }

                return new SqlFunctionCallExpression(value, arguments);
            }

            private SqlBinaryOperator ReadBinaryOperator()
            {
                var token = Current;
                _index++;
                return token.Kind switch
                {
                    SqlTokenKind.Equal => SqlBinaryOperator.Equal,
                    SqlTokenKind.NotEqual => SqlBinaryOperator.NotEqual,
                    SqlTokenKind.GreaterThan => SqlBinaryOperator.GreaterThan,
                    SqlTokenKind.GreaterThanOrEqual => SqlBinaryOperator.GreaterThanOrEqual,
                    SqlTokenKind.LessThan => SqlBinaryOperator.LessThan,
                    SqlTokenKind.LessThanOrEqual => SqlBinaryOperator.LessThanOrEqual,
                    _ => throw new ArgumentException($"Unsupported operator '{token.Value}'.", nameof(_tokens))
                };
            }

            private bool Match(SqlTokenKind kind)
            {
                if (Current.Kind != kind)
                {
                    return false;
                }

                _index++;
                return true;
            }

            private void Expect(SqlTokenKind kind)
            {
                if (!Match(kind))
                {
                    throw new ArgumentException($"Expected token {kind} but found '{Current.Value}'.", nameof(_tokens));
                }
            }

            private SqlToken Current => _index < _tokens.Count ? _tokens[_index] : _tokens[^1];
        }
    }

    internal static class SqlTokenizer
    {
        public static IReadOnlyList<SqlToken> Tokenize(string text)
        {
            var tokens = new List<SqlToken>();
            for (var index = 0; index < text.Length;)
            {
                var current = text[index];
                if (char.IsWhiteSpace(current))
                {
                    index++;
                    continue;
                }

                switch (current)
                {
                    case '(':
                        tokens.Add(new SqlToken(SqlTokenKind.LeftParen, "("));
                        index++;
                        continue;
                    case ')':
                        tokens.Add(new SqlToken(SqlTokenKind.RightParen, ")"));
                        index++;
                        continue;
                    case ',':
                        tokens.Add(new SqlToken(SqlTokenKind.Comma, ","));
                        index++;
                        continue;
                    case '+':
                        tokens.Add(new SqlToken(SqlTokenKind.Plus, "+"));
                        index++;
                        continue;
                    case '-':
                        if (index + 1 < text.Length && char.IsDigit(text[index + 1]))
                        {
                            tokens.Add(ReadNumber(text, ref index));
                            continue;
                        }

                        tokens.Add(new SqlToken(SqlTokenKind.Minus, "-"));
                        index++;
                        continue;
                    case '*':
                        tokens.Add(new SqlToken(SqlTokenKind.Asterisk, "*"));
                        index++;
                        continue;
                    case '/':
                        tokens.Add(new SqlToken(SqlTokenKind.Slash, "/"));
                        index++;
                        continue;
                    case '%':
                        tokens.Add(new SqlToken(SqlTokenKind.Percent, "%"));
                        index++;
                        continue;
                    case '!':
                        if (Match(text, index, "!="))
                        {
                            tokens.Add(new SqlToken(SqlTokenKind.NotEqual, "!="));
                            index += 2;
                            continue;
                        }

                        throw new ArgumentException($"Unsupported character '{current}' in SQL expression.", nameof(text));
                    case '>':
                        if (Match(text, index, ">="))
                        {
                            tokens.Add(new SqlToken(SqlTokenKind.GreaterThanOrEqual, ">="));
                            index += 2;
                            continue;
                        }

                        tokens.Add(new SqlToken(SqlTokenKind.GreaterThan, ">"));
                        index++;
                        continue;
                    case '<':
                        if (Match(text, index, "<="))
                        {
                            tokens.Add(new SqlToken(SqlTokenKind.LessThanOrEqual, "<="));
                            index += 2;
                            continue;
                        }

                        tokens.Add(new SqlToken(SqlTokenKind.LessThan, "<"));
                        index++;
                        continue;
                    case '=':
                        tokens.Add(new SqlToken(SqlTokenKind.Equal, "="));
                        index++;
                        continue;
                    case '\'' or '"':
                        tokens.Add(ReadQuotedString(text, ref index));
                        continue;
                    default:
                        if (char.IsDigit(current))
                        {
                            tokens.Add(ReadNumber(text, ref index));
                            continue;
                        }

                        if (IsIdentifierCharacter(current))
                        {
                            tokens.Add(ReadIdentifier(text, ref index));
                            continue;
                        }

                        throw new ArgumentException($"Unsupported character '{current}' in SQL expression.", nameof(text));
                }
            }

            tokens.Add(new SqlToken(SqlTokenKind.End, string.Empty));
            return tokens;
        }

        private static SqlToken ReadQuotedString(string text, ref int index)
        {
            var quote = text[index++];
            StringBuilder buffer = new();
            while (index < text.Length)
            {
                var current = text[index++];
                if (current == '\\' && index < text.Length)
                {
                    buffer.Append(text[index++]);
                    continue;
                }

                if (current == quote)
                {
                    return new SqlToken(SqlTokenKind.String, buffer.ToString());
                }

                buffer.Append(current);
            }

            throw new ArgumentException("Unterminated SQL string literal.", nameof(text));
        }

        private static SqlToken ReadNumber(string text, ref int index)
        {
            var start = index;
            index++;
            while (index < text.Length && (char.IsDigit(text[index]) || text[index] == '.'))
            {
                index++;
            }

            return new SqlToken(SqlTokenKind.Number, text[start..index]);
        }

        private static SqlToken ReadIdentifier(string text, ref int index)
        {
            var start = index;
            index++;
            while (index < text.Length && IsIdentifierCharacter(text[index]))
            {
                index++;
            }

            var value = text[start..index].Replace("`", string.Empty);
            return value.ToLowerInvariant() switch
            {
                "and" => new SqlToken(SqlTokenKind.And, value),
                "or" => new SqlToken(SqlTokenKind.Or, value),
                "not" => new SqlToken(SqlTokenKind.Not, value),
                "true" => new SqlToken(SqlTokenKind.Boolean, bool.TrueString),
                "false" => new SqlToken(SqlTokenKind.Boolean, bool.FalseString),
                "null" => new SqlToken(SqlTokenKind.Null, value),
                _ => new SqlToken(SqlTokenKind.Identifier, value)
            };
        }

        private static bool Match(string text, int index, string token)
        {
            return text.AsSpan(index).StartsWith(token, StringComparison.Ordinal);
        }

        private static bool IsIdentifierCharacter(char value)
        {
            return char.IsLetterOrDigit(value) || value is '_' or '.' or '`';
        }
    }

    internal enum SqlTokenKind
    {
        Identifier,
        String,
        Number,
        Boolean,
        Null,
        LeftParen,
        RightParen,
        Comma,
        Plus,
        Minus,
        Asterisk,
        Slash,
        Percent,
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        And,
        Or,
        Not,
        End,
    }

    internal readonly record struct SqlToken(SqlTokenKind Kind, string Value);
}

