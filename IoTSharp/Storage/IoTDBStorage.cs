using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Salvini.IoTDB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IoTSharp.Storage
{
    public class IoTDBStorage : IStorage
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly Session _session;

        public IoTDBStorage(ILogger<IoTDBStorage> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options, Salvini.IoTDB.Session  session
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            _session = session;
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<List<TelemetryDataDto>> GetTelemetryLatest(Guid deviceId, string keys)
        {
            throw new NotImplementedException();
        }

        public Task<List<TelemetryDataDto>> LoadTelemetryAsync(Guid deviceId, string keys, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            throw new NotImplementedException();
        }

        public Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(PlayloadData msg)
        {
            //   _session.InsertRecordAsync(msg.DeviceId,new Salvini.IoTDB.Data.RowRecord (msg.ts,msg.MsgBody))
            throw new NotImplementedException();
        }
    }
}
