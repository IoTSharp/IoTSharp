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
        private const string DefaultMeasurement = nameof(TelemetryData);
        private const string DeviceTagName = "DeviceId";

        private static readonly HashSet<string> StorageOptionNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Measurement",
            "MeasurementName",
            "MeasurementPrefix",
            "AutoCreate"
        };

        private readonly string _connectionString;
        private readonly string _measurement;
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
            _measurement = storageOptions.Measurement;
            _autoCreate = storageOptions.AutoCreate;
        }

        public Task<bool> CheckTelemetryStorage()
        {
            try
            {
                using var connection = OpenConnection();
                TryLoadSchema(connection, _measurement);
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
            var fields = ResolveFields(connection, SplitKeys(keys));
            if (fields.Count == 0)
            {
                return Task.FromResult(new List<TelemetryDataDto>());
            }

            var data = QueryTelemetry(connection, deviceId, fields, begin, end);
            return Task.FromResult(AggregateDataHelpers.AggregateData(data, begin, end, every, aggregate));
        }

        public Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(PlayloadData msg)
        {
            var telemetries = ToTelemetryData(msg);
            if (telemetries.Count == 0)
            {
                return Task.FromResult((true, telemetries));
            }

            try
            {
                using var connection = OpenConnection();
                var schema = EnsureMeasurement(connection, telemetries);
                if (schema == null)
                {
                    return Task.FromResult((false, telemetries));
                }

                var writable = ResolveWritableFields(schema, telemetries, out var skipped);
                if (writable.Count == 0)
                {
                    _logger.LogWarning("SonnetDB measurement {Measurement} has no writable telemetry fields for device {DeviceId}.", _measurement, msg.DeviceId);
                    return Task.FromResult((false, telemetries));
                }

                var written = InsertTelemetry(connection, msg.ts, msg.DeviceId, writable);
                _logger.LogInformation("SonnetDB telemetry write completed. DeviceId={DeviceId}, Measurement={Measurement}, Count={Count}", msg.DeviceId, _measurement, written);
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
            var fields = ResolveFields(connection, keys);
            if (fields.Count == 0)
            {
                return [];
            }

            var data = QueryTelemetry(connection, deviceId, fields, null, null);
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

        private MeasurementSchemaInfo? EnsureMeasurement(SndbConnection connection, IReadOnlyList<TelemetryData> telemetries)
        {
            var schemaLock = _schemaLocks.GetOrAdd(_measurement, _ => new object());
            lock (schemaLock)
            {
                var schema = TryLoadSchema(connection, _measurement);
                if (schema != null)
                {
                    if (!schema.HasDeviceTag)
                    {
                        throw new InvalidOperationException($"SonnetDB measurement '{_measurement}' must contain TAG column '{DeviceTagName}'.");
                    }

                    return schema;
                }

                if (!_autoCreate)
                {
                    throw new InvalidOperationException($"SonnetDB measurement '{_measurement}' does not exist. Create it before writing telemetry.");
                }

                var fields = BuildFieldsFromTelemetry(telemetries);
                if (fields.Count == 0)
                {
                    _logger.LogWarning("SonnetDB measurement {Measurement} was not created because the payload has no writable telemetry fields.", _measurement);
                    return null;
                }

                using var command = connection.CreateCommand();
                command.CommandText = BuildCreateMeasurementSql(_measurement, fields);
                command.ExecuteNonQuery();

                schema = new MeasurementSchemaInfo(
                    [new ColumnInfo(DeviceTagName, ColumnRole.Tag, DataType.String), .. fields]);
                _schemas[_measurement] = schema;
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

        private List<ColumnInfo> ResolveFields(SndbConnection connection, IReadOnlyList<string> keys)
        {
            var schema = TryLoadSchema(connection, _measurement);
            if (schema == null)
            {
                return [];
            }

            var names = keys.Count == 0
                ? schema.Fields.Keys.OrderBy(x => x, StringComparer.Ordinal)
                : keys.Where(schema.Fields.ContainsKey);

            return names
                .Distinct(StringComparer.Ordinal)
                .Select(name => schema.Fields[name])
                .ToList();
        }

        private List<FieldWrite> ResolveWritableFields(MeasurementSchemaInfo schema, IReadOnlyList<TelemetryData> telemetries, out int skipped)
        {
            var writable = new List<FieldWrite>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            skipped = 0;

            foreach (var telemetry in telemetries)
            {
                if (IsReservedColumnName(telemetry.KeyName)
                    || string.Equals(telemetry.KeyName, DeviceTagName, StringComparison.Ordinal)
                    || !schema.Fields.TryGetValue(telemetry.KeyName, out var field)
                    || !seen.Add(telemetry.KeyName))
                {
                    skipped++;
                    continue;
                }

                if (!TryGetFieldValue(telemetry, field.DataType, out var value))
                {
                    skipped++;
                    _logger.LogWarning("SonnetDB measurement {Measurement} skipped telemetry key {KeyName} because the payload type {PayloadType} does not match FIELD type {FieldType}.", _measurement, telemetry.KeyName, telemetry.Type, field.DataType);
                    continue;
                }

                writable.Add(new FieldWrite(field, value));
            }

            if (skipped > 0)
            {
                _logger.LogWarning("SonnetDB measurement {Measurement} skipped {Count} telemetry fields because they are missing from schema or type-incompatible.", _measurement, skipped);
            }

            return writable;
        }

        private List<TelemetryDataDto> QueryTelemetry(SndbConnection connection, Guid deviceId, IReadOnlyList<ColumnInfo> fields, DateTime? begin, DateTime? end)
        {
            var data = new List<TelemetryDataDto>();
            using var command = connection.CreateCommand();

            var projection = string.Join(", ", fields.Select(x => QuoteIdentifier(x.Name)));
            var sql = new StringBuilder();
            sql.Append($"SELECT time, {projection} FROM {QuoteIdentifier(_measurement)} WHERE {QuoteIdentifier(DeviceTagName)} = @deviceId");
            command.Parameters.AddWithValue("@deviceId", deviceId.ToString());

            if (begin.HasValue)
            {
                sql.Append(" AND time >= @begin");
                command.Parameters.AddWithValue("@begin", ToUnixMilliseconds(begin.Value));
            }

            if (end.HasValue)
            {
                sql.Append(" AND time < @end");
                command.Parameters.AddWithValue("@end", ToUnixMilliseconds(end.Value));
            }

            command.CommandText = sql.ToString();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var time = ReadTime(reader.GetValue(reader.GetOrdinal("time")));
                foreach (var field in fields)
                {
                    var ordinal = reader.GetOrdinal(field.Name);
                    if (reader.IsDBNull(ordinal))
                    {
                        continue;
                    }

                    var value = ReadValue(reader.GetValue(ordinal), field.DataType);
                    if (value != null)
                    {
                        data.Add(new TelemetryDataDto
                        {
                            DateTime = time,
                            KeyName = field.Name,
                            DataType = field.DataType,
                            Value = value
                        });
                    }
                }
            }

            return data;
        }

        private int InsertTelemetry(SndbConnection connection, DateTime timestamp, Guid deviceId, IReadOnlyList<FieldWrite> writes)
        {
            using var command = connection.CreateCommand();
            var columns = new List<string> { "time", QuoteIdentifier(DeviceTagName) };
            var values = new List<string> { "@time", "@deviceId" };

            command.Parameters.AddWithValue("@time", ToUnixMilliseconds(timestamp));
            command.Parameters.AddWithValue("@deviceId", deviceId.ToString());

            for (var i = 0; i < writes.Count; i++)
            {
                var parameterName = $"@value{i}";
                columns.Add(QuoteIdentifier(writes[i].Field.Name));
                values.Add(parameterName);
                command.Parameters.AddWithValue(parameterName, writes[i].Value);
            }

            command.CommandText = $"INSERT INTO {QuoteIdentifier(_measurement)} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
            return command.ExecuteNonQuery();
        }

        private List<ColumnInfo> BuildFieldsFromTelemetry(IReadOnlyList<TelemetryData> telemetries)
        {
            var fields = new List<ColumnInfo>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var telemetry in telemetries)
            {
                if (string.IsNullOrWhiteSpace(telemetry.KeyName))
                {
                    continue;
                }

                if (IsReservedColumnName(telemetry.KeyName) || string.Equals(telemetry.KeyName, DeviceTagName, StringComparison.Ordinal))
                {
                    _logger.LogWarning("SonnetDB measurement {Measurement} skipped telemetry key {KeyName} because it conflicts with a reserved column.", _measurement, telemetry.KeyName);
                    continue;
                }

                if (!seen.Add(telemetry.KeyName))
                {
                    continue;
                }

                fields.Add(new ColumnInfo(telemetry.KeyName, ColumnRole.Field, ToFieldDataType(telemetry.Type)));
            }

            return fields;
        }

        private static string BuildCreateMeasurementSql(string measurement, IReadOnlyList<ColumnInfo> fields)
        {
            var sql = new StringBuilder();
            sql.Append($"CREATE MEASUREMENT {QuoteIdentifier(measurement)} ({QuoteIdentifier(DeviceTagName)} TAG");
            foreach (var field in fields)
            {
                sql.Append(", ");
                sql.Append(QuoteIdentifier(field.Name));
                sql.Append(" FIELD ");
                sql.Append(GetSonnetFieldType(field.DataType));
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

        private static bool TryGetFieldValue(TelemetryData telemetry, DataType dataType, out object value)
        {
            value = DBNull.Value;
            switch (dataType)
            {
                case DataType.Boolean when telemetry.Type == DataType.Boolean && telemetry.Value_Boolean.HasValue:
                    value = telemetry.Value_Boolean.Value;
                    return true;
                case DataType.String:
                    var text = ToFieldString(telemetry);
                    if (text == null)
                    {
                        return false;
                    }

                    value = text;
                    return true;
                case DataType.Long when telemetry.Type == DataType.Long && telemetry.Value_Long.HasValue:
                    value = telemetry.Value_Long.Value;
                    return true;
                case DataType.Long when telemetry.Type == DataType.DateTime && telemetry.Value_DateTime.HasValue:
                    value = ToUnixMilliseconds(telemetry.Value_DateTime.Value);
                    return true;
                case DataType.Double when telemetry.Type == DataType.Double && telemetry.Value_Double.HasValue:
                    value = telemetry.Value_Double.Value;
                    return true;
                case DataType.Double when telemetry.Type == DataType.Long && telemetry.Value_Long.HasValue:
                    value = Convert.ToDouble(telemetry.Value_Long.Value, CultureInfo.InvariantCulture);
                    return true;
                default:
                    return false;
            }
        }

        private static string? ToFieldString(TelemetryData telemetry)
        {
            return telemetry.Type switch
            {
                DataType.String => telemetry.Value_String,
                DataType.Json => telemetry.Value_Json,
                DataType.XML => telemetry.Value_XML,
                DataType.Binary when telemetry.Value_Binary != null => Convert.ToBase64String(telemetry.Value_Binary),
                _ => null
            };
        }

        private static object? ReadValue(object value, DataType dataType)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            return dataType switch
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

        private static bool IsReservedColumnName(string columnName)
        {
            return string.Equals(columnName, "time", StringComparison.OrdinalIgnoreCase);
        }

        private static DataType ToFieldDataType(DataType dataType)
        {
            return dataType switch
            {
                DataType.Boolean => DataType.Boolean,
                DataType.Long or DataType.DateTime => DataType.Long,
                DataType.Double => DataType.Double,
                DataType.String or DataType.Json or DataType.XML or DataType.Binary => DataType.String,
                _ => DataType.String
            };
        }

        private static string GetSonnetFieldType(DataType dataType)
        {
            return dataType switch
            {
                DataType.Boolean => "BOOL",
                DataType.Long => "INT",
                DataType.Double => "FLOAT",
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

            var measurement = GetOption(raw, "Measurement")
                              ?? GetOption(raw, "MeasurementName")
                              ?? DefaultMeasurement;

            return new SonnetDBStorageOptions(
                sonnetConnection.ConnectionString,
                measurement,
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

        private sealed record FieldWrite(ColumnInfo Field, object Value);

        private sealed class MeasurementSchemaInfo
        {
            public MeasurementSchemaInfo(IEnumerable<ColumnInfo> columns)
            {
                Columns = columns.ToDictionary(x => x.Name, StringComparer.Ordinal);
                Fields = Columns
                    .Where(x => x.Value.Role == ColumnRole.Field)
                    .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
                HasDeviceTag = Columns.TryGetValue(DeviceTagName, out var deviceColumn)
                               && deviceColumn.Role == ColumnRole.Tag;
            }

            public IReadOnlyDictionary<string, ColumnInfo> Columns { get; }

            public IReadOnlyDictionary<string, ColumnInfo> Fields { get; }

            public bool HasDeviceTag { get; }
        }

        private sealed record SonnetDBStorageOptions(string ConnectionString, string Measurement, bool AutoCreate);
    }
}
