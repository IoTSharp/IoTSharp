#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using IoTSharp.Data.JsonDB.Internal;

namespace IoTSharp.Data.JsonDB
{
    /// <summary>
    /// ADO.NET connection to a JSON-backed in-memory database.
    /// Supports SQL SELECT, INSERT, UPDATE, and DELETE over JSON arrays and objects.
    /// </summary>
    public sealed class JsonDbConnection : DbConnection
    {
        private ConnectionState _state = ConnectionState.Closed;
        private string _connectionString = string.Empty;
        private JsonNode? _root;
        private readonly Dictionary<string, Func<IReadOnlyList<object?>, object?>> _methods =
            new(StringComparer.OrdinalIgnoreCase);

        // ─── Constructors ───────────────────────────────────────────────────────────

        /// <summary>Creates an empty connection. Set <see cref="ConnectionString"/> before calling <see cref="Open"/>.</summary>
        public JsonDbConnection() { }

        /// <summary>Creates a connection using a connection string.</summary>
        /// <param name="connectionString">
        ///   Supported keys (case-insensitive):
        ///   <list type="bullet">
        ///     <item><c>Data Source=path/to/file.json</c> – load from a JSON file</item>
        ///     <item><c>Json=[...]</c> – inline JSON string</item>
        ///   </list>
        /// </param>
        public JsonDbConnection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        // ─── Factory helpers ────────────────────────────────────────────────────────

        /// <summary>Creates and opens a connection backed by the given JSON string.</summary>
        public static JsonDbConnection FromJson(string json)
        {
            var conn = new JsonDbConnection();
            conn._root = JsonNode.Parse(json) ?? throw new ArgumentException("JSON string produced a null root.", nameof(json));
            return conn;
        }

        /// <summary>Creates and opens a connection backed by the given JSON file.</summary>
        public static JsonDbConnection FromFile(string path)
        {
            var conn = new JsonDbConnection($"Data Source={path}");
            return conn;
        }

        /// <summary>Creates and opens a connection backed by the given <see cref="JsonNode"/> (deep-cloned to avoid shared state).</summary>
        public static JsonDbConnection FromNode(JsonNode node)
        {
            ArgumentNullException.ThrowIfNull(node);
            var conn = new JsonDbConnection();
            conn._root = node.DeepClone();
            return conn;
        }

        // ─── DbConnection overrides ─────────────────────────────────────────────────

        public override string ConnectionString
        {
            get => _connectionString;
            set => _connectionString = value ?? string.Empty;
        }

        public override string Database => "JsonDB";
        public override string DataSource => _connectionString;
        public override string ServerVersion => "1.0";
        public override ConnectionState State => _state;

        public override void Open()
        {
            if (_state == ConnectionState.Open)
            {
                return;
            }

            if (_root is null)
            {
                _root = LoadFromConnectionString();
            }

            _state = ConnectionState.Open;
        }

        public override void Close()
        {
            _state = ConnectionState.Closed;
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException("JsonDB does not support changing the database.");
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new JsonDbTransaction(this, isolationLevel);
        }

        protected override DbCommand CreateDbCommand()
        {
            return new JsonDbCommand(this);
        }

        // ─── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Registers an external method/function that can be called from SQL expressions.
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

        /// <summary>
        /// Returns the current state of the JSON root as a JSON string.
        /// Useful after INSERT/UPDATE/DELETE to retrieve the modified data.
        /// </summary>
        public string GetCurrentJson()
        {
            EnsureOpen();
            return _root!.ToJsonString();
        }

        // ─── Internal helpers ────────────────────────────────────────────────────────

        internal JsonNode Root
        {
            get
            {
                EnsureOpen();
                return _root!;
            }
        }

        internal IReadOnlyDictionary<string, Func<IReadOnlyList<object?>, object?>> Methods => _methods;

        private void EnsureOpen()
        {
            if (_state != ConnectionState.Open)
            {
                throw new InvalidOperationException("The connection is not open.");
            }
        }

        private JsonNode LoadFromConnectionString()
        {
            var cs = _connectionString;
            if (string.IsNullOrWhiteSpace(cs))
            {
                throw new InvalidOperationException("No data source configured. Provide a connection string or use a factory method.");
            }

            // Parse simple key=value pairs
            var kvp = ParseConnectionString(cs);

            if (kvp.TryGetValue("data source", out var filePath) || kvp.TryGetValue("datasource", out filePath))
            {
                var content = File.ReadAllText(filePath!);
                return JsonNode.Parse(content) ?? throw new InvalidOperationException($"File '{filePath}' produced a null JSON root.");
            }

            if (kvp.TryGetValue("json", out var jsonValue))
            {
                return JsonNode.Parse(jsonValue!) ?? throw new InvalidOperationException("JSON connection string value produced a null root.");
            }

            throw new InvalidOperationException(
                "Unsupported connection string format. Use 'Data Source=file.json' or 'Json=[...]'.");
        }

        private static Dictionary<string, string> ParseConnectionString(string cs)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in cs.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var idx = part.IndexOf('=');
                if (idx <= 0)
                {
                    continue;
                }

                var key = part[..idx].Trim();
                var value = part[(idx + 1)..].Trim();
                result[key] = value;
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            _state = ConnectionState.Closed;
            base.Dispose(disposing);
        }
    }
}
