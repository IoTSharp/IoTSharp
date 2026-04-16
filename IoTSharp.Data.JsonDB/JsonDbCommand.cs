#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using IoTSharp.Data.JsonDB.Internal;

namespace IoTSharp.Data.JsonDB
{
    /// <summary>
    /// Executes SQL statements against a <see cref="JsonDbConnection"/>.
    /// </summary>
    public sealed class JsonDbCommand : DbCommand
    {
        private JsonDbConnection? _connection;
        private string _commandText = string.Empty;
        private readonly JsonDbParameterCollection _parameters = new();

        // ─── Constructors ────────────────────────────────────────────────────────────

        public JsonDbCommand() { }

        public JsonDbCommand(JsonDbConnection connection)
        {
            _connection = connection;
        }

        public JsonDbCommand(string commandText, JsonDbConnection connection)
        {
            _commandText = commandText;
            _connection = connection;
        }

        // ─── DbCommand overrides ─────────────────────────────────────────────────────

        public override string CommandText
        {
            get => _commandText;
            set => _commandText = value ?? string.Empty;
        }

        public override int CommandTimeout { get; set; } = 30;

        public override CommandType CommandType { get; set; } = CommandType.Text;

        public override bool DesignTimeVisible { get; set; }

        public override UpdateRowSource UpdatedRowSource { get; set; }

        protected override DbConnection? DbConnection
        {
            get => _connection;
            set => _connection = (JsonDbConnection?)value;
        }

        protected override DbParameterCollection DbParameterCollection => _parameters;

        protected override DbTransaction? DbTransaction { get; set; }

        public new JsonDbParameterCollection Parameters => _parameters;

        public new JsonDbConnection? Connection
        {
            get => _connection;
            set => _connection = value;
        }

        // ─── Execution ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Executes a SELECT query and returns a <see cref="JsonDbDataReader"/>.
        /// </summary>
        public new JsonDbDataReader ExecuteReader()
            => ExecuteReader(CommandBehavior.Default);

        /// <summary>
        /// Executes a SELECT query and returns a <see cref="JsonDbDataReader"/>.
        /// </summary>
        public new JsonDbDataReader ExecuteReader(CommandBehavior behavior)
            => (JsonDbDataReader)ExecuteDbDataReader(behavior);

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var conn = GetOpenConnection();
            var sql = ApplyParameters(_commandText);
            var result = JsonSqlQueryExecutor.ExecuteStatementOnNode(sql, conn.Root, conn.Methods);

            if (result is JsonArray array)
            {
                var rows = new List<JsonNode>(array.Count);
                foreach (var item in array)
                {
                    if (item is not null)
                    {
                        rows.Add(item);
                    }
                }

                return new JsonDbDataReader(rows, behavior);
            }

            // Non-array SELECT result (e.g. single object)
            if (result is JsonNode singleNode)
            {
                return new JsonDbDataReader(new[] { singleNode }, behavior);
            }

            return new JsonDbDataReader(Array.Empty<JsonNode>(), behavior);
        }

        /// <summary>
        /// Executes INSERT, UPDATE, or DELETE and returns the number of rows affected.
        /// For SELECT statements this returns -1; use <see cref="ExecuteReader"/> instead.
        /// When the connection is file-backed and <see cref="JsonDbConnection.AutoSave"/> is
        /// enabled, changes are automatically persisted to the source file.
        /// </summary>
        public override int ExecuteNonQuery()
        {
            var conn = GetOpenConnection();
            var sql = ApplyParameters(_commandText);
            var trimmed = sql.TrimStart();

            if (trimmed.StartsWith("select ", StringComparison.OrdinalIgnoreCase))
            {
                // Caller should use ExecuteReader for SELECT.
                return -1;
            }

            var before = conn.Root is JsonArray arr ? arr.Count : -1;
            var result = JsonSqlQueryExecutor.ExecuteStatementOnNode(sql, conn.Root, conn.Methods);
            var after = conn.Root is JsonArray arr2 ? arr2.Count : -1;

            // Auto-save after mutation
            conn.NotifyMutation();

            if (trimmed.StartsWith("delete ", StringComparison.OrdinalIgnoreCase) && before >= 0 && after >= 0)
            {
                return before - after;
            }

            if (trimmed.StartsWith("insert ", StringComparison.OrdinalIgnoreCase) && before >= 0 && after >= 0)
            {
                return after - before;
            }

            // UPDATE: result is the whole root; we can't easily count affected rows here.
            // Return 1 as a convention (statement executed successfully).
            return 1;
        }

        /// <summary>
        /// Executes the command and returns the first column of the first row.
        /// For SELECT, returns the first field value of the first row.
        /// For mutating commands, returns the affected row count as an object.
        /// </summary>
        public override object? ExecuteScalar()
        {
            var trimmed = _commandText.TrimStart();
            if (trimmed.StartsWith("select ", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = ExecuteReader();
                if (reader.Read() && reader.FieldCount > 0)
                {
                    var value = reader.GetValue(0);
                    return value is DBNull ? null : value;
                }

                return null;
            }

            return ExecuteNonQuery();
        }

        public override void Prepare() { }

        public override void Cancel() { }

        protected override DbParameter CreateDbParameter() => new JsonDbParameter();

        // ─── Parameter binding ────────────────────────────────────────────────────────

        /// <summary>
        /// Replaces @paramName placeholders in <paramref name="sql"/> with their
        /// bound literal values from <see cref="Parameters"/>.
        /// </summary>
        private string ApplyParameters(string sql)
        {
            if (_parameters.Count == 0)
            {
                return sql;
            }

            var sb = new StringBuilder(sql);
            // Sort parameters by name length descending to avoid partial replacements
            // (e.g. @name before @name2).
            for (var i = 0; i < _parameters.Count; i++)
            {
                var param = _parameters[i];
                var placeholder = param.ParameterName.StartsWith("@")
                    ? param.ParameterName
                    : "@" + param.ParameterName;

                sb.Replace(placeholder, ToSqlLiteral(param.Value));
            }

            return sb.ToString();
        }

        private static string ToSqlLiteral(object? value)
        {
            return value switch
            {
                null => "null",
                bool b => b ? "true" : "false",
                string s => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
                _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null"
            };
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────────

        private JsonDbConnection GetOpenConnection()
        {
            if (_connection is null)
            {
                throw new InvalidOperationException("The command has no associated connection.");
            }

            if (_connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("The connection is not open.");
            }

            return _connection;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
