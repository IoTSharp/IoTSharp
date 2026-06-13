using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SonnetDB.Data;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using DataType = IoTSharp.Contracts.DataType;

namespace IoTSharp.Storage
{
    public class SonnetDBStorage : IStorage
    {
        private const string DefaultMeasurement = nameof(TelemetryData);

        private static readonly HashSet<string> StorageOptionNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Measurement",
            "MeasurementName",
            "MeasurementPrefix",
            "AutoCreate"
        };

        private readonly string _connectionString;
        private readonly string _dataSource;
        private readonly SndbProviderMode _providerMode;
        private readonly string _measurementPrefix;
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
            _dataSource = storageOptions.DataSource;
            _providerMode = storageOptions.ProviderMode;
            _measurementPrefix = storageOptions.MeasurementPrefix;
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
            var measurement = GetMeasurement(deviceId);
            var fields = ResolveFields(connection, measurement, SplitKeys(keys));
            if (fields.Count == 0)
            {
                return Task.FromResult(new List<TelemetryDataDto>());
            }

            if (TryQueryAggregateTelemetry(connection, measurement, fields, begin, end, every, aggregate, out var aggregateData))
            {
                return Task.FromResult(aggregateData);
            }

            var data = QueryTelemetry(connection, measurement, fields, begin, end);
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
                var measurement = GetMeasurement(msg.DeviceId);
                var schema = EnsureMeasurement(connection, measurement, telemetries);
                if (schema == null)
                {
                    return Task.FromResult((false, telemetries));
                }

                var writable = ResolveWritableFields(measurement, schema, telemetries, out var skipped, out var schemaChanged);
                if (writable.Count == 0)
                {
                    _logger.LogWarning("SonnetDB measurement {Measurement} has no writable telemetry fields for device {DeviceId}.", measurement, msg.DeviceId);
                    return Task.FromResult((false, telemetries));
                }

                var written = InsertTelemetry(connection, measurement, msg.ts, writable);
                if (schemaChanged && written == 1)
                {
                    RefreshSchema(connection, measurement);
                }

