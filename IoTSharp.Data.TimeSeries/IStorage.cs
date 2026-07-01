using IoTSharp.Contracts;
using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Storage
{
    public interface IStorage
    {
        Task<bool> CheckTelemetryStorage();
        Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(PlayloadData msg);
        async Task<TelemetryBatchStoreResult> StoreTelemetryBatchAsync(IReadOnlyCollection<PlayloadData> messages)
        {
            var telemetries = new List<TelemetryData>();
            var ok = true;
            foreach (var message in messages)
            {
                var result = await StoreTelemetryAsync(message);
                ok &= result.result;
                telemetries.AddRange(result.telemetries);
            }

            return new TelemetryBatchStoreResult(ok, telemetries, messages.Count);
        }

        Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId);
        Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys);

        Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate);

    }

    public sealed record TelemetryBatchStoreResult(bool Result, List<TelemetryData> Telemetries, int MessageCount);
}
