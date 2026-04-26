using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SonnetDB.Data;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Globalization;
using System.Text;
using DataType = IoTSharp.Contracts.DataType;

namespace IoTSharp.Storage
{
    public class SonnetDBStorage : IStorage
    {
        private static readonly HashSet<string> StorageOptionNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Measurement",
            "MeasurementName",
            "MeasurementPrefix",
            "AutoCreate"
        };

        private readonly string _connectionString;
        private readonly bool _autoCreate;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, MeasurementSchemaInfo> _schemas = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, object> _schemaLocks = new(StringComparer.Ordinal);

        public SonnetDBStorage(ILogger<SonnetDBStorage> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            var settings = options.Value;
            var connectionString = settings.ConnectionStrings?["TelemetryStorage"]
                                   ?? throw new InvalidOperationException("ConnectionStrings:TelemetryStorage is required for SonnetDB.");

            var storageOptions = ParseStorageOptions(connectionString);
            _connectionString = storageOptions.ConnectionString;
            _autoCreate = storageOptions.AutoCreate;
        }

        public Task<bool> CheckTelemetryStorage()
        {
            try
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "SHOW MEASUREMENTS";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    _ = reader.GetValue(0);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SonnetDB telemetry storage check failed: {Message}", ex.Message);
                return Task.FromResult(false);
            }
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {
            return Task.FromResult(GetTelemetryLatestCore(deviceId, Array.Empty<string>()));
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            return Task.FromResult(GetTelemetryLatestCore(deviceId, SplitKeys(keys)));
        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            using var connection = OpenConnection();
            var measurement = GetMeasurementName(deviceId);
            var columns = ResolveColumns(connection, measurement, SplitKeys(keys));
            if (columns.Count == 0)
            {
                return Task.FromResult(new List<TelemetryDataDto>());
            }

            var data = QueryTelemetry(connection, measurement, columns, begin, end);
            return Task.FromResult(AggregateDataHelpers.AggregateData(data, begin, end, every, aggregate));
        }

        public Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(PlayloadData msg)
        {
            var telemetries = ToTelemetryData(msg);
            if (telemetries.Count == 0)
            {
                return Task.FromResult((true, telemetries));
            }

            var measurement = GetMeasurementName(msg.DeviceId);
            try
            {
                using var connection = OpenConnection();
                var schema = EnsureMeasurement(connection, measurement, telemetries);
                if (schema == null)
                {
                    return Task.FromResult((false, telemetries));
                }

                var writable = ResolveWritableColumns(measurement, schema, telemetries, out var skipped);
                if (writable.Count == 0)
                {
                    _logger.LogWarning("SonnetDB measurement {Measurement} has no writable telemetry columns for device {DeviceId}.", measurement, msg.DeviceId);
                    return Task.FromResult((false, telemetries));
                }

                if (!writable.Any(x => x.Column.Role == ColumnRole.Field))
                {
                    _logger.LogWarning("SonnetDB measurement {Measurement} skipped write for device {DeviceId} because INSERT requires at least one FIELD value.", measurement, msg.DeviceId);
                    return Task.FromResult((false, telemetries));
                }

                var written = InsertTelemetry(connection, measurement, msg.ts, writable);
                _logger.LogInformation("SonnetDB telemetry write completed. DeviceId={DeviceId}, Measurement={Measurement}, Count={Count}", msg.DeviceId, measurement, written);
                return Task.FromResult((written == 1 && skipped == 0, telemetries));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{DeviceId} SonnetDB telemetry write failed: {Message} {InnerMessage}", msg.DeviceId, ex.Message, ex.InnerException?.Message);
                return Task.FromResult((false, telemetries));
            }
        }

        private List<TelemetryDataDto> GetTelemetryLatestCore(Guid deviceId, IReadOnlyList<string> keys)
        {
            using var connection = OpenConnection();
            var measurement = GetMeasurementName(deviceId);
            var columns = ResolveColumns(connection, measurement, keys);
            if (columns.Count == 0)
            {
                return [];
            }

            var data = QueryTelemetry(connection, measurement, columns, null, null);
            return data
                .GroupBy(x => x.KeyName, StringComparer.Ordinal)
                .Select(x => x.OrderByDescending(y => y.DateTime).First())
                .ToList();
        }

