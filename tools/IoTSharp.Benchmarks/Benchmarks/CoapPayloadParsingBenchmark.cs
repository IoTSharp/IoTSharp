using BenchmarkDotNet.Attributes;
using IoTSharp.Services.Coap;
using System;
using System.Text;

namespace IoTSharp.Benchmarks.Benchmarks
{
    /// <summary>
    /// CoAP payload 解析基准，覆盖小遥测和 Blockwise 场景下的大 JSON payload。
    /// </summary>
    [MemoryDiagnoser]
    [BenchmarkCategory("Coap")]
    public class CoapPayloadParsingBenchmark
    {
        private byte[] _telemetryPayload;
        private byte[] _alarmPayload;

        [Params(128, 4096, 32768)]
        public int PayloadBytes { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _telemetryPayload = CreateTelemetryPayload(PayloadBytes);
            _alarmPayload = Encoding.UTF8.GetBytes(
                "{\"AlarmType\":\"HighTemperature\",\"AlarmDetail\":\"Temperature is high\",\"Serverity\":\"Major\"}");
        }

        [Benchmark(Baseline = true, Description = "telemetry UTF-8 object parse")]
        public int ParseTelemetryObject()
        {
            var payload = CoapPayloadParser.ParseObject(_telemetryPayload);
            return payload.Count;
        }

        [Benchmark(Description = "alarm source-generated DTO parse")]
        public string ParseAlarmWithSourceGeneration()
        {
            var alarm = CoapPayloadParser.ParseAlarm(_alarmPayload);
            return alarm.AlarmType;
        }

        private static byte[] CreateTelemetryPayload(int targetBytes)
        {
            var builder = new StringBuilder(targetBytes + 64);
            builder.Append("{\"temperature\":23.5,\"humidity\":61,\"sample\":\"");
            while (builder.Length < targetBytes - 2)
            {
                builder.Append("abcdef0123456789");
            }

            builder.Append("\"}");
            return Encoding.UTF8.GetBytes(builder.ToString());
        }
    }
}
