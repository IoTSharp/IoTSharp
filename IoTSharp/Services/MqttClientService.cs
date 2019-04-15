using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Services
{
    public class MqttClientService : IHostedService
    {

        private readonly ILogger _logger;
        readonly IMqttClient _mqtt;
        private readonly IMqttClientOptions _clientOptions;
        private ApplicationDbContext _dbContext;
        public MqttClientService(ILogger<MqttClientService> logger, ApplicationDbContext context, IMqttClient mqtt, IMqttClientOptions clientOptions,DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        {
            _logger = logger;
            _mqtt = mqtt;
            _clientOptions = clientOptions;
            _dbContext = context;
            mqtt.ApplicationMessageReceived += Mqtt_ApplicationMessageReceived;
            mqtt.Connected += Mqtt_Connected;
            mqtt.Disconnected += Mqtt_DisconnectedAsync;
        }

        private async void Mqtt_DisconnectedAsync(object sender, MqttClientDisconnectedEventArgs e)
        {
            _logger.LogInformation($"DISCONNECTED FROM SERVER  ClientWasConnected:{e.ClientWasConnected}, Exception={ e.Exception.Message}");
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

        private void Mqtt_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            _logger.LogInformation($"CONNECTED  IsSessionPresent: {e.IsSessionPresent}");
        }

        Dictionary<string, Device> Devices => MqttEventsHandler.Devices;


        private void Mqtt_ApplicationMessageReceived(object sender, MQTTnet.MqttApplicationMessageReceivedEventArgs e)
        {

            _logger.LogInformation($"Received  : {e.ApplicationMessage.Topic}");
            if (e.ApplicationMessage.Topic.ToLower().StartsWith("/devices/telemetry"))
            {
                if (Devices.ContainsKey(e.ClientId))
                {

                    var device = Devices[e.ClientId];

                    Task.Run(async () =>
                    {
                        try
                        {
                            var telemetrys = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(e.ApplicationMessage.ConvertPayloadToString());
                            var result = await _dbContext.SaveAsync<TelemetryLatest, TelemetryData>(telemetrys, device, DataSide.ClientSide);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Can't upload telemetry to device {device.Name}({device.Id}).the payload is {e.ApplicationMessage.ConvertPayloadToString()}");
                        }
                    });
                }
            }
            else if (e.ApplicationMessage.Topic.ToLower().StartsWith("/devices/attributes"))
            {
                if (Devices.ContainsKey(e.ClientId))
                {
                    var device = Devices[e.ClientId];
                    Task.Run(async () =>
                    {
                        try
                        {
                            var attributes = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(e.ApplicationMessage.ConvertPayloadToString());
                            var result = await _dbContext.SaveAsync<AttributeLatest, AttributeData>(attributes, device, DataSide.ClientSide);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Can't upload attributes to device {device.Name}({device.Id}).the payload is \"{e.ApplicationMessage.ConvertPayloadToString()}\"");
                        }
                    });
                }
            }
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
            _mqtt.Disconnected -= Mqtt_DisconnectedAsync;
            return _mqtt.DisconnectAsync();
        }
    }
}
