using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Diagnostics;
using IoTSharp.Storage;
using IoTSharp.Sys;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Server;
using MQTTnet.Server.Status;

namespace IoTSharp.MQTT
{
    public class MqttService : IService
    {
        private readonly BlockingCollection<MqttApplicationMessageReceivedEventArgs> _incomingMessages = new BlockingCollection<MqttApplicationMessageReceivedEventArgs>();
        private readonly Dictionary<string, MqttTopicImporter> _importers = new Dictionary<string, MqttTopicImporter>();
        private readonly Dictionary<string, MqttSubscriber> _subscribers = new Dictionary<string, MqttSubscriber>();
        private readonly OperationsPerSecondCounter _inboundCounter;
        private readonly OperationsPerSecondCounter _outboundCounter;


        private readonly StorageService _storageService;
        private readonly SystemCancellationToken _systemCancellationToken;

        private readonly ILogger _logger;

        private IMqttServer _mqttServer;
        private bool _enableMqttLogging;

        public MqttService(
            SystemCancellationToken systemCancellationToken,
            DiagnosticsService diagnosticsService,
            StorageService storageService,
            SystemStatusService systemStatusService,
            ILogger<MqttService> logger)
        {
            _systemCancellationToken = systemCancellationToken ?? throw new ArgumentNullException(nameof(systemCancellationToken));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (diagnosticsService == null) throw new ArgumentNullException(nameof(diagnosticsService));
            _inboundCounter = diagnosticsService.CreateOperationsPerSecondCounter("mqtt.inbound_rate");
            _outboundCounter = diagnosticsService.CreateOperationsPerSecondCounter("mqtt.outbound_rate");

            systemStatusService.Set("mqtt.subscribers_count", () => _subscribers.Count);
            systemStatusService.Set("mqtt.incoming_messages_count", () => _incomingMessages.Count);
            systemStatusService.Set("mqtt.inbound_rate", () => _inboundCounter.Count);
            systemStatusService.Set("mqtt.outbound_rate", () => _outboundCounter.Count);
            systemStatusService.Set("mqtt.connected_clients_count", () => _mqttServer.GetClientStatusAsync().GetAwaiter().GetResult().Count);
        }

        public void Start()
        {
            _storageService.TryReadOrCreate(out MqttServiceOptions options, MqttServiceOptions.Filename);

            var mqttFactory = new MqttFactory();

            _enableMqttLogging = options.EnableLogging;
            if (_enableMqttLogging)
            {
                _mqttServer = mqttFactory.CreateMqttServer(new LoggerAdapter(_logger));
            }
            else
            {
                _mqttServer = mqttFactory.CreateMqttServer();
            }

            _mqttServer.UseApplicationMessageReceivedHandler(new MqttApplicationMessageReceivedHandlerDelegate(e => OnApplicationMessageReceived(e)));

            var serverOptions = new MqttServerOptionsBuilder()
                .WithDefaultEndpointPort(options.ServerPort)
                .WithPersistentSessions();

            if (options.PersistRetainedMessages)
            {
                var storage = new MqttServerStorage(_storageService, _logger);
                storage.Start();
                serverOptions.WithStorage(storage);
            }

            _mqttServer.StartAsync(serverOptions.Build()).GetAwaiter().GetResult();

            Task.Factory.StartNew(() => ProcessIncomingMqttMessages(_systemCancellationToken.Token), _systemCancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public List<string> GetTopicImportUids()
        {
            lock (_importers)
            {
                return _importers.Select(i => i.Key).ToList();
            }
        }

        public string StartTopicImport(string uid, MqttImportTopicParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            if (string.IsNullOrEmpty(uid))
            {
                uid = Guid.NewGuid().ToString("D");
            }

            var importer = new MqttTopicImporter(parameters, this, _enableMqttLogging,  _logger);
            importer.Start();

            lock (_importers)
            {
                if (_importers.TryGetValue(uid, out var existingImporter))
                {
                    existingImporter.Stop();
                }

                _importers[uid] = importer;
            }

            _logger.Log(LogLevel.Information, "Started importer '{0}' for topic '{1}' from server '{2}'.", uid, parameters.Topic, parameters.Server);
            return uid;
        }

        public void StopTopicImport(string uid)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            lock (_importers)
            {
                if (_importers.TryGetValue(uid, out var importer))
                {
                    importer.Stop();
                    _logger.Log(LogLevel.Information, "Stopped importer '{0}'.");
                }

                _importers.Remove(uid);
            }
        }

        public List<MqttSubscriber> GetSubscribers()
        {
            lock (_subscribers)
            {
                return _subscribers.Values.ToList();
            }
        }

        public Task<IList<MqttApplicationMessage>> GetRetainedMessagesAsync()
        {
            return _mqttServer.GetRetainedApplicationMessagesAsync();
        }

        public Task DeleteRetainedMessagesAsync()
        {
            return _mqttServer.ClearRetainedApplicationMessagesAsync();
        }

        public void Publish(MqttPublishParameters parameters)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(parameters.Topic)
                .WithPayload(parameters.Payload)
                .WithQualityOfServiceLevel(parameters.QualityOfServiceLevel)
                .WithRetainFlag(parameters.Retain)
                .Build();

            _mqttServer.PublishAsync(message).GetAwaiter().GetResult();
            _outboundCounter.Increment();

            _logger.Log(LogLevel.Trace, $"Published MQTT topic '{parameters.Topic}.");
        }

        public string Subscribe(string uid, string topicFilter, Action<MqttApplicationMessageReceivedEventArgs> callback)
        {
            if (topicFilter == null) throw new ArgumentNullException(nameof(topicFilter));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            if (string.IsNullOrEmpty(uid))
            {
                uid = Guid.NewGuid().ToString("D");
            }

            lock (_subscribers)
            {
                _subscribers[uid] = new MqttSubscriber(uid, topicFilter, callback);
            }

            // Enqueue all retained messages to match the expected MQTT behavior.
            // Here we have no client per subscription. So we need to adopt some
            // features here manually.
            foreach (var retainedMessage in _mqttServer.GetRetainedApplicationMessagesAsync().GetAwaiter().GetResult())
            {
                _incomingMessages.Add(new MqttApplicationMessageReceivedEventArgs(null, retainedMessage));
            }

            return uid;
        }

        public void Unsubscribe(string uid)
        {
            lock (_subscribers)
            {
                _subscribers.Remove(uid);
            }
        }

        public Task<IList<IMqttClientStatus>> GetClientsAsync()
        {
            return _mqttServer.GetClientStatusAsync();
        }

        public Task<IList<IMqttSessionStatus>> GetSessionsAsync()
        {
            return _mqttServer.GetSessionStatusAsync();
        }

        private void ProcessIncomingMqttMessages(CancellationToken cancellationToken)
        {
            Thread.CurrentThread.Name = nameof(ProcessIncomingMqttMessages);

            while (!cancellationToken.IsCancellationRequested)
            {
                MqttApplicationMessageReceivedEventArgs message = null;
                try
                {
                    message = _incomingMessages.Take(cancellationToken);
                    if (message == null || cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var affectedSubscribers = new List<MqttSubscriber>();
                    lock (_subscribers)
                    {
                        foreach (var subscriber in _subscribers.Values)
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

                    _outboundCounter.Increment();
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    _logger.Log(LogLevel.Error, exception, $"Error while processing MQTT message with topic '{message?.ApplicationMessage?.Topic}'.");
                }
            }
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

        private void OnApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            _inboundCounter.Increment();
            _incomingMessages.Add(eventArgs);
        }
    }
}