                _logger.LogInformation("SonnetDB telemetry write completed. DeviceId={DeviceId}, Measurement={Measurement}, Count={Count}", msg.DeviceId, measurement, written);
                return Task.FromResult((written == 1 && skipped == 0, telemetries));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{DeviceId} SonnetDB telemetry write failed: {Message} {InnerMessage}", msg.DeviceId, ex.Message, ex.InnerException?.Message);
                return Task.FromResult((false, telemetries));
            }
        }

        public Task<SonnetDbTelemetryBatchWriteReport> StoreTelemetryBatchAsync(IEnumerable<PlayloadData> messages)
        {
            ArgumentNullException.ThrowIfNull(messages);

            var batches = new Dictionary<BatchWriteKey, BatchWriteGroup>();
            var totalMessages = 0;
            var totalTelemetryValues = 0;
            var skippedTelemetryValues = 0;

            try
            {
                using var connection = OpenConnection();
                foreach (var message in messages)
                {
                    totalMessages++;
                    var telemetries = ToTelemetryData(message);
                    totalTelemetryValues += telemetries.Count;
                    if (telemetries.Count == 0)
                    {
                        continue;
                    }

                    var measurement = GetMeasurement(message.DeviceId);
                    var schema = EnsureMeasurement(connection, measurement, telemetries);
                    if (schema == null)
                    {
                        skippedTelemetryValues += telemetries.Count;
                        continue;
                    }

                    var writable = ResolveWritableFields(measurement, schema, telemetries, out var skipped, out var schemaChanged);
                    skippedTelemetryValues += skipped;
                    if (writable.Count == 0)
                    {
                        continue;
                    }

                    var fields = writable
                        .Select(static x => x.Field)
                        .OrderBy(static x => x.Name, StringComparer.Ordinal)
                        .ToArray();
                    var values = fields
                        .Select(field => writable.Single(x => string.Equals(x.Field.Name, field.Name, StringComparison.Ordinal)).Value)
                        .ToArray();
                    var key = new BatchWriteKey(measurement, string.Join('\u001f', fields.Select(static x => x.Name)));
                    if (!batches.TryGetValue(key, out var group))
                    {
                        group = new BatchWriteGroup(measurement, fields);
                        batches.Add(key, group);
                    }

                    group.Rows.Add(new BatchWriteRow(message.ts, values));
                    group.SchemaChanged |= schemaChanged;
                }

                var writtenRows = 0;
                foreach (var group in batches.Values)
                {
                    writtenRows += InsertTelemetryBatch(connection, group.Measurement, group.Fields, group.Rows);
                    if (group.SchemaChanged)
                    {
                        RefreshSchema(connection, group.Measurement);
                    }
                }

                return Task.FromResult(new SonnetDbTelemetryBatchWriteReport(
                    totalMessages,
                    batches.Count,
                    writtenRows,
                    totalTelemetryValues,
                    skippedTelemetryValues,
                    skippedTelemetryValues == 0));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SonnetDB telemetry batch write failed: {Message} {InnerMessage}", ex.Message, ex.InnerException?.Message);
                return Task.FromResult(new SonnetDbTelemetryBatchWriteReport(
                    totalMessages,
                    batches.Count,
                    0,
                    totalTelemetryValues,
                    Math.Max(skippedTelemetryValues, totalTelemetryValues),
                    false));
            }
        }

        public SonnetDbTelemetryBackupReport CreateBackup(string destinationDirectory, bool overwrite = false, bool includeFullTextIndexes = false)
        {
            EnsureEmbeddedOperation(nameof(CreateBackup));
            _ = includeFullTextIndexes;
            var manifest = CreateDirectoryBackup(GetEmbeddedRootDirectory(), destinationDirectory, overwrite);
            return ToBackupReport(manifest);
        }

        public SonnetDbTelemetryBackupVerificationReport VerifyBackup(string backupDirectory)
        {
            var result = VerifyDirectoryBackup(backupDirectory);
            return new SonnetDbTelemetryBackupVerificationReport(result.IsValid, result.CheckedFiles, result.Errors);
        }

        public SonnetDbTelemetryRestoreDryRunReport RestoreDryRun(string backupDirectory, string targetDirectory, bool overwrite = false, bool verifyBeforeRestore = true)
        {
            EnsureEmbeddedOperation(nameof(RestoreDryRun));
            var result = RestoreDryRunDirectoryBackup(backupDirectory, targetDirectory, overwrite, verifyBeforeRestore);

            return new SonnetDbTelemetryRestoreDryRunReport(
                result.IsValid,
                result.FileCount,
                result.TotalBytes,
                result.IndexCount,
                result.TargetDirectoryExists,
                result.TargetDirectoryEmpty,
                result.Errors);
        }

        public SonnetDbTelemetryBackupReport Restore(string backupDirectory, string targetDirectory, bool overwrite = false, bool verifyBeforeRestore = true)
        {
            EnsureEmbeddedOperation(nameof(Restore));
            var manifest = RestoreDirectoryBackup(backupDirectory, targetDirectory, overwrite, verifyBeforeRestore);
            return ToBackupReport(manifest);
        }

        public Task<SonnetDbTelemetryReplayReport> ReplayFailureAsync(Guid deviceId, string keys, DateTime begin, DateTime end)
        {
            EnsureEmbeddedOperation(nameof(ReplayFailureAsync));
            using var connection = OpenConnection();
            var measurement = GetMeasurement(deviceId);
            var fields = ResolveFields(connection, measurement, SplitKeys(keys));
            var points = fields.Count == 0
                ? []
                : QueryTelemetry(connection, measurement, fields, begin, end);

            return Task.FromResult(new SonnetDbTelemetryReplayReport(
                _dataSource,
                measurement,
                points.Count,
                fields.Select(static x => x.Name).Order(StringComparer.Ordinal).ToArray()));
        }

        private List<TelemetryDataDto> GetTelemetryLatestCore(Guid deviceId, IReadOnlyList<string> keys)
        {
            using var connection = OpenConnection();
            var measurement = GetMeasurement(deviceId);
            var fields = ResolveFields(connection, measurement, keys);
            if (fields.Count == 0)
            {
                return [];
            }

            return fields
                .Select(field => QueryLatestTelemetry(connection, measurement, field))
                .Where(static x => x != null)
                .Select(static x => x!)
                .ToList();
        }

        private SndbConnection OpenConnection()
        {
            var connection = new SndbConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private string GetMeasurement(Guid deviceId)
        {
            return $"{_measurementPrefix}_{deviceId:N}";
        }

        private MeasurementSchemaInfo? EnsureMeasurement(SndbConnection connection, string measurement, IReadOnlyList<TelemetryData> telemetries)
        {
            var schemaLock = _schemaLocks.GetOrAdd(measurement, _ => new object());
            lock (schemaLock)
            {
                if (_schemas.TryGetValue(measurement, out var cached))
                {
                    return cached;
                }

                var existing = TryLoadSchema(connection, measurement);
                if (existing != null)
                {
                    return existing;
                }

                var fields = BuildFieldsFromTelemetry(measurement, telemetries);
                if (fields.Count == 0)
                {
                    _logger.LogWarning("SonnetDB measurement {Measurement} was not created because the payload has no writable telemetry fields.", measurement);
                    return null;
                }

                if (!_autoCreate)
                {
                    throw new InvalidOperationException($"SonnetDB measurement '{measurement}' does not exist. Create it before writing telemetry.");
                }

                using var command = connection.CreateCommand();
                command.CommandText = BuildCreateMeasurementSql(measurement, fields);
                command.ExecuteNonQuery();

                var schema = new MeasurementSchemaInfo(fields);
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

            var columns = new List<ColumnInfo>();
            using var command = connection.CreateCommand();
            command.CommandText = $"DESCRIBE MEASUREMENT {QuoteIdentifier(measurement)}";
            DbDataReader reader;
            try
            {
                reader = command.ExecuteReader();
            }
            catch (NotSupportedException)
            {
                return null;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("不存在", StringComparison.OrdinalIgnoreCase)
                                                       || ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            using (reader)
            {
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
            }

            var schema = new MeasurementSchemaInfo(columns);
            _schemas[measurement] = schema;
            return schema;
        }

        private List<ColumnInfo> ResolveFields(SndbConnection connection, string measurement, IReadOnlyList<string> keys)
        {
            var schema = TryLoadSchema(connection, measurement);
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

        private List<FieldWrite> ResolveWritableFields(
            string measurement,
            MeasurementSchemaInfo schema,
            IReadOnlyList<TelemetryData> telemetries,
            out int skipped,
            out bool schemaChanged)
        {
            var writable = new List<FieldWrite>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            skipped = 0;
            schemaChanged = false;

            foreach (var telemetry in telemetries)
            {
                if (IsReservedColumnName(telemetry.KeyName)
                    || !seen.Add(telemetry.KeyName))
                {
                    skipped++;
                    continue;
                }

                if (!schema.Fields.TryGetValue(telemetry.KeyName, out var field))
                {
                    if (!_autoCreate)
                    {
                        skipped++;
                        continue;
                    }

                    field = new ColumnInfo(telemetry.KeyName, ColumnRole.Field, ToFieldDataType(telemetry.Type));
                    schemaChanged = true;
                }

                if (!TryGetFieldValue(telemetry, field.DataType, out var value))
                {
                    skipped++;
                    _logger.LogWarning("SonnetDB measurement {Measurement} skipped telemetry key {KeyName} because the payload type {PayloadType} does not match FIELD type {FieldType}.", measurement, telemetry.KeyName, telemetry.Type, field.DataType);
                    continue;
                }

                writable.Add(new FieldWrite(field, value));
            }

            if (skipped > 0)
            {
                _logger.LogWarning("SonnetDB measurement {Measurement} skipped {Count} telemetry fields because they are missing from schema or type-incompatible.", measurement, skipped);
            }

            return writable;
        }

        private void RefreshSchema(SndbConnection connection, string measurement)
        {
            _schemas.TryRemove(measurement, out _);
            _ = TryLoadSchema(connection, measurement);
        }

        private List<TelemetryDataDto> QueryTelemetry(SndbConnection connection, string measurement, IReadOnlyList<ColumnInfo> fields, DateTime? begin, DateTime? end)
        {
            var data = new List<TelemetryDataDto>();
            using var command = connection.CreateCommand();

            var projection = string.Join(", ", fields.Select(x => QuoteIdentifier(x.Name)));
            var sql = new StringBuilder();
            sql.Append($"SELECT time, {projection} FROM {QuoteIdentifier(measurement)}");

            if (begin.HasValue)
            {
                AppendWhereOrAnd(sql);
                sql.Append("time >= @begin");
                command.Parameters.AddWithValue("@begin", ToUnixMilliseconds(begin.Value));
            }

            if (end.HasValue)
            {
                AppendWhereOrAnd(sql);
                sql.Append("time < @end");
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

        private TelemetryDataDto? QueryLatestTelemetry(SndbConnection connection, string measurement, ColumnInfo field)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT time, {QuoteIdentifier(field.Name)} FROM {QuoteIdentifier(measurement)} ORDER BY time DESC";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var ordinal = reader.GetOrdinal(field.Name);
                if (reader.IsDBNull(ordinal))
                {
                    continue;
                }

                var value = ReadValue(reader.GetValue(ordinal), field.DataType);
                if (value == null)
                {
                    continue;
                }

                return new TelemetryDataDto
                {
                    DateTime = ReadTime(reader.GetValue(reader.GetOrdinal("time"))),
                    KeyName = field.Name,
                    DataType = field.DataType,
                    Value = value
                };
            }

            return null;
        }

        private bool TryQueryAggregateTelemetry(
            SndbConnection connection,
            string measurement,
            IReadOnlyList<ColumnInfo> fields,
            DateTime begin,
            DateTime end,
            TimeSpan every,
            Aggregate aggregate,
            out List<TelemetryDataDto> data)
        {
            data = [];
            var aggregateFunction = ToSonnetAggregateFunction(aggregate);
            if (aggregateFunction == null)
            {
                return false;
            }

            if (every == TimeSpan.Zero)
            {
                every = TimeSpan.FromMinutes(1);
            }

            var numericFields = fields
                .Where(static x => x.DataType is DataType.Double or DataType.Long)
                .ToArray();
            if (numericFields.Length == 0 || every <= TimeSpan.Zero || end <= begin)
            {
                return true;
            }

            var bucketCount = (int)((end - begin).Ticks / every.Ticks);
            for (var i = 0; i < bucketCount; i++)
            {
                var bucketBegin = begin + (i * every);
                var bucketEnd = begin + ((i + 1) * every);
                foreach (var field in numericFields)
                {
                    var value = QueryAggregateValue(connection, measurement, field, bucketBegin, bucketEnd, aggregateFunction, aggregate);
                    if (value == null)
                    {
                        continue;
                    }

                    data.Add(new TelemetryDataDto
                    {
                        DateTime = bucketEnd,
                        KeyName = field.Name,
                        DataType = field.DataType,
                        Value = value
                    });
                }
            }

            return true;
        }

        private static object? QueryAggregateValue(
            SndbConnection connection,
            string measurement,
            ColumnInfo field,
            DateTime begin,
            DateTime end,
            string aggregateFunction,
            Aggregate aggregate)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT {aggregateFunction}({QuoteIdentifier(field.Name)}) FROM {QuoteIdentifier(measurement)} WHERE time >= @begin AND time < @end";
            command.Parameters.AddWithValue("@begin", ToUnixMilliseconds(begin));
            command.Parameters.AddWithValue("@end", ToUnixMilliseconds(end));
            var raw = command.ExecuteScalar();
            if (raw == null || raw == DBNull.Value)
            {
                return null;
            }

            if (field.DataType == DataType.Long && aggregate != Aggregate.Mean)
            {
                return Convert.ToInt64(raw, CultureInfo.InvariantCulture);
            }

            if (field.DataType == DataType.Long && aggregate == Aggregate.Mean)
            {
                return (long)Convert.ToDouble(raw, CultureInfo.InvariantCulture);
            }

            return Convert.ToDouble(raw, CultureInfo.InvariantCulture);
        }

        private static string? ToSonnetAggregateFunction(Aggregate aggregate)
        {
            return aggregate switch
            {
                Aggregate.Mean => "avg",
                Aggregate.Median => "median",
                Aggregate.Last => "last",
                Aggregate.First => "first",
                Aggregate.Max => "max",
                Aggregate.Min => "min",
                Aggregate.Sum => "sum",
                _ => null
            };
        }

        private static void AppendWhereOrAnd(StringBuilder sql)
        {
            sql.Append(sql.ToString().Contains(" WHERE ", StringComparison.Ordinal) ? " AND " : " WHERE ");
        }

        private int InsertTelemetry(SndbConnection connection, string measurement, DateTime timestamp, IReadOnlyList<FieldWrite> writes)
        {
            using var command = connection.CreateCommand();
            var columns = new List<string> { "time" };
            var values = new List<string> { "@time" };

            command.Parameters.AddWithValue("@time", ToUnixMilliseconds(timestamp));

            for (var i = 0; i < writes.Count; i++)
            {
                var parameterName = $"@value{i}";
                columns.Add(QuoteIdentifier(writes[i].Field.Name));
                values.Add(parameterName);
                command.Parameters.AddWithValue(parameterName, writes[i].Value);
            }

            command.CommandText = $"INSERT INTO {QuoteIdentifier(measurement)} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
            return command.ExecuteNonQuery();
        }

        private static int InsertTelemetryBatch(SndbConnection connection, string measurement, IReadOnlyList<ColumnInfo> fields, IReadOnlyList<BatchWriteRow> rows)
        {
            if (rows.Count == 0)
            {
                return 0;
            }

            using var command = connection.CreateCommand();
            var columns = new List<string> { "time" };
            columns.AddRange(fields.Select(static x => QuoteIdentifier(x.Name)));

            var valuesSql = new List<string>(rows.Count);
            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var rowValues = new List<string> { $"@time{rowIndex}" };
                command.Parameters.AddWithValue($"@time{rowIndex}", ToUnixMilliseconds(row.Timestamp));

                for (var fieldIndex = 0; fieldIndex < fields.Count; fieldIndex++)
                {
                    var parameterName = $"@value{rowIndex}_{fieldIndex}";
                    rowValues.Add(parameterName);
                    command.Parameters.AddWithValue(parameterName, row.Values[fieldIndex]);
                }

                valuesSql.Add($"({string.Join(", ", rowValues)})");
            }

            command.CommandText = $"INSERT INTO {QuoteIdentifier(measurement)} ({string.Join(", ", columns)}) VALUES {string.Join(", ", valuesSql)}";
            return command.ExecuteNonQuery();
        }

        private List<ColumnInfo> BuildFieldsFromTelemetry(string measurement, IReadOnlyList<TelemetryData> telemetries)
        {
            var fields = new List<ColumnInfo>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var telemetry in telemetries)
            {
                if (string.IsNullOrWhiteSpace(telemetry.KeyName))
                {
                    continue;
                }

                if (IsReservedColumnName(telemetry.KeyName))
                {
                    _logger.LogWarning("SonnetDB measurement {Measurement} skipped telemetry key {KeyName} because it conflicts with a reserved column.", measurement, telemetry.KeyName);
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
            sql.Append($"CREATE MEASUREMENT {QuoteIdentifier(measurement)} (");
            for (var i = 0; i < fields.Count; i++)
            {
                if (i > 0)
                {
                    sql.Append(", ");
                }

                var field = fields[i];
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

            var measurementPrefix = GetOption(raw, "MeasurementPrefix")
                                    ?? GetOption(raw, "Measurement")
                                    ?? GetOption(raw, "MeasurementName")
                                    ?? DefaultMeasurement;

            if (string.IsNullOrWhiteSpace(measurementPrefix))
            {
                measurementPrefix = DefaultMeasurement;
            }

            return new SonnetDBStorageOptions(
                sonnetConnection.ConnectionString,
                sonnetConnection.DataSource,
                sonnetConnection.ResolveMode(),
                measurementPrefix.Trim(),
                GetBoolOption(raw, "AutoCreate", true));
        }

        private void EnsureEmbeddedOperation(string operation)
        {
            if (_providerMode != SndbProviderMode.Embedded)
            {
                throw new NotSupportedException($"SonnetDB telemetry {operation} is only available for embedded Data Source paths. Use the SonnetDB server maintenance API for remote deployments.");
            }
        }

        private string GetEmbeddedRootDirectory()
        {
            if (string.IsNullOrWhiteSpace(_dataSource))
            {
                throw new InvalidOperationException("SonnetDB embedded operation requires a Data Source directory.");
            }

            return Path.GetFullPath(NormalizeEmbeddedDataSource(_dataSource));
        }

        private static string NormalizeEmbeddedDataSource(string dataSource)
        {
            const string prefix = "sonnetdb://";
            return dataSource.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? dataSource[prefix.Length..]
                : dataSource;
        }

        private static SonnetDbTelemetryBackupManifest CreateDirectoryBackup(string sourceDirectory, string destinationDirectory, bool overwrite)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);
            var source = NormalizeFullDirectoryPath(sourceDirectory);
            var destination = NormalizeFullDirectoryPath(destinationDirectory);
            EnsureDirectoryIsOutsideSource(source, destination);
            PrepareEmptyDirectory(destination, overwrite, "backup");

            var entries = new List<SonnetDbTelemetryBackupFileEntry>();
            foreach (var sourcePath in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(sourcePath);
                if (fileName.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase)
                    || fileName.EndsWith(".temp", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(fileName, SonnetDbTelemetryBackupManifest.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var relativePath = NormalizeRelativePath(Path.GetRelativePath(source, sourcePath));
                var targetPath = ResolveManifestPath(destination, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                CopyFile(sourcePath, targetPath);
                var info = new FileInfo(targetPath);
                entries.Add(new SonnetDbTelemetryBackupFileEntry(relativePath, info.Length, ComputeSha256(targetPath)));
            }

            var manifest = new SonnetDbTelemetryBackupManifest(
                SonnetDbTelemetryBackupManifest.CurrentFormatVersion,
                DateTimeOffset.UtcNow,
                source,
                entries.OrderBy(static x => x.Path, StringComparer.Ordinal).ToArray());
            WriteManifest(destination, manifest);
            return manifest;
        }

        private static SonnetDbTelemetryBackupVerificationReport VerifyDirectoryBackup(string backupDirectory)
        {
            var errors = new List<string>();
            SonnetDbTelemetryBackupManifest manifest;
            try
            {
                manifest = ReadManifest(backupDirectory);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
            {
                return new SonnetDbTelemetryBackupVerificationReport(false, 0, [ex.Message]);
            }

            if (manifest.FormatVersion != SonnetDbTelemetryBackupManifest.CurrentFormatVersion)
            {
                errors.Add($"Unsupported manifest format version {manifest.FormatVersion}.");
            }

            var checkedFiles = 0;
            foreach (var entry in manifest.Files)
            {
                string path;
                try
                {
                    path = ResolveManifestPath(backupDirectory, entry.Path);
                }
                catch (InvalidDataException ex)
                {
                    errors.Add(ex.Message);
                    continue;
                }

                if (!File.Exists(path))
                {
                    errors.Add($"Missing file: {entry.Path}");
                    continue;
                }

                checkedFiles++;
                var info = new FileInfo(path);
                if (info.Length != entry.SizeBytes)
                {
                    errors.Add($"Size mismatch: {entry.Path}");
                }

                if (!string.Equals(ComputeSha256(path), entry.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"SHA-256 mismatch: {entry.Path}");
                }
            }

            return new SonnetDbTelemetryBackupVerificationReport(errors.Count == 0, checkedFiles, errors);
        }

        private static SonnetDbTelemetryRestoreDryRunReport RestoreDryRunDirectoryBackup(string backupDirectory, string targetDirectory, bool overwrite, bool verifyBeforeRestore)
        {
            var errors = new List<string>();
            SonnetDbTelemetryBackupManifest? manifest = null;
            if (verifyBeforeRestore)
            {
                var verification = VerifyDirectoryBackup(backupDirectory);
                errors.AddRange(verification.Errors);
            }

            try
            {
                manifest = ReadManifest(backupDirectory);
                foreach (var entry in manifest.Files)
                {
                    _ = ResolveManifestPath(backupDirectory, entry.Path);
                    _ = ResolveManifestPath(targetDirectory, entry.Path);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
            {
                errors.Add(ex.Message);
            }

            var target = EvaluateTargetDirectory(targetDirectory, overwrite);
            if (!target.IsAllowed)
            {
                errors.Add($"Restore target directory '{Path.GetFullPath(targetDirectory)}' exists and overwrite is not allowed.");
            }

            return new SonnetDbTelemetryRestoreDryRunReport(
                errors.Count == 0,
                manifest?.Files.Count ?? 0,
                manifest?.Files.Sum(static x => x.SizeBytes) ?? 0,
                0,
                target.Exists,
                target.Empty,
                errors);
        }

        private static SonnetDbTelemetryBackupManifest RestoreDirectoryBackup(string backupDirectory, string targetDirectory, bool overwrite, bool verifyBeforeRestore)
        {
            var dryRun = RestoreDryRunDirectoryBackup(backupDirectory, targetDirectory, overwrite, verifyBeforeRestore);
            if (!dryRun.IsValid)
            {
                throw new InvalidDataException("SonnetDB telemetry restore dry-run failed: " + string.Join("; ", dryRun.Errors));
            }

            var manifest = ReadManifest(backupDirectory);
            PrepareEmptyDirectory(targetDirectory, overwrite, "restore target");
            foreach (var entry in manifest.Files)
            {
                var sourcePath = ResolveManifestPath(backupDirectory, entry.Path);
                var targetPath = ResolveManifestPath(targetDirectory, entry.Path);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.Copy(sourcePath, targetPath, overwrite: true);
            }

            var restoredManifest = manifest with { SourceRoot = NormalizeFullDirectoryPath(targetDirectory) };
            WriteManifest(targetDirectory, restoredManifest);
            return restoredManifest;
        }

        private static SonnetDbTelemetryBackupManifest ReadManifest(string backupDirectory)
        {
            var path = Path.Combine(backupDirectory, SonnetDbTelemetryBackupManifest.FileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("SonnetDB telemetry backup manifest does not exist.", path);
            }

            using var stream = File.OpenRead(path);
            return JsonSerializer.Deserialize(stream, SonnetDbTelemetryBackupJsonContext.Default.SonnetDbTelemetryBackupManifest)
                   ?? throw new InvalidDataException("SonnetDB telemetry backup manifest is invalid.");
        }

        private static void WriteManifest(string directory, SonnetDbTelemetryBackupManifest manifest)
        {
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, SonnetDbTelemetryBackupManifest.FileName);
            var tempPath = path + ".tmp";
            using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, manifest, SonnetDbTelemetryBackupJsonContext.Default.SonnetDbTelemetryBackupManifest);
                stream.Flush(flushToDisk: true);
            }

            File.Move(tempPath, path, overwrite: true);
        }

        private static SonnetDbTelemetryBackupReport ToBackupReport(SonnetDbTelemetryBackupManifest manifest)
        {
            return new SonnetDbTelemetryBackupReport(
                manifest.SourceRoot,
                manifest.CreatedUtc,
                manifest.Files.Count,
                manifest.Files.Sum(static x => x.SizeBytes),
                CountMeasurements(manifest.SourceRoot),
                0,
                manifest.Files.Count(static x => x.Path.StartsWith("segments/", StringComparison.OrdinalIgnoreCase)));
        }

        private static int CountMeasurements(string rootDirectory)
        {
            var path = Path.Combine(rootDirectory, "measurements.tslschema");
            if (!File.Exists(path))
            {
                return 0;
            }

            Span<byte> header = stackalloc byte[20];
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return stream.Read(header) == header.Length
                && header[..8].SequenceEqual("SDBMEAv1"u8)
                ? BinaryPrimitives.ReadInt32LittleEndian(header[16..20])
                : 0;
        }

        private static void PrepareEmptyDirectory(string directory, bool overwrite, string purpose)
        {
            if (Directory.Exists(directory))
            {
                var empty = !Directory.EnumerateFileSystemEntries(directory).Any();
                if (!overwrite || !empty)
                {
                    throw new IOException($"SonnetDB telemetry {purpose} directory '{Path.GetFullPath(directory)}' exists and cannot be reused.");
                }

                return;
            }

            Directory.CreateDirectory(directory);
        }

        private static RestoreTargetEvaluation EvaluateTargetDirectory(string targetDirectory, bool overwrite)
        {
            var exists = Directory.Exists(targetDirectory);
            var empty = !exists || !Directory.EnumerateFileSystemEntries(targetDirectory).Any();
            return new RestoreTargetEvaluation(exists, empty, !exists || overwrite && empty);
        }

        private static void EnsureDirectoryIsOutsideSource(string sourceDirectory, string destinationDirectory)
        {
            if (string.Equals(sourceDirectory, destinationDirectory, StringComparison.OrdinalIgnoreCase)
                || destinationDirectory.StartsWith(sourceDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                throw new IOException("SonnetDB telemetry backup destination cannot be inside the source database directory.");
            }
        }

        private static string ResolveManifestPath(string rootDirectory, string relativePath)
        {
            var normalized = NormalizeRelativePath(relativePath);
            if (Path.IsPathRooted(normalized) || normalized.Split('/').Any(static x => x == ".."))
            {
                throw new InvalidDataException($"Backup manifest contains an unsafe path: {relativePath}");
            }

            var root = NormalizeFullDirectoryPath(rootDirectory);
            var path = Path.GetFullPath(Path.Combine(root, normalized));
            if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(path, root, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"Backup manifest path escapes target directory: {relativePath}");
            }

            return path;
        }

        private static string NormalizeFullDirectoryPath(string path)
        {
            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static string NormalizeRelativePath(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
        }

        private static void CopyFile(string source, string target)
        {
            using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var output = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None);
            input.CopyTo(output);
            output.Flush(flushToDisk: true);
        }

        private static string ComputeSha256(string path)
        {
            using var stream = File.OpenRead(path);
            return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
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
            }

            public IReadOnlyDictionary<string, ColumnInfo> Columns { get; }

            public IReadOnlyDictionary<string, ColumnInfo> Fields { get; }
        }

        private sealed record SonnetDBStorageOptions(
            string ConnectionString,
            string DataSource,
            SndbProviderMode ProviderMode,
            string MeasurementPrefix,
            bool AutoCreate);

        private sealed record BatchWriteKey(string Measurement, string FieldSignature);

        private sealed class BatchWriteGroup
        {
            public BatchWriteGroup(string measurement, IReadOnlyList<ColumnInfo> fields)
            {
                Measurement = measurement;
                Fields = fields;
            }

            public string Measurement { get; }

            public IReadOnlyList<ColumnInfo> Fields { get; }

            public List<BatchWriteRow> Rows { get; } = [];

            public bool SchemaChanged { get; set; }
        }

        private sealed record BatchWriteRow(DateTime Timestamp, IReadOnlyList<object> Values);

        private readonly record struct RestoreTargetEvaluation(bool Exists, bool Empty, bool IsAllowed);
    }

    public sealed record SonnetDbTelemetryBackupManifest(
        int FormatVersion,
        DateTimeOffset CreatedUtc,
        string SourceRoot,
        IReadOnlyList<SonnetDbTelemetryBackupFileEntry> Files)
    {
        public const int CurrentFormatVersion = 1;
        public const string FileName = "iotsharp-sonnetdb-telemetry.backup.json";
    }

    public sealed record SonnetDbTelemetryBackupFileEntry(
        string Path,
        long SizeBytes,
        string Sha256);

    public sealed record SonnetDbTelemetryBackupReport(
        string SourceRoot,
        DateTimeOffset CreatedUtc,
        int FileCount,
        long TotalBytes,
        int MeasurementCount,
        long CheckpointLsn,
        int SegmentCount);

    public sealed record SonnetDbTelemetryBackupVerificationReport(
        bool IsValid,
        int CheckedFiles,
        IReadOnlyList<string> Errors);

    public sealed record SonnetDbTelemetryRestoreDryRunReport(
        bool IsValid,
        int FileCount,
        long TotalBytes,
        int IndexCount,
        bool TargetDirectoryExists,
        bool TargetDirectoryEmpty,
        IReadOnlyList<string> Errors);

    public sealed record SonnetDbTelemetryReplayReport(
        string DataSource,
        string Measurement,
        int PointCount,
        IReadOnlyList<string> Fields);

    public sealed record SonnetDbTelemetryBatchWriteReport(
        int MessageCount,
        int BatchCount,
        int WrittenRows,
        int TelemetryValueCount,
        int SkippedTelemetryValueCount,
        bool IsComplete);

    [JsonSerializable(typeof(SonnetDbTelemetryBackupManifest))]
    internal sealed partial class SonnetDbTelemetryBackupJsonContext : JsonSerializerContext;
}