        private SndbConnection OpenConnection()
        {
            var connection = new SndbConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private MeasurementSchemaInfo? EnsureMeasurement(SndbConnection connection, string measurement, IReadOnlyList<TelemetryData> telemetries)
        {
            var schemaLock = _schemaLocks.GetOrAdd(measurement, _ => new object());
            lock (schemaLock)
            {
                var schema = TryLoadSchema(connection, measurement);
                if (schema != null)
                {
                    return schema;
                }

                if (!_autoCreate)
                {
                    throw new InvalidOperationException($"SonnetDB measurement '{measurement}' does not exist. Create it before writing telemetry.");
                }

                var columns = BuildColumnsFromTelemetry(measurement, telemetries);
                if (!columns.Any(x => x.Role == ColumnRole.Field))
                {
                    _logger.LogWarning("SonnetDB measurement {Measurement} was not created because the payload has no numeric telemetry FIELD.", measurement);
                    return null;
                }

                using var command = connection.CreateCommand();
                command.CommandText = BuildCreateMeasurementSql(measurement, columns);
                command.ExecuteNonQuery();

                schema = new MeasurementSchemaInfo(columns);
                _schemas[measurement] = schema;
                return schema;
            }
        }

        private MeasurementSchemaInfo? TryLoadSchema(SndbConnection connection, string measurement)
        {
            if (_schemas.TryGetValue(measurement, out var cached))
            {
                return cached;
            }

            if (!MeasurementExists(connection, measurement))
            {
                return null;
            }

            var columns = new List<ColumnInfo>();
            using var command = connection.CreateCommand();
            command.CommandText = $"DESCRIBE MEASUREMENT {QuoteIdentifier(measurement)}";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(reader.GetOrdinal("column_name"));
                var columnType = reader.GetString(reader.GetOrdinal("column_type"));
                var dataType = reader.GetString(reader.GetOrdinal("data_type"));
                var role = string.Equals(columnType, "tag", StringComparison.OrdinalIgnoreCase)
                    ? ColumnRole.Tag
                    : ColumnRole.Field;

                columns.Add(new ColumnInfo(name, role, role == ColumnRole.Tag ? DataType.String : ToIoTSharpDataType(dataType)));
            }

            var schema = new MeasurementSchemaInfo(columns);
            _schemas[measurement] = schema;
            return schema;
        }

        private bool MeasurementExists(SndbConnection connection, string measurement)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SHOW MEASUREMENTS";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(0), measurement, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private List<ColumnInfo> ResolveColumns(SndbConnection connection, string measurement, IReadOnlyList<string> keys)
        {
            var schema = TryLoadSchema(connection, measurement);
            if (schema == null)
            {
                return [];
            }

            var names = keys.Count == 0
                ? schema.Columns.Keys.OrderBy(x => x, StringComparer.Ordinal)
                : keys.Where(schema.Columns.ContainsKey);

            return names
                .Distinct(StringComparer.Ordinal)
                .Select(name => schema.Columns[name])
                .ToList();
        }

        private List<ColumnWrite> ResolveWritableColumns(string measurement, MeasurementSchemaInfo schema, IReadOnlyList<TelemetryData> telemetries, out int skipped)
        {
            var writable = new List<ColumnWrite>();
            skipped = 0;

            foreach (var telemetry in telemetries)
            {
                if (IsReservedColumnName(telemetry.KeyName) || !schema.Columns.TryGetValue(telemetry.KeyName, out var column))
                {
                    skipped++;
                    continue;
                }

                if (!TryGetSonnetValue(telemetry, column, out var value))
                {
                    skipped++;
                    _logger.LogWarning("SonnetDB measurement {Measurement} skipped telemetry key {KeyName} because the payload type {PayloadType} does not match schema column role/type.", measurement, telemetry.KeyName, telemetry.Type);
                    continue;
                }

                writable.Add(new ColumnWrite(column, value));
            }

            if (skipped > 0)
            {
                _logger.LogWarning("SonnetDB measurement {Measurement} skipped {Count} telemetry columns because they are missing from schema or type-incompatible.", measurement, skipped);
            }

            return writable;
        }

