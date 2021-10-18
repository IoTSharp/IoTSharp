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
        Task<bool> StoreTelemetryAsync(RawMsg msg);
        Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId);
        Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys);
        Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keyName, DateTime begin);
        Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keyName, DateTime begin, DateTime end);
        Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, DateTime begin);
        Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, DateTime begin, DateTime end);

    }
}
