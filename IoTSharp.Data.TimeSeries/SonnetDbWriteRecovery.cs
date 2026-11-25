using SonnetDB.Data;

namespace IoTSharp.Storage;

/// <summary>在 SonnetMQ 消费范围内保留存储写入保护结果及已写入前缀。</summary>
internal static class SonnetDbWriteRecovery
{
    internal const string Required = "sql_write_recovery_required";
    internal const string Partial = "sql_write_recovery_required_partial";
    private static readonly AsyncLocal<bool> Enabled = new();

    internal static Scope Enable() => new(Enabled.Value);

    internal static bool ShouldPropagate(Exception exception)
        => Enabled.Value && Classify(exception) != Outcome.None;

    internal static bool WasRejected(Exception exception)
        => Enabled.Value && Classify(exception) == Outcome.Rejected;

    internal static SndbServerException AfterEarlierWrites(Exception exception)
        => new(Partial, "The event may already contain completed storage writes.",
            System.Net.HttpStatusCode.OK, exception);

    internal static Outcome Classify(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SndbServerException server)
            {
                if (server.Error == Partial)
                    return Outcome.Partial;
                if (server.Error == Required)
                    return Outcome.Rejected;
            }
        }
        return Outcome.None;
    }

    internal enum Outcome
    {
        None,
        Rejected,
        Partial,
    }

    internal readonly struct Scope : IDisposable
    {
        private readonly bool _previous;

        internal Scope(bool previous)
        {
            _previous = previous;
            Enabled.Value = true;
        }

        public void Dispose() => Enabled.Value = _previous;
    }
}