        private List<TelemetryDataDto> QueryTelemetry(SndbConnection connection, string measurement, IReadOnlyList<ColumnInfo> columns, DateTime? begin, DateTime? end)
        {
            var data = new List<TelemetryDataDto>();
            using var command = connection.CreateCommand();

            var projection = string.Join(", ", columns.Select(x => QuoteIdentifier(x.Name)));
            var sql = new StringBuilder();
            sql.Append($"SELECT time, {projection} FROM {QuoteIdentifier(measurement)}");

            var where = new List<string>();
            if (begin.HasValue)
            {
                where.Add("time >= @begin");
                command.Parameters.AddWithValue("@begin", ToUnixMilliseconds(begin.Value));
            }

            if (end.HasValue)
            {
                where.Add("time < @end");
                command.Parameters.AddWithValue("@end", ToUnixMilliseconds(end.Value));
            }

            if (where.Count > 0)
            {
                sql.Append(" WHERE ");
                sql.Append(string.Join(" AND ", where));
            }

            command.CommandText = sql.ToString();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var time = ReadTime(reader.GetValue(reader.GetOrdinal("time")));
                foreach (var column in columns)
                {
                    var ordinal = reader.GetOrdinal(column.Name);
                    if (reader.IsDBNull(ordinal))
                    {
                        continue;
                    }

                    var value = ReadValue(reader.GetValue(ordinal), column);
                    if (value != null)
                    {
                        data.Add(new TelemetryDataDto
                        {
                            DateTime = time,
                            KeyName = column.Name,
                            DataType = column.Role == ColumnRole.Tag ? DataType.String : column.DataType,
                            Value = value
                        });
                    }
                }
            }

