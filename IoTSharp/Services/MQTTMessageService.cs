using IoTSharp.Handlers;
using IoTSharp.MQTT;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Services
{
    public class MQTTMessageService : IHostedService
    {
        private ILogger _logger;
        private MQTTServerHandler _handler;

        public MQTTMessageService(ILogger<MQTTMessageService> logger, MQTTServerHandler handler)
        {
            _logger = logger;
            _handler = handler;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Thread.CurrentThread.Name = "ProcessIncomingMqttMessages";

                while (!cancellationToken.IsCancellationRequested)
                {
                    MqttApplicationMessageReceivedEventArgs message = null;
                    try
                    {
                        message = _handler.IncomingMessages.Take(cancellationToken);
                        if (message == null || cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        var affectedSubscribers = new List<MqttSubscriber>();
                        lock (_handler.Subscribers)
                        {
                            foreach (var subscriber in _handler.Subscribers.Values)
                            {
                                if (subscriber.IsFilterMatch(message.ApplicationMessage.Topic))
                                {
                                    affectedSubscribers.Add(subscriber);
                                }
                            }
                        }

                        foreach (var subscriber in affectedSubscribers)
                        {
                            TryNotifySubscriber(subscriber, message);
                        }

                        _handler.OutboundCounter.Increment();
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception exception)
                    {
                        _logger.Log(LogLevel.Error, exception, $"Error while processing MQTT message with topic '{message?.ApplicationMessage?.Topic}'.");
                    }
                }
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void TryNotifySubscriber(MqttSubscriber subscriber, MqttApplicationMessageReceivedEventArgs message)
        {
            try
            {
                subscriber.Notify(message);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, $"Error while notifying subscriber '{subscriber.Uid}'.");
            }
        }
    }
}