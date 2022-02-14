using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Extensions
{
    /// <summary>
    /// https://github.com/chkr1011/MQTTnet/blob/master/Source/MQTTnet.Extensions.Rpc/MqttRpcClient.cs
    /// </summary>
    public class  RpcClient : IDisposable
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _waitingCalls = new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();
        private readonly MqttClient _mqttClient;
        private readonly ILogger _logger;
        private MqttClientOptions _mqtt;
        private bool disposedValue;

        public RpcClient(MqttClient mqttClient,  ILogger logger)
        {
            _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));
            _logger = logger;
            _mqttClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceived;
        }

      

        public RpcClient(MqttClientOptions mqtt, Microsoft.Extensions.Logging.ILogger _logger) :this (new MQTTnet.MqttFactory().CreateMqttClient(), _logger)
        {
            _mqtt = mqtt;
          
        }
 
       
        public Task<byte[]> ExecuteAsync(TimeSpan timeout, string deviceid, string methodName, string payload, MqttQualityOfServiceLevel qualityOfServiceLevel)
        {
            return ExecuteAsync(timeout, deviceid, methodName,  Encoding.UTF8.GetBytes(payload), qualityOfServiceLevel, CancellationToken.None);
        }

        public Task<byte[]> ExecuteAsync(TimeSpan timeout, string deviceid, string methodName, string payload, MqttQualityOfServiceLevel qualityOfServiceLevel, CancellationToken cancellationToken)
        {
            return ExecuteAsync(timeout,  deviceid, methodName, Encoding.UTF8.GetBytes(payload), qualityOfServiceLevel, cancellationToken);
        }

        public Task<byte[]> ExecuteAsync(TimeSpan timeout, string deviceid, string methodName, byte[] payload, MqttQualityOfServiceLevel qualityOfServiceLevel)
        {
            return ExecuteAsync(timeout, deviceid, methodName, payload, qualityOfServiceLevel, CancellationToken.None);
        }

        public async Task<byte[]> ExecuteAsync(TimeSpan timeout, string deviceid, string methodName, byte[] payload, MqttQualityOfServiceLevel qualityOfServiceLevel, CancellationToken cancellationToken)
        {
            if (methodName == null) throw new ArgumentNullException(nameof(methodName));

            if (methodName.Contains("/") || methodName.Contains("+") || methodName.Contains("#"))
            {
                throw new ArgumentException("The method name cannot contain /, + or #.");
            }
            string rpcid = $"{Guid.NewGuid():N}";
            var requestTopic = $"/devices/{deviceid}/rpc/request/{methodName}/{rpcid}";
            var responseTopic = $"/devices/{deviceid}/rpc/response/{methodName}/{rpcid}";

            var requestMessage = new MqttApplicationMessageBuilder()
                .WithTopic(requestTopic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(qualityOfServiceLevel)
                .Build();

            try
            {
                var tcs = new TaskCompletionSource<byte[]>();
                if (!_waitingCalls.TryAdd(responseTopic, tcs))
                {
                    throw new InvalidOperationException();
                }

                await _mqttClient.SubscribeAsync(responseTopic, qualityOfServiceLevel).ConfigureAwait(false);
                await _mqttClient.PublishAsync(requestMessage).ConfigureAwait(false);

                using (var timeoutCts = new CancellationTokenSource(timeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token))
                {
                    linkedCts.Token.Register(() =>
                    {
                        if (!tcs.Task.IsCompleted && !tcs.Task.IsFaulted && !tcs.Task.IsCanceled)
                        {
                            tcs.TrySetCanceled();
                        }
                    });

                    try
                    {
                        var result = await tcs.Task.ConfigureAwait(false);
                        timeoutCts.Cancel(false);
                        return result;
                    }
                    catch (OperationCanceledException exception)
                    {
                        if (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                        {
                            throw new MqttCommunicationTimedOutException(exception);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            finally
            {
                _waitingCalls.TryRemove(responseTopic, out _);
                await _mqttClient.UnsubscribeAsync(responseTopic).ConfigureAwait(false);
            }
        }

        private Task OnApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            if (_waitingCalls.TryRemove(eventArgs.ApplicationMessage.Topic, out var tcs))
            {
                if (!tcs.Task.IsCompleted && !tcs.Task.IsCanceled)
                {
                    tcs.TrySetResult(eventArgs.ApplicationMessage.Payload);
                }
            }
            return Task.CompletedTask;
        }

       

        public async Task ConnectAsync()
        {
            await _mqttClient.ConnectAsync(_mqtt);
        }

        public async Task DisconnectAsync()
        {
            await _mqttClient.DisconnectAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var tcs in _waitingCalls)
                    {
                        tcs.Value.SetCanceled();
                    }

                    _waitingCalls.Clear();
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

      

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
