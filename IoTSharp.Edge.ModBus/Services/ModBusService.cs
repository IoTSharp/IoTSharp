using IoT.Things.ModBus.Jobs;
using IoTSharp.EdgeSdk.MQTT;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        public ModBusService(MQTTClient mqtt, IOptions<AppSettings> options, ILogger<ModBusService> logger, ISchedulerFactory factory)
        {
            _logger = logger;
            _options = options.Value;
            _mqtt = mqtt;
            _mqtt.OnExcCommand += _mqtt_OnExcCommand;
            _mqtt.OnReceiveAttributes += Mqtt_OnReceiveAttributesAsync;
            _factory = factory;
        }

        private async void Mqtt_OnReceiveAttributesAsync(object sender, IoTSharp.EdgeSdk.MQTT.AttributeResponse e)
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

     
    }


        private void _mqtt_OnExcCommand(object sender, RpcRequest e)
        {
            if (e.Command =="WriteInt")
            {
                Task.Run(async () =>
                {

                    await _mqtt.ResponseExecommand(new  RpcResponse() { Command = e.Command, Data =  "OK", DeviceName = e.DeviceName, ResponseId = e.RequestId });
                });
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                await _mqtt.ConnectAsync(_options.BrokerUri, _options.AccessToken);


                await _mqtt.ResponseAttributes("me",true, "ModBusConfig");
                do
                {
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