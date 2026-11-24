using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;

namespace IoTSharp.Data;

/// <summary>定义 SonnetMQ 处理链路的固定耗时指标。</summary>
internal static class SonnetMqProcessingMetrics
{
    internal const string MeterName = "IoTSharp.SonnetMQ.Processing";
    private static readonly Meter Meter = new(MeterName);
    private static readonly AsyncLocal<bool> InEventStorage = new();
    private static readonly AsyncLocal<bool> InAttributeSaveChanges = new();

    internal static readonly Histogram<double> TelemetryPull = Create("telemetry.pull");
    internal static readonly Histogram<double> TelemetryBatchDispatch = Create("telemetry.batch.dispatch");
    internal static readonly Histogram<double> TelemetryBatchDecode = Create("telemetry.batch.decode");
    internal static readonly Histogram<double> TelemetrySingleDispatch = Create("telemetry.single.dispatch");
    internal static readonly Histogram<double> TelemetryAck = Create("telemetry.ack");
    internal static readonly Histogram<double> TelemetryStore = Create("telemetry.store");
    internal static readonly Histogram<double> TelemetrySchema = Create("telemetry.schema");
    internal static readonly Histogram<double> TelemetrySql = Create("telemetry.sql");
    internal static readonly Histogram<double> TelemetryRuleCheck = Create("telemetry.rule.check");
    internal static readonly Histogram<double> TelemetryRules = Create("telemetry.rules");
    internal static readonly Histogram<double> AttributePull = Create("attribute.pull");
    internal static readonly Histogram<double> AttributeDispatch = Create("attribute.dispatch");
    internal static readonly Histogram<double> AttributeAck = Create("attribute.ack");
    internal static readonly Histogram<double> AttributeDeviceLookup = Create("attribute.device.lookup");
    internal static readonly Histogram<double> AttributePrepare = Create("attribute.prepare");
    internal static readonly Histogram<double> AttributePreRead = Create("attribute.pre_read");
    internal static readonly Histogram<double> AttributeSaveChanges = Create("attribute.save_changes");
    internal static readonly Histogram<double> AttributeSaveCommand = Create("attribute.save.command");
    internal static readonly Histogram<double> AttributeTransactionCommit = Create("attribute.transaction.commit");
    internal static readonly Histogram<double> AttributeRules = Create("attribute.rules");

    private static Histogram<double> Create(string name) => Meter.CreateHistogram<double>(name, "ms");

    /// <summary>计时至作用域退出，包括失败的调用。</summary>
    internal static Duration Measure(Histogram<double> stage) => stage?.Enabled == true ? new Duration(stage) : default;

    /// <summary>在异步调用中标记事件存储入口，区分共享存储接口的后台和 API 调用。</summary>
    internal static StorageScope BeginStorage(Histogram<double> stage) => stage?.Enabled == true ? new StorageScope(InEventStorage) : default;

    /// <summary>仅记录当前事件存储调用中的嵌套阶段。</summary>
    internal static Duration MeasureStorage(Histogram<double> stage) => stage.Enabled && InEventStorage.Value ? new Duration(stage) : default;

    /// <summary>标记当前属性 SaveChanges 中发生的 SQL 命令和提交。</summary>
    internal static StorageScope BeginAttributeSaveChanges() => (AttributeSaveCommand.Enabled || AttributeTransactionCommit.Enabled) && InEventStorage.Value
        ? new StorageScope(InAttributeSaveChanges)
        : default;

    internal static bool IsSavingAttribute => InAttributeSaveChanges.Value;

    internal readonly struct Duration : IDisposable
    {
        private readonly Histogram<double> _stage;
        private readonly long _started;

        internal Duration(Histogram<double> stage)
        {
            _stage = stage;
            _started = Stopwatch.GetTimestamp();
        }

        public void Dispose() => _stage?.Record(Stopwatch.GetElapsedTime(_started).TotalMilliseconds);
    }

    internal readonly struct StorageScope : IDisposable
    {
        private readonly AsyncLocal<bool> _scope;
        private readonly bool _previous;

        internal StorageScope(AsyncLocal<bool> scope)
        {
            _scope = scope;
            _previous = scope.Value;
            scope.Value = true;
        }

        public void Dispose()
        {
            if (_scope is not null)
            {
                _scope.Value = _previous;
            }
        }
    }
}
