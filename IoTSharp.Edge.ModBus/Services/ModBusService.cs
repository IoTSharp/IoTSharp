using IoT.Things.ModBus.Jobs;
using IoT.Things.ModBus.Models;
using IoTSharp.EdgeSdk.MQTT;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using QuartzHostedService;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Things.ModBus.Services
{
    public class ModBusService : IHostedService
    {
        private readonly MQTTClient _mqtt;
        private readonly AppSettings _options;
        private readonly ILogger _logger;
        private readonly ISchedulerFactory _factory;
        private   Uri _modbusuri;
        public ModBusService(MQTTClient mqtt, IOptions<AppSettings> options, ILogger<ModBusService> logger, ISchedulerFactory factory)
        {
            _logger = logger;
            _options = options.Value;
            _mqtt = mqtt;
            _mqtt.OnExcRpc += _mqtt_OnExcCommand;
            _mqtt.OnReceiveAttributes += Mqtt_OnReceiveAttributesAsync;
            _factory = factory;
            _mqtt.DeviceId = _options.DeviceId;
        }
        bool HaveModBusConfig = false;
        ModBusConfig modBusConfig = null;
        private async void Mqtt_OnReceiveAttributesAsync(object sender, IoTSharp.EdgeSdk.MQTT.AttributeResponse e)
        {
            try
            {
                string key = $"{e.DeviceName}_{e.KeyName}";
                var sc = await _factory.GetScheduler();
                var job = JobBuilder.Create<Slaver>().WithIdentity($"{e.DeviceName}_{e.KeyName}").Build();
                ITrigger trigger = TriggerBuilder.Create()
                                 .WithIdentity(key).UsingJobData(nameof(ModBusConfig), e.Data).UsingJobData(nameof(e.DeviceName), e.DeviceName).UsingJobData(nameof(e.KeyName), e.KeyName).ForJob(job)
                                 .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()).StartNow().Build();

                if (await sc.CheckExists(new JobKey(key)))
                {
                    await sc.RescheduleJob(new TriggerKey(key), trigger);
                }
                else
                {
                    await sc.ScheduleJob(job, trigger);
                }
                modBusConfig = JToken.Parse( e.Data).SelectToken("ModBusConfig").ToObject<ModBusConfig>();
                if (modBusConfig != null )
                {
                    _modbusuri = modBusConfig.ModBusUri;
                    HaveModBusConfig = true;
                }
            }
            catch (Exception ex)
            {
                HaveModBusConfig = false;
                _logger.LogError($"Mqtt_OnReceiveAttributesAsync: {ex.GetType().Name}{ex.Message}");
            }
        }


        private void _mqtt_OnExcCommand(object sender, RpcRequest e)
        {
            if (e.Method =="WriteInt")
            {
                Task.Run(async () =>
                {
                    
                var _modbus = new HslCommunication.ModBus.ModbusTcpNet(_modbusuri.Host, _modbusuri.Port, byte.Parse(_modbusuri.AbsolutePath.Trim('/', '\\')));
                        _modbus.UseSynchronousNet = true;
                        var info = _modbus.ConnectServer();
                        var paramsx = Newtonsoft.Json.JsonConvert.DeserializeObject<RpcParam<int>>(e.Params);
                    var result=  await   _modbus.WriteAsync(paramsx.Address, paramsx.Value);
                    await _mqtt.ResponseExecommand(new  RpcResponse() { Method = e.Method, Data = JsonConvert.SerializeObject(result) , DeviceId = e.DeviceId, ResponseId = e.RequestId });
                });
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                await _mqtt.ConnectAsync(_options.BrokerUri, _options.AccessToken);


              
                do
                {
                    if (!HaveModBusConfig)
                    {
                        await _mqtt.RequestAttributes("me", true, "ModBusConfig");
                    }
                    await _mqtt.UploadAttributeAsync(new { ModBusServiceStatus = "OK" });
                    Thread.Sleep(TimeSpan.FromSeconds(60));
                } while (!cancellationToken.IsCancellationRequested);
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}