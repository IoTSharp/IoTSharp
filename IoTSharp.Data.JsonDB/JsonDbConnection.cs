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
    /// When the connection is backed by a JSON file and <see cref="AutoSave"/> is <c>true</c>
    /// (the default), mutating commands (INSERT, UPDATE, DELETE) automatically persist
    /// changes back to the source file after each execution.
    /// </summary>
    public sealed class JsonDbConnection : DbConnection
    {
        private ConnectionState _state = ConnectionState.Closed;
        private string _connectionString = string.Empty;
        private JsonNode? _root;
        private string? _filePath; // non-null when the connection is backed by a file
        private readonly Dictionary<string, Func<IReadOnlyList<object?>, object?>> _methods =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// When <c>true</c> (the default) and the connection is backed by a JSON file,
        /// INSERT / UPDATE / DELETE commands automatically write the updated JSON back to
        /// the source file after each execution.  Set to <c>false</c> to opt out of
        /// automatic persistence and use <see cref="SaveToFile"/> explicitly instead.
        /// Has no effect when the connection is not file-backed.
        /// </summary>
        public bool AutoSave { get; set; } = true;

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

        /// <summary>
        /// Returns <c>true</c> when this connection was opened from a JSON file.
        /// </summary>
        public bool IsFileBacked => _filePath is not null;

        /// <summary>
        /// Writes the current in-memory JSON state back to the source file.
        /// Throws <see cref="InvalidOperationException"/> if the connection is not file-backed.
        /// </summary>
        /// <remarks>
        /// This is called automatically after each mutating command when <see cref="AutoSave"/>
        /// is <c>true</c>. You can also call it manually when <see cref="AutoSave"/> is disabled.
        /// The file is written as UTF-8 with indentation for readability.
        /// </remarks>
        public void SaveToFile()
        {
            EnsureOpen();
            if (_filePath is null)
            {
                throw new InvalidOperationException(
                    "SaveToFile is only supported for file-backed connections. " +
                    "Use GetCurrentJson() to retrieve the updated JSON string.");
            }

            var json = _root!.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            // Write to a temp file first, then replace atomically to reduce the risk of
            // data loss if the process is interrupted mid-write.
            var tmpPath = _filePath + ".tmp";
            try
            {
                File.WriteAllText(tmpPath, json, System.Text.Encoding.UTF8);
                File.Move(tmpPath, _filePath, overwrite: true);
            }
            catch
            {
                // Best-effort cleanup of the temp file on failure.
                try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { /* ignore */ }
                throw;
            }
        }

        /// <summary>
        /// Called internally by <see cref="JsonDbCommand"/> after every mutating statement.
        /// Persists changes to the source file when <see cref="AutoSave"/> is enabled.
        /// </summary>
        internal void NotifyMutation()
        {
            if (AutoSave && _filePath is not null)
            {
                SaveToFile();
            }
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
                _filePath = filePath;
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
