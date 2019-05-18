using IoTSharp.Data;
using IoTSharp.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Services
{
    public class MqttClientService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IMqttClient _mqtt;
        private readonly IMqttClientOptions _clientOptions;
        private ApplicationDbContext _dbContext;
        private IServiceScope _serviceScope;

        public MqttClientService(ILogger<MqttClientService> logger, IServiceScopeFactory scopeFactor, IMqttClient mqtt, IMqttClientOptions clientOptions)
        {
            _logger = logger;
            _mqtt = mqtt;
            _clientOptions = clientOptions;
            mqtt.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(args => Mqtt_ApplicationMessageReceived(mqtt, args));
            mqtt.ConnectedHandler = new MqttClientConnectedHandlerDelegate(args => Mqtt_ConnectedAsync(mqtt, args));
            mqtt.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(args => Mqtt_DisconnectedAsync(mqtt, args));
            _serviceScope = scopeFactor.CreateScope();
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        private async void Mqtt_DisconnectedAsync(object sender, MqttClientDisconnectedEventArgs e)
        {
            _logger.LogInformation($"DISCONNECTED FROM SERVER  ClientWasConnected:{e.ClientWasConnected}, Exception={ e.Exception?.Message}");
            try
            {
                await _mqtt.ConnectAsync(_clientOptions);
                _logger.LogInformation("RECONNECT AGAIN");
            }
            catch (Exception exception)
            {
                _logger.LogError("CONNECTING FAILED", exception);
            }
        }

        private void Mqtt_ConnectedAsync(object sender, MqttClientConnectedEventArgs e)
        {
            _logger.LogInformation($"CONNECTED  IsSessionPresent:  {e.AuthenticateResult.IsSessionPresent } ResultCode: { e.AuthenticateResult.ResultCode}");
        }

        Dictionary<string, Device> Devices => MqttEventsHandler.Devices;

        private void Mqtt_ApplicationMessageReceived(object sender, MQTTnet.MqttApplicationMessageReceivedEventArgs e)
        {
            _logger.LogInformation($"Received  : {e.ApplicationMessage.Topic}");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    _mqtt.ConnectAsync(_clientOptions);
                    _logger.LogInformation("CONNECTED");
                }
                catch (Exception exception)
                {
                    _logger.LogError("CONNECTING FAILED", exception);
                }
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _mqtt.DisconnectedHandler = null;
            return _mqtt.DisconnectAsync();
        }
    }
}