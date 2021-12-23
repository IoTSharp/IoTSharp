using DotNetCore.CAP;
using Dynamitey;
using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Extensions
{
    public static class EventBusExtensions
    {

        public static void PublishAttributeData(this ICapPublisher cap, RawMsg msg)
        {
            cap.Publish("iotsharp.services.datastream.attributedata", msg);
        }
        public static void PublishTelemetryData(this ICapPublisher cap, RawMsg msg)
        {
            cap.Publish("iotsharp.services.datastream.telemetrydata", msg);
        }
        public static void PublishDeviceStatus(this ICapPublisher cap,  Guid  devid , bool  status)
        {
            cap.Publish("iotsharp.services.datastream.devicestatus",  new  DeviceStatus {  DeviceId=devid, Status=status }  );
        }
    }
}
