using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using IoTSharp.Data;
using Microsoft.Extensions.Logging;

namespace IoTSharp.EventBus.SonnetMQ;

/// <summary>在启动后的有界时间内，将 SonnetMQ 分段耗时汇总到应用日志。</summary>
internal static class SonnetMqProcessingLog
{
    private const int WindowSeconds = 30;
    private const int MaxSeconds = 1800;

    internal static async Task RunAsync(ILogger logger, int seconds, CancellationToken stoppingToken)
    {
        if (seconds <= 0 || !logger.IsEnabled(LogLevel.Information))
        {
            return;
        }

        var stages = new ConcurrentDictionary<string, StageWindow>(StringComparer.Ordinal);
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, source) =>
        {
            if (instrument.Meter.Name == SonnetMqProcessingMetrics.MeterName && instrument is Histogram<double>)
            {
                source.EnableMeasurementEvents(instrument, stages.GetOrAdd(instrument.Name, static _ => new StageWindow()));
            }
        };
        listener.SetMeasurementEventCallback<double>(static (_, milliseconds, _, state) =>
        {
            ((StageWindow)state!).Record(milliseconds);
        });
        listener.Start();
        using var efDiagnostics = new SonnetMqAttributeEfDiagnostics();

        var duration = Math.Min(seconds, MaxSeconds);
        try
        {
            for (var elapsed = 0; elapsed < duration; elapsed += WindowSeconds)
            {
                var window = Math.Min(WindowSeconds, duration - elapsed);
                await Task.Delay(TimeSpan.FromSeconds(window), stoppingToken);
                foreach (var (stage, values) in stages)
                {
                    if (values.TakeSnapshot() is { } sample)
                    {
                        logger.LogInformation(
                            "SonnetMQ processing stage {Stage} over {WindowSeconds}s. Samples={Samples} TotalMs={TotalMs:F2} MeanMs={MeanMs:F2} P50UpperMs={P50UpperMs:F2} P95UpperMs={P95UpperMs:F2} MaxMs={MaxMs:F2}",
                            stage, window, sample.Count, sample.Total, sample.Total / sample.Count, sample.P50Upper, sample.P95Upper, sample.Max);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private sealed class StageWindow
    {
        private static readonly double[] Bounds = [0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000];
        private readonly Lock _lock = new();
        private readonly long[] _bins = new long[Bounds.Length + 1];
        private long _count;
        private double _total;
        private double _max;

        internal void Record(double milliseconds)
        {
            var position = Array.BinarySearch(Bounds, milliseconds);
            lock (_lock)
            {
                _bins[position >= 0 ? position : ~position]++;
                _count++;
                _total += milliseconds;
                _max = Math.Max(_max, milliseconds);
            }
        }

        internal StageSnapshot? TakeSnapshot()
        {
            lock (_lock)
            {
                if (_count == 0)
                {
                    return null;
                }

                var snapshot = new StageSnapshot(_count, _total, BucketUpper(0.5), BucketUpper(0.95), _max);
                _count = 0;
                _total = 0;
                _max = 0;
                Array.Clear(_bins);
                return snapshot;
            }
        }

        private double BucketUpper(double quantile)
        {
            var rank = (long)Math.Ceiling(_count * quantile);
            for (var index = 0; index < Bounds.Length; index++)
            {
                rank -= _bins[index];
                if (rank <= 0)
                {
                    return Math.Min(Bounds[index], _max);
                }
            }

            return _max;
        }
    }

    private readonly record struct StageSnapshot(long Count, double Total, double P50Upper, double P95Upper, double Max);
}
