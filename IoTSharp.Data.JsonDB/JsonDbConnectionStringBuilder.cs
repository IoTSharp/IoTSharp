#nullable enable

using System;
using System.Data.Common;

namespace IoTSharp.Data.JsonDB
{
    /// <summary>
    /// Builds connection strings for <see cref="JsonDbConnection"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// // File-based
    /// var builder = new JsonDbConnectionStringBuilder { DataSource = "data.json" };
    /// var conn = new JsonDbConnection(builder.ConnectionString);
    ///
    /// // JSON string (small payloads only; for large JSON use FromJson() or FromNode())
    /// var builder2 = new JsonDbConnectionStringBuilder { Json = "[{\"id\":1}]" };
    /// var conn2 = new JsonDbConnection(builder2.ConnectionString);
    /// </code>
    /// </example>
    public sealed class JsonDbConnectionStringBuilder : DbConnectionStringBuilder
    {
        private const string DataSourceKey = "Data Source";
        private const string JsonKey = "Json";

        /// <summary>Path to a JSON file.</summary>
        public string? DataSource
        {
            get => ContainsKey(DataSourceKey) ? (string?)this[DataSourceKey] : null;
            set
            {
                if (value is null)
                {
                    Remove(DataSourceKey);
                }
                else
                {
                    this[DataSourceKey] = value;
                }
            }
        }

        /// <summary>Inline JSON string (for small payloads). Large JSON should use <see cref="JsonDbConnection.FromJson"/>.</summary>
        public string? Json
        {
            get => ContainsKey(JsonKey) ? (string?)this[JsonKey] : null;
            set
            {
                if (value is null)
                {
                    Remove(JsonKey);
                }
                else
                {
                    this[JsonKey] = value;
                }
            }
        }
    }
}
