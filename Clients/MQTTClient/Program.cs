using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace MQTTClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            MQTTnet.MqttFactory factory = new MQTTnet.MqttFactory();
            var client = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                        .WithClientId(Guid.NewGuid().ToString())
                        .WithTcpServer("localhost")
                        .WithCredentials("3cb97cd31fbc40b08d12ec47a6fad622")//token
                        .Build();
            Task.Run(async () =>
            {
                await client.ConnectAsync(options);
                do
                {
                    var message = new MqttApplicationMessageBuilder()
                                   .WithTopic("/devices/me/telemetry")
                                   .WithPayload(JsonConvert.SerializeObject(new
                                   {
                                       RandomString = Guid.NewGuid().ToString(),
                                       NowTime = DateTime.Now
                                   }))
                                   .Build();
                    Console.WriteLine(message.ConvertPayloadToString());
                    await client.PublishAsync(message);
                    await Task.Delay(TimeSpan.FromSeconds(10));
                } while (Console.ReadKey().Key != ConsoleKey.Escape);
                await client.DisconnectAsync();
            }).Wait();
        }
    }
}