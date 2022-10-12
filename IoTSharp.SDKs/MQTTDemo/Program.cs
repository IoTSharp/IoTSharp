// See https://aka.ms/new-console-template for more information
using IoTSharp.EdgeSdk.MQTT;

Console.WriteLine("Hello, World!");
var client = new MQTTClient();
await client.ConnectAsync(new FileInfo( Environment.GetCommandLineArgs().Skip(1).First()));
while (true)
{
    if (client.IsConnected)
    {
        await client.UploadTelemetryDataAsync(new { DateTime.Now });
        Console.WriteLine(DateTime.Now);
    }
    Thread.Sleep(TimeSpan.FromSeconds(5));
    Console.WriteLine("====");
}
   
 

Console.ReadLine();
