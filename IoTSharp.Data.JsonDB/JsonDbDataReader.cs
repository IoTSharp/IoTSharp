#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Nodes;
using IoTSharp.Data.JsonDB.Internal;

namespace IoTSharp.Data.JsonDB
{
    /// <summary>
    /// Provides forward-only access to query results from a <see cref="JsonDbCommand"/>.
    /// Each row is a JSON object; columns correspond to the properties of that object.
    /// </summary>
    public sealed class JsonDbDataReader : DbDataReader
    {
        private readonly IReadOnlyList<JsonNode> _rows;
        private readonly CommandBehavior _behavior;
        private int _rowIndex = -1;
        private bool _closed;

        // Current row field names and values
        private string[] _fieldNames = Array.Empty<string>();
        private object?[] _fieldValues = Array.Empty<object?>();

        internal JsonDbDataReader(IReadOnlyList<JsonNode> rows, CommandBehavior behavior = CommandBehavior.Default)
        {
            _rows = rows;
            _behavior = behavior;
        }

        // ─── DbDataReader overrides ─────────────────────────────────────────────────

        public override bool IsClosed => _closed;

        public override int RecordsAffected => 0;

        public override int Depth => 0;

        public override bool HasRows => _rows.Count > 0;

        public override int FieldCount => _fieldNames.Length;

        public override object this[int ordinal] => GetValue(ordinal);

        public override object this[string name] => GetValue(GetOrdinal(name));

        public override bool Read()
        {
            if (_closed)
            {
                return false;
            }

            _rowIndex++;
            if (_rowIndex >= _rows.Count)
            {
                return false;
            }

            LoadCurrentRow();
            return true;
        }

        public override bool NextResult() => false;

        public override void Close()
        {
            _closed = true;
        }

        // ─── Schema ─────────────────────────────────────────────────────────────────

        public override string GetName(int ordinal)
        {
            CheckOrdinal(ordinal);
            return _fieldNames[ordinal];
        }

        public override int GetOrdinal(string name)
        {
            for (var i = 0; i < _fieldNames.Length; i++)
            {
                if (string.Equals(_fieldNames[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            throw new IndexOutOfRangeException($"Field '{name}' not found in the current row.");
        }

        public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

        public override Type GetFieldType(int ordinal)
        {
            var value = GetValue(ordinal);
            return value?.GetType() ?? typeof(object);
        }

        public override DataTable? GetSchemaTable() => null;

        // ─── Value accessors ─────────────────────────────────────────────────────────

        public override object GetValue(int ordinal)
        {
            CheckOrdinal(ordinal);
            return _fieldValues[ordinal] ?? DBNull.Value;
        }

        public override int GetValues(object[] values)
        {
            var count = Math.Min(values.Length, _fieldValues.Length);
            for (var i = 0; i < count; i++)
            {
                values[i] = _fieldValues[i] ?? DBNull.Value;
            }

            return count;
        }

        public override bool IsDBNull(int ordinal)
        {
            CheckOrdinal(ordinal);
            return _fieldValues[ordinal] is null;
        }

        public override bool GetBoolean(int ordinal) => Convert<bool>(ordinal);
        public override byte GetByte(int ordinal) => Convert<byte>(ordinal);
        public override char GetChar(int ordinal) => Convert<char>(ordinal);
        public override DateTime GetDateTime(int ordinal) => Convert<DateTime>(ordinal);
        public override decimal GetDecimal(int ordinal) => Convert<decimal>(ordinal);
        public override double GetDouble(int ordinal) => Convert<double>(ordinal);
        public override float GetFloat(int ordinal) => Convert<float>(ordinal);
        public override Guid GetGuid(int ordinal) => Convert<Guid>(ordinal);
        public override short GetInt16(int ordinal) => Convert<short>(ordinal);
        public override int GetInt32(int ordinal) => Convert<int>(ordinal);
        public override long GetInt64(int ordinal) => Convert<long>(ordinal);
        public override string GetString(int ordinal) => Convert<string>(ordinal) ?? string.Empty;

        public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
            => throw new NotSupportedException();

        public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
            => throw new NotSupportedException();

        public override IEnumerator GetEnumerator() => new DbEnumerator(this, closeReader: false);

        // ─── Helpers ─────────────────────────────────────────────────────────────────

        private void LoadCurrentRow()
        {
            var node = _rows[_rowIndex];
            if (node is JsonObject obj)
            {
                var names = new List<string>();
                var values = new List<object?>();
                foreach (var kv in obj)
                {
                    names.Add(kv.Key);
                    values.Add(JsonSqlQueryExecutor.ConvertNodeToValue(kv.Value));
                }

                _fieldNames = names.ToArray();
                _fieldValues = values.ToArray();
            }
            else
            {
                // Scalar / array value: expose as a single "value" column
                _fieldNames = new[] { "value" };
                _fieldValues = new[] { JsonSqlQueryExecutor.ConvertNodeToValue(node) };
            }
        }

        private void CheckOrdinal(int ordinal)
        {
            if (ordinal < 0 || ordinal >= _fieldNames.Length)
            {
                throw new IndexOutOfRangeException($"Ordinal {ordinal} is out of range (0..{_fieldNames.Length - 1}).");
            }
        }

        private T? Convert<T>(int ordinal)
        {
            var raw = _fieldValues[ordinal];
            if (raw is null)
            {
                return default;
            }

            if (raw is T typed)
            {
                return typed;
            }

            return (T)System.Convert.ChangeType(raw, typeof(T));
        }

        protected override void Dispose(bool disposing)
        {
            _closed = true;
            base.Dispose(disposing);
        }
    }
}