            return data;
        }

        private static int InsertTelemetry(SndbConnection connection, string measurement, DateTime timestamp, IReadOnlyList<ColumnWrite> writes)
        {
            using var command = connection.CreateCommand();
            var columns = new List<string> { "time" };
            var values = new List<string> { "@time" };

            command.Parameters.AddWithValue("@time", ToUnixMilliseconds(timestamp));

            for (var i = 0; i < writes.Count; i++)
            {
                var parameterName = $"@value{i}";
                columns.Add(QuoteIdentifier(writes[i].Column.Name));
                values.Add(parameterName);
                command.Parameters.AddWithValue(parameterName, writes[i].Value);
            }

            command.CommandText = $"INSERT INTO {QuoteIdentifier(measurement)} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
            return command.ExecuteNonQuery();
        }

        private List<ColumnInfo> BuildColumnsFromTelemetry(string measurement, IReadOnlyList<TelemetryData> telemetries)
        {
            var columns = new List<ColumnInfo>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var telemetry in telemetries)
            {
                if (string.IsNullOrWhiteSpace(telemetry.KeyName))
                {
                    continue;
                }

                if (IsReservedColumnName(telemetry.KeyName))
                {
                    _logger.LogWarning("SonnetDB measurement {Measurement} skipped telemetry key {KeyName} because it conflicts with the reserved time column.", measurement, telemetry.KeyName);
                    continue;
                }

                if (!seen.Add(telemetry.KeyName))
                {
                    continue;
                }

                columns.Add(IsNumericTelemetry(telemetry.Type)
                    ? new ColumnInfo(telemetry.KeyName, ColumnRole.Field, ToNumericDataType(telemetry.Type))
                    : new ColumnInfo(telemetry.KeyName, ColumnRole.Tag, DataType.String));
            }

            return columns;
        }

        private static string BuildCreateMeasurementSql(string measurement, IReadOnlyList<ColumnInfo> columns)
        {
            var sql = new StringBuilder();
            sql.Append($"CREATE MEASUREMENT {QuoteIdentifier(measurement)} (");
            for (var i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                {
                    sql.Append(", ");
                }

                var column = columns[i];
                sql.Append(QuoteIdentifier(column.Name));
                sql.Append(column.Role == ColumnRole.Tag ? " TAG" : $" FIELD {GetSonnetFieldType(column.DataType)}");
            }

            sql.Append(')');
            return sql.ToString();
        }

        private static List<TelemetryData> ToTelemetryData(PlayloadData msg)
        {
            var telemetries = new List<TelemetryData>();
            foreach (var item in msg.MsgBody)
            {
                if (item.Value == null)
                {
                    continue;
                }

                var telemetry = new TelemetryData
                {
                    DateTime = msg.ts,
                    DeviceId = msg.DeviceId,
                    KeyName = item.Key,
                    DataSide = msg.DataSide,
                    Value_DateTime = DateTime.UnixEpoch
                };
                telemetry.FillKVToMe(item);

                if (HasValue(telemetry))
                {
                    telemetries.Add(telemetry);
                }
            }

            return telemetries;
        }

        private static bool HasValue(TelemetryData telemetry)
        {
            return telemetry.Type switch
            {
                DataType.Boolean => telemetry.Value_Boolean.HasValue,
                DataType.String => telemetry.Value_String != null,
                DataType.Long => telemetry.Value_Long.HasValue,
                DataType.Double => telemetry.Value_Double.HasValue,
                DataType.Json => telemetry.Value_Json != null,
                DataType.XML => telemetry.Value_XML != null,
                DataType.Binary => telemetry.Value_Binary != null,
                DataType.DateTime => telemetry.Value_DateTime.HasValue,
                _ => false
            };
        }

        private static bool TryGetSonnetValue(TelemetryData telemetry, ColumnInfo column, out object value)
        {
            value = DBNull.Value;

            if (column.Role == ColumnRole.Tag)
            {
                if (IsNumericTelemetry(telemetry.Type))
                {
                    return false;
                }

                var tagValue = ToTagValue(telemetry);
                if (tagValue == null)
                {
                    return false;
                }

                value = tagValue;
                return true;
            }

            return TryGetFieldValue(telemetry, column.DataType, out value);
        }

        private static bool TryGetFieldValue(TelemetryData telemetry, DataType dataType, out object value)
        {
            value = DBNull.Value;
            switch (dataType)
            {
                case DataType.Long when telemetry.Type == DataType.Long && telemetry.Value_Long.HasValue:
                    value = telemetry.Value_Long.Value;
                    return true;
                case DataType.Double when telemetry.Type == DataType.Double && telemetry.Value_Double.HasValue:
                    value = telemetry.Value_Double.Value;
                    return true;
                case DataType.Double when telemetry.Type == DataType.Long && telemetry.Value_Long.HasValue:
                    value = Convert.ToDouble(telemetry.Value_Long.Value, CultureInfo.InvariantCulture);
                    return true;
                case DataType.Boolean when telemetry.Type == DataType.Boolean && telemetry.Value_Boolean.HasValue:
                    value = telemetry.Value_Boolean.Value;
                    return true;
                case DataType.String:
                    var text = ToTagValue(telemetry);
                    if (text == null)
                    {
                        return false;
                    }

                    value = text;
                    return true;
                default:
                    return false;
            }
        }

        private static string? ToTagValue(TelemetryData telemetry)
        {
            return telemetry.Type switch
            {
                DataType.Boolean when telemetry.Value_Boolean.HasValue => telemetry.Value_Boolean.Value.ToString().ToLowerInvariant(),
                DataType.String => telemetry.Value_String,
                DataType.Json => telemetry.Value_Json,
                DataType.XML => telemetry.Value_XML,
                DataType.Binary when telemetry.Value_Binary != null => Convert.ToBase64String(telemetry.Value_Binary),
                DataType.DateTime when telemetry.Value_DateTime.HasValue => telemetry.Value_DateTime.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                _ => null
            };
        }

        private static object? ReadValue(object value, ColumnInfo column)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            if (column.Role == ColumnRole.Tag)
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            return column.DataType switch
            {
                DataType.Boolean => Convert.ToBoolean(value, CultureInfo.InvariantCulture),
                DataType.String => Convert.ToString(value, CultureInfo.InvariantCulture),
                DataType.Long => Convert.ToInt64(value, CultureInfo.InvariantCulture),
                DataType.Double => Convert.ToDouble(value, CultureInfo.InvariantCulture),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture)
            };
        }

        private static DateTime ReadTime(object value)
        {
            return value switch
            {
                DateTime dateTime => dateTime,
                DateTimeOffset dateTimeOffset => dateTimeOffset.UtcDateTime,
                long milliseconds => DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime,
                int milliseconds => DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime,
                _ => Convert.ToDateTime(value, CultureInfo.InvariantCulture)
            };
        }

        private static long ToUnixMilliseconds(DateTime dateTime)
        {
            var utc = dateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.ToUniversalTime();
            return new DateTimeOffset(utc).ToUnixTimeMilliseconds();
        }

        private static string GetMeasurementName(Guid deviceId)
        {
            return deviceId.ToString("N");
        }

        private static bool IsNumericTelemetry(DataType dataType)
        {
            return dataType is DataType.Long or DataType.Double;
        }

        private static DataType ToNumericDataType(DataType dataType)
        {
            return dataType == DataType.Long ? DataType.Long : DataType.Double;
        }

        private static bool IsReservedColumnName(string columnName)
        {
            return string.Equals(columnName, "time", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetSonnetFieldType(DataType dataType)
        {
            return dataType switch
            {
                DataType.Long => "INT",
                DataType.Double => "FLOAT",
                DataType.Boolean => "BOOL",
                DataType.String => "STRING",
                _ => "STRING"
            };
        }

        private static DataType ToIoTSharpDataType(string sonnetDataType)
        {
            return sonnetDataType.ToLowerInvariant() switch
            {
                "boolean" or "bool" => DataType.Boolean,
                "int64" or "int" => DataType.Long,
                "float64" or "float" or "double" => DataType.Double,
                "string" => DataType.String,
                _ => DataType.String
            };
        }

        private static string[] SplitKeys(string keys)
        {
            if (string.IsNullOrWhiteSpace(keys))
            {
                return [];
            }

            return keys
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }

        private static SonnetDBStorageOptions ParseStorageOptions(string connectionString)
        {
            var raw = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };
            var sonnetConnection = new SndbConnectionStringBuilder();

            foreach (string key in raw.Keys)
            {
                if (!StorageOptionNames.Contains(key))
                {
                    sonnetConnection[key] = raw[key];
                }
            }

            return new SonnetDBStorageOptions(
                sonnetConnection.ConnectionString,
                GetBoolOption(raw, "AutoCreate", true));
        }

        private static string? GetOption(DbConnectionStringBuilder builder, string key)
        {
            return builder.TryGetValue(key, out var value) ? value?.ToString() : null;
        }

        private static bool GetBoolOption(DbConnectionStringBuilder builder, string key, bool defaultValue)
        {
            var value = GetOption(builder, key);
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }

        private static string QuoteIdentifier(string identifier)
        {
            return "\"" + identifier.Replace("\"", "\"\"") + "\"";
        }

        private enum ColumnRole
        {
            Tag,
            Field
        }

        private sealed record ColumnInfo(string Name, ColumnRole Role, DataType DataType);

        private sealed record ColumnWrite(ColumnInfo Column, object Value);

        private sealed class MeasurementSchemaInfo
        {
            public MeasurementSchemaInfo(IEnumerable<ColumnInfo> columns)
            {
                Columns = columns.ToDictionary(x => x.Name, StringComparer.Ordinal);
            }

            public IReadOnlyDictionary<string, ColumnInfo> Columns { get; }
        }

        private sealed record SonnetDBStorageOptions(string ConnectionString, bool AutoCreate);
    }
}
