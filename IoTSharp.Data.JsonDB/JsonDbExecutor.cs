#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using IoTSharp.Data.JsonDB.Internal;

namespace IoTSharp.Data.JsonDB
{
    /// <summary>
    /// Public facade for executing SQL against JSON data without the ADO.NET wrapper.
    /// Useful for lightweight integration where the full ADO.NET lifecycle is not needed.
    /// </summary>
    public static class JsonDbExecutor
    {
        /// <summary>
        /// Executes a SQL statement against a JSON string and returns the result as a JSON string.
        /// </summary>
        /// <param name="sql">The SQL statement to execute.</param>
        /// <param name="jsonInput">A JSON string representing the data (array or object).</param>
        /// <param name="methods">Optional dictionary of registered functions callable from SQL expressions.</param>
        /// <returns>The result serialized as a JSON string.</returns>
        public static string Execute(
            string sql,
            string jsonInput,
            IReadOnlyDictionary<string, Func<IReadOnlyList<object?>, object?>>? methods = null)
        {
            return JsonSqlQueryExecutor.Execute(sql, jsonInput, methods);
        }

        /// <summary>
        /// Executes a SQL statement against a <see cref="JsonNode"/> and returns the result as a JSON string.
        /// The node is modified in-place for INSERT/UPDATE/DELETE statements.
        /// </summary>
        public static string ExecuteOnNode(
            string sql,
            JsonNode root,
            IReadOnlyDictionary<string, Func<IReadOnlyList<object?>, object?>>? methods = null)
        {
            return JsonSqlQueryExecutor.ExecuteOnNode(sql, root, methods);
        }
    }
}
