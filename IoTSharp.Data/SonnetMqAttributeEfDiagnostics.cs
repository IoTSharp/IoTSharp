using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IoTSharp.Data;

/// <summary>在诊断窗口内使用 EF 的耗时事件归集属性写命令和事务提交。</summary>
internal sealed class SonnetMqAttributeEfDiagnostics : IDisposable, IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>>
{
    private readonly object _sync = new();
    private readonly List<IDisposable> _subscriptions = new();
    private readonly IDisposable _allListeners;
    private bool _disposed;

    internal SonnetMqAttributeEfDiagnostics() => _allListeners = DiagnosticListener.AllListeners.Subscribe(this);

    void IObserver<DiagnosticListener>.OnNext(DiagnosticListener listener)
    {
        if (listener.Name != "Microsoft.EntityFrameworkCore")
        {
            return;
        }

        lock (_sync)
        {
            if (!_disposed)
            {
                // EF 会在 CommandExecuting 无监听时短暂停发命令诊断。用固定订阅保持命令完成事件可用。
                _subscriptions.Add(listener.Subscribe(this, static (name, _, _) =>
                    name == RelationalEventId.CommandExecuting.Name
                    || name == RelationalEventId.CommandExecuted.Name
                    || name == RelationalEventId.TransactionCommitted.Name));
            }
        }
    }

    void IObserver<KeyValuePair<string, object>>.OnNext(KeyValuePair<string, object> entry)
    {
        if (!SonnetMqProcessingMetrics.IsSavingAttribute)
        {
            return;
        }

        if (entry.Key == RelationalEventId.CommandExecuted.Name && entry.Value is CommandExecutedEventData command)
        {
            SonnetMqProcessingMetrics.AttributeSaveCommand.Record(command.Duration.TotalMilliseconds);
        }
        else if (entry.Key == RelationalEventId.TransactionCommitted.Name && entry.Value is TransactionEndEventData commit)
        {
            SonnetMqProcessingMetrics.AttributeTransactionCommit.Record(commit.Duration.TotalMilliseconds);
        }
    }

    public void OnCompleted() { }
    public void OnError(Exception error) { }

    public void Dispose()
    {
        _allListeners.Dispose();
        lock (_sync)
        {
            _disposed = true;
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
        }
    }
}
