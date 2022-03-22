using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Storage
{
    public interface IStorage
    {
        Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(RawMsg msg);
        Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId);
        Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys);

        Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate);

    }
}
