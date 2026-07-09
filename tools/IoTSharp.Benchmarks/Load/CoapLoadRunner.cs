using CoAP;
using CoAP.Net;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Benchmarks.Load
{
    /// <summary>
    /// 简单 CoAP 压测 runner，用于对运行中的 IoTSharp CoAP 入口发起真实 UDP POST。
    /// </summary>
    internal static class CoapLoadRunner
    {
        public static async Task<int> RunAsync(string[] args)
        {
            var options = CoapLoadOptions.Parse(args);
            if (options.ShowHelp)
            {
                Console.WriteLine(CoapLoadOptions.HelpText);
                return 0;
            }

            using var endpoint = new CoAPEndPoint(new CoapConfig
            {
                DefaultBlockSize = options.BlockSize,
                MaxMessageSize = options.BlockSize
            });
            endpoint.Start();

            var latencies = new long[options.Requests];
            var next = 0;
            var completed = 0;
            var succeeded = 0;
            var failed = 0;
            var startedAt = Stopwatch.GetTimestamp();
            var workers = new Task[options.Concurrency];

            for (var i = 0; i < workers.Length; i++)
            {
                workers[i] = Task.Run(() =>
                {
                    while (true)
                    {
                        var index = Interlocked.Increment(ref next) - 1;
                        if (index >= options.Requests)
                        {
                            return;
                        }

                        var elapsed = SendOne(endpoint, options);
                        latencies[index] = elapsed;
                        if (elapsed >= 0)
                        {
                            Interlocked.Increment(ref succeeded);
                        }
                        else
                        {
                            Interlocked.Increment(ref failed);
                        }

                        Interlocked.Increment(ref completed);
                    }
                });
            }

            await Task.WhenAll(workers);
            var totalSeconds = Stopwatch.GetElapsedTime(startedAt).TotalSeconds;
            Array.Sort(latencies);

            Console.WriteLine("CoAP load test completed.");
            Console.WriteLine("Uri: {0}", options.Uri);
            Console.WriteLine("Requests: {0}, Concurrency: {1}, Success: {2}, Failed: {3}", options.Requests, options.Concurrency, succeeded, failed);
            Console.WriteLine("Throughput: {0:N2} req/s", completed / totalSeconds);
            Console.WriteLine("Latency: p50={0:N2} ms, p90={1:N2} ms, p99={2:N2} ms, max={3:N2} ms",
                Percentile(latencies, 0.50),
                Percentile(latencies, 0.90),
                Percentile(latencies, 0.99),
                StopwatchTicksToMilliseconds(latencies[^1]));

            return failed == 0 ? 0 : 2;
        }

        private static long SendOne(IEndPoint endpoint, CoapLoadOptions options)
        {
            var startedAt = Stopwatch.GetTimestamp();
            try
            {
                var request = new Request(Method.POST);
                request.SetUri(options.Uri.ToString());
                request.SetPayload(options.Payload, options.ContentFormat);
                var query = "access_token=" + Uri.EscapeDataString(options.AccessToken);
                request.AddUriQuery(query);
                request.Send(endpoint);

                var response = request.WaitForResponse(options.TimeoutMilliseconds);
                if (response == null || response.StatusCode != options.ExpectedStatusCode)
                {
                    return -1;
                }

                return Stopwatch.GetTimestamp() - startedAt;
            }
            catch
            {
                return -1;
            }
        }

        private static double Percentile(long[] sortedLatencies, double percentile)
        {
            var firstSuccess = Array.FindIndex(sortedLatencies, value => value >= 0);
            if (firstSuccess < 0)
            {
                return double.NaN;
            }

            var count = sortedLatencies.Length - firstSuccess;
            var index = firstSuccess + Math.Clamp((int)Math.Ceiling(count * percentile) - 1, 0, count - 1);
            return StopwatchTicksToMilliseconds(sortedLatencies[index]);
        }

        private static double StopwatchTicksToMilliseconds(long ticks)
        {
            return ticks < 0 ? double.NaN : ticks * 1000d / Stopwatch.Frequency;
        }
    }

    internal sealed class CoapLoadOptions
    {
        public const string HelpText = """
IoTSharp CoAP load runner

Usage:
  dotnet run -c Release --project tools/IoTSharp.Benchmarks -- --coap-load --uri coap://127.0.0.1:5683/devices/device-001/telemetry --token <access-token>

Options:
  --requests <n>       Total requests. Default: 10000
  --concurrency <n>    Concurrent workers. Default: 32
  --payload <json>     JSON payload. Default: {"temperature":23.5}
  --timeout-ms <n>     Per-request timeout. Default: 5000
  --block-size <n>     CoAP max message/block size. Default: 1024
""";

        public Uri Uri { get; private set; }

        public string AccessToken { get; private set; }

        public int Requests { get; private set; } = 10000;

        public int Concurrency { get; private set; } = 32;

        public byte[] Payload { get; private set; } = Encoding.UTF8.GetBytes("{\"temperature\":23.5}");

        public int TimeoutMilliseconds { get; private set; } = 5000;

        public int BlockSize { get; private set; } = 1024;

        public int ContentFormat { get; private set; } = MediaType.ApplicationJson;

        public StatusCode ExpectedStatusCode { get; private set; } = StatusCode.Changed;

        public bool ShowHelp { get; private set; }

        public static CoapLoadOptions Parse(string[] args)
        {
            var options = new CoapLoadOptions();
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase))
                {
                    options.ShowHelp = true;
                    return options;
                }

                if (string.Equals(arg, "--coap-load", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = i + 1 < args.Length ? args[++i] : throw new ArgumentException("Missing value for " + arg);
                switch (arg)
                {
                    case "--uri":
                        options.Uri = new Uri(value);
                        break;
                    case "--token":
                        options.AccessToken = value;
                        break;
                    case "--requests":
                        options.Requests = ParsePositiveInt(value, arg);
                        break;
                    case "--concurrency":
                        options.Concurrency = ParsePositiveInt(value, arg);
                        break;
                    case "--payload":
                        options.Payload = Encoding.UTF8.GetBytes(value);
                        break;
                    case "--timeout-ms":
                        options.TimeoutMilliseconds = ParsePositiveInt(value, arg);
                        break;
                    case "--block-size":
                        options.BlockSize = ParsePositiveInt(value, arg);
                        break;
                    default:
                        throw new ArgumentException("Unknown argument: " + arg);
                }
            }

            if (options.Uri == null)
            {
                throw new ArgumentException("--uri is required.");
            }

            if (string.IsNullOrWhiteSpace(options.AccessToken))
            {
                throw new ArgumentException("--token is required.");
            }

            return options;
        }

        private static int ParsePositiveInt(string value, string name)
        {
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
                || result <= 0)
            {
                throw new ArgumentException(name + " must be a positive integer.");
            }

            return result;
        }
    }
}
